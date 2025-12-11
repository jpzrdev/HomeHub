using MediatR;

namespace HomeHub.Application.Features.ShoppingList.GenerateShoppingList;

public record GenerateShoppingListCommand() : IRequest<Guid>;
