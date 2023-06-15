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
        /// Checks the validity of the dragged objects
        /// </summary>
        /// <param name="position"><see cref="Rect"/> fields to activate validation</param>
        /// <param name="requiredType">Required <see cref="System.Type"/> of reference being checked</param>
        public static void ChecDragAndDrops(Rect position, params System.Type[] requiredTypes)
        {
            // If the cursor is in the area of the rendered field
            if (position.Contains(Event.current.mousePosition))
            {
                // Iterate over all draggable references
                foreach (var @object in DragAndDrop.objectReferences)
                {
                    // If we do not find the required type
                    if (!IsValidObject(@object, requiredTypes))
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
        private static bool IsValidObject(Object @object, params System.Type[] requiredTypes)
        {
            foreach (var type in requiredTypes)
            {
                // If the object is a GameObject
                if (@object is GameObject go)
                {
                    // Check if it has a component of the required type and return result
                    if (go.GetComponent(type) != null) { return true; }
                }

                // Check the reference itself for compliance with the required type
                if (type.IsAssignableFrom(@object.GetType())) { return true; }
            }
            return false;
        }

        /// <summary>
        /// Checks a previously added object for compliance
        /// </summary>
        /// <param name="object">Checked object</param>
        /// <param name="requiredType">Required <see cref="System.Type"/> of reference being checked</param>
        /// <returns></returns>
        public static Object ChecValues(Object @object, params System.Type[] requiredTypes) => @object != null && IsValidObject(@object, requiredTypes) ? @object : null;
    }
}
