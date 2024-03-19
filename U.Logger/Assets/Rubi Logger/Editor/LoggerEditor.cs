using System;
using UnityEditor;
using UnityEngine;

namespace Rubickanov.Logger.Editor
{
    [CustomEditor(typeof(Logger))]
    public class LoggerEditor : UnityEditor.Editor
    {
        private SerializedProperty showLogsProperty;
        private SerializedProperty prefixProperty;
        private SerializedProperty prefixColorProperty;
        private SerializedProperty logLevelFilterProperty;
        private SerializedProperty logFormatProperty;

        private SerializedProperty screenLogsEnabledProperty;
        private SerializedProperty fileLogsEnabledProperty;

        private SerializedProperty fontSizeProperty;
        private SerializedProperty maxLinesProperty;
        private SerializedProperty logLifetimeProperty;

        private SerializedProperty defaultPathProperty;
        private SerializedProperty logFilePathProperty;

        // Editor Variables
        private bool showEditorLogPreview = true;
        private bool showFileLogPreview = true;
        private DateTime logTime;
        private GUIStyle headerStyle;

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

            showLogsProperty = serializedObject.FindProperty("showLogs");
            prefixProperty = serializedObject.FindProperty("prefix");
            prefixColorProperty = serializedObject.FindProperty("prefixColor");
            logLevelFilterProperty = serializedObject.FindProperty("logLevelFilter");
            logFormatProperty = serializedObject.FindProperty("logFormat");

            screenLogsEnabledProperty = serializedObject.FindProperty("screenLogsEnabled");
            fileLogsEnabledProperty = serializedObject.FindProperty("fileLogsEnabled");

            fontSizeProperty = serializedObject.FindProperty("fontSize");
            maxLinesProperty = serializedObject.FindProperty("maxLines");
            logLifetimeProperty = serializedObject.FindProperty("logLifetime");

            defaultPathProperty = serializedObject.FindProperty("DEFAULT_PATH");
            logFilePathProperty = serializedObject.FindProperty("logFilePath");
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

            // Main Settings

            EditorGUILayout.LabelField("Main Settings", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(showLogsProperty, new GUIContent("Show Logs"));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(prefixProperty, new GUIContent("Prefix"), GUILayout.ExpandWidth(true));
            if (GUILayout.Button(new GUIContent("Auto Prefix", "Click to automatically name the Logger prefix"),
                    GUILayout.Width(100)))
            {
                prefixProperty.stringValue = serializedObject.targetObject.name;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(prefixColorProperty, new GUIContent("Prefix Color"));
            EditorGUILayout.PropertyField(logLevelFilterProperty, new GUIContent("Log Level Filter"));
            EditorGUILayout.PropertyField(logFormatProperty, new GUIContent("Log Format"));
            EditorGUILayout.Space(15);

            // Screen Settings

            string screenLogsLabel =
                screenLogsEnabledProperty.boolValue ? "Screen Logs Enabled" : "Screen Logs Disabled";
            EditorGUILayout.PropertyField(screenLogsEnabledProperty, new GUIContent(screenLogsLabel));
            if (screenLogsEnabledProperty.boolValue)
            {
                EditorGUILayout.LabelField("Screen Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(fontSizeProperty, new GUIContent("Font Size"));
                EditorGUILayout.PropertyField(maxLinesProperty, new GUIContent("Max Lines"));
                EditorGUILayout.PropertyField(logLifetimeProperty, new GUIContent("Log Lifetime"));
            }

            EditorGUILayout.Space(15);

            // File Settings
            string fileLogsLabel = screenLogsEnabledProperty.boolValue ? "File Logs Enabled" : "File Logs Disabled";
            EditorGUILayout.PropertyField(fileLogsEnabledProperty, new GUIContent(fileLogsLabel));
            if (fileLogsEnabledProperty.boolValue)
            {
                EditorGUILayout.LabelField("File Settings", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(logFilePathProperty, new GUIContent("Log File Path"));
                if (GUILayout.Button(new GUIContent("Set Default Path",
                        "Click to set the default path for the log file.\nYou can change default path in the Logger script.")))
                {
                    logFilePathProperty.stringValue = defaultPathProperty.stringValue;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(15);


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
                        $"<color={logTypeColor}>[{logType}]</color> <color={hexColor}>[{prefix}] </color> [SenderName]: This is a {logType} message",
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

            if (fileLogsEnabledProperty.boolValue)
            {
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
            }


            serializedObject.ApplyModifiedProperties();
        }
    }
}