using System;
using UnityEditor;
using UnityEngine;

namespace Rubickanov.Logger.Editor
{
    [CustomEditor(typeof(Logger))]
    public class LoggerEditor : UnityEditor.Editor
    {
        private bool showEditorLogPreview = true;
        private bool showFileLogPreview = true;
        private DateTime logTime;
        private GUIStyle headerStyle;
        private SerializedProperty prefixColorProperty;
        private SerializedProperty prefixProperty;
        private SerializedProperty showLogsProperty;
        private SerializedProperty logLevelFilterProperty;
        private SerializedProperty logFilePathProperty;
        private SerializedProperty defaultPathProperty;

        private void OnEnable()
        {
            logTime = DateTime.Now;
            headerStyle = new GUIStyle()
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(0, 0, 10, 10),
                normal = { textColor = new Color(0.56f, 0.56f, 0.56f) }
            };

            prefixColorProperty = serializedObject.FindProperty("prefixColor");
            prefixProperty = serializedObject.FindProperty("prefix");
            showLogsProperty = serializedObject.FindProperty("showLogs");
            logLevelFilterProperty = serializedObject.FindProperty("logLevelFilter");
            logFilePathProperty = serializedObject.FindProperty("logFilePath");
            defaultPathProperty = serializedObject.FindProperty("DEFAULT_PATH");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            Color prefixColor = prefixColorProperty.colorValue;

            float brightness = 0.299f * prefixColor.r + 0.587f * prefixColor.g + 0.114f * prefixColor.b;
            if (brightness > 0.5f)
            {
                headerStyle.normal.textColor = Color.black;
            }
            else
            {
                headerStyle.normal.textColor = Color.white;
            }

            Rect rect = EditorGUILayout.GetControlRect(false, 30);
            EditorGUI.DrawRect(rect, prefixColor);

            string name = prefixProperty.stringValue;

            EditorGUI.LabelField(rect, $"{name} Settings", headerStyle);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(showLogsProperty, new GUIContent("Show Logs"));
            EditorGUILayout.PropertyField(prefixProperty, new GUIContent("Prefix"));
            EditorGUILayout.PropertyField(prefixColorProperty, new GUIContent("Prefix Color"));
            EditorGUILayout.PropertyField(logLevelFilterProperty, new GUIContent("Log Level Filter"));
            EditorGUILayout.PropertyField(logFilePathProperty, new GUIContent("Log File Path"));

            EditorGUILayout.Space();

            if (GUILayout.Button(new GUIContent("Auto Prefix", "Click to automatically name the Logger prefix")))
            {
                prefixProperty.stringValue = serializedObject.targetObject.name;
            }

            if (GUILayout.Button(new GUIContent("Set Default Path", "Click to set the default path for the log file")))
            {
                logFilePathProperty.stringValue = defaultPathProperty.stringValue;
            }

            EditorGUILayout.Space(20);

            string prefix = prefixProperty.stringValue;
            string hexColor = "#" + ColorUtility.ToHtmlStringRGB(prefixColor);
            EditorGUILayout.LabelField("Logs Preview", new GUIStyle()
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(0, 0, 10, 10),
                normal = { textColor = new Color(0.56f, 0.56f, 0.56f) }
            });

            showEditorLogPreview = EditorGUILayout.Foldout(showEditorLogPreview, "Editor Log Preview");
            if (showEditorLogPreview)
            {
                foreach (LogLevel logType in Enum.GetValues(typeof(LogLevel)))
                {
                    string logTypeColor = ((Logger)target).GetLogTypeColor(logType);
                    EditorGUILayout.LabelField(
                        $"<color={logTypeColor}>[{logType}]</color> <color={hexColor}>[{prefix}] </color>  [SenderName]: This is a {logType} message",
                        new GUIStyle()
                        {
                            fontSize = 14,
                            fontStyle = FontStyle.Normal,
                            alignment = TextAnchor.MiddleLeft,
                            margin = new RectOffset(0, 0, 10, 10),
                            normal = { textColor = new Color(0.56f, 0.56f, 0.56f) },
                            richText = true
                        });
                }
            }

            EditorGUILayout.Space(10);

            showFileLogPreview = EditorGUILayout.Foldout(showFileLogPreview, "File Log Preview");
            if (showFileLogPreview)
            {
                foreach (LogLevel logType in Enum.GetValues(typeof(LogLevel)))
                {
                    EditorGUILayout.LabelField(
                        $"{logTime} [{logType}] [{prefix}] [SenderName]: This is a {logType} message",
                        new GUIStyle()
                        {
                            fontSize = 14,
                            fontStyle = FontStyle.Normal,
                            alignment = TextAnchor.MiddleLeft,
                            margin = new RectOffset(0, 0, 10, 10),
                            normal = { textColor = new Color(0.56f, 0.56f, 0.56f) }
                        });
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}