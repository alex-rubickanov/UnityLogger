using UnityEditor;
using UnityEngine;
using Logger = Rubickanov.Logger.Logger;
using LogType = Rubickanov.Logger.LogType;

[CustomEditor(typeof(Logger))]
public class LoggerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button(new GUIContent("Auto Name", "Click to automatically name the Logger object")))
        {
            serializedObject.FindProperty("prefix").stringValue = serializedObject.targetObject.name;
            serializedObject.ApplyModifiedProperties();
        }

        if (GUILayout.Button(new GUIContent("Test Log", "Click to test the logger")))
        {
            Logger logger = (Logger)target;
            logger.Log(LogType.Info, "Test Log", logger, true);
        }

        if (GUILayout.Button(new GUIContent("Set Default Path", "Click to set the default path for the log file")))
        {
            serializedObject.FindProperty("logFilePath").stringValue = serializedObject.FindProperty("DEFAULT_PATH").stringValue;
            serializedObject.ApplyModifiedProperties();
        }
    }
}