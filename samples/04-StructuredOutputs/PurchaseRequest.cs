using System.Text.Json.Serialization;

namespace _04_StructuredOutputs;

internal class PurchaseRequest
{
    [JsonPropertyName("item")]
    public required string? Item { get; init; }

    [JsonPropertyName("quantity")]
    public required int? Quantity { get; init; }

    [JsonPropertyName("totalBudget")]
    public required decimal? TotalBudget { get; init; }

    [JsonPropertyName("costCenter")]
    public required string? CostCenter { get; init; }
}
