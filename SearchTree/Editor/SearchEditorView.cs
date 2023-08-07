﻿using UnityEditor;
using UnityEngine;

namespace UnityExtended
{
    public interface IDisplay
    {
        void Repaint();
    }

    public class SearchEditorView
    {
        // Styles
        static class Styles
        {
            public static GUIStyle header = "AC BoldHeader";
            public static GUIStyle componentButton = "AC ComponentButton";
            public static GUIStyle groupButton = "AC GroupButton";
            public static GUIStyle border = "grey_border";
            public static GUIStyle rightArrow = "ArrowNavigationRight";
            public static GUIStyle leftArrow = "ArrowNavigationLeft";
        }

        #region Fields
        // Constants
        private const int headerHeight = 30;

        // Member variables
        private SearchTree searchTree;
        private IDisplay display;
        private ISearchTreeProvider provider;
        private bool grabFocus;
        private int controlId;

        // Animation variables
        private string delayedSearch = null;
        private float currentAnimation = 1;
        private int targetAnimation = 1;
        private long lastTime = 0;
        private bool scrollToSelected = false;
        #endregion

        // Constructor
        public SearchEditorView(IDisplay display, ISearchTreeProvider provider, bool grabFocus = true)
        {
            this.display = display;
            this.provider = provider;
            this.grabFocus = grabFocus;
            searchTree = provider.GetSearchTree();
        }

        #region Properties
        public bool IsChanged { get; set; }
        public string SearchKeyword => searchTree.keyword;
        public SearchTreeEntry CurrentEntry { get; private set; }
        private SearchTreeGroupEntry ActiveParent
        {
            get
            {
                int index = searchTree.selectionStack.Count - 2 + targetAnimation;

                if (index < 0 || index >= searchTree.selectionStack.Count)
                    return null;

                return searchTree.selectionStack[index];
            }
        }
        private SearchTreeEntry ActiveSearchEntry
        {
            get
            {
                if (searchTree.ActiveTree == null)
                    return null;

                SearchTreeEntry[] children = ActiveParent.GetChildren(searchTree.ActiveTree, searchTree.SearchKeyIsEmpty);
                if (ActiveParent == null || ActiveParent.SelectedIndex < 0 || ActiveParent.SelectedIndex >= children.Length)
                    return null;

                return children[ActiveParent.SelectedIndex];
            }
        }
        private bool isAnimating { get => currentAnimation != targetAnimation; }
        private bool isOnFocus { get => GUIUtility.keyboardControl == controlId || GUIUtility.keyboardControl == 0; }
        #endregion

        /// <summary>
        /// Method to display search field and tree.
        /// </summary>
        /// <param name="context">Object for interacting with an object in which data is displayed</param>
        public void OnGUI()
        {
            var count = ActiveParent.GetChildren(searchTree.ActiveTree, searchTree.SearchKeyIsEmpty).Length;
            var position = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, count * (EditorGUIUtility.singleLineHeight + 2) + 50);
            position.width = EditorGUIUtility.currentViewWidth;

            if (IsChanged)
            {
                searchTree = provider.GetSearchTree();
                if (searchTree.SearchKeyIsEmpty)
                {
                    targetAnimation = 1;
                    lastTime = System.DateTime.Now.Ticks;
                }
                IsChanged = false;
            }

            // Keyboard
            HandleKeyboard(Event.current);

            // Search
            Rect searchRect = new Rect(8, 8, position.width - 16, 20);

            GUI.SetNextControlName("SearchField");
            EditorGUI.BeginChangeCheck();
            var newSearch = ExtendedEditor.SearchField(searchRect, delayedSearch ?? searchTree.keyword, out controlId);
            if (EditorGUI.EndChangeCheck() && (newSearch != searchTree.keyword || delayedSearch != null))
            {
                if (!isAnimating)
                {
                    searchTree.keyword = delayedSearch ?? newSearch;
                    searchTree.Update();
                    // Always select the first search result when search is changed (e.g. a character was typed in or deleted),
                    // because it's usually the best match.
                    if (ActiveParent.GetChildren(searchTree.ActiveTree, searchTree.SearchKeyIsEmpty).Length >= 1)
                        ActiveParent.SelectedIndex = 0;
                    else
                        ActiveParent.SelectedIndex = -1;
                    delayedSearch = null;
                }
                else { delayedSearch = newSearch; }
            }

            if (grabFocus) { grabFocus = false; EditorGUI.FocusTextInControl("SearchField"); }
        
            // Show lists
            ListGUI(position, searchTree.ActiveTree, currentAnimation, searchTree.GetReturnGroupEntry(0), searchTree.GetReturnGroupEntry(-1));
            if (currentAnimation < 1) { ListGUI(position, searchTree.ActiveTree, currentAnimation + 1, searchTree.GetReturnGroupEntry(-1), searchTree.GetReturnGroupEntry(-2)); }
        
            FocusEntry(ActiveSearchEntry);
        
            // Animate
            if (isAnimating && Event.current.type == EventType.Repaint)
            {
                long now = System.DateTime.Now.Ticks;
                float deltaTime = (now - lastTime) / (float)System.TimeSpan.TicksPerSecond;
                lastTime = now;
                currentAnimation = Mathf.MoveTowards(currentAnimation, targetAnimation, deltaTime * 4);
                if (targetAnimation == 0 && currentAnimation == 0)
                {
                    currentAnimation = 1;
                    targetAnimation = 1;
                    searchTree.selectionStack.RemoveAt(searchTree.selectionStack.Count - 1);
                }
                display.Repaint();
            }
        }

        /// <summary>
        /// Method for animated displaying search list.
        /// </summary>
        /// <param name="context">Object for interacting with an object in which data is displayed</param>
        /// <param name="tree">Search tree</param>
        /// <param name="animationValue">Animation value at the moment</param>
        /// <param name="parent">The parent entry of the search tree</param>
        /// <param name="grandParent">The parent of the parent entry</param>
        private void ListGUI(Rect position, SearchTreeEntry[] tree, float animationValue, SearchTreeGroupEntry parent, SearchTreeGroupEntry grandParent)
        {
            // Smooth the fractional part of the animation value
            animationValue = Mathf.Floor(animationValue) + Mathf.SmoothStep(0, 1, Mathf.Repeat(animationValue, 1));

            // Calculate rect for animated area
            Rect animRect = new Rect (position);
            animRect.x = position.width * (1 - animationValue) + 1;
            animRect.y = headerHeight;
            animRect.height -= headerHeight;
            animRect.width -= 2;

            // Header
            Rect headerRect = new Rect(animRect);
            headerRect.height = 25;
            animRect.y += headerRect.height;
            animRect.height -= headerRect.height;
            string name = parent.Name;
            GUI.Label(headerRect, name, Styles.header);

            // Back button
            if (grandParent != null)
            {
                float yOffset = (headerRect.height - Styles.leftArrow.fixedHeight) / 2;
                Rect arrowRect = new Rect(
                    headerRect.x + Styles.leftArrow.margin.left,
                    headerRect.y + yOffset,
                    Styles.leftArrow.fixedWidth,
                    Styles.leftArrow.fixedHeight);
                if (Event.current.type == EventType.Repaint) { Styles.leftArrow.Draw(arrowRect, false, false, false, false); }

                if (Event.current.type == EventType.MouseDown && headerRect.Contains(Event.current.mousePosition))
                {
                    GoToParent();
                    Event.current.Use();
                }
            }

            ListGUI(animRect, tree, parent);
        }

        /// <summary>
        /// Method directly to display the search list
        /// </summary>
        /// <param name="context">Object for interacting with an object in which data is displayed</param>
        /// <param name="tree">Search tree</param>
        /// <param name="parent">The parent entry of the search tree</param>
        private void ListGUI(Rect position, SearchTreeEntry[] tree, SearchTreeGroupEntry parent)
        {
            var height = EditorGUIUtility.singleLineHeight;

            EditorGUIUtility.SetIconSize(new Vector2(height, height));

            SearchTreeEntry[] children = parent.GetChildren(tree, searchTree.SearchKeyIsEmpty);

            Rect selectedRect = new Rect();

            // Iterate through the children
            for (int i = 0; i < children.Length; i++)
            {
                SearchTreeEntry entry = children[i];
                Rect entryRect = new Rect(position); // GUILayoutUtility.GetRect(height, height + 2, GUILayout.ExpandWidth(true));
                entryRect.height = height + 2;
                position.y += entryRect.height;
                position.height -= entryRect.height;
                // Select the SearchTreeEntry the mouse cursor is over.
                // Only do it on mouse move - keyboard controls are allowed to overwrite this until the next time the mouse moves.
                if (Event.current.type == EventType.MouseMove || Event.current.type == EventType.MouseDown)
                {
                    if (parent.SelectedIndex != i && entryRect.Contains(Event.current.mousePosition))
                    {
                        parent.SelectedIndex = i;
                        display.Repaint();
                    }
                }

                if (Event.current.type == EventType.MouseDown)
                {
                    // Reset Focus
                    EditorGUI.FocusTextInControl(null);
                    if (entryRect.Contains(Event.current.mousePosition))
                    {
                        Event.current.Use();
                        parent.SelectedIndex = i;
                        SelectEntry(entry);
                    }
                }

                bool selected = false;
                // Handle selected item
                if (i == parent.SelectedIndex)
                {
                    selected = true;
                    selectedRect = entryRect;
                }

                // Draw SearchTreeEntry
                if (Event.current.type == EventType.Repaint)
                {
                    GUIStyle labelStyle = (entry is SearchTreeGroupEntry) ? Styles.groupButton : Styles.componentButton;
                    labelStyle.Draw(entryRect, entry.Content, false, false, selected, selected);
                    if (entry is SearchTreeGroupEntry)
                    {
                        float yOffset = (entryRect.height - Styles.rightArrow.fixedHeight) / 2;
                        Rect arrowRect = new Rect(
                            entryRect.xMax - Styles.rightArrow.fixedWidth - Styles.rightArrow.margin.right,
                            entryRect.y + yOffset,
                            Styles.rightArrow.fixedWidth,
                            Styles.rightArrow.fixedHeight);
                        Styles.rightArrow.Draw(arrowRect, false, false, false, false);
                    }
                }
            }

            EditorGUIUtility.SetIconSize(Vector2.zero);

            // Scroll to show selected
            if (scrollToSelected && Event.current.type == EventType.Repaint)
            {
                scrollToSelected = false;
                Rect scrollRect = GUILayoutUtility.GetLastRect();
                if (selectedRect.yMax - scrollRect.height > parent.ScrollPosition.y)
                {
                    parent.ScrollPosition.y = selectedRect.yMax - scrollRect.height;
                    display.Repaint();
                }
                if (selectedRect.y < parent.ScrollPosition.y)
                {
                    parent.ScrollPosition.y = selectedRect.y;
                    display.Repaint();
                }
            }
        }

        private void FocusEntry(SearchTreeEntry entry)
        {
            if (!isAnimating && CurrentEntry != entry)
            {
                CurrentEntry = entry;
                provider.OnFocusEntry(CurrentEntry);
            }
        }

        /// <summary>
        /// Action associated with the selected item.
        /// </summary>
        /// <param name="context">Object for interacting with an object in which data is displayed</param>
        /// <param name="entry">Selected search tree entry</param>
        private void SelectEntry(SearchTreeEntry entry)
        {
            if (entry is SearchTreeGroupEntry group)
            {
                if (searchTree.SearchKeyIsEmpty)
                {
                    lastTime = System.DateTime.Now.Ticks;
                    if (targetAnimation == 0) { targetAnimation = 1; }
                    else if (currentAnimation == 1)
                    {
                        currentAnimation = 0;
                        searchTree.selectionStack.Add(group);
                    }
                }
            }
            else { provider.OnSelectEntry(entry); }
        }

        /// <summary>
        /// Method for go to previous level
        /// </summary>
        private void GoToParent()
        {
            if (searchTree.selectionStack.Count > 1)
            {
                targetAnimation = 0;
                lastTime = System.DateTime.Now.Ticks;
            }
        }

        /// <summary>
        /// Handles keystrokes
        /// </summary>
        /// <param name="context">Object for interacting with an object in which data is displayed</param>
        private void HandleKeyboard(Event curentEvent)
        {
            if (curentEvent.type == EventType.KeyDown && isOnFocus)
            {
                switch (curentEvent.keyCode)
                {
                    case KeyCode.PageUp:
                        {
                            ActiveParent.SelectedIndex--;
                            ActiveParent.SelectedIndex = 0;
                            scrollToSelected = true;
                            curentEvent.Use();
                        }
                        return;
                    case KeyCode.PageDown:
                        {
                            ActiveParent.SelectedIndex = ActiveParent.GetChildren(searchTree.ActiveTree, searchTree.SearchKeyIsEmpty).Length - 1;
                            scrollToSelected = true;
                            curentEvent.Use();
                        }
                        return;
                    case KeyCode.UpArrow:
                        {
                            ActiveParent.SelectedIndex--;
                            ActiveParent.SelectedIndex = Mathf.Max(ActiveParent.SelectedIndex, 0);
                            scrollToSelected = true;
                            curentEvent.Use();
                        }
                        return;
                    case KeyCode.DownArrow:
                        {
                            ActiveParent.SelectedIndex++;
                            ActiveParent.SelectedIndex = Mathf.Min(ActiveParent.SelectedIndex, ActiveParent.GetChildren(searchTree.ActiveTree, searchTree.SearchKeyIsEmpty).Length - 1);
                            scrollToSelected = true;
                            curentEvent.Use();
                        }
                        return;
                    case KeyCode.RightArrow:
                        if (searchTree.SearchKeyIsEmpty && ActiveSearchEntry != null)
                        {
                            SelectEntry(ActiveSearchEntry);
                            curentEvent.Use();
                        }
                        return;
                    case KeyCode.LeftArrow:
                        if (searchTree.SearchKeyIsEmpty)
                        {
                            GoToParent();
                            curentEvent.Use();
                        }
                        return;
                    case KeyCode.KeypadEnter:
                        if (ActiveSearchEntry != null)
                        {
                            SelectEntry(ActiveSearchEntry);
                            curentEvent.Use();
                        }
                        return;
                    case KeyCode.Return:
                        if (ActiveSearchEntry != null)
                        {
                            SelectEntry(ActiveSearchEntry);
                            curentEvent.Use();
                        }
                        return;
                    case KeyCode.Backspace:
                        if (searchTree.SearchKeyIsEmpty)
                        {
                            GoToParent();
                            curentEvent.Use();
                        }
                        return;
                }
            }
        }
    }
}