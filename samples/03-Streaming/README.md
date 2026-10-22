# 03 - Streaming

This sample demonstrates how a .NET 10 application can connect to a Microsoft Foundry project and display a model response incrementally as it is generated using the Responses API.

The sample builds on the conversation flow from `02-Conversations` by preserving the previous response reference while streaming text updates to the terminal.

## What this sample demonstrates

- Creating a client for a Microsoft Foundry project.
- Authenticating with Microsoft Entra ID through `DefaultAzureCredential`.
- Selecting a model by deployment name.
- Receiving response updates asynchronously through the streaming API.
- Displaying text deltas as they arrive instead of waiting for the complete response.
- Maintaining conversation context with `previousResponseId`.
- Interrupting an in-progress response with `Ctrl+C`.

## Prerequisites

- [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0).
- An Azure subscription.
- A project in [Microsoft Foundry](https://ai.azure.com/).
- A model deployed in that project.
- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli).
- Permission to access the project and model deployment.

Authenticate locally with the Azure CLI:

```bash
az login
```

`DefaultAzureCredential` uses the credentials available in the environment to authenticate the application without requiring a key in the source code.

## Configuration

The program requires two settings:

| Name | Description |
| --- | --- |
| `AZURE_AI_PROJECT_ENDPOINT` | Microsoft Foundry project endpoint. |
| `MODEL_DEPLOYMENT` | Name of the model deployment that will receive the prompts. |

### Using User Secrets

From the `samples/03-Streaming` directory, run:

```bash
dotnet user-secrets set "AZURE_AI_PROJECT_ENDPOINT" "https://<your-project>.services.ai.azure.com/api/projects/<your-project>"
dotnet user-secrets set "MODEL_DEPLOYMENT" "<deployment-name>"
```

The values can also be configured as environment variables:

```powershell
$env:AZURE_AI_PROJECT_ENDPOINT = "https://<your-project>.services.ai.azure.com/api/projects/<your-project>"
$env:MODEL_DEPLOYMENT = "<deployment-name>"
```

Do not store keys, tokens, or other secrets in source code or version control.

## Running the project

From the `samples/03-Streaming` directory:

```bash
dotnet restore
dotnet run
```

Or, from the solution root:

```bash
dotnet run --project samples/03-Streaming/03-Streaming.csproj
```

After the application starts, enter prompts when requested:

```text
You:
```

The assistant response is displayed incrementally as text is received. Enter `/new` to start a new conversation or `/exit` to quit. Press `Ctrl+C` while a response is streaming to cancel the current response.

## How the code works

1. The application loads settings from User Secrets and environment variables.
2. `AIProjectClient` is created with the project endpoint and `DefaultAzureCredential`.
3. The responses client is obtained for the deployment specified in `MODEL_DEPLOYMENT`.
4. `CreateResponseStreamingAsync` starts a streaming response using the current conversation reference.
5. Text delta updates are written to the console as they arrive.
6. The completed response ID is stored as `previousResponseId` for the next prompt.
7. If the response is cancelled or does not complete, the previous conversation reference is kept unchanged.

## Commands

- `/new` - clears the previous response reference and starts a new conversation.
- `/exit` - exits the application.
- `Ctrl+C` - interrupts the response currently being streamed without exiting the application.

## Packages used

- [`Azure.AI.Projects`](https://www.nuget.org/packages/Azure.AI.Projects) - Microsoft Foundry project client.
- [`Microsoft.Extensions.Configuration`](https://www.nuget.org/packages/Microsoft.Extensions.Configuration) - application configuration.
- `Microsoft.Extensions.Configuration.EnvironmentVariables` - reading environment variables.
- `Microsoft.Extensions.Configuration.UserSecrets` - local storage for development settings.

## Next steps

After running this sample, the following concepts are recommended next:

- Configure generation parameters and structured responses.
- Explore tools and function calling.
- Create agents and connect tools available in Microsoft Foundry.
- Evaluate response quality, cost, and performance.
