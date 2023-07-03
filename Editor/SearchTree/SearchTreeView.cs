using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityExtended
{
    /// <summary>
    /// Class for displaying search in the Unity3d editor.
    /// </summary>
    public class SearchTreeView
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
        private const string searchHeader = "Search";
        private readonly Color background = new Color(0.22f, 0.22f, 0.22f);

        // Member variables
        private string searchKeyword = string.Empty;
        private EditorWindow window;
        private ISearchTreeProvider provider;
        private SearchTreeEntry[] currentTree;
        private SearchTreeEntry[] searchResultTree;
        private List<SearchTreeGroupEntry> selectionStack = new List<SearchTreeGroupEntry>();
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
        public SearchTreeView(EditorWindow window, ISearchTreeProvider provider, bool grabFocus = true)
        {
            this.window = window;
            this.provider = provider;
            this.grabFocus = grabFocus;
            IsChanged = true;
        }

        #region Properties
        public bool IsChanged { get; set; }
        private bool SearchKeyIsEmpty { get { return string.IsNullOrEmpty(searchKeyword); } }
        private SearchTreeGroupEntry ActiveParent
        {
            get
            {
                int index = selectionStack.Count - 2 + targetAnimation;

                if (index < 0 || index >= selectionStack.Count)
                    return null;

                return selectionStack[index];
            }
        }
        private SearchTreeEntry[] ActiveTree { get => (!SearchKeyIsEmpty) ? searchResultTree : currentTree; }
        private SearchTreeEntry ActiveSearchEntry
        {
            get
            {
                if (ActiveTree == null)
                    return null;

                SearchTreeEntry[] children = ActiveParent.GetChildren(ActiveTree, SearchKeyIsEmpty);
                if (ActiveParent == null || ActiveParent.SelectedIndex < 0 || ActiveParent.SelectedIndex >= children.Length)
                    return null;

                return children[ActiveParent.SelectedIndex];
            }
        }
        private bool isAnimating { get => currentAnimation != targetAnimation; }
        private bool isOnFocus { get => GUIUtility.keyboardControl == controlId || GUIUtility.keyboardControl == 0; }
        #endregion

        /// <summary>
        /// Creating a tree for search
        /// </summary>
        private void CreateSearchTree()
        {
            currentTree = provider.CreateSearchTree() ?? new SearchTreeEntry[0];

            // Rebuild stack
            if (selectionStack.Count == 0)
                selectionStack.Add(currentTree[0] as SearchTreeGroupEntry);
            else
            {
                // The root is always the match for level 0
                SearchTreeGroupEntry match = currentTree[0] as SearchTreeGroupEntry;
                int level = 0;
                while (true)
                {
                    // Assign the match for the current level
                    SearchTreeGroupEntry oldSearchTreeEntry = selectionStack[level];
                    selectionStack[level] = match;
                    selectionStack[level].SelectedIndex = oldSearchTreeEntry.SelectedIndex;
                    selectionStack[level].ScrollPosition = oldSearchTreeEntry.ScrollPosition;

                    // See if we reached last SearchTreeEntry of stack
                    level++;
                    if (level == selectionStack.Count)
                        break;

                    // Try to find a child of the same name as we had before
                    SearchTreeEntry[] children = match.GetChildren(ActiveTree, SearchKeyIsEmpty);
                    SearchTreeEntry childMatch = children.FirstOrDefault(entry => entry.Name == selectionStack[level].Name);
                    if (childMatch != null && childMatch is SearchTreeGroupEntry)
                    {
                        match = childMatch as SearchTreeGroupEntry;
                    }
                    else
                    {
                        // If we couldn't find the child, remove all further SearchTreeEntrys from the stack
                        selectionStack.RemoveRange(level, selectionStack.Count - level);
                    }
                }
            }

            IsChanged = false;
            RebuildSearch();
        }

        /// <summary>
        /// Rebuil a search tree
        /// </summary>
        private void RebuildSearch()
        {
            if (SearchKeyIsEmpty)
            {
                searchResultTree = null;
                if (selectionStack[selectionStack.Count - 1].Name == searchHeader)
                {
                    selectionStack.Clear();
                    selectionStack.Add(currentTree[0] as SearchTreeGroupEntry);
                }
                targetAnimation = 1;
                lastTime = System.DateTime.Now.Ticks;
                return;
            }

            // Support multiple search words separated by spaces.
            string[] searchWords = searchKeyword.ToLower().Split(' ');

            // We keep two lists. Matches that matches the start of an item always get first priority.
            List<SearchTreeEntry> matchesStart = new List<SearchTreeEntry>();
            List<SearchTreeEntry> matchesWithin = new List<SearchTreeEntry>();

            foreach (SearchTreeEntry entry in currentTree)
            {
                if (entry is SearchTreeGroupEntry)
                    continue;

                string name = entry.Name.ToLower().Replace(" ", "");
                bool didMatchAll = true;
                bool didMatchStart = false;

                // See if we match ALL the seaarch words.
                for (int w = 0; w < searchWords.Length; w++)
                {
                    string search = searchWords[w];
                    if (name.Contains(search))
                    {
                        // If the start of the item matches the first search word, make a note of that.
                        if (w == 0 && name.StartsWith(search))
                            didMatchStart = true;
                    }
                    else
                    {
                        // As soon as any word is not matched, we disregard this item.
                        didMatchAll = false;
                        break;
                    }
                }
                // We always need to match all search words.
                // If we ALSO matched the start, this item gets priority.
                if (didMatchAll)
                {
                    if (didMatchStart)
                        matchesStart.Add(entry);
                    else
                        matchesWithin.Add(entry);
                }
            }

            matchesStart.Sort();
            matchesWithin.Sort();

            // Create search tree
            List<SearchTreeEntry> tree = new List<SearchTreeEntry>();
            // Add parent
            tree.Add(new SearchTreeGroupEntry(new GUIContent(searchHeader)));
            // Add search results
            tree.AddRange(matchesStart);
            tree.AddRange(matchesWithin);

            // Create search result tree
            searchResultTree = tree.ToArray();
            selectionStack.Clear();
            selectionStack.Add(searchResultTree[0] as SearchTreeGroupEntry);

            // Always select the first search result when search is changed (e.g. a character was typed in or deleted),
            // because it's usually the best match.
            if (ActiveParent.GetChildren(ActiveTree, SearchKeyIsEmpty).Length >= 1)
                ActiveParent.SelectedIndex = 0;
            else
                ActiveParent.SelectedIndex = -1;
        }

        /// <summary>
        /// Method to display search field and tree.
        /// </summary>
        /// <param name="context">Object for interacting with an object in which data is displayed</param>
        public void OnGUI(Rect position)
        {
            EditorGUI.DrawRect(position, background);
            GUI.Label(position, GUIContent.none, Styles.border);
            if (IsChanged) { CreateSearchTree(); }

            // Keyboard
            HandleKeyboard(Event.current);

            // Search
            Rect searchRect = new Rect(8, 8, position.width - 16, 20);

            GUI.SetNextControlName("SearchField");
            EditorGUI.BeginChangeCheck();
            var newSearch = ExtendedEditorGUI.SearchField(searchRect, delayedSearch ?? searchKeyword, out controlId);
            if (EditorGUI.EndChangeCheck() && (newSearch != searchKeyword || delayedSearch != null))
            {
                if (!isAnimating)
                {
                    searchKeyword = delayedSearch ?? newSearch;
                    RebuildSearch();
                    delayedSearch = null;
                }
                else {  delayedSearch = newSearch; }
            }

            if (grabFocus) { grabFocus = false; EditorGUI.FocusTextInControl("SearchField"); }

            // Show lists
            ListGUI(position, ActiveTree, currentAnimation, GetReturnGroupEntry(0), GetReturnGroupEntry(-1));
            if (currentAnimation < 1) { ListGUI(position, ActiveTree, currentAnimation + 1, GetReturnGroupEntry(-1), GetReturnGroupEntry(-2)); }

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
                    selectionStack.RemoveAt(selectionStack.Count - 1);
                }
                window.Repaint();
            }
        }

        /// <summary>
        /// Method for getting the parent entries of the search tree
        /// </summary>
        /// <param name="relativelayer"></param>
        /// <returns>Return parent group entry if it exists</returns>
        private SearchTreeGroupEntry GetReturnGroupEntry(int relativelayer)
        {
            int i = selectionStack.Count + relativelayer - 1;
            if (i < 0 || i >= selectionStack.Count) { return null; }
            else { return selectionStack[i] as SearchTreeGroupEntry; }
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
            Rect animRect = position;
            animRect.x = position.width * (1 - animationValue) + 1;
            animRect.y = headerHeight;
            animRect.height -= headerHeight;
            animRect.width -= 2;

            // Start of animated area (the part that moves left and right)
            GUILayout.BeginArea(animRect);

            // Header
            Rect headerRect = GUILayoutUtility.GetRect(10, 25);
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

            ListGUI(tree, parent);
            GUILayout.EndArea();
        }

        /// <summary>
        /// Method directly to display the search list
        /// </summary>
        /// <param name="context">Object for interacting with an object in which data is displayed</param>
        /// <param name="tree">Search tree</param>
        /// <param name="parent">The parent entry of the search tree</param>
        private void ListGUI(SearchTreeEntry[] tree, SearchTreeGroupEntry parent)
        {
            var height = EditorGUIUtility.singleLineHeight;
            // Start of scroll view list
            parent.ScrollPosition = GUILayout.BeginScrollView(parent.ScrollPosition);

            EditorGUIUtility.SetIconSize(new Vector2(height, height));

            SearchTreeEntry[] children = parent.GetChildren(tree, SearchKeyIsEmpty);

            Rect selectedRect = new Rect();

            // Iterate through the children
            for (int i = 0; i < children.Length; i++)
            {
                SearchTreeEntry entry = children[i];
                Rect entryRect = GUILayoutUtility.GetRect(height, height + 2, GUILayout.ExpandWidth(true));

                // Select the SearchTreeEntry the mouse cursor is over.
                // Only do it on mouse move - keyboard controls are allowed to overwrite this until the next time the mouse moves.
                if (Event.current.type == EventType.MouseMove || Event.current.type == EventType.MouseDown)
                {
                    if (parent.SelectedIndex != i && entryRect.Contains(Event.current.mousePosition))
                    {
                        parent.SelectedIndex = i;
                        window.Repaint();
                    }
                }

                bool selected = false;
                // Handle selected item
                if (i == parent.SelectedIndex)
                {
                    selected = true;
                    selectedRect = entryRect;
                    provider.OnFocusEntry(entry);
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
                if (Event.current.type == EventType.MouseDown && entryRect.Contains(Event.current.mousePosition))
                {
                    Event.current.Use();
                    parent.SelectedIndex = i;
                    SelectEntry(entry, true);
                    // Reset Focus
                    EditorGUI.FocusTextInControl(null);
                }
            }

            EditorGUIUtility.SetIconSize(Vector2.zero);

            GUILayout.EndScrollView();

            // Scroll to show selected
            if (scrollToSelected && Event.current.type == EventType.Repaint)
            {
                scrollToSelected = false;
                Rect scrollRect = GUILayoutUtility.GetLastRect();
                if (selectedRect.yMax - scrollRect.height > parent.ScrollPosition.y)
                {
                    parent.ScrollPosition.y = selectedRect.yMax - scrollRect.height;
                    window.Repaint();
                }
                if (selectedRect.y < parent.ScrollPosition.y)
                {
                    parent.ScrollPosition.y = selectedRect.y;
                    window.Repaint();
                }
            }
        }

        /// <summary>
        /// Action associated with the selected item.
        /// </summary>
        /// <param name="context">Object for interacting with an object in which data is displayed</param>
        /// <param name="entry">Selected search tree entry</param>
        /// <param name="hasCallback">Should invoke callback</param>
        private void SelectEntry(SearchTreeEntry entry, bool hasCallback)
        {
            if (entry is SearchTreeGroupEntry group)
            {
                if (SearchKeyIsEmpty)
                {
                    lastTime = System.DateTime.Now.Ticks;
                    if (targetAnimation == 0) { targetAnimation = 1; }
                    else if (currentAnimation == 1)
                    {
                        currentAnimation = 0;
                        selectionStack.Add(group);
                    }
                }
            }
            else if (hasCallback && provider.OnSelectEntry(entry)) { window.Close(); }
        }

        /// <summary>
        /// Method for go to previous level
        /// </summary>
        private void GoToParent()
        {
            if (selectionStack.Count > 1)
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
                            ActiveParent.SelectedIndex = ActiveParent.GetChildren(ActiveTree, SearchKeyIsEmpty).Length - 1;
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
                            ActiveParent.SelectedIndex = Mathf.Min(ActiveParent.SelectedIndex, ActiveParent.GetChildren(ActiveTree, SearchKeyIsEmpty).Length - 1);
                            scrollToSelected = true;
                            curentEvent.Use();
                        }
                        return;
                    case KeyCode.RightArrow:
                        if (SearchKeyIsEmpty && ActiveSearchEntry != null)
                        {
                            SelectEntry(ActiveSearchEntry, false);
                            curentEvent.Use();
                        }
                        return;
                    case KeyCode.LeftArrow:
                        if (SearchKeyIsEmpty)
                        {
                            GoToParent();
                            curentEvent.Use();
                        }
                        return;
                    case KeyCode.KeypadEnter:
                        if (ActiveSearchEntry != null)
                        {
                            SelectEntry(ActiveSearchEntry, true);
                            curentEvent.Use();
                        }
                        return;
                    case KeyCode.Return:
                        if(ActiveSearchEntry != null)
                        {
                            SelectEntry(ActiveSearchEntry, true);
                            curentEvent.Use();
                        }
                        return;
                    case KeyCode.Backspace:
                        if (SearchKeyIsEmpty)
                        {
                            GoToParent();
                            curentEvent.Use();
                        }
                        return;
                    case KeyCode.Escape:
                        {
                            window.Close();
                            curentEvent.Use();
                        }
                        return;
                }
            }
        }
    }
}