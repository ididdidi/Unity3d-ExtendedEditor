
using UnityEngine;

namespace UnityExtended
{
    /// <summary>
    /// Class for implementing interaction with the context.
    /// Allows you to replace the Rect of the parent context with another.
    /// </summary>
    public class EditorPane : IContext
    {
        private IContext parent;
        private Rect rect;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent">Parent context</param>
        public EditorPane(IContext parent)
        {
            this.parent = parent ?? throw new System.ArgumentNullException(nameof(parent));
            rect = parent.position;
        }

        /// <summary>
        /// Position and size for displaying content
        /// </summary>
        public Rect position { get => rect; set => rect = value; }

        public float Width { get => rect.width; set => rect.width = value; }
        public float Height { get => rect.height; set => rect.height = value; }

        /// <summary>
        /// Method to close the inspector window and stop displaying data.
        /// </summary>
        public void Close() => parent?.Close();

        /// <summary>
        /// Method for redrawing content display
        /// </summary>
        public void Repaint() => parent?.Repaint();
    }
}