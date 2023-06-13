using UnityEditor;
using UnityEngine;

namespace UnityExtended
{
    /// <summary>
    /// Class that complements the capabilities of the editor GUI
    /// </summary>
    public static partial class ExtendedEditorGUI
    {
        /// <summary>
        /// Display a field for adding a reference to an object
        /// </summary>
        /// <param name="position"><see cref="Rect"/> fields to activate validation</param>
        /// <param name="property">Serializedproperty the object</param>
        /// <param name="requiredType">Required <see cref="System.Type"/> of reference being checked</param>
        /// <param name="label">Displaу field label</param>
        public static void ObjectField(Rect position, SerializedProperty property, System.Type requiredType, GUIContent label = null)
        {
            // If the reference is not null and points to an object that does match the type
            ChecValues(property.objectReferenceValue, requiredType);

            // Make sure the objects being moved are of the right type
            ChecDragAndDrops(position, requiredType);

            // Start change checks
            EditorGUI.BeginChangeCheck();
            // Display a ObjectField
            EditorGUI.ObjectField(position, property, label);
            // If changes were made to the contents of the field and a GameObject was added to the field
            if (EditorGUI.EndChangeCheck() && property.objectReferenceValue is GameObject @object)
            {
                // Get component of the required type on the object and save a reference to it in a property
                property.objectReferenceValue = @object.GetComponent(requiredType);
            }
        }

        /// <summary>
        /// Display a field for adding a reference to an object
        /// </summary>
        /// <param name="position"><see cref="Rect"/> fields to activate validation</param>
        /// <param name="property">Serializedproperty the object</param>
        /// <param name="requiredType">Required <see cref="System.Type"/> of reference being checked</param>
        /// <param name="label">Displaу field label</param>
        public static void ObjectField(Rect position, SerializedProperty property, System.Type requiredType, string label)
            => ObjectField(position, property, requiredType, new GUIContent(label));

        /// <summary>
        /// Display a field for adding a reference to an object
        /// </summary>
        /// <param name="position"><see cref="Rect"/> fields to activate validation</param>
        /// <param name="requiredType">Object field</param>
        /// <param name="label">Displaу field label</param>
        public static Object ObjectField(Rect position, Object field, System.Type requiredType, GUIContent label = null, System.Predicate<Object> predicate = null)
        {
            // If the reference is not null and points to an object that does match the type
            var curent = ChecValues(field, requiredType);

            // Make sure the objects being moved are of the right type
            ChecDragAndDrops(position, requiredType);

            // Start change checks
            EditorGUI.BeginChangeCheck();
            // Display a ObjectField
            if (label != null) { field = EditorGUI.ObjectField(position, label, field, typeof(object), true); }
            else { field = EditorGUI.ObjectField(position, field, typeof(object), true); }

            // If changes were made to the contents of the field and a GameObject was added to the field
            if (EditorGUI.EndChangeCheck())
            {
                if(field is GameObject gameObject)
                {
                    // Get component of the required type on the object and save a reference to it in a property
                    foreach (var component in gameObject.GetComponents(requiredType))
                    {
                        if (predicate == null || predicate.Invoke(component)) { return component; }
                    }
                }
                else
                {
                    if (predicate != null && predicate.Invoke(field)) return field;
                }
            }
            return curent;
        }

        /// <summary>
        /// Display a field for adding a reference to an object
        /// </summary>
        /// <param name="position"><see cref="Rect"/> fields to activate validation</param>
        /// <param name="requiredType">Object field</param>
        /// <param name="label">Displaу field label</param>
        public static void ObjectField(Rect position, Object field, System.Type requiredType, string label)
            => ObjectField(position, field, requiredType, new GUIContent(label));
    }
}