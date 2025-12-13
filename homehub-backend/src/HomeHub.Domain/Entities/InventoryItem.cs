using HomeHub.Domain.Common;

namespace HomeHub.Domain.Entities;

public class InventoryItem : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public decimal QuantityAvailable { get; private set; }
    public decimal MinimumQuantity { get; private set; }
    public bool NotifyOnBelowMinimumQuantity { get; private set; }

    public InventoryItem(string name, decimal quantityAvailable, decimal minimumQuantity, bool notifyOnBelowMinimumQuantity)
    {
        Name = name;
        QuantityAvailable = quantityAvailable;
        MinimumQuantity = minimumQuantity;
        NotifyOnBelowMinimumQuantity = notifyOnBelowMinimumQuantity;
    }

    public void Update(string? name, decimal? quantityAvailable, decimal? minimumQuantity, bool? notifyOnBelowMinimumQuantity)
    {
        Name = name ?? Name;
        QuantityAvailable = quantityAvailable ?? QuantityAvailable;
        MinimumQuantity = minimumQuantity ?? MinimumQuantity;
        NotifyOnBelowMinimumQuantity = notifyOnBelowMinimumQuantity ?? NotifyOnBelowMinimumQuantity;
    }
}
