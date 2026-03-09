namespace VivaLaResistance.Tests;

using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntime;
using Moq;
using VivaLaResistance.Services;
using Xunit;

/// <summary>
/// Tests error handling in OnnxResistorLocalizationService.
/// Validates that ML inference failures are handled gracefully without throwing.
/// </summary>
public class OnnxResistorLocalizationServiceErrorTests
{
    [Fact]
    public async Task InferAsync_WhenModelFileNotFound_ReturnsEmptyList()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<OnnxResistorLocalizationService>>();
        var service = new OnnxResistorLocalizationService(mockLogger.Object);
        
        // Service not initialized (no model loaded)
        var frameData = new byte[640 * 480 * 4]; // BGRA8888 dummy frame
        
        // Act
        var result = await service.InferAsync(frameData, 640, 480);
        
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        Assert.False(service.IsInitialized);
    }

    [Fact]
    public async Task InferAsync_WhenFrameDataIsNull_ReturnsEmptyListGracefully()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<OnnxResistorLocalizationService>>();
        var service = new OnnxResistorLocalizationService(mockLogger.Object);
        await service.InitializeAsync();
        
        // Act - null frame data should be handled gracefully
        var result = await service.InferAsync(null!, 640, 480);
        
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task InferAsync_WhenDimensionsAreZero_ReturnsEmptyListGracefully()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<OnnxResistorLocalizationService>>();
        var service = new OnnxResistorLocalizationService(mockLogger.Object);
        await service.InitializeAsync();
        
        var frameData = new byte[640 * 480 * 4];
        
        // Act
        var result = await service.InferAsync(frameData, 0, 0);
        
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task InitializeAsync_WhenModelPathInvalid_DoesNotThrow()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<OnnxResistorLocalizationService>>();
        var service = new OnnxResistorLocalizationService(mockLogger.Object);
        
        // Act & Assert - should not throw, even if model is missing
        var exception = await Record.ExceptionAsync(async () => await service.InitializeAsync());
        Assert.Null(exception);
    }

    [Fact]
    public async Task InferAsync_WhenCalledBeforeInitialization_ReturnsEmptyList()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<OnnxResistorLocalizationService>>();
        var service = new OnnxResistorLocalizationService(mockLogger.Object);
        
        var frameData = new byte[640 * 480 * 4];
        
        // Act - call InferAsync without initializing first
        var result = await service.InferAsync(frameData, 640, 480);
        
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task InitializeAsync_LogsInformationMessage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<OnnxResistorLocalizationService>>();
        var service = new OnnxResistorLocalizationService(mockLogger.Object);
        
        // Act
        await service.InitializeAsync();
        
        // Assert - verify information was logged about model not being available
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ONNX")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
