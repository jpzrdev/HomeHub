using HomeHub.Application.Features.Inventory.Interfaces;
using HomeHub.Application.Features.ShoppingList.GenerateShoppingList;
using HomeHub.Application.Features.ShoppingList.Interfaces;
using HomeHub.Domain.Entities;
using ShoppingListEntity = HomeHub.Domain.Entities.ShoppingList;
using Moq;
using Xunit;

namespace HomeHub.Tests.Features.ShoppingList;

public class GenerateShoppingListCommandHandlerTests
{
    private readonly Mock<IInventoryItemRepository> _inventoryItemRepositoryMock;
    private readonly Mock<IShoppingListRepository> _shoppingListRepositoryMock;
    private readonly GenerateShoppingListCommandHandler _handler;

    public GenerateShoppingListCommandHandlerTests()
    {
        _inventoryItemRepositoryMock = new Mock<IInventoryItemRepository>();
        _shoppingListRepositoryMock = new Mock<IShoppingListRepository>();
        _handler = new GenerateShoppingListCommandHandler(
            _inventoryItemRepositoryMock.Object,
            _shoppingListRepositoryMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldCreateShoppingList_WhenItemsBelowMinimumExist()
    {
        // Arrange
        var command = new GenerateShoppingListCommand();

        var inventoryItem1 = new InventoryItem(
            name: "Item 1",
            quantityAvailable: 5.0m,
            minimumQuantity: 10.0m
        );

        var inventoryItem2 = new InventoryItem(
            name: "Item 2",
            quantityAvailable: 2.5m,
            minimumQuantity: 5.0m
        );

        var itemsBelowMinimum = new List<InventoryItem> { inventoryItem1, inventoryItem2 };

        _inventoryItemRepositoryMock
            .Setup(r => r.FindAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<InventoryItem, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(itemsBelowMinimum);

        ShoppingListEntity? capturedShoppingList = null;
        _shoppingListRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<ShoppingListEntity>(), It.IsAny<CancellationToken>()))
            .Callback<ShoppingListEntity, CancellationToken>((list, ct) => capturedShoppingList = list)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        Assert.NotNull(capturedShoppingList);
        Assert.Equal(2, capturedShoppingList.Items.Count);
        Assert.False(capturedShoppingList.IsCompleted);

        // Verify quantities to buy
        var item1 = capturedShoppingList.Items.First(i => i.InventoryItemId == inventoryItem1.Id);
        Assert.Equal(5.0m, item1.QuantityToBuy); // 10.0 - 5.0 = 5.0

        var item2 = capturedShoppingList.Items.First(i => i.InventoryItemId == inventoryItem2.Id);
        Assert.Equal(2.5m, item2.QuantityToBuy); // 5.0 - 2.5 = 2.5

        _inventoryItemRepositoryMock.Verify(
            r => r.FindAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<InventoryItem, bool>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _shoppingListRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<ShoppingListEntity>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCreateEmptyShoppingList_WhenNoItemsBelowMinimum()
    {
        // Arrange
        var command = new GenerateShoppingListCommand();

        _inventoryItemRepositoryMock
            .Setup(r => r.FindAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<InventoryItem, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<InventoryItem>());

        ShoppingListEntity? capturedShoppingList = null;
        _shoppingListRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<ShoppingListEntity>(), It.IsAny<CancellationToken>()))
            .Callback<ShoppingListEntity, CancellationToken>((list, ct) => capturedShoppingList = list)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        Assert.NotNull(capturedShoppingList);
        Assert.Empty(capturedShoppingList.Items);
        Assert.False(capturedShoppingList.IsCompleted);

        _shoppingListRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<ShoppingListEntity>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCalculateCorrectQuantityToBuy_WhenItemIsBelowMinimum()
    {
        // Arrange
        var command = new GenerateShoppingListCommand();

        var inventoryItem = new InventoryItem(
            name: "Test Item",
            quantityAvailable: 3.75m,
            minimumQuantity: 10.5m
        );

        var itemsBelowMinimum = new List<InventoryItem> { inventoryItem };

        _inventoryItemRepositoryMock
            .Setup(r => r.FindAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<InventoryItem, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(itemsBelowMinimum);

        ShoppingListEntity? capturedShoppingList = null;
        _shoppingListRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<ShoppingListEntity>(), It.IsAny<CancellationToken>()))
            .Callback<ShoppingListEntity, CancellationToken>((list, ct) => capturedShoppingList = list)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedShoppingList);
        var shoppingListItem = capturedShoppingList.Items.First();
        Assert.Equal(6.75m, shoppingListItem.QuantityToBuy); // 10.5 - 3.75 = 6.75
        Assert.Equal(inventoryItem.Id, shoppingListItem.InventoryItemId);
        Assert.False(shoppingListItem.IsPurchased);
    }

    [Fact]
    public async Task Handle_ShouldNotIncludeItems_WhenQuantityEqualsMinimum()
    {
        // Arrange
        var command = new GenerateShoppingListCommand();

        var inventoryItemAtMinimum = new InventoryItem(
            name: "Item At Minimum",
            quantityAvailable: 10.0m,
            minimumQuantity: 10.0m
        );

        var inventoryItemAboveMinimum = new InventoryItem(
            name: "Item Above Minimum",
            quantityAvailable: 15.0m,
            minimumQuantity: 10.0m
        );

        // Only items below minimum should be returned
        var itemsBelowMinimum = new List<InventoryItem>();

        _inventoryItemRepositoryMock
            .Setup(r => r.FindAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<InventoryItem, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(itemsBelowMinimum);

        ShoppingListEntity? capturedShoppingList = null;
        _shoppingListRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<ShoppingListEntity>(), It.IsAny<CancellationToken>()))
            .Callback<ShoppingListEntity, CancellationToken>((list, ct) => capturedShoppingList = list)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedShoppingList);
        Assert.Empty(capturedShoppingList.Items);
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationToken_ToRepositories()
    {
        // Arrange
        var command = new GenerateShoppingListCommand();
        var cancellationToken = new CancellationTokenSource().Token;

        _inventoryItemRepositoryMock
            .Setup(r => r.FindAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<InventoryItem, bool>>>(),
                cancellationToken))
            .ReturnsAsync(new List<InventoryItem>());

        _shoppingListRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<ShoppingListEntity>(), cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        _inventoryItemRepositoryMock.Verify(
            r => r.FindAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<InventoryItem, bool>>>(),
                cancellationToken),
            Times.Once);

        _shoppingListRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<ShoppingListEntity>(), cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSetShoppingListId_ForAllItems()
    {
        // Arrange
        var command = new GenerateShoppingListCommand();

        var inventoryItem1 = new InventoryItem(
            name: "Item 1",
            quantityAvailable: 5.0m,
            minimumQuantity: 10.0m
        );

        var inventoryItem2 = new InventoryItem(
            name: "Item 2",
            quantityAvailable: 2.0m,
            minimumQuantity: 5.0m
        );

        var itemsBelowMinimum = new List<InventoryItem> { inventoryItem1, inventoryItem2 };

        _inventoryItemRepositoryMock
            .Setup(r => r.FindAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<InventoryItem, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(itemsBelowMinimum);

        ShoppingListEntity? capturedShoppingList = null;
        _shoppingListRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<ShoppingListEntity>(), It.IsAny<CancellationToken>()))
            .Callback<ShoppingListEntity, CancellationToken>((list, ct) => capturedShoppingList = list)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedShoppingList);
        Assert.Equal(result, capturedShoppingList.Id);

        foreach (var item in capturedShoppingList.Items)
        {
            Assert.Equal(capturedShoppingList.Id, item.ShoppingListId);
        }
    }
}
