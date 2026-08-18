using CongestionTax.Domain.Rules;
namespace CongestionTax.Domain.Calculation;

public class CongestionTaxCalculator
{
    /// <summary>
    /// Calculates congestion tax for one vehicle's passages. Timestamps are
    /// interpreted as Swedish local wall-clock time; DateTimeKind is not consulted.
    /// </summary>
    public static TaxCalculationResult Calculate(
        CityTaxRules rules,
        VehicleType vehicle,
        IEnumerable<DateTime> passages)
    {
        return null;
    }

    private static readonly (TimeOnly Start, TimeOnly EndExclusive, int Amount)[] TariffBands =
    [
        (new(6, 0),   new(6, 30),   8),
        (new(6, 30),  new(7, 0),   13),
        (new(7, 0),   new(8, 0),   18),
        (new(8, 0),   new(8, 30),  13),
        (new(8, 30),  new(15, 0),   8),
        (new(15, 0),  new(15, 30), 13),
        (new(15, 30), new(17, 0),  18),
        (new(17, 0),  new(18, 0),  13),
        (new(18, 0),  new(18, 30),  8),
    ];

    private static readonly HashSet<VehicleType> TollFreeVehicles =
    [
        VehicleType.Motorcycle,
        VehicleType.Bus,
        VehicleType.Emergency,
        VehicleType.Diplomat,
        VehicleType.Military,
        VehicleType.Foreign,
    ];

    private static bool IsTollFreeVehicle(VehicleType vehicle) =>
        TollFreeVehicles.Contains(vehicle);

    public int GetTax(VehicleType vehicle, DateTime[] dates) // handles total tax for one vehicle for one day
    {
        if (dates == null || dates.Length == 0) return 0;

        // Skatteverket: the hour and the started minute decide the amount, so we don´t need seconds 
        var passages = dates
            .Select(d => new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, 0))
            .OrderBy(d => d)
            .ToArray();

        int total = 0;
        DateTime? windowStart = null;
        int windowHighest = 0;

        foreach (var passage in passages)
        {
            int fee = GetTollFee(passage, vehicle);

            // free passage is not taxed, so it cannot start a 60-minute window
            if (fee == 0) continue;

            bool outsideWindow = windowStart == null
                || (passage - windowStart.Value).TotalMinutes > 60;

            if (outsideWindow)
            {
                total += windowHighest;   // bank the previous window (0 on the first pass)
                windowStart = passage;
                windowHighest = fee;
            }
            else
            {
                windowHighest = Math.Max(windowHighest, fee);
            }
        }

        total += windowHighest;           // bank the last window

        if (total > 60) total = 60;
        return total;
    }

    public int GetTollFee(DateTime date, VehicleType vehicle)
    {
        if (IsTollFreeDate(date) || IsTollFreeVehicle(vehicle)) return 0;

        var time = TimeOnly.FromDateTime(date);

        foreach (var band in TariffBands)
            if (time >= band.Start && time < band.EndExclusive)
                return band.Amount;

        return 0;
    }

    private bool IsTollFreeDate(DateTime date)
    {
        int year = date.Year;
        int month = date.Month;
        int day = date.Day;

        if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) return true;

        if (year == 2013)
        {
            if (month == 1 && day == 1 ||
                month == 3 && (day == 28 || day == 29) ||
                month == 4 && (day == 1 || day == 30) ||
                month == 5 && (day == 1 || day == 8 || day == 9) ||
                month == 6 && (day == 5 || day == 6 || day == 21) ||
                month == 7 ||
                month == 11 && day == 1 ||
                month == 12 && (day == 24 || day == 25 || day == 26 || day == 31))
            {
                return true;
            }
        }
        return false;
    }
}
