using HomeHub.Application.Features.Inventory.CreateInventory;
using HomeHub.Application.Features.Inventory.Interfaces;
using HomeHub.Domain.Entities;
using Moq;
using Xunit;

namespace HomeHub.Tests.Features.Inventory;

public class CreateInventoryItemCommandHandlerTests
{
    private readonly Mock<IInventoryItemRepository> _repositoryMock;
    private readonly CreateInventoryItemCommandHandler _handler;

    public CreateInventoryItemCommandHandlerTests()
    {
        _repositoryMock = new Mock<IInventoryItemRepository>();
        _handler = new CreateInventoryItemCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateInventoryItem_WhenValidCommandProvided()
    {
        // Arrange
        var command = new CreateInventoryItemCommand(
            Name: "Test Item",
            QuantityAvailable: 100.5m,
            MinimumQuantity: 10.0m,
            NotifyOnBelowMinimumQuantity: true
        );

        InventoryItem? capturedItem = null;
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<InventoryItem>(), It.IsAny<CancellationToken>()))
            .Callback<InventoryItem, CancellationToken>((item, ct) => capturedItem = item)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        Assert.NotNull(capturedItem);
        Assert.Equal(command.Name, capturedItem.Name);
        Assert.Equal(command.QuantityAvailable, capturedItem.QuantityAvailable);
        Assert.Equal(command.MinimumQuantity, capturedItem.MinimumQuantity);
        Assert.Equal(command.NotifyOnBelowMinimumQuantity, capturedItem.NotifyOnBelowMinimumQuantity);
        Assert.Equal(result, capturedItem.Id);

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<InventoryItem>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnInventoryItemId_WhenItemCreated()
    {
        // Arrange
        var command = new CreateInventoryItemCommand(
            Name: "Another Item",
            QuantityAvailable: 50.0m,
            MinimumQuantity: 5.0m,
            NotifyOnBelowMinimumQuantity: false
        );

        InventoryItem? createdItem = null;
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<InventoryItem>(), It.IsAny<CancellationToken>()))
            .Callback<InventoryItem, CancellationToken>((item, ct) => createdItem = item)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        Assert.NotNull(createdItem);
        Assert.Equal(result, createdItem.Id);
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationToken_ToRepository()
    {
        // Arrange
        var command = new CreateInventoryItemCommand(
            Name: "Test Item",
            QuantityAvailable: 100.0m,
            MinimumQuantity: 10.0m,
            NotifyOnBelowMinimumQuantity: true
        );

        var cancellationToken = new CancellationTokenSource().Token;

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<InventoryItem>(), cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<InventoryItem>(), cancellationToken),
            Times.Once);
    }
}
