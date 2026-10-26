# 05 - Function Calling

This sample demonstrates how a .NET 10 application can use function calling with the Responses API to answer budget-related questions with external data.

The model can request a local function (`get_cost_center_budget`) when a budget lookup is needed. The application executes the function, returns the result to the model, and then the model produces the final user-facing response.

## What this sample demonstrates

- Creating a client for a Microsoft Foundry project.
- Authenticating with Microsoft Entra ID through `DefaultAzureCredential`.
- Registering a function tool with strict JSON parameters.
- Running a multi-round model-tool-model loop.
- Executing tool calls through an explicit allowlist.
- Validating tool arguments before data lookup.
- Returning structured tool errors for invalid requests.
- Limiting tool-calling rounds to avoid infinite loops.

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

`DefaultAzureCredential` uses credentials available in the environment to authenticate the application without requiring a key in source code.

## Configuration

The program requires two settings:

| Name | Description |
| --- | --- |
| `AZURE_AI_PROJECT_ENDPOINT` | Microsoft Foundry project endpoint. |
| `MODEL_DEPLOYMENT` | Name of the model deployment that will process prompts. |

### Using User Secrets

From the `samples/05-FunctionCalling` directory, run:

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

From the `samples/05-FunctionCalling` directory:

```bash
dotnet restore
dotnet run
```

Or, from the solution root:

```bash
dotnet run --project samples/05-FunctionCalling/05-FunctionCalling.csproj
```

After startup, enter a question. Type `/exit` to quit.

## Function tool

The sample defines a single function tool:

- **Name:** `get_cost_center_budget`
- **Purpose:** return available budget for a cost center from fictional lab data.
- **Parameters schema:**
  - `costCenter` (string, required)
- **Strict mode:** enabled (`additionalProperties: false`)

Known fictional cost centers in this sample:

- `TI-001` → `8000 BRL`
- `OPS-002` → `2500 BRL`

## Interaction flow

For each user input, the app runs an independent interaction:

1. Sends the user message plus instructions and tool metadata.
2. If the model returns function calls, executes them locally.
3. Sends only function outputs back to the model, referencing the previous response.
4. Repeats until the model returns plain text or reaches the round limit.

The maximum number of model calls per input is `5`.

## Tool execution safety and validation

The function executor applies defensive checks:

- Explicit allowlist for function names.
- JSON parsing of function arguments.
- `costCenter` must be a non-empty string with up to 50 characters.
- Unknown cost centers return a not-found payload.

Possible tool error payloads:

- `unknown_function`
- `invalid_arguments`
- `invalid_cost_center`
- `not_found`
- `invalid_json`

The model is instructed not to invent cost centers or budgets and to clarify that values are fictional laboratory data.

## How the code works

1. The application loads settings from User Secrets and environment variables.
2. `AIProjectClient` is created with the project endpoint and `DefaultAzureCredential`.
3. A function tool is registered with JSON schema and strict mode.
4. User input is sent to the model with instructions and tool metadata.
5. Function calls are executed locally and returned as function output items.
6. The model uses tool output to generate final text.
7. API errors are handled and shown without crashing the input loop.

## Packages used

- [`Azure.AI.Projects`](https://www.nuget.org/packages/Azure.AI.Projects) - Microsoft Foundry project client.
- [`Microsoft.Extensions.Configuration`](https://www.nuget.org/packages/Microsoft.Extensions.Configuration) - application configuration.
- `Microsoft.Extensions.Configuration.EnvironmentVariables` - reading environment variables.
- `Microsoft.Extensions.Configuration.UserSecrets` - local storage for development settings.

## Next steps

After running this sample, the following concepts are recommended next:

- Combine function calling with structured outputs.
- Add more tools with clear responsibility boundaries.
- Add observability for tool usage and failure patterns.
- Evaluate tool-calling reliability with realistic prompts.
