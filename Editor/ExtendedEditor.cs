using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityExtended
{
    /// <summary>
    /// Class for extending the capabilities of the Unity3d editor
    /// </summary>
    public abstract class ExtendedEditor : Editor
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
                        ExtendedEditor.DisplayMessage($"The {serializedProperty.displayName} field must not be empty!", MessageType.Error);
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

        // List of prepared object property editors
        private List<ObjectPropertyView> properties = new List<ObjectPropertyView>();

        /// <summary>
        /// Make a custom inspector
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Display default Inspector 
            DrawDefaultInspector();

            // Display ptoperties on Inspector
            DrawProperties();

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Method for adding displayable serialized object properties
        /// </summary>
        /// <param name="serializedProperty">Properties to display</param>
        /// <param name="label">Label of the field</param>
        protected void AddPropertyView(SerializedProperty serializedProperty, GUIContent label = null)
        {
            if (target.GetType().ToString().Equals(serializedProperty.type.TrimStart("PPtr <$".ToCharArray()).TrimEnd('>')))
            {
                throw new System.ArgumentException("The type of the field must be different from the type of the parent object, otherwise recursion occurs.");
            }
            properties.Add(new ObjectPropertyView(serializedProperty, label));
        }

        /// <summary>
        /// Method for adding displayable serialized object properties
        /// </summary>
        /// <param name="serializedProperty">Properties to display</param>
        /// <param name="label">Label of the field</param>
        protected void AddPropertyView(SerializedProperty serializedProperty, string label) => AddPropertyView(serializedProperty, new GUIContent(label));

        /// <summary>
        /// Display the properties of objects added to the list
        /// </summary>
        protected void DrawProperties()
        {
            for (int i = 0; i < properties.Count; i++)
            {
                properties[i].Draw();
            }
        }

        /// <summary>
        /// Static method to display message in Unity3d inspector
        /// </summary>
        /// <param name="message">Message text as <see cref="string"/></param>
        /// <param name="messageType">Message type determines the icon before the message(None, Information, Warning, Error)</param>
        public static void DisplayMessage(string message, MessageType messageType = MessageType.None)
        {
            // Create content from the icon and text of the message
            GUIContent label = new GUIContent(message);
            switch (messageType)
            {
                case MessageType.Info: { label.image = EditorGUIUtility.Load("icons/console.infoicon.png") as Texture2D; break; }
                case MessageType.Warning: { label.image = EditorGUIUtility.Load("icons/console.warnicon.png") as Texture2D; break; }
                case MessageType.Error: { label.image = EditorGUIUtility.Load("icons/console.erroricon.png") as Texture2D; break; }
            }

            // Define the message display style
            var style = new GUIStyle();
            style.wordWrap = true;
            style.normal.textColor = GUI.skin.label.normal.textColor;

            // Display message
            EditorGUILayout.LabelField(label, style);
        }
    }
}