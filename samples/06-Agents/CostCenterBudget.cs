namespace _06_Agents;

public sealed record CostCenterBudget(
    string CostCenter,
    decimal AvailableBudget,
    string Currency);
