using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rubickanov.Logger.Editor
{
    [CustomEditor(typeof(CFLogger))]
    public class CFLoggerEditor : UnityEditor.Editor
    {
        private SerializedProperty showLogsProperty;
        private SerializedProperty prefixProperty;
        private SerializedProperty prefixColorProperty;
        private SerializedProperty logLevelFilterProperty;
        private SerializedProperty logFormatProperty;

        private SerializedProperty screenLogsEnabledProperty;
        private SerializedProperty fileLogsEnabledProperty;

        private string putLoggerDisplayWarn =
            "Settings for Screen Logs are in LoggerDisplay component.\n Please be sure you added LoggerDisplay in the scene.";

        private SerializedProperty defaultPathProperty;
        private SerializedProperty logFilePathProperty;

        // Editor Variables
        private bool showEditorLogPreview = true;
        private bool showFileLogPreview = true;
        private bool showScreenLogPreview = true;

        private GUIStyle headerStyle;

        private void OnEnable()
        {
            headerStyle = CreateHeaderStyle();

            showLogsProperty = serializedObject.FindProperty("showLogs");
            prefixProperty = serializedObject.FindProperty("prefix");
            prefixColorProperty = serializedObject.FindProperty("prefixColor");
            logLevelFilterProperty = serializedObject.FindProperty("logLevelFilter");
            logFormatProperty = serializedObject.FindProperty("logFormat");

            screenLogsEnabledProperty = serializedObject.FindProperty("screenLogsEnabled");
            fileLogsEnabledProperty = serializedObject.FindProperty("fileLogsEnabled");

            defaultPathProperty = serializedObject.FindProperty("DEFAULT_PATH");
            logFilePathProperty = serializedObject.FindProperty("logFilePath");
        }

        private GUIStyle CreateHeaderStyle()
        {
            return new GUIStyle()
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(0, 0, 10, 10),
                normal = { textColor = new Color(0.56f, 0.56f, 0.56f) }
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            UpdateHeaderStyleColor();

            DrawHeader();

            DrawMainSettings();

            DrawScreenSettings();

            DrawFileSettings();

            DrawLogsPreview();

            serializedObject.ApplyModifiedProperties();
        }

        private void UpdateHeaderStyleColor()
        {
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
        }

        private new void DrawHeader()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 30);
            EditorGUI.DrawRect(rect, prefixColorProperty.colorValue);

            string name = prefixProperty.stringValue;

            EditorGUI.LabelField(rect, $"{name} Settings", headerStyle);
        }

        private void DrawMainSettings()
        {
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
        }

        private void DrawScreenSettings()
        {
            string screenLogsLabel =
                screenLogsEnabledProperty.boolValue ? "Screen Logs Enabled" : "Screen Logs Disabled";
            EditorGUILayout.PropertyField(screenLogsEnabledProperty, new GUIContent(screenLogsLabel));
            if (screenLogsEnabledProperty.boolValue)
            {
                EditorGUILayout.LabelField(
                    putLoggerDisplayWarn,
                    new GUIStyle()
                    {
                        fontSize = 14,
                        fontStyle = FontStyle.Normal,
                        alignment = TextAnchor.MiddleLeft,
                        margin = new RectOffset(0, 0, 10, 10),
                        normal = { textColor = new Color(0.56f, 0.56f, 0.56f) },
                        wordWrap = true
                    },
                    GUILayout.MaxWidth(EditorGUIUtility.currentViewWidth));
            }

            EditorGUILayout.Space(15);
        }

        private void DrawFileSettings()
        {
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
        }

        private void DrawLogsPreview()
        {
            EditorGUILayout.LabelField("Logs Preview", new GUIStyle()
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(0, 0, 10, 10),
                normal = { textColor = new Color(0.56f, 0.56f, 0.56f) }
            });

            DrawLogPreview(ref showEditorLogPreview, "Editor Log Preview", ((CFLogger)target).LogFormat.ConsoleFormat,
                true);
            if (screenLogsEnabledProperty.boolValue)
            {
                DrawLogPreview(ref showScreenLogPreview, "Screen Log Preview", ((CFLogger)target).LogFormat.ScreenFormat,
                    true);
            }

            if (fileLogsEnabledProperty.boolValue)
            {
                DrawLogPreview(ref showFileLogPreview, "File Log Preview", ((CFLogger)target).LogFormat.FileFormat,
                    false);
            }
        }

        private void DrawLogPreview(ref bool showLogPreview, string previewLabel, List<LogPart> logFormat,
            bool richText = false)
        {
            showLogPreview = EditorGUILayout.Foldout(showLogPreview, previewLabel);
            if (showLogPreview)
            {
                foreach (LogLevel logType in Enum.GetValues(typeof(LogLevel)))
                {
                    string generatedMessage =
                        ((CFLogger)target).GenerateLogPreview(logType, logFormat, richText);
                    EditorGUILayout.LabelField(
                        generatedMessage,
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
        }
    }
}