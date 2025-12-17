using HomeHub.Application.Features.Recipe.Interfaces;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace HomeHub.Infrastructure.Services;

public class FilePromptService : IPromptService
{
    private readonly ILogger<FilePromptService> _logger;
    private readonly string _promptsDirectory;

    public FilePromptService(ILogger<FilePromptService> logger)
    {
        _logger = logger;

        // Get the directory where the executing assembly is located
        // This will be the bin directory when running, and prompts will be copied there
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);

        if (string.IsNullOrEmpty(assemblyDirectory))
        {
            // Fallback to current directory
            assemblyDirectory = Directory.GetCurrentDirectory();
        }

        // Prompts should be in the output directory (copied from source via .csproj configuration)
        _promptsDirectory = Path.Combine(assemblyDirectory, "Prompts");

        // If not found in output directory, try to find it relative to the current working directory
        if (!Directory.Exists(_promptsDirectory))
        {
            var currentDir = Directory.GetCurrentDirectory();
            var promptsPath = Path.Combine(currentDir, "Prompts");
            if (Directory.Exists(promptsPath))
            {
                _promptsDirectory = promptsPath;
            }
            else
            {
                // Try to find it in the Infrastructure project source directory (for development)
                var infrastructureProjectPath = Path.Combine(currentDir, "src", "HomeHub.Infrastructure", "Prompts");
                if (Directory.Exists(infrastructureProjectPath))
                {
                    _promptsDirectory = infrastructureProjectPath;
                }
                else
                {
                    // Try one more level up (if running from solution root)
                    var alternativePath = Path.Combine(currentDir, "homehub-backend", "src", "HomeHub.Infrastructure", "Prompts");
                    if (Directory.Exists(alternativePath))
                    {
                        _promptsDirectory = alternativePath;
                    }
                }
            }
        }

        if (!Directory.Exists(_promptsDirectory))
        {
            _logger.LogWarning("Prompts directory not found. Searched in: {Directory}. Prompts will not be available.", _promptsDirectory);
        }
        else
        {
            _logger.LogInformation("Prompts directory found at: {Directory}", _promptsDirectory);
        }
    }

    public async Task<string> GetPromptAsync(string promptId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(promptId))
        {
            throw new ArgumentException("Prompt ID cannot be null or empty.", nameof(promptId));
        }

        var promptFilePath = Path.Combine(_promptsDirectory, $"{promptId}.txt");

        if (!File.Exists(promptFilePath))
        {
            _logger.LogError("Prompt file not found: {FilePath}", promptFilePath);
            throw new FileNotFoundException($"Prompt file not found for ID: {promptId}", promptFilePath);
        }

        try
        {
            var promptContent = await File.ReadAllTextAsync(promptFilePath, cancellationToken);
            return promptContent.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading prompt file: {FilePath}", promptFilePath);
            throw new InvalidOperationException($"Failed to read prompt file for ID: {promptId}", ex);
        }
    }
}
