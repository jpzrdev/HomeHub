using HomeHub.Application.Features.Inventory.Interfaces;
using HomeHub.Application.Features.Recipe.GenerateRecipesFromInventory;
using HomeHub.Application.Features.Recipe.Interfaces;
using HomeHub.Domain.Entities;
using Moq;
using Xunit;

namespace HomeHub.Tests.Features.Recipe;

public class GenerateRecipesFromInventoryCommandHandlerTests
{
    private readonly Mock<IAiRecipeService> _aiRecipeServiceMock;
    private readonly Mock<IInventoryItemRepository> _inventoryItemRepositoryMock;
    private readonly GenerateRecipesFromInventoryCommandHandler _handler;

    public GenerateRecipesFromInventoryCommandHandlerTests()
    {
        _aiRecipeServiceMock = new Mock<IAiRecipeService>();
        _inventoryItemRepositoryMock = new Mock<IInventoryItemRepository>();
        _handler = new GenerateRecipesFromInventoryCommandHandler(
            _aiRecipeServiceMock.Object,
            _inventoryItemRepositoryMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldGenerateAndReturnThreeRecipes_WhenValidCommandProvided()
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

        var command = new GenerateRecipesFromInventoryCommand(
            InventoryItemIds: new List<Guid> { inventoryItemId1, inventoryItemId2 },
            UserDescription: "Make it healthy"
        );

        var generatedRecipes = new List<GeneratedRecipe>
        {
            new GeneratedRecipe(
                Title: "Recipe 1",
                Description: "Description 1",
                Steps: new List<GeneratedRecipeStep>
                {
                    new GeneratedRecipeStep(Order: 1, Description: "Step 1"),
                    new GeneratedRecipeStep(Order: 2, Description: "Step 2")
                },
                Ingredients: new List<GeneratedRecipeIngredient>
                {
                    new GeneratedRecipeIngredient("Flour", 2.0m),
                    new GeneratedRecipeIngredient("Sugar", 1.0m)
                }
            ),
            new GeneratedRecipe(
                Title: "Recipe 2",
                Description: "Description 2",
                Steps: new List<GeneratedRecipeStep>
                {
                    new GeneratedRecipeStep(Order: 1, Description: "Step 1")
                },
                Ingredients: new List<GeneratedRecipeIngredient>
                {
                    new GeneratedRecipeIngredient("Flour", 1.5m)
                }
            ),
            new GeneratedRecipe(
                Title: "Recipe 3",
                Description: "Description 3",
                Steps: new List<GeneratedRecipeStep>
                {
                    new GeneratedRecipeStep(Order: 1, Description: "Step 1"),
                    new GeneratedRecipeStep(Order: 2, Description: "Step 2"),
                    new GeneratedRecipeStep(Order: 3, Description: "Step 3")
                },
                Ingredients: new List<GeneratedRecipeIngredient>
                {
                    new GeneratedRecipeIngredient("Sugar", 0.5m)
                }
            )
        };

        _inventoryItemRepositoryMock
            .Setup(r => r.GetByIdAsync(inventoryItemId1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventoryItem1);

        _inventoryItemRepositoryMock
            .Setup(r => r.GetByIdAsync(inventoryItemId2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventoryItem2);

        _aiRecipeServiceMock
            .Setup(s => s.GenerateRecipesAsync(
                It.Is<List<string>>(names => names.Contains("Flour") && names.Contains("Sugar")),
                "Make it healthy",
                "generate-recipes-from-inventory",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(generatedRecipes);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);

        Assert.Equal("Recipe 1", result[0].Title);
        Assert.Equal("Description 1", result[0].Description);
        Assert.Equal(2, result[0].Steps.Count);
        Assert.Equal(2, result[0].Ingredients.Count);
        Assert.Equal(inventoryItemId1, result[0].Ingredients[0].InventoryItemId);
        Assert.Equal("Flour", result[0].Ingredients[0].InventoryItemName);
        Assert.Equal(2.0m, result[0].Ingredients[0].Quantity);
        Assert.Equal(inventoryItemId2, result[0].Ingredients[1].InventoryItemId);
        Assert.Equal("Sugar", result[0].Ingredients[1].InventoryItemName);
        Assert.Equal(1.0m, result[0].Ingredients[1].Quantity);

        Assert.Equal("Recipe 2", result[1].Title);
        Assert.Single(result[1].Steps);
        Assert.Single(result[1].Ingredients);
        Assert.Equal(inventoryItemId1, result[1].Ingredients[0].InventoryItemId);
        Assert.Equal(1.5m, result[1].Ingredients[0].Quantity);

        Assert.Equal("Recipe 3", result[2].Title);
        Assert.Equal(3, result[2].Steps.Count);
        Assert.Single(result[2].Ingredients);
        Assert.Equal(inventoryItemId2, result[2].Ingredients[0].InventoryItemId);
        Assert.Equal(0.5m, result[2].Ingredients[0].Quantity);

        _aiRecipeServiceMock.Verify(
            s => s.GenerateRecipesAsync(
                It.IsAny<List<string>>(),
                "Make it healthy",
                "generate-recipes-from-inventory",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowArgumentException_WhenNoInventoryItemIdsProvided()
    {
        // Arrange
        var command = new GenerateRecipesFromInventoryCommand(
            InventoryItemIds: new List<Guid>(),
            UserDescription: null
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.Handle(command, CancellationToken.None));

        _aiRecipeServiceMock.Verify(
            s => s.GenerateRecipesAsync(It.IsAny<List<string>>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenInventoryItemNotFound()
    {
        // Arrange
        var nonExistentInventoryItemId = Guid.NewGuid();

        var command = new GenerateRecipesFromInventoryCommand(
            InventoryItemIds: new List<Guid> { nonExistentInventoryItemId },
            UserDescription: null
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

        _aiRecipeServiceMock.Verify(
            s => s.GenerateRecipesAsync(It.IsAny<List<string>>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldMatchIngredientsByCaseInsensitiveName_WhenGeneratingRecipes()
    {
        // Arrange
        var inventoryItemId = Guid.NewGuid();
        var inventoryItem = new InventoryItem(
            name: "Flour",
            quantityAvailable: 100.0m,
            minimumQuantity: 10.0m
        );

        var command = new GenerateRecipesFromInventoryCommand(
            InventoryItemIds: new List<Guid> { inventoryItemId },
            UserDescription: null
        );

        var generatedRecipes = new List<GeneratedRecipe>
        {
            new GeneratedRecipe(
                Title: "Test Recipe",
                Description: "Test",
                Steps: new List<GeneratedRecipeStep>
                {
                    new GeneratedRecipeStep(Order: 1, Description: "Step 1")
                },
                Ingredients: new List<GeneratedRecipeIngredient>
                {
                    new GeneratedRecipeIngredient("flour", 2.0m), // lowercase
                    new GeneratedRecipeIngredient("FLOUR", 1.0m)  // uppercase
                }
            ),
            new GeneratedRecipe(
                Title: "Test Recipe 2",
                Description: "Test",
                Steps: new List<GeneratedRecipeStep>(),
                Ingredients: new List<GeneratedRecipeIngredient>()
            ),
            new GeneratedRecipe(
                Title: "Test Recipe 3",
                Description: "Test",
                Steps: new List<GeneratedRecipeStep>(),
                Ingredients: new List<GeneratedRecipeIngredient>()
            )
        };

        _inventoryItemRepositoryMock
            .Setup(r => r.GetByIdAsync(inventoryItemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventoryItem);

        _aiRecipeServiceMock
            .Setup(s => s.GenerateRecipesAsync(
                It.IsAny<List<string>>(),
                It.IsAny<string?>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(generatedRecipes);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(2, result[0].Ingredients.Count);
        // Both ingredients should match the inventory item (case-insensitive)
        Assert.All(result[0].Ingredients, ingredient =>
        {
            Assert.Equal(inventoryItemId, ingredient.InventoryItemId);
            Assert.Equal("Flour", ingredient.InventoryItemName);
        });
    }

    [Fact]
    public async Task Handle_ShouldHandleNullUserDescription()
    {
        // Arrange
        var inventoryItemId = Guid.NewGuid();
        var inventoryItem = new InventoryItem(
            name: "Flour",
            quantityAvailable: 100.0m,
            minimumQuantity: 10.0m
        );

        var command = new GenerateRecipesFromInventoryCommand(
            InventoryItemIds: new List<Guid> { inventoryItemId },
            UserDescription: null
        );

        var generatedRecipes = new List<GeneratedRecipe>
        {
            new GeneratedRecipe(
                Title: "Recipe 1",
                Description: "Description",
                Steps: new List<GeneratedRecipeStep>(),
                Ingredients: new List<GeneratedRecipeIngredient>()
            ),
            new GeneratedRecipe(
                Title: "Recipe 2",
                Description: "Description",
                Steps: new List<GeneratedRecipeStep>(),
                Ingredients: new List<GeneratedRecipeIngredient>()
            ),
            new GeneratedRecipe(
                Title: "Recipe 3",
                Description: "Description",
                Steps: new List<GeneratedRecipeStep>(),
                Ingredients: new List<GeneratedRecipeIngredient>()
            )
        };

        _inventoryItemRepositoryMock
            .Setup(r => r.GetByIdAsync(inventoryItemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventoryItem);

        _aiRecipeServiceMock
            .Setup(s => s.GenerateRecipesAsync(
                It.IsAny<List<string>>(),
                null,
                "generate-recipes-from-inventory",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(generatedRecipes);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.All(result, recipe =>
        {
            Assert.NotNull(recipe.Title);
            Assert.NotNull(recipe.Description);
        });
        _aiRecipeServiceMock.Verify(
            s => s.GenerateRecipesAsync(
                It.IsAny<List<string>>(),
                null,
                "generate-recipes-from-inventory",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationToken_ToAllServices()
    {
        // Arrange
        var inventoryItemId = Guid.NewGuid();
        var inventoryItem = new InventoryItem(
            name: "Flour",
            quantityAvailable: 100.0m,
            minimumQuantity: 10.0m
        );

        var command = new GenerateRecipesFromInventoryCommand(
            InventoryItemIds: new List<Guid> { inventoryItemId },
            UserDescription: null
        );

        var cancellationToken = new CancellationTokenSource().Token;

        var generatedRecipes = new List<GeneratedRecipe>
        {
            new GeneratedRecipe(
                Title: "Recipe 1",
                Description: "Description",
                Steps: new List<GeneratedRecipeStep>(),
                Ingredients: new List<GeneratedRecipeIngredient>()
            ),
            new GeneratedRecipe(
                Title: "Recipe 2",
                Description: "Description",
                Steps: new List<GeneratedRecipeStep>(),
                Ingredients: new List<GeneratedRecipeIngredient>()
            ),
            new GeneratedRecipe(
                Title: "Recipe 3",
                Description: "Description",
                Steps: new List<GeneratedRecipeStep>(),
                Ingredients: new List<GeneratedRecipeIngredient>()
            )
        };

        _inventoryItemRepositoryMock
            .Setup(r => r.GetByIdAsync(inventoryItemId, cancellationToken))
            .ReturnsAsync(inventoryItem);

        _aiRecipeServiceMock
            .Setup(s => s.GenerateRecipesAsync(
                It.IsAny<List<string>>(),
                It.IsAny<string?>(),
                "generate-recipes-from-inventory",
                cancellationToken))
            .ReturnsAsync(generatedRecipes);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.Equal(3, result.Count);
        _inventoryItemRepositoryMock.Verify(
            r => r.GetByIdAsync(inventoryItemId, cancellationToken),
            Times.Once);

        _aiRecipeServiceMock.Verify(
            s => s.GenerateRecipesAsync(
                It.IsAny<List<string>>(),
                It.IsAny<string?>(),
                "generate-recipes-from-inventory",
                cancellationToken),
            Times.Once);
    }
}
