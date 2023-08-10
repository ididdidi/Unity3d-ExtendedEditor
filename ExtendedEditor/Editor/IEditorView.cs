using UnityEngine;

namespace UnityExtended
{
    public interface IEditorView
    {
        float HeightInGUI { get; }
        void OnGUI(Rect rect);
    }
}