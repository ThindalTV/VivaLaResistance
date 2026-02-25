namespace VivaLaResistance.Core.Interfaces;

/// <summary>
/// Abstraction over platform preferences/settings storage to enable testing.
/// </summary>
public interface IPreferencesWrapper
{
    /// <summary>
    /// Gets a value from preferences.
    /// </summary>
    T? Get<T>(string key, T? defaultValue);

    /// <summary>
    /// Sets a value in preferences.
    /// </summary>
    void Set<T>(string key, T value);

    /// <summary>
    /// Removes a key from preferences.
    /// </summary>
    void Remove(string key);
}
