using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using VivaLaResistance.Core.Interfaces;
using VivaLaResistance.Core.Models;

namespace VivaLaResistance.ViewModels;

/// <summary>
/// ViewModel for the main camera/detection page.
/// Follows MVVM pattern - all business logic delegated to services.
/// </summary>
public class MainViewModel : INotifyPropertyChanged
{
    private readonly IResistorDetectionService _detectionService;
    private readonly IResistorValueCalculatorService _calculatorService;
    private readonly ITrialService _trialService;

    private string _statusText = "Initializing...";
    private string _trialStatusText = string.Empty;
    private bool _showTrialStatus;
    private bool _isCameraNotReady = true;
    private bool _isPermissionDenied;
    private bool _hasDetections;
    private int _detectionCount;

    public MainViewModel(
        IResistorDetectionService detectionService,
        IResistorValueCalculatorService calculatorService,
        ITrialService trialService)
    {
        _detectionService = detectionService ?? throw new ArgumentNullException(nameof(detectionService));
        _calculatorService = calculatorService ?? throw new ArgumentNullException(nameof(calculatorService));
        _trialService = trialService ?? throw new ArgumentNullException(nameof(trialService));

        DetectedResistors = new ObservableCollection<ResistorReading>();
    }

    #region Properties

    /// <summary>
    /// Collection of currently detected resistors in the camera view.
    /// </summary>
    public ObservableCollection<ResistorReading> DetectedResistors { get; }

    /// <summary>
    /// Current status message displayed to the user.
    /// </summary>
    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    /// <summary>
    /// Trial status message (days remaining, etc.).
    /// </summary>
    public string TrialStatusText
    {
        get => _trialStatusText;
        set => SetProperty(ref _trialStatusText, value);
    }

    /// <summary>
    /// Whether to show the trial status text.
    /// </summary>
    public bool ShowTrialStatus
    {
        get => _showTrialStatus;
        set => SetProperty(ref _showTrialStatus, value);
    }

    /// <summary>
    /// Indicates the camera is not yet ready (showing placeholder).
    /// </summary>
    public bool IsCameraNotReady
    {
        get => _isCameraNotReady;
        set
        {
            if (SetProperty(ref _isCameraNotReady, value))
                OnPropertyChanged(nameof(IsCameraInitializing));
        }
    }

    /// <summary>
    /// Indicates camera permission was denied by the user.
    /// </summary>
    public bool IsPermissionDenied
    {
        get => _isPermissionDenied;
        set
        {
            if (SetProperty(ref _isPermissionDenied, value))
                OnPropertyChanged(nameof(IsCameraInitializing));
        }
    }

    /// <summary>
    /// True when camera is not ready AND permission has not been denied — shows the loading/waiting state.
    /// </summary>
    public bool IsCameraInitializing => IsCameraNotReady && !IsPermissionDenied;

    /// <summary>
    /// Whether there are any current detections.
    /// </summary>
    public bool HasDetections
    {
        get => _hasDetections;
        set => SetProperty(ref _hasDetections, value);
    }

    /// <summary>
    /// Number of detected resistors.
    /// </summary>
    public int DetectionCount
    {
        get => _detectionCount;
        set
        {
            if (SetProperty(ref _detectionCount, value))
            {
                HasDetections = value > 0;
                OnPropertyChanged(nameof(DetectionCountText));
            }
        }
    }

    /// <summary>
    /// Formatted detection count for display.
    /// </summary>
    public string DetectionCountText => DetectionCount == 1
        ? "1 resistor"
        : $"{DetectionCount} resistors";

    #endregion

    #region Initialization

    /// <summary>
    /// Initializes the ViewModel and its dependencies.
    /// Called when the page appears.
    /// </summary>
    public async Task InitializeAsync()
    {
        // Handle trial logic
        _trialService.RecordLaunchIfNeeded();
        UpdateTrialStatus();

        // Check if we need to show the support modal
        if (_trialService.ShouldShowSupportModal())
        {
            // TODO: Show dismissible support modal
            // This will be implemented by the UI specialist (Shuri)
        }

        // Initialize detection service
        StatusText = "Loading detection model...";
        try
        {
            await _detectionService.InitializeAsync();
            StatusText = "Ready - Point at a resistor";
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Cleanup when the page disappears.
    /// </summary>
    public void Cleanup()
    {
        DetectedResistors.Clear();
        DetectionCount = 0;
    }

    #endregion

    #region Detection Processing

    /// <summary>
    /// Processes a camera frame and updates detected resistors.
    /// Called by the camera handler for each frame.
    /// </summary>
    public async Task ProcessFrameAsync(byte[] imageData, int width, int height)
    {
        if (!_detectionService.IsInitialized)
        {
            return;
        }

        try
        {
            var readings = await _detectionService.DetectResistorsAsync(imageData, width, height);

            // Update the collection on the main thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                DetectedResistors.Clear();
                foreach (var reading in readings)
                {
                    // Enrich with formatted value
                    reading.FormattedValue = _calculatorService.FormatResistance(reading.ValueInOhms);
                    DetectedResistors.Add(reading);
                }
                DetectionCount = readings.Count;
            });
        }
        catch (NotImplementedException)
        {
            // ML detection not yet implemented - this is expected
        }
        catch (Exception ex)
        {
            StatusText = $"Detection error: {ex.Message}";
        }
    }

    #endregion

    #region Private Helpers

    private void UpdateTrialStatus()
    {
        if (_trialService.IsTrialActive)
        {
            var days = _trialService.DaysRemaining;
            TrialStatusText = days == 1
                ? "Trial: 1 day remaining"
                : $"Trial: {days} days remaining";
            ShowTrialStatus = true;
        }
        else if (_trialService.IsTrialExpired)
        {
            TrialStatusText = "Trial ended - Thanks for using Viva La Resistance!";
            ShowTrialStatus = true;
        }
        else
        {
            ShowTrialStatus = false;
        }
    }

    #endregion

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    #endregion
}
