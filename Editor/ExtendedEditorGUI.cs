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
        /// Provides a new rect on top of the existing one.
        /// </summary>
		/// <param name="rect">Base <see cref="Rect"/></param>
        /// <param name="size">Dimensions of the new rectangle</param>
        /// <param name="dX">X offset</param>
        /// <param name="dY">Y offset</param>
        /// <returns></returns>
        public static Rect GetNewRect(Rect rect, Vector2 size, Vector2 padding, float dX = 0f, float dY = 0f)
        {
            return new Rect(new Vector2(rect.x + dX + padding.x, rect.y + dY + padding.y), new Vector2(size.x - padding.x * 2f, size.y - padding.y * 2f));
        }

        /// <summary>
        /// Simple button with a cross
        /// </summary>
        /// <param name="rect"><see cref="Rect"/></param>
        /// <param name="tooltip">Tooltip on hover</param>
        /// <returns></returns>
        public static bool CancelButton(Rect rect, string tooltip = null)
        {
            GUIContent iconButton = EditorGUIUtility.TrIconContent("Toolbar Minus", tooltip);
            if (GUI.Button(rect, iconButton, "SearchCancelButton")) { return true; }
            return false;
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
            foreach(var type in requiredTypes)
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