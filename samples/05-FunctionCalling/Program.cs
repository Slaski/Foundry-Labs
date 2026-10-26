using _05_FunctionCalling;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using OpenAI.Responses;
using System.ClientModel;
using System.Text.Json;

#pragma warning disable OPENAI001

var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .Build();

var projectEndpoint = configuration["AZURE_AI_PROJECT_ENDPOINT"]
    ?? throw new InvalidOperationException(
        "Please set the AZURE_AI_PROJECT_ENDPOINT configuration value.");

var modelDeployment = configuration["MODEL_DEPLOYMENT"]
    ?? throw new InvalidOperationException(
        "Please set the MODEL_DEPLOYMENT configuration value.");

var projectClient = new AIProjectClient(
    endpoint: new Uri(projectEndpoint),
    tokenProvider: new DefaultAzureCredential());

var responsesClient = projectClient
    .ProjectOpenAIClient
    .GetProjectResponsesClientForModel(modelDeployment);

const string functionName = "get_cost_center_budget";

var budgetTool = ResponseTool.CreateFunctionTool(
    functionName: functionName,
    functionDescription: """
        Gets the available budget for a cost center.
        This is a read-only lookup using fictional laboratory data.
        Use it whenever the user asks about an available budget.
        """,
    functionParameters: BinaryData.FromString("""
        {
          "type": "object",
          "properties": {
            "costCenter": {
              "type": "string",
              "description": "The cost center code, for example TI-001."
            }
          },
          "required": ["costCenter"],
          "additionalProperties": false
        }
        """),
        strictModeEnabled: true);

const string instructions = """
    You are a purchasing assistant in an educational laboratory.

    Use the provided tool to retrieve available budgets.
    Never invent a cost center code or a budget.
    If the user does not provide a cost center code, ask for it.
    If the tool returns an error, explain it without inventing data.
    For general conceptual questions, answer without using the tool.
    A budget lookup does not approve, reserve, or execute a purchase.
    Make clear that budget values are fictional laboratory data.
    Respond in the user's language.
    """;

const int maxModelCalls = 5;

Console.WriteLine("Foundry Labs - Functions");
Console.WriteLine($"Deployed model: {modelDeployment}");
Console.WriteLine("Budget values are fictional laboratory data.");
Console.WriteLine("Each input is processed independently.");
Console.WriteLine("Type /exit to quit.");
Console.WriteLine();

while(true)
{
    Console.Write("You: ");
    var input = Console.ReadLine();

    if (input is null)
        break;

    var prompt = input.Trim();

    if (string.Equals(prompt, "/exit", StringComparison.OrdinalIgnoreCase))
        break;

    if (string.IsNullOrWhiteSpace(prompt))
        continue;

    // Each console input starts an independent interaction.
    string? previousResponseId = null;

    // Keep the full interaction, including tool calls and their results.
    var items = new List<ResponseItem>
    {
        ResponseItem.CreateUserMessageItem(prompt)
    };

    try
    {
        for (var round = 0; round < maxModelCalls; round++)
        {
            var options = new CreateResponseOptions
            {
                Instructions = instructions,
                PreviousResponseId = previousResponseId
            };

            options.Tools.Add(budgetTool);

            foreach (var item in items)
                options.InputItems.Add(item);

            var result = await responsesClient.CreateResponseAsync(options);
            var response = result.Value;

            if (response.Status != ResponseStatus.Completed)
            {
                Console.WriteLine($"Response not completed. Status: {response.Status}");
                break;
            }

            // Preserve all output items, not just function calls or text.
            items.AddRange(response.OutputItems);

            var functionCalls = response.OutputItems
                .OfType<FunctionCallResponseItem>()
                .ToList();

            if (functionCalls.Count == 0)
            {
                var text = response.GetOutputText();

                Console.WriteLine(
                    string.IsNullOrWhiteSpace(text)
                        ? "Assistant: No text response returned."
                        : $"Assistant: {text}");

                break;
            }

            // Do not execute tools if there is no remaining model round
            // available to process their results.
            if (round == maxModelCalls - 1)
            {
                Console.WriteLine(
                    "Tool-calling limit reached. Please simplify the request.");

                break;
            }

            // Reference the response containing the function calls.
            previousResponseId = response.Id;

            // The next request sends only the corresponding tool results.
            items.Clear();

            foreach (var call in functionCalls)
            {
                Console.WriteLine(
                    $"[Tool requested: {call.FunctionName}]");

                var output = ExecuteFunction(call);

                Console.WriteLine($"[Tool result: {output}]");

                items.Add(
                    ResponseItem.CreateFunctionCallOutputItem(
                        call.CallId,
                        output));
            }
        }
    }
    catch (ClientResultException exception)
    {
        Console.WriteLine(
            $"Request failed (HTTP {exception.Status}): {exception.Message}");
    }

    Console.WriteLine();
}

static string ExecuteFunction(FunctionCallResponseItem call)
{
    // Explicit allowlist: never execute an arbitrary function name.
    if (call.FunctionName != "get_cost_center_budget")
    {
        return JsonSerializer.Serialize(new
        {
            error = "unknown_function",
            message = "The requested function is not available."
        });
    }

    try
    {
        using var document = JsonDocument.Parse(call.FunctionArguments);
        var root = document.RootElement;

        if (root.ValueKind != JsonValueKind.Object ||
            !root.TryGetProperty("costCenter", out var element) ||
            element.ValueKind != JsonValueKind.String)
        {
            return JsonSerializer.Serialize(new
            {
                error = "invalid_arguments",
                message = "A string costCenter argument is required."
            });
        }

        var costCenter = element.GetString()?.Trim();

        if (string.IsNullOrWhiteSpace(costCenter) ||
            costCenter.Length > 50)
        {
            return JsonSerializer.Serialize(new
            {
                error = "invalid_cost_center",
                message = "Provide a cost center code with 1 to 50 characters."
            });
        }

        var budget = GetCostCenterBudget(costCenter);

        if (budget is null)
        {
            return JsonSerializer.Serialize(new
            {
                error = "not_found",
                message = "The cost center was not found.",
                costCenter
            });
        }

        return JsonSerializer.Serialize(new
        {
            costCenter = budget.CostCenter,
            availableBudget = budget.AvailableBudget,
            currency = budget.Currency,
            isFictional = true
        });
    }
    catch (JsonException)
    {
        return JsonSerializer.Serialize(new
        {
            error = "invalid_json",
            message = "The function arguments must be valid JSON."
        });
    }
}


static CostCenterBudget? GetCostCenterBudget(string costCenter)
{
    // Fictional data for this laboratory only.
    return costCenter.ToUpperInvariant() switch
    {
        "TI-001" => new CostCenterBudget("TI-001", 8000m, "BRL"),
        "OPS-002" => new CostCenterBudget("OPS-002", 2500m, "BRL"),
        _ => null
    };
}
