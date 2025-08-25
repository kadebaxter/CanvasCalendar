namespace CanvasCalendar.Tests.Helpers;

/// <summary>
/// Mock implementation of MAUI Preferences for testing
/// </summary>
public static class TestPreferences
{
    private static readonly Dictionary<string, object> _preferences = new();
    
    public static void Set(string key, string value)
    {
        _preferences[key] = value;
    }
    
    public static string Get(string key, string defaultValue)
    {
        return _preferences.TryGetValue(key, out var value) ? value.ToString() ?? defaultValue : defaultValue;
    }
    
    public static void Remove(string key)
    {
        _preferences.Remove(key);
    }
    
    public static void Clear()
    {
        _preferences.Clear();
    }
    
    public static bool ContainsKey(string key)
    {
        return _preferences.ContainsKey(key);
    }
}

/// <summary>
/// Mock Preferences class for testing
/// </summary>
public static class Preferences
{
    public static IPreferences Default => new TestPreferencesImplementation();
}

public interface IPreferences
{
    string Get(string key, string defaultValue);
    void Set(string key, string value);
    void Remove(string key);
    bool ContainsKey(string key);
}

public class TestPreferencesImplementation : IPreferences
{
    public string Get(string key, string defaultValue) => TestPreferences.Get(key, defaultValue);
    public void Set(string key, string value) => TestPreferences.Set(key, value);
    public void Remove(string key) => TestPreferences.Remove(key);
    public bool ContainsKey(string key) => TestPreferences.ContainsKey(key);
}
