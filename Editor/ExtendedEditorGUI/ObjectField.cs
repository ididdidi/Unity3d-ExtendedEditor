using UnityEditor;
using UnityEngine;

/// <summary>
/// Class that complements the capabilities of the editor GUI
/// </summary>
public partial class ExtendedEditorGUI
{
    /// <summary>
    /// Display a field for adding a reference to an object
    /// </summary>
    /// <param name="position"><see cref="Rect"/> fields to activate validation</param>
    /// <param name="property">Serializedproperty the object</param>
    /// <param name="requiredType">Required <see cref="System.Type"/> of reference being checked</param>
    /// <param name="label">Displaу field label</param>
    private void ObjectField(Rect position, SerializedProperty property, System.Type requiredType, GUIContent label)
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
    /// <param name="requiredType">Object field</param>
    /// <param name="label">Displaу field label</param>
    public Object ObjectField(Rect position, Object field, System.Type requiredType, string label = null, System.Predicate<Object> predicate = null)
    {
        // If the reference is not null and points to an object that does match the type
        ChecValues(field, requiredType);

        // Make sure the objects being moved are of the right type
        ChecDragAndDrops(position, requiredType);

        // Start change checks
        EditorGUI.BeginChangeCheck();
        // Display a ObjectField
        if (!string.IsNullOrEmpty(label)) { field = EditorGUI.ObjectField(position, label, field, typeof(object), true); }
        else { field = EditorGUI.ObjectField(position, field, typeof(object), true); }

        // If changes were made to the contents of the field and a GameObject was added to the field
        if (EditorGUI.EndChangeCheck() && field is GameObject gameObject)
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

        return null;
    }

    /// <summary>
    /// Checks the validity of the dragged objects
    /// </summary>
    /// <param name="position"><see cref="Rect"/> fields to activate validation</param>
    /// <param name="requiredType">Required <see cref="System.Type"/> of reference being checked</param>
    private void ChecDragAndDrops(Rect position, System.Type requiredType)
    {
        // If the cursor is in the area of the rendered field
        if (position.Contains(Event.current.mousePosition))
        {
            // Iterate over all draggable references
            foreach (var @object in DragAndDrop.objectReferences)
            {
                // If we do not find the required type
                if (!IsValidObject(@object, requiredType))
                {
                    // Disable drag and drop
                    DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Checks if an reference matches the required type
    /// </summary>
    /// <param name="object">Checked reference</param>
    /// <param name="requiredType">Required <see cref="System.Type"/> of reference being checked</param>
    /// <returns></returns>
    private bool IsValidObject(Object @object, System.Type requiredType)
    {
        // If the object is a GameObject
        if (@object is GameObject go)
        {
            // Check if it has a component of the required type and return result
            return go.GetComponent(requiredType) != null;
        }

        // Check the reference itself for compliance with the required type
        return requiredType.IsAssignableFrom(@object.GetType());
    }

    /// <summary>
    /// Checks a previously added object for compliance
    /// </summary>
    /// <param name="object">Checked object</param>
    /// <param name="requiredType">Required <see cref="System.Type"/> of reference being checked</param>
    /// <returns></returns>
    private Object ChecValues(Object @object, System.Type requiredType) => @object != null && IsValidObject(@object, requiredType) ? @object : null;
}
