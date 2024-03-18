using UnityEditor;
using UnityEngine;

namespace Rubickanov.Logger.Editor
{
    [CustomEditor(typeof(LoggerDisplay))]
    public class LoggerDisplayEditor : UnityEditor.Editor
    {
        private SerializedProperty fontSizeProperty;
        private SerializedProperty maxLinesProperty;
        private SerializedProperty logLifetimeProperty;
        private SerializedProperty addIndexPrefixProperty;
        
        private GUIStyle headerStyle;

        private void OnEnable()
        {
            fontSizeProperty = serializedObject.FindProperty("fontSize");
            maxLinesProperty = serializedObject.FindProperty("maxLines");
            logLifetimeProperty = serializedObject.FindProperty("logLifetime");
            addIndexPrefixProperty = serializedObject.FindProperty("addIndexPrefix");
            
            headerStyle = new GUIStyle()
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
            
            Rect rect = EditorGUILayout.GetControlRect(false, 30);
            EditorGUI.DrawRect(rect, new Color(0,0,0,1));

            string name = "Logger Display";

            EditorGUI.LabelField(rect, $"{name}", headerStyle);
            
            EditorGUILayout.PropertyField(fontSizeProperty);
            EditorGUILayout.PropertyField(maxLinesProperty);
            EditorGUILayout.PropertyField(logLifetimeProperty);
            EditorGUILayout.PropertyField(addIndexPrefixProperty);

            serializedObject.ApplyModifiedProperties();
        }
    }
}