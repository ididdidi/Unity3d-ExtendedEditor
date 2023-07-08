using UnityEngine;
using UnityEditor;

namespace UnityExtended
{
    /// <summary>
    /// Drawer for the RequireInterface attribute
    /// </summary>
    [CustomPropertyDrawer(typeof(RequireTypeAttribute))]
    public class RequireTypeDrawer : PropertyDrawer
    {
        /// <summary>
        /// Overrides GUI drawing for the attribute
        /// </summary>
        /// <param name="position"><see cref="Rect"/> fields to activate validation</param>
        /// <param name="property">Serializedproperty the object</param>
        /// <param name="label">Displaу field label</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Check if this is reference type property
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                // If field is not reference, show error message.            
                var style = new GUIStyle(EditorStyles.objectField);
                style.normal.textColor = Color.red;

                // Display label with error message
                EditorGUI.LabelField(position, label, new GUIContent($"{ property.propertyType } is not a reference type"), style);
                return;
            }

            var requiredType = ((RequireTypeAttribute)attribute).RequiredType;
            ExtendedEditor.ObjectField(position, property, requiredType, new GUIContent(label));
        }
    }
}
