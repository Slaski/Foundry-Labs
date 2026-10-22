# 04 - Structured Outputs

This sample demonstrates how a .NET 10 application can use the Responses API with a strict JSON Schema to extract structured data from natural-language purchase requests.

The model receives a user description and returns a predictable JSON object that the application deserializes into a `PurchaseRequest` and validates locally.

## What this sample demonstrates

- Creating a client for a Microsoft Foundry project.
- Authenticating with Microsoft Entra ID through `DefaultAzureCredential`.
- Selecting a model by deployment name.
- Defining a strict JSON Schema for model responses.
- Extracting only information explicitly provided by the user.
- Using `null` for missing or ambiguous values.
- Deserializing the structured response into a C# model.
- Applying application-side validation after extraction.

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
| `MODEL_DEPLOYMENT` | Name of the model deployment that will process the request. |

### Using User Secrets

From the `samples/04-StructuredOutputs` directory, run:

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

From the `samples/04-StructuredOutputs` directory:

```bash
dotnet restore
dotnet run
```

Or, from the solution root:

```bash
dotnet run --project samples/04-StructuredOutputs/04-StructuredOutputs.csproj
```

After the application starts, describe one purchase request:

```text
Input: 12 laptops for cost center IT-042 with a total budget of 18000
```

The application displays the extracted JSON and the result of local validation. Enter `/exit` to quit.

## Structured schema

The response uses the `purchase_request` JSON Schema with the following fields:

| Field | Type | Description |
| --- | --- | --- |
| `item` | `string` or `null` | Item explicitly requested for purchase. |
| `quantity` | `integer` or `null` | Requested quantity when explicitly provided. |
| `totalBudget` | `number` or `null` | Total budget, not the unit price. |
| `costCenter` | `string` or `null` | Cost center identifier when explicitly provided. |

All four fields are required by the schema, but each field may contain `null`. Additional properties are not allowed, and strict schema mode is enabled.

The extraction instructions tell the model to treat user text as data rather than instructions, avoid inventing values, avoid calculating a total budget from a unit price, and preserve explicitly stated zero or negative numeric values for application validation.

## Validation

After deserialization, the application validates the extracted request:

- `item` must be present.
- `quantity` must be present and greater than zero.
- `totalBudget` must be present and cannot be negative.
- `costCenter` must be present.

Structured output ensures the response has the expected shape, while these rules enforce application-specific business constraints. A response can therefore be structurally valid but fail business validation.

## How the code works

1. The application loads settings from User Secrets and environment variables.
2. `AIProjectClient` is created with the project endpoint and `DefaultAzureCredential`.
3. A strict JSON Schema is configured with `ResponseTextFormat.CreateJsonSchemaFormat`.
4. The user's text is sent to the model with extraction instructions.
5. The JSON response is deserialized into `PurchaseRequest`.
6. The extracted data is printed and validated locally.
7. The application reports incomplete responses, refused output, API errors, or invalid JSON without terminating the input loop.

## Packages used

- [`Azure.AI.Projects`](https://www.nuget.org/packages/Azure.AI.Projects) - Microsoft Foundry project client.
- [`Microsoft.Extensions.Configuration`](https://www.nuget.org/packages/Microsoft.Extensions.Configuration) - application configuration.
- `Microsoft.Extensions.Configuration.EnvironmentVariables` - reading environment variables.
- `Microsoft.Extensions.Configuration.UserSecrets` - local storage for development settings.

## Next steps

After running this sample, the following concepts are recommended next:

- Combine structured outputs with streaming responses.
- Add richer domain validation and error handling.
- Explore structured responses for tool calls and agent workflows.
- Evaluate extraction accuracy with representative input data.
