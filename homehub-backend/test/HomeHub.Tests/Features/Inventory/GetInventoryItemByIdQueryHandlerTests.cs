using HomeHub.Application.Features.Inventory.GetInventoryItemById;
using HomeHub.Application.Features.Inventory.Interfaces;
using HomeHub.Domain.Entities;
using Moq;
using Xunit;

namespace HomeHub.Tests.Features.Inventory;

public class GetInventoryItemByIdQueryHandlerTests
{
    private readonly Mock<IInventoryItemRepository> _repositoryMock;
    private readonly GetInventoryItemByIdQueryHandler _handler;

    public GetInventoryItemByIdQueryHandlerTests()
    {
        _repositoryMock = new Mock<IInventoryItemRepository>();
        _handler = new GetInventoryItemByIdQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnInventoryItem_WhenItemExists()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var expectedItem = new InventoryItem(
            name: "Test Item",
            quantityAvailable: 100.0m,
            minimumQuantity: 10.0m
        );

        var query = new GetInventoryItemByIdQuery(itemId);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedItem);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedItem.Id, result.Id);
        Assert.Equal(expectedItem.Name, result.Name);
        Assert.Equal(expectedItem.QuantityAvailable, result.QuantityAvailable);
        Assert.Equal(expectedItem.MinimumQuantity, result.MinimumQuantity);

        _repositoryMock.Verify(r => r.GetByIdAsync(itemId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenItemNotFound()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var query = new GetInventoryItemByIdQuery(itemId);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((InventoryItem?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.Handle(query, CancellationToken.None));

        Assert.Contains(itemId.ToString(), exception.Message);
        _repositoryMock.Verify(r => r.GetByIdAsync(itemId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationToken_ToRepository()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var expectedItem = new InventoryItem(
            name: "Test Item",
            quantityAvailable: 100.0m,
            minimumQuantity: 10.0m
        );

        var query = new GetInventoryItemByIdQuery(itemId);
        var cancellationToken = new CancellationTokenSource().Token;

        _repositoryMock
            .Setup(r => r.GetByIdAsync(itemId, cancellationToken))
            .ReturnsAsync(expectedItem);

        // Act
        await _handler.Handle(query, cancellationToken);

        // Assert
        _repositoryMock.Verify(r => r.GetByIdAsync(itemId, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectItem_WhenMultipleItemsExist()
    {
        // Arrange
        var itemId1 = Guid.NewGuid();
        var itemId2 = Guid.NewGuid();

        var item1 = new InventoryItem("Item 1", 100.0m, 10.0m);
        var item2 = new InventoryItem("Item 2", 200.0m, 20.0m);

        var query = new GetInventoryItemByIdQuery(itemId2);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(itemId1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item1);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(itemId2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item2);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(item2.Id, result.Id);
        Assert.Equal("Item 2", result.Name);
        Assert.Equal(200.0m, result.QuantityAvailable);

        _repositoryMock.Verify(r => r.GetByIdAsync(itemId2, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.GetByIdAsync(itemId1, It.IsAny<CancellationToken>()), Times.Never);
    }
}
