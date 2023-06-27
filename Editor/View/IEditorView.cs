namespace UnityExtended
{
    /// <summary>
    /// Interface for displaying data in the editor
    /// </summary>
    public interface IEditorView
    {
        /// <summary>
        /// Method for displaying data in the editor
        /// </summary>
        /// <param name="context">Interface for interacting with an object in which data is displayed</param>
        void OnGUI(IContext context);
    }
}