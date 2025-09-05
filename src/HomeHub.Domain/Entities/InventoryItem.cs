namespace HomeHub.Domain.Entities;

public class InventoryItem : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal QuantityAvailable { get; set; }
    public decimal MinimumQuantity { get; set; }
    public bool NotifyOnBelowMinimumQuantity { get; set; }
}
