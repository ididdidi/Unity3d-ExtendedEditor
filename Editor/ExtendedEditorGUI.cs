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

        public static string SearchField(Rect position, string text)
        {
            var buttonSize = EditorGUIUtility.singleLineHeight;
            Rect textRect = position;
            textRect.width -= buttonSize / 2;
            text = GUI.TextField(textRect, text, EditorStyles.toolbarSearchField);
            Rect buttonRect = position;
            buttonRect.x += position.width - buttonSize / 2;
            buttonRect.width = buttonSize;
            if (!string.IsNullOrEmpty(text) && CancelButton(buttonRect))
            {
                text = "";
                GUIUtility.keyboardControl = 0;
            }
            return text;
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