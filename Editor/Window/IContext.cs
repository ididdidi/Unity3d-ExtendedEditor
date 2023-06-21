using UnityEngine;

namespace UnityExtended
{
    /// <summary>
    /// Interface for interacting with an object in which data is displayed
    /// </summary>
    public interface IContext
    {
        /// <summary>
        /// Position and size for displaying content
        /// </summary>
        Rect position { get; }

        /// <summary>
        /// Method for redrawing content display
        /// </summary>
        void Repaint();

        /// <summary>
        /// Method to close the inspector window and stop displaying data.
        /// </summary>
        void Close();
    }
}