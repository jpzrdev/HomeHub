using HomeHub.Application.Features.Inventory.Interfaces;
using HomeHub.Application.Features.Recipe.CreateRecipe;
using HomeHub.Application.Features.Recipe.Interfaces;
using HomeHub.Domain.Entities;
using RecipeEntity = HomeHub.Domain.Entities.Recipe;
using Moq;
using Xunit;

namespace HomeHub.Tests.Features.Recipe;

public class CreateRecipeCommandHandlerTests
{
    private readonly Mock<IRecipeRepository> _recipeRepositoryMock;
    private readonly Mock<IInventoryItemRepository> _inventoryItemRepositoryMock;
    private readonly CreateRecipeCommandHandler _handler;

    public CreateRecipeCommandHandlerTests()
    {
        _recipeRepositoryMock = new Mock<IRecipeRepository>();
        _inventoryItemRepositoryMock = new Mock<IInventoryItemRepository>();
        _handler = new CreateRecipeCommandHandler(
            _recipeRepositoryMock.Object,
            _inventoryItemRepositoryMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldCreateRecipe_WhenValidCommandProvided()
    {
        // Arrange
        var inventoryItemId1 = Guid.NewGuid();
        var inventoryItemId2 = Guid.NewGuid();

        var inventoryItem1 = new InventoryItem(
            name: "Flour",
            quantityAvailable: 100.0m,
            minimumQuantity: 10.0m
        );

        var inventoryItem2 = new InventoryItem(
            name: "Sugar",
            quantityAvailable: 50.0m,
            minimumQuantity: 5.0m
        );

        var command = new CreateRecipeCommand(
            Title: "Test Recipe",
            Description: "A test recipe description",
            Steps: new List<RecipeStepDto>
            {
                new RecipeStepDto(Order: 1, Description: "First step"),
                new RecipeStepDto(Order: 2, Description: "Second step")
            },
            Ingredients: new List<RecipeIngredientDto>
            {
                new RecipeIngredientDto(inventoryItemId1, Quantity: 2.5m),
                new RecipeIngredientDto(inventoryItemId2, Quantity: 1.0m)
            }
        );

        _inventoryItemRepositoryMock
            .Setup(r => r.GetByIdAsync(inventoryItemId1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventoryItem1);

        _inventoryItemRepositoryMock
            .Setup(r => r.GetByIdAsync(inventoryItemId2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventoryItem2);

        RecipeEntity? capturedRecipe = null;
        _recipeRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<RecipeEntity>(), It.IsAny<CancellationToken>()))
            .Callback<RecipeEntity, CancellationToken>((recipe, ct) => capturedRecipe = recipe)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        Assert.NotNull(capturedRecipe);
        Assert.Equal(command.Title, capturedRecipe.Title);
        Assert.Equal(command.Description, capturedRecipe.Description);
        Assert.Equal(2, capturedRecipe.Steps.Count);
        Assert.Equal(2, capturedRecipe.Ingredients.Count);
        Assert.Equal(result, capturedRecipe.Id);

        _inventoryItemRepositoryMock.Verify(
            r => r.GetByIdAsync(inventoryItemId1, It.IsAny<CancellationToken>()),
            Times.Once);
        _inventoryItemRepositoryMock.Verify(
            r => r.GetByIdAsync(inventoryItemId2, It.IsAny<CancellationToken>()),
            Times.Once);
        _recipeRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<RecipeEntity>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldAddStepsInCorrectOrder_WhenMultipleStepsProvided()
    {
        // Arrange
        var command = new CreateRecipeCommand(
            Title: "Test Recipe",
            Description: "Test description",
            Steps: new List<RecipeStepDto>
            {
                new RecipeStepDto(Order: 3, Description: "Third step"),
                new RecipeStepDto(Order: 1, Description: "First step"),
                new RecipeStepDto(Order: 2, Description: "Second step")
            },
            Ingredients: new List<RecipeIngredientDto>()
        );

        RecipeEntity? capturedRecipe = null;
        _recipeRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<RecipeEntity>(), It.IsAny<CancellationToken>()))
            .Callback<RecipeEntity, CancellationToken>((recipe, ct) => capturedRecipe = recipe)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedRecipe);
        Assert.Equal(3, capturedRecipe.Steps.Count);
        
        var stepsList = capturedRecipe.Steps.ToList();
        Assert.Equal(1, stepsList[0].Order);
        Assert.Equal("First step", stepsList[0].Description);
        Assert.Equal(2, stepsList[1].Order);
        Assert.Equal("Second step", stepsList[1].Description);
        Assert.Equal(3, stepsList[2].Order);
        Assert.Equal("Third step", stepsList[2].Description);
    }

    [Fact]
    public async Task Handle_ShouldLinkIngredientsToInventoryItems_WhenIngredientsProvided()
    {
        // Arrange
        var inventoryItemId = Guid.NewGuid();
        var inventoryItem = new InventoryItem(
            name: "Test Ingredient",
            quantityAvailable: 100.0m,
            minimumQuantity: 10.0m
        );

        var command = new CreateRecipeCommand(
            Title: "Test Recipe",
            Description: "Test description",
            Steps: new List<RecipeStepDto>(),
            Ingredients: new List<RecipeIngredientDto>
            {
                new RecipeIngredientDto(inventoryItemId, Quantity: 5.5m)
            }
        );

        _inventoryItemRepositoryMock
            .Setup(r => r.GetByIdAsync(inventoryItemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventoryItem);

        RecipeEntity? capturedRecipe = null;
        _recipeRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<RecipeEntity>(), It.IsAny<CancellationToken>()))
            .Callback<RecipeEntity, CancellationToken>((recipe, ct) => capturedRecipe = recipe)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedRecipe);
        Assert.Single(capturedRecipe.Ingredients);
        var ingredient = capturedRecipe.Ingredients.First();
        Assert.Equal(inventoryItemId, ingredient.InventoryItemId);
        Assert.Equal(5.5m, ingredient.Quantity);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenInventoryItemNotFound()
    {
        // Arrange
        var nonExistentInventoryItemId = Guid.NewGuid();

        var command = new CreateRecipeCommand(
            Title: "Test Recipe",
            Description: "Test description",
            Steps: new List<RecipeStepDto>(),
            Ingredients: new List<RecipeIngredientDto>
            {
                new RecipeIngredientDto(nonExistentInventoryItemId, Quantity: 1.0m)
            }
        );

        _inventoryItemRepositoryMock
            .Setup(r => r.GetByIdAsync(nonExistentInventoryItemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((InventoryItem?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.Contains(nonExistentInventoryItemId.ToString(), exception.Message);

        _inventoryItemRepositoryMock.Verify(
            r => r.GetByIdAsync(nonExistentInventoryItemId, It.IsAny<CancellationToken>()),
            Times.Once);
        _recipeRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<RecipeEntity>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCreateRecipeWithEmptySteps_WhenNoStepsProvided()
    {
        // Arrange
        var command = new CreateRecipeCommand(
            Title: "Test Recipe",
            Description: "Test description",
            Steps: new List<RecipeStepDto>(),
            Ingredients: new List<RecipeIngredientDto>()
        );

        RecipeEntity? capturedRecipe = null;
        _recipeRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<RecipeEntity>(), It.IsAny<CancellationToken>()))
            .Callback<RecipeEntity, CancellationToken>((recipe, ct) => capturedRecipe = recipe)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedRecipe);
        Assert.Empty(capturedRecipe.Steps);
        Assert.Empty(capturedRecipe.Ingredients);
    }

    [Fact]
    public async Task Handle_ShouldCreateRecipeWithEmptyIngredients_WhenNoIngredientsProvided()
    {
        // Arrange
        var command = new CreateRecipeCommand(
            Title: "Test Recipe",
            Description: "Test description",
            Steps: new List<RecipeStepDto>
            {
                new RecipeStepDto(Order: 1, Description: "Step 1")
            },
            Ingredients: new List<RecipeIngredientDto>()
        );

        RecipeEntity? capturedRecipe = null;
        _recipeRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<RecipeEntity>(), It.IsAny<CancellationToken>()))
            .Callback<RecipeEntity, CancellationToken>((recipe, ct) => capturedRecipe = recipe)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedRecipe);
        Assert.Single(capturedRecipe.Steps);
        Assert.Empty(capturedRecipe.Ingredients);

        _inventoryItemRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenFirstInventoryItemNotFound()
    {
        // Arrange
        var existingInventoryItemId = Guid.NewGuid();
        var nonExistentInventoryItemId = Guid.NewGuid();

        var existingInventoryItem = new InventoryItem(
            name: "Existing Item",
            quantityAvailable: 100.0m,
            minimumQuantity: 10.0m
        );

        var command = new CreateRecipeCommand(
            Title: "Test Recipe",
            Description: "Test description",
            Steps: new List<RecipeStepDto>(),
            Ingredients: new List<RecipeIngredientDto>
            {
                new RecipeIngredientDto(nonExistentInventoryItemId, Quantity: 1.0m),
                new RecipeIngredientDto(existingInventoryItemId, Quantity: 2.0m)
            }
        );

        _inventoryItemRepositoryMock
            .Setup(r => r.GetByIdAsync(nonExistentInventoryItemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((InventoryItem?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.Contains(nonExistentInventoryItemId.ToString(), exception.Message);

        _inventoryItemRepositoryMock.Verify(
            r => r.GetByIdAsync(nonExistentInventoryItemId, It.IsAny<CancellationToken>()),
            Times.Once);
        _inventoryItemRepositoryMock.Verify(
            r => r.GetByIdAsync(existingInventoryItemId, It.IsAny<CancellationToken>()),
            Times.Never);
        _recipeRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<RecipeEntity>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnRecipeId_WhenRecipeCreated()
    {
        // Arrange
        var command = new CreateRecipeCommand(
            Title: "Test Recipe",
            Description: "Test description",
            Steps: new List<RecipeStepDto>
            {
                new RecipeStepDto(Order: 1, Description: "Step 1")
            },
            Ingredients: new List<RecipeIngredientDto>()
        );

        RecipeEntity? capturedRecipe = null;
        _recipeRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<RecipeEntity>(), It.IsAny<CancellationToken>()))
            .Callback<RecipeEntity, CancellationToken>((recipe, ct) => capturedRecipe = recipe)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        Assert.NotNull(capturedRecipe);
        Assert.Equal(result, capturedRecipe.Id);
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationToken_ToRepositories()
    {
        // Arrange
        var inventoryItemId = Guid.NewGuid();
        var inventoryItem = new InventoryItem(
            name: "Test Item",
            quantityAvailable: 100.0m,
            minimumQuantity: 10.0m
        );

        var command = new CreateRecipeCommand(
            Title: "Test Recipe",
            Description: "Test description",
            Steps: new List<RecipeStepDto>(),
            Ingredients: new List<RecipeIngredientDto>
            {
                new RecipeIngredientDto(inventoryItemId, Quantity: 1.0m)
            }
        );

        var cancellationToken = new CancellationTokenSource().Token;

        _inventoryItemRepositoryMock
            .Setup(r => r.GetByIdAsync(inventoryItemId, cancellationToken))
            .ReturnsAsync(inventoryItem);

        _recipeRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<RecipeEntity>(), cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        _inventoryItemRepositoryMock.Verify(
            r => r.GetByIdAsync(inventoryItemId, cancellationToken),
            Times.Once);
        _recipeRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<RecipeEntity>(), cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldValidateAllInventoryItems_WhenMultipleIngredientsProvided()
    {
        // Arrange
        var inventoryItemId1 = Guid.NewGuid();
        var inventoryItemId2 = Guid.NewGuid();
        var inventoryItemId3 = Guid.NewGuid();

        var inventoryItem1 = new InventoryItem(
            name: "Item 1",
            quantityAvailable: 100.0m,
            minimumQuantity: 10.0m
        );

        var inventoryItem2 = new InventoryItem(
            name: "Item 2",
            quantityAvailable: 50.0m,
            minimumQuantity: 5.0m
        );

        var inventoryItem3 = new InventoryItem(
            name: "Item 3",
            quantityAvailable: 25.0m,
            minimumQuantity: 2.5m
        );

        var command = new CreateRecipeCommand(
            Title: "Test Recipe",
            Description: "Test description",
            Steps: new List<RecipeStepDto>(),
            Ingredients: new List<RecipeIngredientDto>
            {
                new RecipeIngredientDto(inventoryItemId1, Quantity: 1.0m),
                new RecipeIngredientDto(inventoryItemId2, Quantity: 2.0m),
                new RecipeIngredientDto(inventoryItemId3, Quantity: 3.0m)
            }
        );

        _inventoryItemRepositoryMock
            .Setup(r => r.GetByIdAsync(inventoryItemId1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventoryItem1);

        _inventoryItemRepositoryMock
            .Setup(r => r.GetByIdAsync(inventoryItemId2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventoryItem2);

        _inventoryItemRepositoryMock
            .Setup(r => r.GetByIdAsync(inventoryItemId3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventoryItem3);

        RecipeEntity? capturedRecipe = null;
        _recipeRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<RecipeEntity>(), It.IsAny<CancellationToken>()))
            .Callback<RecipeEntity, CancellationToken>((recipe, ct) => capturedRecipe = recipe)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedRecipe);
        Assert.Equal(3, capturedRecipe.Ingredients.Count);

        _inventoryItemRepositoryMock.Verify(
            r => r.GetByIdAsync(inventoryItemId1, It.IsAny<CancellationToken>()),
            Times.Once);
        _inventoryItemRepositoryMock.Verify(
            r => r.GetByIdAsync(inventoryItemId2, It.IsAny<CancellationToken>()),
            Times.Once);
        _inventoryItemRepositoryMock.Verify(
            r => r.GetByIdAsync(inventoryItemId3, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

