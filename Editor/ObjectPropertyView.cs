using UnityEditor;
using UnityEngine;

namespace UnityExtended
{
    public abstract partial class ExtendedEditor
    {
        /// <summary>
        /// Class for displaying properties of serialized object fields
        /// </summary>
        private class ObjectPropertyView
        {
            private GUIContent label;
            private SerializedProperty serializedProperty;
            private Editor editor;
            private bool foldout;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="property">Property of serialized object</param>
            /// <param name="label">Label of the field</param>
            public ObjectPropertyView(SerializedProperty property, GUIContent label = null)
            {
                this.label = (label != null) ? label : new GUIContent(property.name);
                this.serializedProperty = property;
                this.foldout = false;
                UpdateEditor();
            }

            /// <summary>
            /// Object property display method
            /// </summary>
            public void Draw()
            {
                if (serializedProperty.propertyType != SerializedPropertyType.ObjectReference)
                {
                    // If field is not reference, show error message.            
                    var style = new GUIStyle(EditorStyles.objectField);
                    style.normal.textColor = Color.red;

                    // Display label with error message
                    EditorGUILayout.LabelField(label, new GUIContent($"{serializedProperty.propertyType} is not a reference type"), style);
                    return;
                }

                // We draw a spoiler
                foldout = EditorGUILayout.Foldout(foldout, new GUIContent());

                EditorGUI.BeginChangeCheck();
                // Draw the field of the object
                EditorGUI.ObjectField(GUILayoutUtility.GetLastRect(), serializedProperty, label);

                // If there are changes
                if (EditorGUI.EndChangeCheck() || (serializedProperty.objectReferenceValue ^ editor))
                {
                    // Update the object editor
                    UpdateEditor();
                }

                if (foldout)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    // Display the properties of object or display an error if they are not there
                    if (editor != null)
                    {
                        editor.OnInspectorGUI();
                    }
                    else
                    {
                        ExtendedEditorGUI.DisplayMessage($"The {serializedProperty.displayName} field must not be empty!", MessageType.Error);
                    }
                    GUILayout.EndVertical();
                }
            }

            /// <summary>
            /// Update the object editor
            /// </summary>
            private void UpdateEditor()
            {
                if (serializedProperty.propertyType == SerializedPropertyType.ObjectReference)
                {
                    editor = Editor.CreateEditor(serializedProperty.objectReferenceValue);
                }
            }
        }
    }
}