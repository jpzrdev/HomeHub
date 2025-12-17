using HomeHub.Application.Common.Pagination;
using HomeHub.Application.Features.Inventory.GetAll;
using HomeHub.Application.Features.Inventory.GetAllInventoryItems;
using HomeHub.Application.Features.Inventory.Interfaces;
using HomeHub.Domain.Entities;
using Moq;
using Xunit;

namespace HomeHub.Tests.Features.Inventory;

public class GetAllInventoryItemsQueryHandlerTests
{
    private readonly Mock<IInventoryItemRepository> _repositoryMock;
    private readonly GetAllInventoryItemsQueryHandler _handler;

    public GetAllInventoryItemsQueryHandlerTests()
    {
        _repositoryMock = new Mock<IInventoryItemRepository>();
        _handler = new GetAllInventoryItemsQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedResult_WhenItemsExist()
    {
        // Arrange
        var items = new List<InventoryItem>
        {
            new InventoryItem("Item 1", 100.0m, 10.0m),
            new InventoryItem("Item 2", 200.0m, 20.0m),
            new InventoryItem("Item 3", 300.0m, 30.0m)
        };

        var paginationResult = new PaginationResult<InventoryItem>(
            items: items,
            totalCount: 3,
            pageNumber: 1,
            pageSize: 10
        );

        var query = new GetAllInventoryItemsQuery
        {
            PageNumber = 1,
            PageSize = 10
        };

        _repositoryMock
            .Setup(r => r.GetPaginatedAsync(1, 10, It.IsAny<CancellationToken>(), null, null, null))
            .ReturnsAsync(paginationResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(3, result.Items.Count());
        Assert.True(result.HasNextPage == false);
        Assert.False(result.HasPreviousPage);

        _repositoryMock.Verify(
            r => r.GetPaginatedAsync(1, 10, It.IsAny<CancellationToken>(), null, null, null),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyResult_WhenNoItemsExist()
    {
        // Arrange
        var paginationResult = new PaginationResult<InventoryItem>(
            items: Enumerable.Empty<InventoryItem>(),
            totalCount: 0,
            pageNumber: 1,
            pageSize: 10
        );

        var query = new GetAllInventoryItemsQuery
        {
            PageNumber = 1,
            PageSize = 10
        };

        _repositoryMock
            .Setup(r => r.GetPaginatedAsync(1, 10, It.IsAny<CancellationToken>(), null, null, null))
            .ReturnsAsync(paginationResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);
        Assert.False(result.HasNextPage);
        Assert.False(result.HasPreviousPage);
    }

    [Fact]
    public async Task Handle_ShouldHandlePagination_WhenMultiplePages()
    {
        // Arrange
        var items = new List<InventoryItem>
        {
            new InventoryItem("Item 1", 100.0m, 10.0m),
            new InventoryItem("Item 2", 200.0m, 20.0m)
        };

        var paginationResult = new PaginationResult<InventoryItem>(
            items: items,
            totalCount: 25,
            pageNumber: 2,
            pageSize: 10
        );

        var query = new GetAllInventoryItemsQuery
        {
            PageNumber = 2,
            PageSize = 10
        };

        _repositoryMock
            .Setup(r => r.GetPaginatedAsync(2, 10, It.IsAny<CancellationToken>(), null, null, null))
            .ReturnsAsync(paginationResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(25, result.TotalCount);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(2, result.Items.Count());
        Assert.True(result.HasNextPage);
        Assert.True(result.HasPreviousPage);
        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationToken_ToRepository()
    {
        // Arrange
        var paginationResult = new PaginationResult<InventoryItem>(
            items: Enumerable.Empty<InventoryItem>(),
            totalCount: 0,
            pageNumber: 1,
            pageSize: 10
        );

        var query = new GetAllInventoryItemsQuery
        {
            PageNumber = 1,
            PageSize = 10
        };

        var cancellationToken = new CancellationTokenSource().Token;

        _repositoryMock
            .Setup(r => r.GetPaginatedAsync(1, 10, cancellationToken, null, null, null))
            .ReturnsAsync(paginationResult);

        // Act
        await _handler.Handle(query, cancellationToken);

        // Assert
        _repositoryMock.Verify(
            r => r.GetPaginatedAsync(1, 10, cancellationToken, null, null, null),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUseQueryPageNumberAndPageSize()
    {
        // Arrange
        var paginationResult = new PaginationResult<InventoryItem>(
            items: Enumerable.Empty<InventoryItem>(),
            totalCount: 0,
            pageNumber: 5,
            pageSize: 20
        );

        var query = new GetAllInventoryItemsQuery
        {
            PageNumber = 5,
            PageSize = 20
        };

        _repositoryMock
            .Setup(r => r.GetPaginatedAsync(5, 20, It.IsAny<CancellationToken>(), null, null, null))
            .ReturnsAsync(paginationResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.PageNumber);
        Assert.Equal(20, result.PageSize);

        _repositoryMock.Verify(
            r => r.GetPaginatedAsync(5, 20, It.IsAny<CancellationToken>(), null, null, null),
            Times.Once);
    }
}
