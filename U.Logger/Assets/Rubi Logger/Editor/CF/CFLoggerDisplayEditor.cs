using UnityEditor;
using UnityEngine;

namespace Rubickanov.Logger.Editor
{
    [CustomEditor(typeof(CFLoggerDisplay))]
    public class CFLoggerDisplayEditor : UnityEditor.Editor
    {
        private SerializedProperty fontSizeProperty;
        private SerializedProperty maxLinesProperty;
        private SerializedProperty logLifetimeProperty;

        private GUIStyle headerStyle;

        private void OnEnable()
        {
            fontSizeProperty = serializedObject.FindProperty("fontSize");
            maxLinesProperty = serializedObject.FindProperty("maxLines");
            logLifetimeProperty = serializedObject.FindProperty("logLifetime");

            headerStyle = new GUIStyle()
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(0, 0, 10, 10),
                normal = { textColor = new Color(0.6784313725490196f, 1f, 0.054901960784313725f) }
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            Rect rect = EditorGUILayout.GetControlRect(false, 30);
            EditorGUI.DrawRect(rect, new Color(0.10196078431372549f, 0.10196078431372549f, 00.10196078431372549f));

            string name = "CF Logger Display";

            EditorGUI.LabelField(rect, $"{name}", headerStyle);

            EditorGUILayout.PropertyField(fontSizeProperty);
            EditorGUILayout.PropertyField(maxLinesProperty);
            EditorGUILayout.PropertyField(logLifetimeProperty);

            serializedObject.ApplyModifiedProperties();
        }
    }
}