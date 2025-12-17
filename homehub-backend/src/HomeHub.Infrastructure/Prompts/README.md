# Prompts Directory

This directory contains prompt templates used by the AI service for generating content.

## File Naming Convention

Prompt files should be named using the format: `{prompt-id}.txt`

For example:
- `generate-recipes-from-inventory.txt` - Used for generating recipes from inventory items

## Prompt Template Format

Prompts can use placeholders that will be replaced at runtime:
- `{0}` - First parameter (typically inventory item names list)
- `{1}` - Second parameter (typically user description)
- Additional placeholders can be added as needed

## Usage

Prompts are loaded by the `FilePromptService` which reads files from this directory. The service automatically looks for prompt files in:
1. The output directory (where the compiled assembly is located)
2. The current working directory
3. The Infrastructure project source directory

Prompt files are automatically copied to the output directory during build via the `.csproj` configuration.

## Adding New Prompts

1. Create a new `.txt` file in this directory with a descriptive name
2. Use the format `{prompt-id}.txt` where `prompt-id` matches the identifier used in code
3. Use placeholders `{0}`, `{1}`, etc. for dynamic content
4. Reference the prompt in code using: `await _promptService.GetPromptAsync("prompt-id", cancellationToken)`
