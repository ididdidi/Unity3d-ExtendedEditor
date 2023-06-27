using UnityEditor;
using UnityEngine;

namespace UnityExtended
{
    public class SimpleWindow : EditorWindow, IContext
    {
        public const float WIDTH = 720f;
        public const float HIGHT = 480f;

        // Data renderer in a editor window
        public IEditorView View { get; private set; }

        /// <summary>
        /// Creation of initialization and display of a window on the monitor screen.
        /// </summary>
        /// <param name="view">Data renderer in a editor window</param>
        public static SimpleWindow Show(IEditorView view, float width = WIDTH, float hight = HIGHT)
        {
            var instance = (SimpleWindow)GetWindow(typeof(SimpleWindow));
            
            instance.hideFlags = HideFlags.HideAndDontSave;
            instance.View = view;
            instance.wantsMouseMove = true;
            instance.maxSize = instance.minSize = new Vector2(width, hight);

            return instance;
        }

        /// <summary>
        /// Method for rendering window content
        /// </summary>
        internal void OnGUI() => View?.OnGUI(this);
    }
}