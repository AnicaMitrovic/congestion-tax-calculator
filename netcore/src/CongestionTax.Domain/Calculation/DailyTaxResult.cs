namespace CongestionTax.Domain.Calculation;

/// <summary>A single amount charged, attributed to the single-charge window it opened.</summary>
public sealed record Charge(DateTime WindowStart, int Amount);

/// <summary>The tax owed for one calendar day, before and after the daily cap.</summary>
public sealed record DailyTaxResult(
    DateOnly Date,
    int Total,
    int SumOfCharges,
    IReadOnlyList<Charge> Charges);

/// <summary>The tax owed across every day covered by a set of passages.</summary>
public sealed record TaxCalculationResult(IReadOnlyList<DailyTaxResult> Days)
{
    public int Total => Days.Sum(d => d.Total);
}
