using UnityEditor;
using UnityEngine;

namespace UnityExtended
{
    public class SimpleWindow : EditorWindow, IContext
    {
        public const float MIN_WIDTH = 720f;
        public const float MIN_HIGHT = 320f;

        // Data renderer in a editor window
        public IEditorView View { get; private set; }

        /// <summary>
        /// Creation of initialization and display of a window on the monitor screen.
        /// </summary>
        /// <param name="view">Data renderer in a editor window</param>
        public static SimpleWindow Show(IEditorView view, float minWidth = MIN_WIDTH, float minHight = MIN_HIGHT)
        {
            var instance = (SimpleWindow)GetWindow(typeof(SimpleWindow));
            
            instance.hideFlags = HideFlags.HideAndDontSave;
            instance.View = view;
            instance.wantsMouseMove = true;
            instance.minSize = new Vector2(minWidth, minHight);

            return instance;
        }

        /// <summary>
        /// Method for rendering window content
        /// </summary>
        internal void OnGUI() => View?.OnGUI(this);
    }
}