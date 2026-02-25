namespace VivaLaResistance.Services;

using VivaLaResistance.Core.Interfaces;

/// <summary>
/// Service for managing the 7-day free trial and monetization logic.
/// Uses the preferences abstraction for persistent storage.
/// </summary>
public class TrialService : ITrialService
{
    private const string FirstLaunchKey = "VivaLaResistance_FirstLaunchDate";
    private const int TrialDurationDays = 7;

    private readonly IPreferencesWrapper _preferences;
    private readonly IDateTimeProvider _dateTimeProvider;

    /// <summary>
    /// Creates a new instance of the trial service.
    /// </summary>
    /// <param name="preferences">Preferences wrapper for persistent storage.</param>
    /// <param name="dateTimeProvider">Provider for current date/time (allows testing).</param>
    public TrialService(IPreferencesWrapper preferences, IDateTimeProvider dateTimeProvider)
    {
        _preferences = preferences ?? throw new ArgumentNullException(nameof(preferences));
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    /// <inheritdoc />
    public DateTimeOffset? FirstLaunchDate
    {
        get
        {
            var storedValue = _preferences.Get<string?>(FirstLaunchKey, null);
            if (string.IsNullOrEmpty(storedValue))
            {
                return null;
            }

            return DateTimeOffset.TryParse(storedValue, out var date) ? date : null;
        }
    }

    /// <inheritdoc />
    public int DaysRemaining
    {
        get
        {
            if (FirstLaunchDate is null)
            {
                return TrialDurationDays;
            }

            var elapsed = _dateTimeProvider.UtcNow - FirstLaunchDate.Value;
            var remaining = TrialDurationDays - (int)elapsed.TotalDays;
            return Math.Max(0, remaining);
        }
    }

    /// <inheritdoc />
    public bool IsTrialActive => DaysRemaining > 0;

    /// <inheritdoc />
    public bool IsTrialExpired => !IsFreshInstall && !IsTrialActive;

    /// <inheritdoc />
    public bool IsFreshInstall => FirstLaunchDate is null;

    /// <inheritdoc />
    public void RecordLaunchIfNeeded()
    {
        if (IsFreshInstall)
        {
            var now = _dateTimeProvider.UtcNow;
            _preferences.Set(FirstLaunchKey, now.ToString("O"));
        }
    }

    /// <inheritdoc />
    public bool ShouldShowSupportModal()
    {
        return IsTrialExpired;
    }

    /// <inheritdoc />
    public void ResetTrial()
    {
        _preferences.Remove(FirstLaunchKey);
    }
}

