using UnityEngine;

namespace UnityExtended
{
    /// <summary>
    /// Class for displaying two Views in one window.
    /// </summary>
    public class TwoPaneView : IEditorView
    {
        public const float LEFT_WEIGHT = 1f;
        public const float RIGHT_WEIGHT = 1f;

        private IContext parent;
        private IContext leftPane;
        private IContext rightPane;

        private IEditorView leftView;
        private IEditorView rightView;

        float leftWidthK = 0.5f;
        float rightWidthK = 0.5f;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="leftView">Left view</param>
        /// <param name="rightView">Right view</param>
        /// <param name="leftWeight">Relative left pane weight</param>
        /// <param name="rightWeight">Relative right pane weight</param>
        public TwoPaneView(IEditorView leftView, IEditorView rightView, float leftWeight = LEFT_WEIGHT, float rightWeight = RIGHT_WEIGHT)
        {
            if (leftWeight == 0 || rightWeight == 0) { throw new System.ArgumentNullException("Weight must not be zero"); }
            this.leftView = leftView ?? throw new System.ArgumentNullException(nameof(leftView));
            this.rightView = rightView ?? throw new System.ArgumentNullException(nameof(rightView));

            leftWidthK = leftWeight / (leftWeight + rightWeight);
            rightWidthK = rightWeight / (leftWeight + rightWeight);
        }

        /// <summary>
        /// Method for displaying data in the editor
        /// </summary>
        /// <param name="context">Interface for interacting with an object in which data is displayed</param>
        public void OnGUI(IContext context)
        {
            if (context == null) { throw new System.ArgumentNullException(nameof(leftView)); }

            if (parent != context)
            {
                parent = context;
                leftPane = GetPane(parent, leftWidthK);
                rightPane = GetPane(parent, rightWidthK, parent.position.width * leftWidthK);
            }

            leftView?.OnGUI(leftPane);
            rightView?.OnGUI(rightPane);
        }

        /// <summary>
        /// Method for getting panes.
        /// </summary>
        /// <param name="context">Interface for interacting with an object in which data is displayed</param>
        /// <param name="ratio">The ratio of the weight of the pane</param>
        /// <param name="ofset">Pane offset x-axis</param>
        /// <returns>Returns a pane as <see cref="IContext"/></returns>
        private IContext GetPane(IContext context, float ratio, float ofset = 0f)
        {
            var layout = new EditorPane(context);
            var width = context.position.width * ratio;
            layout.position = new Rect(0 + ofset, 0, (ofset == 0f)? width + 1 : width, context.position.height);
            return layout;
        }
    }
}