using CongestionTax.Domain.Rules;

namespace CongestionTax.Domain.Calculation;

public static class CongestionTaxCalculator
{
    /// <summary>
    /// Calculates congestion tax for one vehicle's passages. Timestamps are
    /// interpreted as Swedish local wall-clock time; DateTimeKind is not consulted.
    /// </summary>
    public static TaxCalculationResult Calculate(
        CityTaxRules rules,
        VehicleType vehicle,
        IEnumerable<DateTime> passages) => throw new NotImplementedException();
}
