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
│   └── 01-GettingStarted
│       ├── 01-GettingStarted.csproj
│       └── Program.cs
└── README.md
```

### Available samples

| Sample | Description |
| --- | --- |
| [`01-GettingStarted`](samples/01-GettingStarted) | Creates a client for a Microsoft Foundry project and sends prompts to a deployed model using the Responses API. |

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

## Configuring the first sample

The `01-GettingStarted` sample requires the following values:

| Setting | Description |
| --- | --- |
| `AZURE_AI_PROJECT_ENDPOINT` | Microsoft Foundry project endpoint. |
| `MODEL_DEPLOYMENT` | Name of the model deployment to use. |

### Using User Secrets

From the project directory, run:

```bash
dotnet user-secrets set "AZURE_AI_PROJECT_ENDPOINT" "https://<your-project>.services.ai.azure.com/api/projects/<your-project>"
dotnet user-secrets set "MODEL_DEPLOYMENT" "<deployment-name>"
```

The values can also be provided as environment variables. Do not commit private endpoints, keys, tokens, or any other secrets to the repository.

## Running

From the solution root:

```bash
dotnet restore
dotnet run --project samples/01-GettingStarted/01-GettingStarted.csproj
```

When the program starts, enter a prompt. Type `exit` to quit the application.

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
- [ ] Explore structured responses and generation options.
- [ ] Work with tools and function calling.
- [ ] Create and use agents.
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
