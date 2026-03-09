using VivaLaResistance.Core.Exceptions;
using Xunit;

namespace VivaLaResistance.Tests
{
    /// <summary>
    /// Tests for custom exception types used in error handling.
    /// Validates inheritance, constructors, and message propagation.
    /// </summary>
    public class ExceptionTypeTests
    {
        [Fact]
        public void CameraPermissionException_InheritsFromException()
        {
            // Arrange & Act
            var exception = new CameraPermissionException("Camera permission denied by user");

            // Assert
            Assert.IsAssignableFrom<Exception>(exception);
        }

        [Fact]
        public void CameraPermissionException_CarriesMeaningfulMessage()
        {
            // Arrange
            const string expectedMessage = "Camera permission denied by user";

            // Act
            var exception = new CameraPermissionException(expectedMessage);

            // Assert
            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public void CameraPermissionException_SupportsInnerException()
        {
            // Arrange
            var innerException = new InvalidOperationException("Platform error");
            const string message = "Camera permission denied";

            // Act
            var exception = new CameraPermissionException(message, innerException);

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.Same(innerException, exception.InnerException);
        }

        [Fact]
        public void CameraUnavailableException_InheritsFromException()
        {
            // Arrange & Act
            var exception = new CameraUnavailableException("Camera hardware not available");

            // Assert
            Assert.IsAssignableFrom<Exception>(exception);
        }

        [Fact]
        public void CameraUnavailableException_CarriesMeaningfulMessage()
        {
            // Arrange
            const string expectedMessage = "Camera hardware not available";

            // Act
            var exception = new CameraUnavailableException(expectedMessage);

            // Assert
            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public void CameraUnavailableException_SupportsInnerException()
        {
            // Arrange
            var innerException = new InvalidOperationException("Platform error");
            const string message = "Camera unavailable";

            // Act
            var exception = new CameraUnavailableException(message, innerException);

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.Same(innerException, exception.InnerException);
        }

        [Fact]
        public void CameraPermissionException_DoesNotInheritFromCameraUnavailableException()
        {
            // Arrange & Act
            Exception permissionException = new CameraPermissionException("Permission denied");
            Exception unavailableException = new CameraUnavailableException("Camera unavailable");

            // Assert - they should be sibling exceptions, not parent/child
            Assert.False(permissionException is CameraUnavailableException);
            Assert.False(unavailableException is CameraPermissionException);

            // Both should inherit from Exception directly
            Assert.IsAssignableFrom<Exception>(permissionException);
            Assert.IsAssignableFrom<Exception>(unavailableException);
        }

        [Fact]
        public void CameraPermissionException_DefaultConstructor_HasMeaningfulMessage()
        {
            // Act
            var exception = new CameraPermissionException();

            // Assert
            Assert.NotNull(exception);
            Assert.IsAssignableFrom<Exception>(exception);
            Assert.Contains("Camera permission", exception.Message);
        }

        [Fact]
        public void CameraUnavailableException_DefaultConstructor_HasMeaningfulMessage()
        {
            // Act
            var exception = new CameraUnavailableException();

            // Assert
            Assert.NotNull(exception);
            Assert.IsAssignableFrom<Exception>(exception);
            Assert.Contains("Camera", exception.Message);
            Assert.Contains("unavailable", exception.Message);
        }

        [Fact]
        public void CameraPermissionException_CanBeCaught_AsBaseException()
        {
            // Arrange
            var exception = new CameraPermissionException("Permission denied");

            // Act & Assert
            try
            {
                throw exception;
            }
            catch (Exception ex)
            {
                Assert.IsType<CameraPermissionException>(ex);
                Assert.Contains("Permission denied", ex.Message);
            }
        }

        [Fact]
        public void CameraUnavailableException_CanBeCaught_AsBaseException()
        {
            // Arrange
            var exception = new CameraUnavailableException("Camera unavailable");

            // Act & Assert
            try
            {
                throw exception;
            }
            catch (Exception ex)
            {
                Assert.IsType<CameraUnavailableException>(ex);
                Assert.Contains("Camera unavailable", ex.Message);
            }
        }
    }
}
