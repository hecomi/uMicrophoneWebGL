using UnityEditor;

namespace uMicrophoneWebGL
{

[CustomEditor(typeof(MicrophoneWebGL))]
public class MicrophoneWebGLEditor : Editor
{
    MicrophoneWebGL mic => target as MicrophoneWebGL;
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (EditorUtil.Foldout("Microphone", true))
        {
            ++EditorGUI.indentLevel;
            DrawMicrophone();
            EditorGUILayout.Separator();
            --EditorGUI.indentLevel;
        }

        if (EditorUtil.Foldout("Events", false))
        {
            ++EditorGUI.indentLevel;
            DrawEvents();
            EditorGUILayout.Separator();
            --EditorGUI.indentLevel;
        }

        serializedObject.ApplyModifiedProperties();
    }
    
    private void DrawMicrophone()
    {
        EditorUtil.DrawProperty(serializedObject, "isAutoStart");
        
        DrawMicrophoneDevicePopup();
        
        EditorGUILayout.Space();
    }
    
    private void DrawMicrophoneDevicePopup()
    {
        var n = Lib.GetDeviceCount();
        var devices = new string[n];
        for (int i = 0; i < n; ++i)
        {
            devices[i] = $"{Lib.GetLabel(i)} ({i})";
        }
        mic.micIndex = EditorGUILayout.Popup("Device", mic.micIndex, devices);
    }
    
    private void DrawEvents()
    {
        EditorGUILayout.Space();
        EditorUtil.DrawProperty(serializedObject, "readyEvent");
        EditorGUILayout.Space();
        EditorUtil.DrawProperty(serializedObject, "startEvent");
        EditorGUILayout.Space();
        EditorUtil.DrawProperty(serializedObject, "stopEvent");
        EditorGUILayout.Space();
        EditorUtil.DrawProperty(serializedObject, "dataEvent");
        EditorGUILayout.Space();
        EditorUtil.DrawProperty(serializedObject, "deviceListEvent");
        EditorGUILayout.Space();
    }
}

}