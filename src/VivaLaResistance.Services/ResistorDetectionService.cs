namespace VivaLaResistance.Services;

using Microsoft.Extensions.Logging;
using VivaLaResistance.Core.Interfaces;
using VivaLaResistance.Core.Models;

/// <summary>
/// Full-pipeline resistor detection service that orchestrates:
/// 1. ONNX-based resistor body localization (YOLOv8-nano)
/// 2. HSV-based color band extraction from cropped regions
/// 3. Resistance value calculation from color bands
/// 
/// Implements graceful degradation: if ONNX model is unavailable, returns empty results
/// without throwing exceptions (allows app to start before model is trained/deployed).
/// 
/// Performance characteristics:
/// - Target: less than 100ms per frame on mid-range devices
/// - Confidence threshold: 0.65 (filters low-confidence detections)
/// - Supports multiple resistors per frame
/// 
/// Frame-skip strategy (issue #27):
/// Uses a SemaphoreSlim(1,1) as a non-blocking gate. When a new camera frame arrives,
/// TryWait(0) is called: if inference is already running the semaphore is taken and the
/// new frame is dropped immediately rather than queued. This keeps latency bounded and
/// prevents a backlog of stale frames building up on slow devices.
/// </summary>
public class ResistorDetectionService : IResistorDetectionService, IDisposable
{
    // Minimum confidence threshold for accepting detections (0.65 = 65%)
    // Lower values increase recall but add false positives; higher values increase precision.
    // Hysteresis: once a detection passes threshold, it remains visible until confidence drops below 0.60.
    private const double ConfidenceThreshold = 0.65;
    private const double ConfidenceHysteresisLower = 0.60;

    private readonly IResistorLocalizationService _localizationService;
    private readonly IResistorValueCalculatorService _calculatorService;
    private readonly ILogger<ResistorDetectionService> _logger;

    // Track previous frame's detections for hysteresis (reduce flicker)
    private readonly Dictionary<Guid, ResistorReading> _previousDetections = new();

    // Non-blocking semaphore for frame-skip throttle: only one inference at a time.
    // If TryWait(0) returns false the incoming frame is dropped (never queued).
    private readonly SemaphoreSlim _inferenceSemaphore = new(1, 1);

    private volatile bool _isPaused;

    private bool _disposed;

    /// <inheritdoc />
    public bool IsInitialized { get; private set; }

    public ResistorDetectionService(
        IResistorLocalizationService localizationService,
        IResistorValueCalculatorService calculatorService,
        ILogger<ResistorDetectionService> logger)
    {
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _calculatorService = calculatorService ?? throw new ArgumentNullException(nameof(calculatorService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Initializing resistor detection service");

        try
        {
            await _localizationService.InitializeAsync().ConfigureAwait(false);
            IsInitialized = true;

            if (!_localizationService.IsInitialized)
            {
                _logger.LogWarning("ONNX localization service initialized but model not loaded - detection will return empty results until model is available");
            }
            else
            {
                _logger.LogInformation("Resistor detection service initialized successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize resistor detection service");
            // Graceful degradation: mark as initialized but log the error
            // Service will return empty results when DetectResistorsAsync is called
            IsInitialized = true;
        }
    }
    /// <inheritdoc />
    public void Pause()
    {
        _isPaused = true;
    }

    /// <inheritdoc />
    public void Resume()
    {
        _isPaused = false;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ResistorReading>> DetectResistorsAsync(
        byte[] imageData,
        int width,
        int height,
        CancellationToken cancellationToken = default)
    {
        if (!IsInitialized)
        {
            throw new InvalidOperationException("Detection service must be initialized before use. Call InitializeAsync first.");
        }

        if (_isPaused) return Array.Empty<ResistorReading>();

        if (imageData == null || imageData.Length != width * height * 4)
        {
            _logger.LogWarning("Invalid image data: expected {Expected} bytes (BGRA8888), got {Actual}", width * height * 4, imageData?.Length ?? 0);
            return Array.Empty<ResistorReading>();
        }

        // Frame-skip throttle: drop incoming frame if a previous inference is still running.
        // This keeps per-frame latency predictable and avoids building a backlog of stale frames.
        if (!_inferenceSemaphore.Wait(0))
        {
            _logger.LogDebug("Frame dropped - inference already in progress (frame-skip throttle)");
            return Array.Empty<ResistorReading>();
        }

        try
        {
            // Step 1: Localize resistor bodies using ONNX model
            var boundingBoxes = await _localizationService.InferAsync(imageData, width, height).ConfigureAwait(false);

            if (boundingBoxes.Count == 0)
            {
                _previousDetections.Clear();
                return Array.Empty<ResistorReading>();
            }

            // Step 2: For each bounding box, extract color bands and calculate resistance
            var detections = new List<ResistorReading>();

            foreach (var box in boundingBoxes)
            {
                // Apply confidence threshold with hysteresis
                if (!ShouldProcessDetection(box))
                {
                    continue;
                }

                try
                {
                    // Step 2a: Extract color bands from bounding box region using HSV analysis
                    // TODO: Implement HSV-based color band extraction in a separate service
                    // For now, this is a placeholder that returns empty color bands
                    var colorBands = ExtractColorBandsFromRegion(imageData, width, height, box);

                    if (colorBands.Count == 0)
                    {
                        _logger.LogDebug("No color bands extracted from bounding box at ({X}, {Y})", box.X, box.Y);
                        continue;
                    }

                    // Step 2b: Calculate resistance value from color bands
                    var resistanceValue = _calculatorService.CalculateResistance(colorBands);
                    var formattedValue = _calculatorService.FormatResistance(resistanceValue);
                    var tolerance = _calculatorService.GetTolerancePercent(colorBands[^1]);

                    var reading = new ResistorReading(
                        Id: Guid.NewGuid(),
                        ValueInOhms: resistanceValue,
                        FormattedValue: formattedValue,
                        TolerancePercent: tolerance,
                        BandCount: colorBands.Count,
                        ColorBands: colorBands,
                        BoundingBox: box,
                        Confidence: box.Confidence,
                        Timestamp: DateTimeOffset.UtcNow);

                    detections.Add(reading);
                    _previousDetections[reading.Id] = reading;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to process detection at ({X}, {Y}) - skipping", box.X, box.Y);
                }
            }

            return detections;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during resistor detection");
            return Array.Empty<ResistorReading>();
        }
        finally
        {
            _inferenceSemaphore.Release();
        }
    }

    /// <summary>
    /// Determines whether a detection should be processed based on confidence threshold with hysteresis.
    /// This reduces flicker by requiring higher confidence to ADD a detection, but lower confidence to REMOVE it.
    /// </summary>
    private bool ShouldProcessDetection(ResistorBoundingBox box)
    {
        // If confidence is above the main threshold, always process
        if (box.Confidence >= ConfidenceThreshold)
        {
            return true;
        }

        // If confidence is in the hysteresis band (0.60-0.65), only process if we saw it in the previous frame
        if (box.Confidence >= ConfidenceHysteresisLower)
        {
            // Check if any previous detection overlaps significantly with this one
            foreach (var prevDetection in _previousDetections.Values)
            {
                if (BoundingBoxesOverlap(box, prevDetection.BoundingBox))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if two bounding boxes overlap significantly.
    /// Uses center-distance heuristic for efficiency.
    /// </summary>
    private static bool BoundingBoxesOverlap(ResistorBoundingBox a, ResistorBoundingBox b)
    {
        // Calculate centers
        var centerAX = a.X + a.Width / 2;
        var centerAY = a.Y + a.Height / 2;
        var centerBX = b.X + b.Width / 2;
        var centerBY = b.Y + b.Height / 2;

        // Simple center-distance heuristic (more efficient than full IoU calculation)
        var centerDistX = Math.Abs(centerAX - centerBX);
        var centerDistY = Math.Abs(centerAY - centerBY);
        var maxDist = Math.Max(a.Width, a.Height) * 0.5f;

        return centerDistX < maxDist && centerDistY < maxDist;
    }

    /// <summary>
    /// Extracts color bands from the resistor bounding box region using HSV color classification.
    /// 
    /// PLACEHOLDER: This is a stub implementation that returns empty color bands.
    /// The full implementation requires:
    /// 1. Crop BGRA8888 image data to bounding box region
    /// 2. Convert RGB to HSV color space
    /// 3. Segment resistor body into color bands (typically 4-6 bands)
    /// 4. Classify each band's dominant color using HSV thresholds
    /// 5. Return ordered list of ColorBand enum values
    /// 
    /// This will be implemented in a follow-up issue once the ONNX model provides reliable bounding boxes.
    /// </summary>
    private IReadOnlyList<ColorBand> ExtractColorBandsFromRegion(
        byte[] imageData,
        int width,
        int height,
        ResistorBoundingBox box)
    {
        // TODO: Implement HSV-based color band extraction
        // For now, return empty list so the service can be tested with ONNX localization
        _logger.LogDebug("Color band extraction not yet implemented - returning empty list");
        return Array.Empty<ColorBand>();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the inference semaphore and any disposable downstream services.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            _inferenceSemaphore.Dispose();
            (_localizationService as IDisposable)?.Dispose();
        }
        _disposed = true;
    }
}
