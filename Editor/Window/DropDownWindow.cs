using UnityEngine;
using UnityEditor;

namespace UnityExtended
{
    /// <summary>
    /// Class for creating and displaying a drop-down editor window.
    /// </summary>
    public class DropDownWindow : EditorWindow, IContext
    {
        // Defaul size
        private const float defaultWidth = 240f;
        private const float defaultHeight = 320f;

        // Data renderer in a editor window
        public IEditorView View{ get; private set; }

        /// <summary>
        /// Creation of initialization and display of a window on the monitor screen.
        /// </summary>
        /// <param name="view">Data renderer in a editor window</param>
        /// <param name="screenPosition">Point coordinates to display on the screen</param>
        /// <param name="size">Size window</param>
        public static void Show(IEditorView view, Vector2 screenPosition, Vector2 size = default)
        {
            float width = System.Math.Max(size.x, defaultWidth);
            float height = System.Math.Max(size.y, defaultHeight);
            Rect buttonRect = new Rect(screenPosition.x - width / 2, screenPosition.y - EditorGUIUtility.singleLineHeight, width, 1);

            var instance = (DropDownWindow)CreateInstance(typeof(DropDownWindow));
            instance.hideFlags = HideFlags.HideAndDontSave;
            instance.View = view;

            instance.ShowAsDropDown(buttonRect, new Vector2(buttonRect.width, height));

            instance.Focus();

            instance.wantsMouseMove = true;
        }

        /// <summary>
        /// Method for rendering window content
        /// </summary>
        internal void OnGUI() => View?.OnGUI(this);
    }
}