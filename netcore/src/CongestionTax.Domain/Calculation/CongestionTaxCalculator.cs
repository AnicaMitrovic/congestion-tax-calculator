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

    public int GetTax(Vehicle vehicle, DateTime[] dates)
    {
        DateTime intervalStart = dates[0];
        int totalFee = 0;
        foreach (DateTime date in dates)
        {
            int nextFee = GetTollFee(date, vehicle);
            int tempFee = GetTollFee(intervalStart, vehicle);

            long diffInMillies = date.Millisecond - intervalStart.Millisecond;
            long minutes = diffInMillies / 1000 / 60;

            if (minutes <= 60)
            {
                if (totalFee > 0) totalFee -= tempFee;
                if (nextFee >= tempFee) tempFee = nextFee;
                totalFee += tempFee;
            }
            else
            {
                totalFee += nextFee;
            }
        }
        if (totalFee > 60) totalFee = 60;
        return totalFee;
    }

    private bool IsTollFreeVehicle(Vehicle vehicle)
    {
        if (vehicle == null) return false;
        String vehicleType = vehicle.GetVehicleType();
        return vehicleType.Equals(TollFreeVehicles.Motorcycle.ToString()) ||
               vehicleType.Equals(TollFreeVehicles.Tractor.ToString()) ||
               vehicleType.Equals(TollFreeVehicles.Emergency.ToString()) ||
               vehicleType.Equals(TollFreeVehicles.Diplomat.ToString()) ||
               vehicleType.Equals(TollFreeVehicles.Foreign.ToString()) ||
               vehicleType.Equals(TollFreeVehicles.Military.ToString());
    }

    public int GetTollFee(DateTime date, Vehicle vehicle)
    {
        if (IsTollFreeDate(date) || IsTollFreeVehicle(vehicle)) return 0;

        var time = TimeOnly.FromDateTime(date);

        foreach (var band in TariffBands)
            if (time >= band.Start && time < band.EndExclusive)
                return band.Amount;

        return 0;
    }

    private Boolean IsTollFreeDate(DateTime date)
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

    private enum TollFreeVehicles
    {
        Motorcycle = 0,
        Tractor = 1,
        Emergency = 2,
        Diplomat = 3,
        Foreign = 4,
        Military = 5
    }
}
