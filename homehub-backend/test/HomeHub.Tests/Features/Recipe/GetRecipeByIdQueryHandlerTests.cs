using HomeHub.Application.Features.Recipe.GetRecipeById;
using HomeHub.Application.Features.Recipe.Interfaces;
using HomeHub.Domain.Entities;
using RecipeEntity = HomeHub.Domain.Entities.Recipe;
using Moq;
using Xunit;

namespace HomeHub.Tests.Features.Recipe;

public class GetRecipeByIdQueryHandlerTests
{
    private readonly Mock<IRecipeRepository> _repositoryMock;
    private readonly GetRecipeByIdQueryHandler _handler;

    public GetRecipeByIdQueryHandlerTests()
    {
        _repositoryMock = new Mock<IRecipeRepository>();
        _handler = new GetRecipeByIdQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnRecipe_WhenRecipeExists()
    {
        // Arrange
        var recipeId = Guid.NewGuid();
        var expectedRecipe = new RecipeEntity("Test Recipe", "Test Description");

        // Use reflection to set the Id since it's protected
        var idProperty = typeof(RecipeEntity).GetProperty("Id", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        idProperty?.SetValue(expectedRecipe, recipeId);

        var query = new GetRecipeByIdQuery(recipeId);

        _repositoryMock
            .Setup(r => r.GetByIdWithRelationsAsync(recipeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedRecipe);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedRecipe.Id, result.Id);
        Assert.Equal(expectedRecipe.Title, result.Title);
        Assert.Equal(expectedRecipe.Description, result.Description);

        _repositoryMock.Verify(
            r => r.GetByIdWithRelationsAsync(recipeId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenRecipeNotFound()
    {
        // Arrange
        var recipeId = Guid.NewGuid();
        var query = new GetRecipeByIdQuery(recipeId);

        _repositoryMock
            .Setup(r => r.GetByIdWithRelationsAsync(recipeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RecipeEntity?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.Handle(query, CancellationToken.None));

        Assert.Contains(recipeId.ToString(), exception.Message);
        _repositoryMock.Verify(
            r => r.GetByIdWithRelationsAsync(recipeId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationToken_ToRepository()
    {
        // Arrange
        var recipeId = Guid.NewGuid();
        var expectedRecipe = new RecipeEntity("Test Recipe", "Test Description");

        var idProperty = typeof(RecipeEntity).GetProperty("Id", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        idProperty?.SetValue(expectedRecipe, recipeId);

        var query = new GetRecipeByIdQuery(recipeId);
        var cancellationToken = new CancellationTokenSource().Token;

        _repositoryMock
            .Setup(r => r.GetByIdWithRelationsAsync(recipeId, cancellationToken))
            .ReturnsAsync(expectedRecipe);

        // Act
        await _handler.Handle(query, cancellationToken);

        // Assert
        _repositoryMock.Verify(
            r => r.GetByIdWithRelationsAsync(recipeId, cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectRecipe_WhenMultipleRecipesExist()
    {
        // Arrange
        var recipeId1 = Guid.NewGuid();
        var recipeId2 = Guid.NewGuid();

        var recipe1 = new RecipeEntity("Recipe 1", "Description 1");
        var recipe2 = new RecipeEntity("Recipe 2", "Description 2");

        var idProperty = typeof(RecipeEntity).GetProperty("Id", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        idProperty?.SetValue(recipe1, recipeId1);
        idProperty?.SetValue(recipe2, recipeId2);

        var query = new GetRecipeByIdQuery(recipeId2);

        _repositoryMock
            .Setup(r => r.GetByIdWithRelationsAsync(recipeId1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recipe1);

        _repositoryMock
            .Setup(r => r.GetByIdWithRelationsAsync(recipeId2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recipe2);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(recipe2.Id, result.Id);
        Assert.Equal("Recipe 2", result.Title);
        Assert.Equal("Description 2", result.Description);

        _repositoryMock.Verify(
            r => r.GetByIdWithRelationsAsync(recipeId2, It.IsAny<CancellationToken>()),
            Times.Once);
        _repositoryMock.Verify(
            r => r.GetByIdWithRelationsAsync(recipeId1, It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldIncludeStepsAndIngredients_WhenRecipeExists()
    {
        // Arrange
        var recipeId = Guid.NewGuid();
        var inventoryItem = new InventoryItem("Flour", 100.0m, 10.0m);

        var recipe = new RecipeEntity("Test Recipe", "Test Description");
        var idProperty = typeof(RecipeEntity).GetProperty("Id", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        idProperty?.SetValue(recipe, recipeId);

        // Add steps and ingredients using reflection to access private methods
        var addStepMethod = typeof(RecipeEntity).GetMethod("AddStep", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        addStepMethod?.Invoke(recipe, new object[] { 1, "Step 1" });
        addStepMethod?.Invoke(recipe, new object[] { 2, "Step 2" });

        var addIngredientMethod = typeof(RecipeEntity).GetMethod("AddIngredient", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        addIngredientMethod?.Invoke(recipe, new object[] { inventoryItem.Id, 2.5m });

        var query = new GetRecipeByIdQuery(recipeId);

        _repositoryMock
            .Setup(r => r.GetByIdWithRelationsAsync(recipeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recipe);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Steps.Count);
        Assert.Single(result.Ingredients);

        // Verify that GetByIdWithRelationsAsync is called
        _repositoryMock.Verify(
            r => r.GetByIdWithRelationsAsync(recipeId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}