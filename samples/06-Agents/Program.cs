using _06_Agents;
using Azure.AI.Projects;
using Azure.AI.Projects.Agents;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
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

var budgetTool = ResponseTool.CreateFunctionTool(
    functionName: "get_cost_center_budget",
    functionDescription: """
        Gets the available budget for a cost center.
        This is a read-only lookup using fictional laboratory data.
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

var agentDefinition = new DeclarativeAgentDefinition(modelDeployment)
{
    Instructions = """
        You are a purchasing assistant in an educational laboratory.

        Use get_cost_center_budget whenever you need an available budget.
        Never invent cost center codes or budget values.

        If the cost center code is missing, ask the user for it.
        Use information already provided in the current conversation.

        If the tool returns an error, explain it without inventing data.

        You may compare a purchase amount with the available budget
        and calculate the remaining balance.

        A budget lookup does not approve, reserve, or execute a purchase.
        Budget values returned by this laboratory are fictional.

        For general conceptual questions, answer without using the tool.
        Respond in the user's language.
        """,
    Tools = { budgetTool }
};

// A unique name prevents cleanup from targeting an existing agent.
var agentName = $"foundry-labs-{Guid.NewGuid():N}";

var agentCreated = false;

try
{
    var agentResult = await projectClient.AgentAdministrationClient.CreateAgentVersionAsync(
        agentName: agentName, 
        options: new(agentDefinition));

    var agentVersion = agentResult.Value;

    // Set immediately after successful creation.
    agentCreated = true;

    Console.WriteLine($"Agent created: {agentVersion.Name}, version {agentVersion.Version}");

    var responsesClient = projectClient
        .ProjectOpenAIClient
        .GetProjectResponsesClientForAgent(agentVersion.Name);

    const int maxModelCalls = 5;

    string? previousResponseId = null;

    Console.WriteLine();
    Console.WriteLine("Foundry Labs - Agents");
    Console.WriteLine("Budget values are fictional laboratory data.");
    Console.WriteLine("Commands: /new to start over, /exit to quit.");
    Console.WriteLine();

    while(true)
    {
        Console.Write("You: ");
        var input = Console.ReadLine();

        if (input is null)
            break;

        var prompt = input.Trim();

        if (string.IsNullOrWhiteSpace(prompt))
            continue;

        if (string.Equals(prompt, "/exit", StringComparison.OrdinalIgnoreCase))
            break;

        if (string.Equals(
            prompt, "/new", StringComparison.OrdinalIgnoreCase))
        {
            previousResponseId = null;

            Console.WriteLine("New conversation started.");
            Console.WriteLine();

            continue;
        }

        var pendingItems = new List<ResponseItem>
        {
            ResponseItem.CreateUserMessageItem(prompt)
        };

        try
        {
            for (var round = 0; round < maxModelCalls; round++)
            {
                var options = new CreateResponseOptions
                {
                    PreviousResponseId = previousResponseId
                };

                foreach (var item in pendingItems)
                {
                    options.InputItems.Add(item);
                }

                var result = await responsesClient.CreateResponseAsync(options);

                var response = result.Value;

                if (response.Status != ResponseStatus.Completed)
                {
                    Console.WriteLine($"Response not completed. Status: {response.Status}");

                    previousResponseId = null;
                    break;
                }

                var functionCalls = response.OutputItems
                    .OfType<FunctionCallResponseItem>()
                    .ToList();

                if (functionCalls.Count == 0)
                {
                    previousResponseId = response.Id;

                    var text = response.GetOutputText();

                    Console.WriteLine(
                        string.IsNullOrWhiteSpace(text)
                            ? "Assistant: No text response returned."
                            : $"Assistant: {text}");

                    break;
                }

                if (round == maxModelCalls - 1)
                {
                    Console.WriteLine(
                        "Tool-calling limit reached. Conversation reset.");

                    previousResponseId = null;
                    break;
                }

                previousResponseId = response.Id;
                pendingItems.Clear();

                foreach (var call in functionCalls)
                {
                    Console.WriteLine($"[Tool requested: {call.FunctionName}]");

                    var output = ExecuteFunction(call);

                    Console.WriteLine($"[Tool result: {output}]");

                    pendingItems.Add(
                        ResponseItem.CreateFunctionCallOutputItem(
                            call.CallId,
                            output));
                }

            }
        }
        catch (ClientResultException exception)
        {
            Console.WriteLine(
                $"Request failed (HTTP {exception.Status}): " +
                exception.Message);

            previousResponseId = null;

            Console.WriteLine("Conversation reset after the failed request.");
        }

        Console.WriteLine();
    }
}
catch (ClientResultException exception)
{
    Console.Error.WriteLine(
        $"Agent operation failed (HTTP {exception.Status}): " +
        exception.Message);
}
finally
{
    if (agentCreated)
    {
        Console.WriteLine();
        Console.WriteLine($"Deleting agent: {agentName}");

        try
        {
            await projectClient.AgentAdministrationClient.DeleteAgentAsync(
                agentName: agentName);

            Console.WriteLine("Agent deleted.");
        }
        catch (ClientResultException exception)
        {
            Console.Error.WriteLine(
                $"Could not delete agent '{agentName}' " +
                $"(HTTP {exception.Status}): {exception.Message}");

            Console.Error.WriteLine(
                "Check the project and remove this laboratory agent manually.");
        }
    }
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

#pragma warning restore OPENAI001