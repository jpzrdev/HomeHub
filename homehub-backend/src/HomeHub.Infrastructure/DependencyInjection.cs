using HomeHub.Application.Features.Inventory.Interfaces;
using HomeHub.Application.Features.Recipe.Interfaces;
using HomeHub.Application.Features.ShoppingList.Interfaces;
using HomeHub.Infrastructure.Data;
using HomeHub.Infrastructure.Data.Repositories;
using HomeHub.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HomeHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<HomeHubContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IInventoryItemRepository, InventoryItemRepository>();
        services.AddScoped<IShoppingListRepository, ShoppingListRepository>();
        services.AddScoped<IRecipeRepository, RecipeRepository>();
        services.AddScoped<IPromptService, FilePromptService>();
        services.AddScoped<IAiRecipeService, OpenAiRecipeService>();

        return services;
    }
}