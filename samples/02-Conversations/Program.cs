using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using System.ClientModel;

var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>()
    .Build();

var projectEndpoint = configuration["AZURE_AI_PROJECT_ENDPOINT"]
    ?? throw new InvalidOperationException("Please set the AZURE_AI_PROJECT_ENDPOINT configuration value.");

var modelDeployment = configuration["MODEL_DEPLOYMENT"]
    ?? throw new InvalidOperationException("Please set the MODEL_DEPLOYMENT configuration value.");

var projectClient = new AIProjectClient(
    endpoint: new Uri(projectEndpoint),
    tokenProvider: new DefaultAzureCredential());

var responsesClient = projectClient
    .ProjectOpenAIClient
    .GetProjectResponsesClientForModel(modelDeployment);

string? previousResponseId = null;

Console.WriteLine("Foundry Labs - Conversations!");
Console.WriteLine($"Deployed model: {modelDeployment}");
Console.WriteLine("Commands: /new to start over, /exit to quit.");
Console.WriteLine();

while(true)
{
    Console.Write("You: ");
    
    var input = Console.ReadLine();
    var prompt = input?.Trim();

    if (string.IsNullOrWhiteSpace(prompt))
        continue;

    if (string.Equals(prompt, "/exit", StringComparison.OrdinalIgnoreCase))
        break;

    if (string.Equals(prompt, "/new", StringComparison.OrdinalIgnoreCase))
    {
        previousResponseId = null;

        Console.WriteLine("New conversation started.");
        Console.WriteLine();

        continue;
    }

    try
    {
        var response = await responsesClient.CreateResponseAsync(
            userInputText: prompt,
            previousResponseId: previousResponseId);

        previousResponseId = response.Value.Id;

        Console.WriteLine();
        Console.WriteLine($"Assistant: {response.Value.GetOutputText()}");
        Console.WriteLine();
    }
    catch(ClientResultException exception)
    {
        Console.WriteLine($"Request failed (HTTP {exception.Status}): {exception.Message}");
        Console.WriteLine("The previous conversation reference was kept. Use /new if you want to start over.");
        Console.WriteLine();
    }
}