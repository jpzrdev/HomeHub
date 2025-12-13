using HomeHub.Application.Features.Inventory.DeleteInventoryItem;
using HomeHub.Application.Features.Inventory.Interfaces;
using HomeHub.Domain.Entities;
using MediatR;
using Moq;
using Xunit;

namespace HomeHub.Tests.Features.Inventory;

public class DeleteInventoryItemCommandHandlerTests
{
    private readonly Mock<IInventoryItemRepository> _repositoryMock;
    private readonly DeleteInventoryItemCommandHandler _handler;

    public DeleteInventoryItemCommandHandlerTests()
    {
        _repositoryMock = new Mock<IInventoryItemRepository>();
        _handler = new DeleteInventoryItemCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldDeleteInventoryItem_WhenItemExists()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var existingItem = new InventoryItem(
            name: "Test Item",
            quantityAvailable: 100.0m,
            minimumQuantity: 10.0m,
            notifyOnBelowMinimumQuantity: false
        );

        var command = new DeleteInventoryItemCommand(itemId);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingItem);

        _repositoryMock
            .Setup(r => r.RemoveAsync(existingItem, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(Unit.Value, result);
        _repositoryMock.Verify(r => r.GetByIdAsync(itemId, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.RemoveAsync(existingItem, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenItemNotFound()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var command = new DeleteInventoryItemCommand(itemId);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((InventoryItem?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.Contains(itemId.ToString(), exception.Message);
        _repositoryMock.Verify(r => r.RemoveAsync(It.IsAny<InventoryItem>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationToken_ToRepository()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var existingItem = new InventoryItem(
            name: "Test Item",
            quantityAvailable: 100.0m,
            minimumQuantity: 10.0m,
            notifyOnBelowMinimumQuantity: false
        );

        var command = new DeleteInventoryItemCommand(itemId);
        var cancellationToken = new CancellationTokenSource().Token;

        _repositoryMock
            .Setup(r => r.GetByIdAsync(itemId, cancellationToken))
            .ReturnsAsync(existingItem);

        _repositoryMock
            .Setup(r => r.RemoveAsync(existingItem, cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        _repositoryMock.Verify(r => r.GetByIdAsync(itemId, cancellationToken), Times.Once);
        _repositoryMock.Verify(r => r.RemoveAsync(existingItem, cancellationToken), Times.Once);
    }
}
