# Foundry Labs

A series of .NET samples for learning how to use the Azure AI SDK in applications and incrementally explore Microsoft Foundry capabilities.

The solution starts with simple integrations with models deployed in a Microsoft Foundry project and will evolve toward more complete scenarios, such as generative applications, agents, tools, evaluation, and observability.

> This repository is intended for educational purposes. The samples prioritize clarity and experimentation and are not a substitute for production security, governance, and operational practices.

## Goals

- Learn the fundamental concepts of the Azure AI SDK.
- Connect .NET applications to Microsoft Foundry projects and models.
- Explore Microsoft Foundry capabilities through focused, independent samples.
- Document configuration, authentication, and development patterns that can be reused in other applications.

## Solution structure

```text
.
├── Foundry Labs.slnx
├── samples
│   ├── 01-GettingStarted
│   │   ├── 01-GettingStarted.csproj
│   │   └── Program.cs
│   ├── 02-Conversations
│   │   ├── 02-Conversations.csproj
│   │   └── Program.cs
│   ├── 03-Streaming
│   │   ├── 03-Streaming.csproj
│   │   └── Program.cs
│   ├── 04-StructuredOutputs
│   │   ├── 04-StructuredOutputs.csproj
│   │   ├── Program.cs
│   │   └── PurchaseRequest.cs
│   ├── 05-FunctionCalling
│   │   ├── 05-FunctionCalling.csproj
│   │   ├── CostCenterBudget.cs
│   │   └── Program.cs
│   └── 06-Agents
│       ├── 06-Agents.csproj
│       ├── CostCenterBudget.cs
│       ├── Program.cs
│       └── README.md
└── README.md
```

### Available samples

| Sample | Description |
| --- | --- |
| [`01-GettingStarted`](samples/01-GettingStarted) | Sends a single prompt to a deployed model using the Responses API and displays the response. |
| [`02-Conversations`](samples/02-Conversations) | Maintains a multi-turn conversation and supports `/new` and `/exit` commands. |
| [`03-Streaming`](samples/03-Streaming) | Streams response text incrementally, preserves conversation context, and supports interrupting a response with `Ctrl+C`. |
| [`04-StructuredOutputs`](samples/04-StructuredOutputs) | Extracts purchase request data using a strict JSON Schema and validates the result locally. |
| [`05-FunctionCalling`](samples/05-FunctionCalling) | Uses function calling to look up fictional cost center budgets with validated tool arguments. |
| [`06-Agents`](samples/06-Agents) | Creates a declarative agent, attaches a local tool, and drives a multi-turn conversation with function-calling behavior. |

New samples will be added to the `samples` directory following an incremental progression in complexity.

## Prerequisites

- [Visual Studio 2026](https://visualstudio.microsoft.com/) with .NET development support or the [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0).
- An Azure subscription.
- A project in [Microsoft Foundry](https://ai.azure.com/).
- A model deployed in the Foundry project.
- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli) installed and authenticated.
- Permission to access the project and model deployment.

The samples use `DefaultAzureCredential` for authentication. For local development, the simplest option is to authenticate with the Azure CLI:

```bash
az login
```

## Configuration

All samples require the following values:

| Setting | Description |
| --- | --- |
| `AZURE_AI_PROJECT_ENDPOINT` | Microsoft Foundry project endpoint. |
| `MODEL_DEPLOYMENT` | Name of the model deployment to use. |

### Using User Secrets

From the directory of the sample you want to configure, run:

```bash
dotnet user-secrets set "AZURE_AI_PROJECT_ENDPOINT" "https://<your-project>.services.ai.azure.com/api/projects/<your-project>"
dotnet user-secrets set "MODEL_DEPLOYMENT" "<deployment-name>"
```

The values can also be provided as environment variables. Do not commit private endpoints, keys, tokens, or any other secrets to the repository.

```powershell
$env:AZURE_AI_PROJECT_ENDPOINT = "https://<your-project>.services.ai.azure.com/api/projects/<your-project>"
$env:MODEL_DEPLOYMENT = "<deployment-name>"
```

## Running

From the solution root:

```bash
dotnet restore
dotnet run --project samples/01-GettingStarted/01-GettingStarted.csproj
```

To run the conversations sample, use:

```bash
dotnet run --project samples/02-Conversations/02-Conversations.csproj
```

To run the streaming sample, use:

```bash
dotnet run --project samples/03-Streaming/03-Streaming.csproj
```

To run the structured outputs sample, use:

```bash
dotnet run --project samples/04-StructuredOutputs/04-StructuredOutputs.csproj
```

To run the function calling sample, use:

```bash
dotnet run --project samples/05-FunctionCalling/05-FunctionCalling.csproj
```

To run the agents sample, use:

```bash
dotnet run --project samples/06-Agents/06-Agents.csproj
```

The `01-GettingStarted` sample requests one prompt, displays the response, and waits for a key press before exiting. An empty prompt is rejected.

The `02-Conversations` sample keeps reading prompts until `/exit` is entered. Use `/new` to clear the previous response reference and start a new conversation.

The `03-Streaming` sample displays response text as it arrives. Use `/new` to start a new conversation, `/exit` to quit, or `Ctrl+C` to interrupt the response currently being streamed.

The `04-StructuredOutputs` sample extracts a purchase request into a strict JSON Schema, displays the structured data, and applies local business validation. Use `/exit` to quit.

The `05-FunctionCalling` sample lets the model request a local budget lookup function, returns tool output to the model, and then produces the final response. Use `/exit` to quit.

The `06-Agents` sample creates a declarative agent in the Foundry project, gives it a local `get_cost_center_budget` tool, and keeps the chat loop alive while the model uses the tool as needed. Use `/new` to reset the conversation and `/exit` to quit.

## Authorization and security

- Apply the principle of least privilege when granting access to the project and deployments.
- Prefer keyless authentication, such as Microsoft Entra ID, during development and in production.
- Keep secrets out of source code and version control.
- Review the data sent to models before using the samples with real information.
- Consider content filters, data protection, secure logging, costs, and usage limits when turning the samples into production applications.

## Learning roadmap

The following sequence represents the planned direction for the solution and may evolve as new samples are added:

- [x] Connect to a Microsoft Foundry project.
- [x] Send prompts to a deployed model using the Azure AI SDK.
- [x] Maintain conversations across requests.
- [x] Stream response updates to the terminal.
- [x] Extract structured data with JSON Schema and validate the result.
- [x] Work with tools and function calling.
- [x] Create and use agents.
- [ ] Explore grounding, knowledge, and Microsoft Foundry tools.
- [ ] Evaluate responses and track quality, cost, and performance.
- [ ] Add observability and operational practice samples.

## Technologies

- .NET 10
- C#
- Azure AI SDK
- [`Azure.AI.Projects`](https://www.nuget.org/packages/Azure.AI.Projects)
- Microsoft Foundry
- Microsoft Entra ID / `DefaultAzureCredential`

## Contributing

Suggestions for new samples, documentation improvements, and fixes are welcome. When proposing a change, keep each sample focused on one concept and avoid including credentials or sensitive data.

## License

The license for this repository has not been defined yet.
