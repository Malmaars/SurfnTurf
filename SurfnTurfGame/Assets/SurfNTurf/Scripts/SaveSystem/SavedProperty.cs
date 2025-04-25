using UnityEngine;
using System;

[System.Serializable]
public class SavedProperty<T>
{
    [SerializeField] private string key;
    [SerializeField] public T defaultValue;
    [SerializeField] private T currentValue; // Backing field for Inspector display

    public SavedProperty(string key, T defaultValue = default)
    {
        this.key = key;
        this.defaultValue = defaultValue;

        // Check if the key exists in PlayerPrefs
        if (!PlayerPrefs.HasKey(this.key))
        {
            // If the key doesn't exist, set it to the default value
            currentValue = defaultValue;
            SaveToPlayerPrefs(defaultValue);
        }
        else
        {
            // If the key exists, load the value from PlayerPrefs
            currentValue = LoadFromPlayerPrefs(key);
        }
    }

    public T Value
    {
        get
        {
            return currentValue;
        }

        set
        {
            currentValue = value; // Update the backing field for Inspector display
            SaveToPlayerPrefs(value); // Save the value to PlayerPrefs
        }
    }

    private void SaveToPlayerPrefs(T value)
    {
        if (typeof(T) == typeof(float))
            PlayerPrefs.SetFloat(key, (float)(object)value);
        else if (typeof(T) == typeof(int))
            PlayerPrefs.SetInt(key, (int)(object)value);
        else if (typeof(T) == typeof(string))
            PlayerPrefs.SetString(key, (string)(object)value);
        else if (typeof(T) == typeof(bool))
            PlayerPrefs.SetInt(key, (bool)(object)value ? 1 : 0);
        else
        {
            // Handle unsupported types by serializing to JSON
            try
            {
                string json = JsonUtility.ToJson(value);
                PlayerPrefs.SetString(key, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to serialize value to JSON for key '{key}': {e.Message}");
            }
        }

        PlayerPrefs.Save();
    }

    private T LoadFromPlayerPrefs(String key)
    {
        if (typeof(T) == typeof(float))
            return (T)(object)PlayerPrefs.GetFloat(key, (float)(object)defaultValue);
        if (typeof(T) == typeof(int))
            return (T)(object)PlayerPrefs.GetInt(key, (int)(object)defaultValue);
        if (typeof(T) == typeof(string))
            return (T)(object)PlayerPrefs.GetString(key, (string)(object)defaultValue);
        if (typeof(T) == typeof(bool))
            return (T)(object)(PlayerPrefs.GetInt(key, ((bool)(object)defaultValue) ? 1 : 0) == 1);

        // Handle unsupported types by deserializing JSON
        string json = PlayerPrefs.GetString(key, null);
        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to deserialize JSON for key '{key}': {e.Message}");
            }
        }

        return defaultValue;
    }

    public static implicit operator T(SavedProperty<T> prop) => prop.Value;
}