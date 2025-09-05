using HomeHub.Application.Features.Inventory.Interfaces;
using HomeHub.Domain.Entities;

namespace HomeHub.Application.Features.Inventory.Services;

public class InventoryService : IInventoryService
{
    //in-memory data while waiting for database creation
    private IEnumerable<InventoryItem> _inventoryItems =
    [
        new(){
            Name = "Coca-cola Zero",
            QuantityAvailable = 2,
            MinimumQuantity = 1,
            NotifyOnBelowMinimumQuantity = true
        },
        new(){
            Name = "Iogurte",
            QuantityAvailable = 2,
            MinimumQuantity = 1,
            NotifyOnBelowMinimumQuantity = true
        },
        new(){
            Name = "Água com gás",
            QuantityAvailable = 3,
            MinimumQuantity = 1,
            NotifyOnBelowMinimumQuantity = false
        }
    ];
    public IEnumerable<InventoryItem> GetAll()
    {
        return _inventoryItems;
    }
}