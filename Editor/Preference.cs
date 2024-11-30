using UnityEditor;

namespace uMicrophoneWebGL
{
    
internal class EditorPrefsStr
{
    public const string MicrophoneChannels = "uMicrophoneWebGL-MicrophoneChannels";
}

public static class Preference
{
    public static int microphoneChannels
    {
        get => EditorPrefs.GetInt(EditorPrefsStr.MicrophoneChannels, 1);
        set => EditorPrefs.SetInt(EditorPrefsStr.MicrophoneChannels, value);
    }
}

public class PreferenceProvider : SettingsProvider
{
    const string PreferencePath = "Preferences/uMicrophonWebGL";
    const int LabelWidth = 200;

    PreferenceProvider(string path, SettingsScope scopes) : base(path, scopes)
    {
    }

    public override void OnGUI(string searchContext)
    {
        EditorGUILayout.Separator();
        
        var defaultLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = LabelWidth;
        
        ++EditorGUI.indentLevel;
        
        {
            int current = Preference.microphoneChannels;
            int result = EditorGUILayout.IntField("Microphone Channels", current);
            if (current != result)
            {
                Preference.microphoneChannels = result;
            }
        }
        
        --EditorGUI.indentLevel;
        
        EditorGUIUtility.labelWidth = defaultLabelWidth;
    }

    [SettingsProvider]
    public static SettingsProvider CreateSettingProvider()
    {
        return new PreferenceProvider(PreferencePath, SettingsScope.User);
    }
}

}