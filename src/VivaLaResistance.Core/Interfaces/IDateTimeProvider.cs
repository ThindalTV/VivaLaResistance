namespace VivaLaResistance.Core.Interfaces;

/// <summary>
/// Abstraction over date/time to enable deterministic testing.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    DateTimeOffset UtcNow { get; }
}
