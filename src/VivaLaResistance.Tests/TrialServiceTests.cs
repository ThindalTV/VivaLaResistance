namespace VivaLaResistance.Tests;

using VivaLaResistance.Core.Interfaces;
using VivaLaResistance.Services;
using Xunit;

/// <summary>
/// Unit tests for the TrialService.
/// </summary>
public class TrialServiceTests
{
    #region Test Helpers

    private class TestPreferencesWrapper : IPreferencesWrapper
    {
        private readonly Dictionary<string, object?> _storage = new();

        public T? Get<T>(string key, T? defaultValue)
        {
            return _storage.TryGetValue(key, out var value) && value is T typedValue
                ? typedValue
                : defaultValue;
        }

        public void Set<T>(string key, T value)
        {
            _storage[key] = value;
        }

        public void Remove(string key)
        {
            _storage.Remove(key);
        }
    }

    private class TestDateTimeProvider : IDateTimeProvider
    {
        public DateTimeOffset UtcNow { get; set; } = DateTimeOffset.UtcNow;
    }

    private TrialService CreateService(DateTimeOffset? currentTime = null)
    {
        var preferences = new TestPreferencesWrapper();
        var dateProvider = new TestDateTimeProvider
        {
            UtcNow = currentTime ?? DateTimeOffset.UtcNow
        };
        return new TrialService(preferences, dateProvider);
    }

    private TrialService CreateServiceWithFirstLaunch(DateTimeOffset firstLaunch, DateTimeOffset currentTime)
    {
        var preferences = new TestPreferencesWrapper();
        var dateProvider = new TestDateTimeProvider { UtcNow = firstLaunch };
        var service = new TrialService(preferences, dateProvider);

        // Record the first launch
        service.RecordLaunchIfNeeded();

        // Update the current time
        dateProvider.UtcNow = currentTime;
        return service;
    }

    #endregion

    #region Fresh Install Tests

    [Fact]
    public void IsFreshInstall_NoFirstLaunchRecorded_ReturnsTrue()
    {
        var service = CreateService();
        Assert.True(service.IsFreshInstall);
    }

    [Fact]
    public void IsFreshInstall_AfterRecordLaunch_ReturnsFalse()
    {
        var service = CreateService();
        service.RecordLaunchIfNeeded();
        Assert.False(service.IsFreshInstall);
    }

    [Fact]
    public void FirstLaunchDate_NoLaunchRecorded_ReturnsNull()
    {
        var service = CreateService();
        Assert.Null(service.FirstLaunchDate);
    }

    [Fact]
    public void FirstLaunchDate_AfterRecordLaunch_ReturnsDate()
    {
        var now = DateTimeOffset.UtcNow;
        var service = CreateService(now);
        service.RecordLaunchIfNeeded();

        Assert.NotNull(service.FirstLaunchDate);
        Assert.Equal(now.Date, service.FirstLaunchDate!.Value.Date);
    }

    #endregion

    #region Trial Active Tests

    [Fact]
    public void IsTrialActive_FreshInstall_ReturnsTrue()
    {
        var service = CreateService();
        Assert.True(service.IsTrialActive);
    }

    [Fact]
    public void IsTrialActive_Day1_ReturnsTrue()
    {
        var firstLaunch = DateTimeOffset.UtcNow;
        var currentTime = firstLaunch.AddDays(1);
        var service = CreateServiceWithFirstLaunch(firstLaunch, currentTime);

        Assert.True(service.IsTrialActive);
    }

    [Fact]
    public void IsTrialActive_Day6_ReturnsTrue()
    {
        var firstLaunch = DateTimeOffset.UtcNow;
        var currentTime = firstLaunch.AddDays(6);
        var service = CreateServiceWithFirstLaunch(firstLaunch, currentTime);

        Assert.True(service.IsTrialActive);
    }

    [Fact]
    public void IsTrialActive_Day7_ReturnsFalse()
    {
        var firstLaunch = DateTimeOffset.UtcNow;
        var currentTime = firstLaunch.AddDays(7);
        var service = CreateServiceWithFirstLaunch(firstLaunch, currentTime);

        Assert.False(service.IsTrialActive);
    }

    [Fact]
    public void IsTrialActive_Day30_ReturnsFalse()
    {
        var firstLaunch = DateTimeOffset.UtcNow;
        var currentTime = firstLaunch.AddDays(30);
        var service = CreateServiceWithFirstLaunch(firstLaunch, currentTime);

        Assert.False(service.IsTrialActive);
    }

    #endregion

    #region Days Remaining Tests

    [Fact]
    public void DaysRemaining_FreshInstall_Returns7()
    {
        var service = CreateService();
        Assert.Equal(7, service.DaysRemaining);
    }

    [Fact]
    public void DaysRemaining_Day1_Returns6()
    {
        var firstLaunch = DateTimeOffset.UtcNow;
        var currentTime = firstLaunch.AddDays(1);
        var service = CreateServiceWithFirstLaunch(firstLaunch, currentTime);

        Assert.Equal(6, service.DaysRemaining);
    }

    [Fact]
    public void DaysRemaining_Day7_Returns0()
    {
        var firstLaunch = DateTimeOffset.UtcNow;
        var currentTime = firstLaunch.AddDays(7);
        var service = CreateServiceWithFirstLaunch(firstLaunch, currentTime);

        Assert.Equal(0, service.DaysRemaining);
    }

    [Fact]
    public void DaysRemaining_Day30_Returns0_NotNegative()
    {
        var firstLaunch = DateTimeOffset.UtcNow;
        var currentTime = firstLaunch.AddDays(30);
        var service = CreateServiceWithFirstLaunch(firstLaunch, currentTime);

        Assert.Equal(0, service.DaysRemaining);
    }

    #endregion

    #region Trial Expired Tests

    [Fact]
    public void IsTrialExpired_FreshInstall_ReturnsFalse()
    {
        var service = CreateService();
        Assert.False(service.IsTrialExpired);
    }

    [Fact]
    public void IsTrialExpired_WithinTrial_ReturnsFalse()
    {
        var firstLaunch = DateTimeOffset.UtcNow;
        var currentTime = firstLaunch.AddDays(3);
        var service = CreateServiceWithFirstLaunch(firstLaunch, currentTime);

        Assert.False(service.IsTrialExpired);
    }

    [Fact]
    public void IsTrialExpired_AfterTrial_ReturnsTrue()
    {
        var firstLaunch = DateTimeOffset.UtcNow;
        var currentTime = firstLaunch.AddDays(8);
        var service = CreateServiceWithFirstLaunch(firstLaunch, currentTime);

        Assert.True(service.IsTrialExpired);
    }

    #endregion

    #region ShouldShowSupportModal Tests

    [Fact]
    public void ShouldShowSupportModal_FreshInstall_ReturnsFalse()
    {
        var service = CreateService();
        Assert.False(service.ShouldShowSupportModal());
    }

    [Fact]
    public void ShouldShowSupportModal_WithinTrial_ReturnsFalse()
    {
        var firstLaunch = DateTimeOffset.UtcNow;
        var currentTime = firstLaunch.AddDays(3);
        var service = CreateServiceWithFirstLaunch(firstLaunch, currentTime);

        Assert.False(service.ShouldShowSupportModal());
    }

    [Fact]
    public void ShouldShowSupportModal_AfterTrial_ReturnsTrue()
    {
        var firstLaunch = DateTimeOffset.UtcNow;
        var currentTime = firstLaunch.AddDays(8);
        var service = CreateServiceWithFirstLaunch(firstLaunch, currentTime);

        Assert.True(service.ShouldShowSupportModal());
    }

    #endregion

    #region ResetTrial Tests

    [Fact]
    public void ResetTrial_AfterLaunch_BecomeFreshInstall()
    {
        var service = CreateService();
        service.RecordLaunchIfNeeded();

        Assert.False(service.IsFreshInstall);

        service.ResetTrial();

        Assert.True(service.IsFreshInstall);
        Assert.Equal(7, service.DaysRemaining);
    }

    #endregion

    #region RecordLaunchIfNeeded Tests

    [Fact]
    public void RecordLaunchIfNeeded_CalledTwice_DoesNotOverwrite()
    {
        var firstTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var secondTime = new DateTimeOffset(2024, 1, 5, 0, 0, 0, TimeSpan.Zero);

        var preferences = new TestPreferencesWrapper();
        var dateProvider = new TestDateTimeProvider { UtcNow = firstTime };
        var service = new TrialService(preferences, dateProvider);

        // First launch
        service.RecordLaunchIfNeeded();
        var firstLaunchDate = service.FirstLaunchDate;

        // Simulate time passing
        dateProvider.UtcNow = secondTime;

        // Second "launch" - should not overwrite
        service.RecordLaunchIfNeeded();

        Assert.Equal(firstLaunchDate, service.FirstLaunchDate);
    }

    #endregion
}
