using HomeHub.Application.Features.Inventory.UpdateInventoryItem;
using HomeHub.Application.Features.Inventory.Interfaces;
using HomeHub.Domain.Entities;
using Moq;
using Xunit;

namespace HomeHub.Tests.Features.Inventory;

public class UpdateInventoryItemCommandHandlerTests
{
    private readonly Mock<IInventoryItemRepository> _repositoryMock;
    private readonly UpdateInventoryItemCommandHandler _handler;

    public UpdateInventoryItemCommandHandlerTests()
    {
        _repositoryMock = new Mock<IInventoryItemRepository>();
        _handler = new UpdateInventoryItemCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateInventoryItem_WhenItemExists()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var existingItem = new InventoryItem(
            name: "Original Name",
            quantityAvailable: 100.0m,
            minimumQuantity: 10.0m
        );

        var command = new UpdateInventoryItemCommand(
            Id: itemId,
            Name: "Updated Name",
            QuantityAvailable: 150.0m,
            MinimumQuantity: 15.0m
        );

        _repositoryMock
            .Setup(r => r.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingItem);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<InventoryItem>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal(150.0m, result.QuantityAvailable);
        Assert.Equal(15.0m, result.MinimumQuantity);

        _repositoryMock.Verify(r => r.GetByIdAsync(itemId, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.UpdateAsync(existingItem, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenItemNotFound()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var command = new UpdateInventoryItemCommand(
            Id: itemId,
            Name: "Updated Name",
            QuantityAvailable: null,
            MinimumQuantity: null
        );

        _repositoryMock
            .Setup(r => r.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((InventoryItem?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.Contains(itemId.ToString(), exception.Message);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<InventoryItem>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldUpdateOnlyProvidedFields_WhenPartialUpdate()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var existingItem = new InventoryItem(
            name: "Original Name",
            quantityAvailable: 100.0m,
            minimumQuantity: 10.0m
        );

        var command = new UpdateInventoryItemCommand(
            Id: itemId,
            Name: "Updated Name",
            QuantityAvailable: null,
            MinimumQuantity: null
        );

        _repositoryMock
            .Setup(r => r.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingItem);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<InventoryItem>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal(100.0m, result.QuantityAvailable); // Unchanged
        Assert.Equal(10.0m, result.MinimumQuantity); // Unchanged
    }

    [Fact]
    public async Task Handle_ShouldUpdateQuantityOnly_WhenOnlyQuantityProvided()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var existingItem = new InventoryItem(
            name: "Original Name",
            quantityAvailable: 100.0m,
            minimumQuantity: 10.0m
        );

        var command = new UpdateInventoryItemCommand(
            Id: itemId,
            Name: null,
            QuantityAvailable: 200.0m,
            MinimumQuantity: null
        );

        _repositoryMock
            .Setup(r => r.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingItem);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<InventoryItem>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Original Name", result.Name); // Unchanged
        Assert.Equal(200.0m, result.QuantityAvailable); // Updated
        Assert.Equal(10.0m, result.MinimumQuantity); // Unchanged
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationToken_ToRepository()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var existingItem = new InventoryItem(
            name: "Test Item",
            quantityAvailable: 100.0m,
            minimumQuantity: 10.0m
        );

        var command = new UpdateInventoryItemCommand(
            Id: itemId,
            Name: "Updated Name",
            QuantityAvailable: null,
            MinimumQuantity: null
        );

        var cancellationToken = new CancellationTokenSource().Token;

        _repositoryMock
            .Setup(r => r.GetByIdAsync(itemId, cancellationToken))
            .ReturnsAsync(existingItem);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<InventoryItem>(), cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        _repositoryMock.Verify(r => r.GetByIdAsync(itemId, cancellationToken), Times.Once);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<InventoryItem>(), cancellationToken), Times.Once);
    }
}
