# 08-RAG

This sample demonstrates a local Retrieval-Augmented Generation (RAG) flow using .NET 10 with Microsoft Foundry and Azure OpenAI embeddings.

Unlike the `07-Knowledge` sample, this project does not create a vector store in the Foundry project. It builds an in-memory index at runtime from a local Markdown file and uses that retrieved context to answer questions.

## What the sample does

The program:

1. Reads configuration values from user secrets or environment variables.
2. Loads `Documents/purchasing-policy.md` from the output directory.
3. Splits the document into sections using level-two Markdown headings (`##`).
4. Generates embeddings for each section and stores them in memory.
5. Starts an interactive console loop.
6. Generates an embedding for each user question.
7. Computes cosine similarity across all chunks and selects the top 2 matches.
8. Sends only the retrieved context plus the user question to the model.
9. Prints the retrieved chunks and the model answer.

## RAG data source

The sample uses the fictional policy document:

- `Documents/purchasing-policy.md`

This file is treated as the authoritative source for answers.

## Prerequisites

- .NET 10 SDK or Visual Studio 2026
- An Azure subscription
- A Microsoft Foundry project with a deployed model for responses
- An Azure OpenAI embedding deployment
- Azure CLI logged in with a user that can access these resources

## Configuration

Set the following values:

| Name | Description |
| --- | --- |
| `AZURE_AI_PROJECT_ENDPOINT` | Microsoft Foundry project endpoint. |
| `MODEL_DEPLOYMENT` | Name of the model deployment used to generate answers. |
| `AZURE_OPENAI_ENDPOINT` | Azure OpenAI resource endpoint used for embeddings. |
| `EMBEDDING_DEPLOYMENT` | Name of the embedding model deployment. |

Using user secrets:

```bash
dotnet user-secrets set "AZURE_AI_PROJECT_ENDPOINT" "https://<your-project>.services.ai.azure.com/api/projects/<your-project>"
dotnet user-secrets set "MODEL_DEPLOYMENT" "<deployment-name>"
dotnet user-secrets set "AZURE_OPENAI_ENDPOINT" "https://<your-openai-resource>.openai.azure.com/"
dotnet user-secrets set "EMBEDDING_DEPLOYMENT" "<embedding-deployment-name>"
```

Or environment variables:

```powershell
$env:AZURE_AI_PROJECT_ENDPOINT = "https://<your-project>.services.ai.azure.com/api/projects/<your-project>"
$env:MODEL_DEPLOYMENT = "<deployment-name>"
$env:AZURE_OPENAI_ENDPOINT = "https://<your-openai-resource>.openai.azure.com/"
$env:EMBEDDING_DEPLOYMENT = "<embedding-deployment-name>"
```

## Run

From the repository root:

```bash
dotnet run --project samples/08-RAG/08-RAG.csproj
```

## How to interact

The console supports:

- `/exit` to quit the sample

Example:

```text
You: Who needs approval for a purchase of 7000 BRL?
Assistant: Based on the policy, purchases above the defined threshold require director approval [chunk-2].
```

## Important behavior

- Each question is processed independently.
- Retrieval uses cosine similarity over embedding vectors.
- The model is instructed to avoid inventing missing policy information.
- The model must cite supporting chunk IDs like `[chunk-2]`.
- The sample does not approve purchases; it only explains policy rules found in context.
