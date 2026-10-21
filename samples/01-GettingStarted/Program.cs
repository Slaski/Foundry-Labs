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

Console.WriteLine("Foundry Labs - Getting Started!");
Console.WriteLine($"Deployed model: {modelDeployment}");
Console.WriteLine();
Console.Write("Enter a prompt: ");

var input = Console.ReadLine();
var prompt = input?.Trim();

if (string.IsNullOrWhiteSpace(prompt))
{
    Console.WriteLine("Prompt cannot be empty.");
    return;
}

try
{
    var response = await responsesClient.CreateResponseAsync(prompt);
    Console.WriteLine(response.Value.GetOutputText());
}
catch(ClientResultException exception)
{
    Console.WriteLine($"Request failed (HTTP {exception.Status}): {exception.Message}");
    Console.WriteLine();
}

Console.WriteLine("Press any key to exit...");
Console.ReadKey();