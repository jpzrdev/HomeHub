using System.Text.Json;
using HomeHub.Application.Features.Recipe.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;

namespace HomeHub.Infrastructure.Services;

public class OpenAiRecipeService : IAiRecipeService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<OpenAiRecipeService> _logger;
    private readonly IPromptService _promptService;
    private readonly ChatClient? _chatClient;

    public OpenAiRecipeService(
        IConfiguration configuration,
        ILogger<OpenAiRecipeService> logger,
        IPromptService promptService)
    {
        _configuration = configuration;
        _logger = logger;
        _promptService = promptService;

        var apiKey = _configuration["OpenAI:ApiKey"];
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            _chatClient = new ChatClient(model: "gpt-4o-mini", apiKey: apiKey);
        }
    }

    public async Task<List<GeneratedRecipe>> GenerateRecipesAsync(
        List<string> inventoryItemNames,
        string? userDescription,
        string promptId,
        CancellationToken cancellationToken)
    {
        if (inventoryItemNames == null || inventoryItemNames.Count == 0)
        {
            throw new ArgumentException("At least one inventory item name must be provided.", nameof(inventoryItemNames));
        }

        if (_chatClient == null)
        {
            throw new InvalidOperationException("OpenAI API key is not configured. Please set 'OpenAI:ApiKey' in appsettings.json");
        }

        var promptTemplate = await _promptService.GetPromptAsync(promptId, cancellationToken);
        var prompt = BuildPrompt(promptTemplate, inventoryItemNames, userDescription);

        try
        {
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are a helpful cooking assistant that generates recipes in JSON format. Always return exactly 3 recipes."),
                new UserChatMessage(prompt)
            };

            var completion = await _chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);

            var content = completion.Value.Content[0].Text;
            _logger.LogInformation("OpenAI response received: {Content}", content);

            return ParseRecipesFromJson(content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recipes from OpenAI");
            throw new InvalidOperationException("Failed to generate recipes from AI service.", ex);
        }
    }

    private string BuildPrompt(string promptTemplate, List<string> inventoryItemNames, string? userDescription)
    {
        var itemsList = string.Join(", ", inventoryItemNames);

        var userDescriptionText = string.IsNullOrWhiteSpace(userDescription)
            ? string.Empty
            : $" Additional requirements or preferences: {userDescription}";

        // Replace placeholders in the template
        var prompt = string.Format(promptTemplate, itemsList, userDescriptionText);

        return prompt;
    }

    private List<GeneratedRecipe> ParseRecipesFromJson(string jsonContent)
    {
        try
        {
            // Clean up the JSON if it's wrapped in markdown code blocks
            var cleanedJson = jsonContent.Trim();
            if (cleanedJson.StartsWith("```json"))
            {
                cleanedJson = cleanedJson.Substring(7);
            }
            if (cleanedJson.StartsWith("```"))
            {
                cleanedJson = cleanedJson.Substring(3);
            }
            if (cleanedJson.EndsWith("```"))
            {
                cleanedJson = cleanedJson.Substring(0, cleanedJson.Length - 3);
            }
            cleanedJson = cleanedJson.Trim();

            using var document = JsonDocument.Parse(cleanedJson);
            var root = document.RootElement;

            if (!root.TryGetProperty("recipes", out var recipesElement))
            {
                throw new InvalidOperationException("Invalid JSON response: 'recipes' property not found.");
            }

            var recipes = new List<GeneratedRecipe>();

            foreach (var recipeElement in recipesElement.EnumerateArray())
            {
                var title = recipeElement.GetProperty("title").GetString() ?? string.Empty;
                var description = recipeElement.GetProperty("description").GetString() ?? string.Empty;

                var steps = new List<GeneratedRecipeStep>();
                if (recipeElement.TryGetProperty("steps", out var stepsElement))
                {
                    foreach (var stepElement in stepsElement.EnumerateArray())
                    {
                        var order = stepElement.GetProperty("order").GetInt32();
                        var stepDescription = stepElement.GetProperty("description").GetString() ?? string.Empty;
                        steps.Add(new GeneratedRecipeStep(order, stepDescription));
                    }
                }

                var ingredients = new List<GeneratedRecipeIngredient>();
                if (recipeElement.TryGetProperty("ingredients", out var ingredientsElement))
                {
                    foreach (var ingredientElement in ingredientsElement.EnumerateArray())
                    {
                        var inventoryItemName = ingredientElement.GetProperty("inventoryItemName").GetString() ?? string.Empty;
                        var quantity = ingredientElement.GetProperty("quantity").GetDecimal();
                        ingredients.Add(new GeneratedRecipeIngredient(inventoryItemName, quantity));
                    }
                }

                recipes.Add(new GeneratedRecipe(title, description, steps, ingredients));
            }

            if (recipes.Count != 3)
            {
                _logger.LogWarning("Expected 3 recipes but got {Count}", recipes.Count);
            }

            return recipes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing recipes from JSON: {JsonContent}", jsonContent);
            throw new InvalidOperationException("Failed to parse recipes from AI response.", ex);
        }
    }
}
