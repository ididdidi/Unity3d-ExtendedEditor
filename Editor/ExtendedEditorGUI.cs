using System.Reflection;
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
        /// Drawing the button in the center of the layout
        /// </summary>
        /// <param name="label">Button label</param>
        /// <param name="width">Button width</param>
        /// <param name="height">Button height</param>
        /// <returns>Was the button pressed</returns>
        public static bool CenterButton(string label, float width = 240f, float height = 24f)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            var result = GUILayout.Button(label, GUILayout.Width(width), GUILayout.Height(height));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            return result;
        }

        /// <summary>
        /// Simple button with a cross
        /// </summary>
        /// <param name="rect"><see cref="Rect"/></param>
        /// <param name="tooltip">Tooltip on hover</param>
        /// <returns>Button is pressed</returns>
        public static bool CancelButton(Rect rect, string tooltip = null)
        {
            GUIContent iconButton = EditorGUIUtility.TrIconContent("Toolbar Minus", tooltip);
            if (GUI.Button(rect, iconButton, "SearchCancelButton")) { return true; }
            return false;
        }

        /// <summary>
        /// Displays the search bar
        /// </summary>
        /// <param name="position">Field position</param>
        /// <param name="keyword">Search keyword</param>
        /// <returns>ControlId</returns>
        public static int SearchField(Rect position, ref string keyword)
        {
            Event current = Event.current;

            var buttonSize = EditorGUIUtility.singleLineHeight;
            Rect rect = position;
            rect.width -= buttonSize / 2;

            keyword = GUI.TextField(rect, keyword, EditorStyles.toolbarSearchField);
            int controlId = GetLastControlId();

            rect.x += rect.width;
            rect.width = buttonSize;
            // Clear search keyword
            if (keyword != string.Empty && CancelButton(rect) || (current.type == EventType.KeyDown && current.keyCode == KeyCode.Escape))
            {
                keyword = string.Empty;
                GUIUtility.keyboardControl = 0;
            }
            // Unfocus search field
            else if (current.type == EventType.MouseUp)
            {
                if (controlId != GUIUtility.hotControl && controlId == GUIUtility.keyboardControl)
                {
                    GUIUtility.keyboardControl = 0;
                    EditorGUIUtility.editingTextField = false;
                    current.Use();
                }
            }

            return controlId;
        }

        /// <summary>
        /// Return last control ID setted in GUI
        /// </summary>
        /// <returns>Last control ID setted</returns>
        public static int GetLastControlId()
        {
            FieldInfo getLastControlId = typeof(EditorGUIUtility).GetField("s_LastControlID", BindingFlags.Static | BindingFlags.NonPublic);
            if (getLastControlId != null)
                return (int)getLastControlId.GetValue(null);
            return 0;
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