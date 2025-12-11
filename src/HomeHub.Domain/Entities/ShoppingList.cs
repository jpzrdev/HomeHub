using HomeHub.Domain.Common;

namespace HomeHub.Domain.Entities;

public class ShoppingList : BaseEntity
{
    private readonly List<ShoppingListItem> _items = new();

    public IReadOnlyCollection<ShoppingListItem> Items => _items.AsReadOnly();
    public bool IsCompleted { get; private set; }

    private ShoppingList()
    {
        // EF Core constructor
    }

    public ShoppingList(IEnumerable<ShoppingListItem> items)
    {
        _items.AddRange(items);
        // Set ShoppingListId for all items
        foreach (var item in _items)
        {
            item.ShoppingListId = Id;
        }
        IsCompleted = false;
    }

    public void AddItem(ShoppingListItem item)
    {
        item.ShoppingListId = Id;
        _items.Add(item);
    }

    public void MarkAsCompleted()
    {
        IsCompleted = true;
    }

    public void MarkAsIncomplete()
    {
        IsCompleted = false;
    }
}
