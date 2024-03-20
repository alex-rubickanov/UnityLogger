using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Rubickanov.Logger;

namespace Rubickanov.Logger.Editor
{
    [CustomEditor(typeof(LogFormat))]
    public class LogFormatEditor : UnityEditor.Editor
    {
        ReorderableList consoleFormatList;
        ReorderableList screenFormatList;
        ReorderableList fileFormatList;
        
        GUIStyle headerStyle;

        private void OnEnable()
        {
            SerializedProperty consoleFormatProperty = serializedObject.FindProperty("consoleLogFormat");
            SerializedProperty screenFormatProperty = serializedObject.FindProperty("screenLogFormat");
            SerializedProperty fileFormatProperty = serializedObject.FindProperty("fileLogFormat");

            consoleFormatList = new ReorderableList(serializedObject, consoleFormatProperty);
            screenFormatList = new ReorderableList(serializedObject, screenFormatProperty);
            fileFormatList = new ReorderableList(serializedObject, fileFormatProperty);

            SetupReorderableList(consoleFormatList, "Console Format");
            SetupReorderableList(screenFormatList, "Screen Format");
            SetupReorderableList(fileFormatList, "File Format");
            
            headerStyle = new GUIStyle()
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(0, 0, 10, 10),
                normal = { textColor = new Color(0.8431373f, 0.7294118f, 0.4901961f) }
            };
        }

        private void SetupReorderableList(ReorderableList list, string listName)
        {
            list.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, listName);
            };

            list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            Rect rect = EditorGUILayout.GetControlRect(false, 30);
            EditorGUI.DrawRect(rect, new Color(0.1686275f, 0.1686275f,0.1686275f));

            EditorGUI.LabelField(rect, $"Log Format Settings", headerStyle);
            
            EditorGUILayout.Space(20);
            
            consoleFormatList.DoLayoutList();
            screenFormatList.DoLayoutList();
            fileFormatList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        private void CopyFormat(ReorderableList sourceList, ReorderableList targetList)
        {
            targetList.serializedProperty.arraySize = sourceList.serializedProperty.arraySize;

            for (int i = 0; i < sourceList.serializedProperty.arraySize; i++)
            {
                targetList.serializedProperty.GetArrayElementAtIndex(i).enumValueIndex = sourceList.serializedProperty.GetArrayElementAtIndex(i).enumValueIndex;
            }
        }
    }
}