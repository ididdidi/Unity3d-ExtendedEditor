namespace UnityExtended
{
    public interface ISearchTreeProvider
    {
        SearchTreeEntry[] CreateSearchTree();
        bool OnSelectEntry(SearchTreeEntry SearchTreeEntry);
    }
}