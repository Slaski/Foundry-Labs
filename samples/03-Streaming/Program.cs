using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using OpenAI.Responses;
using System.ClientModel;

#pragma warning disable OPENAI001

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
Console.WriteLine("Press Ctrl+C during a response to interrupt it.");
Console.WriteLine();

while (true)
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

    using var cancellationSource = new CancellationTokenSource();

    ConsoleCancelEventHandler cancelHandler = (_, eventArgs) =>
    {
        eventArgs.Cancel = true;
        cancellationSource.Cancel();
    };

    Console.CancelKeyPress += cancelHandler;

    try
    {
        string? completedResponseId = null;

        Console.WriteLine();
        Console.Write("Assistant: ");

        var updates = responsesClient.CreateResponseStreamingAsync(
            userInputText: prompt,
            previousResponseId: previousResponseId,
            cancellationToken: cancellationSource.Token);

        await foreach (var update in updates)
        {
            switch (update)
            {
                case StreamingResponseOutputTextDeltaUpdate textDelta:
                    Console.Write(textDelta.Delta);
                    break;

                case StreamingResponseCompletedUpdate completed:
                    completedResponseId = completed.Response.Id;
                    break;

                case StreamingResponseErrorUpdate error:
                    throw new InvalidOperationException($"Streaming error: {error.Message}");
            }
        }

        if (completedResponseId is null)
        {
            Console.WriteLine();
            Console.WriteLine(
                "[Response not completed. Conversation reference unchanged.]");
        }
        else
        {
            previousResponseId = completedResponseId;
            Console.WriteLine();
        }
    }
    catch (OperationCanceledException) when (cancellationSource.IsCancellationRequested)
    {
        Console.WriteLine();
        Console.WriteLine(
            "[Response interrupted. Conversation reference unchanged.]");
    }
    catch (ClientResultException exception)
    {
        Console.WriteLine($"Request failed (HTTP {exception.Status}): {exception.Message}");
        Console.WriteLine("The previous conversation reference was kept. Use /new if you want to start over.");
        Console.WriteLine();
    }
    catch (InvalidOperationException exception)
    {
        Console.WriteLine();
        Console.WriteLine($"[{exception.Message}]");
    }
    finally
    {
        Console.CancelKeyPress -= cancelHandler;
    }

    Console.WriteLine();
}

#pragma warning restore OPENAI001