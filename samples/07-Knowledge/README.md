# 07-Knowledge

This sample shows how to upload a document to a Microsoft Foundry project, build a vector store from it, and give an agent access to that content through the hosted file-search tool.

The sample is designed to answer questions using a local knowledge base instead of relying on the model's general knowledge alone.

## What the sample does

The program:

1. Reads `AZURE_AI_PROJECT_ENDPOINT` and `MODEL_DEPLOYMENT` from user secrets or environment variables.
2. Locates the file `Documents/purchasing-policy.md` in the project output directory.
3. Uploads the document to the Foundry project.
4. Creates a vector store and attaches the uploaded file to it.
5. Waits for the document to be processed before continuing.
6. Creates a declarative agent that uses the `file_search` tool against the new vector store.
7. Opens an interactive loop where the user can ask policy questions.
8. Prints citations from the uploaded document when the model answers using the knowledge source.
9. Cleans up the agent, vector store, and uploaded file in the final `finally` block.

## Knowledge source

The sample uses the fictional document:

- `Documents/purchasing-policy.md`

It contains approval thresholds and policy constraints for a laboratory purchasing scenario.

## Key concepts

- Uploading a file and indexing it in a vector store are separate steps.
- The sample waits for document processing before querying the agent.
- File search is a hosted tool: the application does not execute
  a local function to perform the search.
- Conversation context is maintained by chaining response IDs.
- `/new` clears the local response reference. It does not delete
  previously stored responses.
- The document is used as retrieval context; it does not train
  or fine-tune the model.

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
dotnet run --project samples/07-Knowledge/07-Knowledge.csproj
```

## How to interact

The console supports the following commands:

- `/new` to start a new conversation state
- `/exit` to quit the sample

Example:

```text
You: What is the approval threshold for purchases above 7,000 BRL?
Assistant: Purchases above BRL 5,000 require director approval.
```

The sample expects the answer to come from the uploaded document. If the document does not contain a rule, the model is instructed to say so instead of inventing one.

## Important behavior

- The uploaded document is treated as the authoritative policy source for this sample.
- The model may cite the document when explaining the answer.
- The sample never claims that a purchase was approved; it only reports policy rules from the document.
- The named resources are created with a GUID-based name to avoid collisions in the project.

## Cleanup

When the sample ends, it attempts to delete:

- the agent;
- the vector store;
- the uploaded file.

Each cleanup is performed independently so that one failure does not prevent the other cleanup steps from being attempted.
