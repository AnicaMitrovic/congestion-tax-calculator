using CongestionTax.Domain.Rules;
namespace CongestionTax.Domain.Calculation;

public class CongestionTaxCalculator
{
    /// <summary>
    /// Calculates congestion tax for one vehicle's passages.
    /// </summary>
    public static TaxCalculationResult Calculate(
    CityTaxRules rules,
    VehicleType vehicle,
    IEnumerable<DateTime> passages)
    {
        ArgumentNullException.ThrowIfNull(rules);
        ArgumentNullException.ThrowIfNull(passages);

        if (rules.ExemptVehicles.Contains(vehicle))
            return new TaxCalculationResult([]);

        // the hour and the started minute decide the amount,so seconds are not used
        var days = passages
            .Select(TruncateToMinute)
            .OrderBy(p => p)
            .GroupBy(DateOnly.FromDateTime)
            .Where(day => !rules.IsTaxFreeDate(day.Key))
            .Select(day =>
            {
                var (total, sumOfCharges, charges) = CalculateDay(rules, day);
                return new DailyTaxResult(day.Key, total, sumOfCharges, charges);
            })
            .ToList();

        return new TaxCalculationResult(days);
    }

    private static DateTime TruncateToMinute(DateTime d) =>
        new(d.Year, d.Month, d.Day, d.Hour, d.Minute, 0);

    private static (int Total, int SumOfCharges, List<Charge> Charges) CalculateDay(
        CityTaxRules rules, IEnumerable<DateTime> orderedPassages)
    {
        var charges = new List<Charge>();
        DateTime? windowStart = null;
        int windowHighest = 0;

        foreach (var passage in orderedPassages)
        {
            int fee = rules.FeeAt(passage);

            // free passage is free of tax so it cannot open a window
            if (fee == 0) continue;

            bool outsideWindow = windowStart == null
                || passage - windowStart.Value > rules.SingleChargeWindow;

            if (outsideWindow)
            {
                if (windowStart != null)
                    charges.Add(new Charge(windowStart.Value, windowHighest));

                windowStart = passage;
                windowHighest = fee;
            }
            else
            {
                windowHighest = Math.Max(windowHighest, fee);
            }
        }

        if (windowStart != null)
            charges.Add(new Charge(windowStart.Value, windowHighest));

        int sumOfCharges = charges.Sum(c => c.Amount);
        return (Math.Min(sumOfCharges, rules.DailyCapSek), sumOfCharges, charges);
    }
}
