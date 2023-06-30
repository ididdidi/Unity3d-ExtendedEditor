namespace UnityExtended
{
    public interface ISearchTreeProvider
    {
        SearchTreeEntry[] CreateSearchTree();
        void OnFocusEntry(SearchTreeEntry entry);
        bool OnSelectEntry(SearchTreeEntry entry);
    }
}