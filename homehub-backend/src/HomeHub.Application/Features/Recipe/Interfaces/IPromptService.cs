namespace HomeHub.Application.Features.Recipe.Interfaces;

public interface IPromptService
{
    Task<string> GetPromptAsync(string promptId, CancellationToken cancellationToken);
}
