namespace VivaLaResistance.Services;

using VivaLaResistance.Core.Interfaces;

/// <summary>
/// Default <see cref="IDateTimeProvider"/> implementation that uses the system clock.
/// </summary>
public class SystemDateTimeProvider : IDateTimeProvider
{
    /// <inheritdoc />
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
