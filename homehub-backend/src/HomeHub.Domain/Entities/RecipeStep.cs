using System.Text.Json.Serialization;
using HomeHub.Domain.Common;

namespace HomeHub.Domain.Entities;

public class RecipeStep : BaseEntity
{
    public Guid RecipeId { get; internal set; }

    [JsonIgnore]
    public Recipe Recipe { get; private set; } = null!;

    public int Order { get; private set; }
    public string Description { get; private set; } = string.Empty;

    private RecipeStep()
    {
        // EF Core constructor
    }

    public RecipeStep(Guid recipeId, int order, string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Step description cannot be null or empty.", nameof(description));

        RecipeId = recipeId;
        Order = order;
        Description = description;
    }

    public void UpdateOrder(int order)
    {
        Order = order;
    }

    public void UpdateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Step description cannot be null or empty.", nameof(description));

        Description = description;
    }
}

