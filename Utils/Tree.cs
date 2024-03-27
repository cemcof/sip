namespace sip.Utils;

public interface ITreeItem<TItem>
{
    public TItem? Parent { get; }
    public ICollection<TItem> Children { get; }
}


public class Tree<TItem>(TItem root)
    where TItem : ITreeItem<TItem>
{
    public IEnumerable<TItem> EnumerateBfs(TItem? from = default)
    {
        var queue = new Queue<TItem>();
        
        queue.Enqueue(from ?? root);

        while (queue.Count != 0)
        {
            var item = queue.Dequeue();
            yield return item;

            foreach (var itemChild in item.Children)
            {
                queue.Enqueue(itemChild);
            }
        }
    }
    
    public static IEnumerable<TItem> EnumerateToRoot(TItem fromItem)
    {
        // TODO - this is not very safe since it can be infinite loop if tree is cycled
        var curr = fromItem;
        while (curr is not null)
        {
            yield return curr;
            curr = curr.Parent;
        }
    }

    public IEnumerable<TItem> EnumerateLeaves(TItem? fromItem = default)
        => EnumerateBfs(fromItem).Where(item => item.Children.Count == 0);

}