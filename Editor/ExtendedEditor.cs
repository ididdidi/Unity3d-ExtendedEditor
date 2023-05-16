using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityExtended
{
    /// <summary>
    /// Class for extending the capabilities of the Unity3d editor
    /// </summary>
    public abstract partial class ExtendedEditor : Editor
    {
        // List of prepared object property editors
        private List<ObjectProperty> properties = new List<ObjectProperty>();

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
            properties.Add(new ObjectProperty(serializedProperty, label));
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
    }
}