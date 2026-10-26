# 06-Agents

This sample demonstrates how to create and use a declarative agent in Azure AI Foundry with a local tool. It shows the pattern of:

- creating an agent version from a declarative definition;
- attaching a function tool to the agent;
- invoking the model with tool-calling behavior;
- collecting tool output and continuing the conversation;
- cleaning up the created agent when the sample exits.

## What the sample does

The program:

1. Reads `AZURE_AI_PROJECT_ENDPOINT` and `MODEL_DEPLOYMENT` from user secrets or environment variables.
2. Creates an `AIProjectClient` with `DefaultAzureCredential`.
3. Defines a `DeclarativeAgentDefinition` with instructions and a `get_cost_center_budget` tool.
4. Creates a named agent version in the Foundry project.
5. Starts an interactive terminal loop where the user can chat with the agent.
6. When the model asks for a budget lookup, the local tool validates the argument and returns fictional laboratory data.
7. Deletes the created agent in the `finally` block when the sample ends.

This is the first sample in the repository that creates a real agent resource in the project instead of only calling a model directly.

## Prerequisites

- .NET 10 SDK or Visual Studio 2026
- An Azure subscription
- A Microsoft Foundry project with a deployed model
- Azure CLI logged in with a user that can access the project

## Configuration

Set the same values used by the other samples:

```bash
dotnet user-secrets set "AZURE_AI_PROJECT_ENDPOINT" "https://<your-project>.services.ai.azure.com/api/projects/<your-project>"
dotnet user-secrets set "MODEL_DEPLOYMENT" "<deployment-name>"
```

Or use environment variables:

```powershell
$env:AZURE_AI_PROJECT_ENDPOINT = "https://<your-project>.services.ai.azure.com/api/projects/<your-project>"
$env:MODEL_DEPLOYMENT = "<deployment-name>"
```

## Run

From the repository root:

```bash
dotnet run --project samples/06-Agents/06-Agents.csproj
```

## How to interact

The console supports these commands:

- `/new` to start a new conversation state
- `/exit` to quit the sample

Examples:

```text
You: What is the available budget for TI-001?
Assistant: I need to check the budget for TI-001.
[Tool requested: get_cost_center_budget]
[Tool result: {"costCenter":"TI-001","availableBudget":8000,"currency":"BRL","isFictional":true}]
Assistant: The available budget for TI-001 is 8000 BRL.
```

## Tool behavior

The tool is intentionally limited to a small allowlist. It only accepts the `get_cost_center_budget` function and validates arguments before returning results.

The fictional budget records in this sample are:

- `TI-001` => 8000 BRL
- `OPS-002` => 2500 BRL

The tool is used only to provide laboratory data; it does not approve purchases or reserve funds.

## Notes

- The agent name is generated with a GUID to avoid conflicts with an existing agent.
- The sample automatically deletes the created agent at shutdown when the operation succeeds.
- This sample is educational and intentionally uses fictional values to keep the scenario simple.
