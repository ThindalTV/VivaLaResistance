namespace VivaLaResistance.Core.Interfaces;

/// <summary>
/// Service for managing the 7-day free trial and monetization logic.
/// After the trial period, a dismissible support modal is shown on each app start.
/// </summary>
public interface ITrialService
{
    /// <summary>
    /// Gets or sets the date when the app was first launched.
    /// </summary>
    DateTimeOffset? FirstLaunchDate { get; }

    /// <summary>
    /// Gets the number of days remaining in the trial period.
    /// Returns 0 if the trial has expired.
    /// </summary>
    int DaysRemaining { get; }

    /// <summary>
    /// Gets whether the trial period is still active.
    /// </summary>
    bool IsTrialActive { get; }

    /// <summary>
    /// Gets whether the trial has expired (trial period has passed).
    /// </summary>
    bool IsTrialExpired { get; }

    /// <summary>
    /// Gets whether this is a fresh install (no prior launch recorded).
    /// </summary>
    bool IsFreshInstall { get; }

    /// <summary>
    /// Records the first launch date if not already recorded.
    /// Should be called on every app startup.
    /// </summary>
    void RecordLaunchIfNeeded();

    /// <summary>
    /// Determines whether the support modal should be shown.
    /// Returns true if the trial has expired.
    /// </summary>
    bool ShouldShowSupportModal();

    /// <summary>
    /// Resets the trial (for testing purposes only).
    /// </summary>
    void ResetTrial();
}
