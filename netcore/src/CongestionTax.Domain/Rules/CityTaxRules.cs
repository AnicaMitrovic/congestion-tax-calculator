namespace CongestionTax.Domain.Rules;

public sealed record CityTaxRules(
    string City,
    IReadOnlyList<(TimeOnly Start, TimeOnly EndExclusive, int Amount)> Bands,
    int DailyCapSek,
    TimeSpan SingleChargeWindow,
    IReadOnlySet<VehicleType> ExemptVehicles,
    IReadOnlySet<DayOfWeek> TaxFreeWeekdays,
    IReadOnlySet<int> TaxFreeMonths,
    IReadOnlySet<DateOnly> TaxFreeDates)
{
    public int FeeAt(DateTime passage)
    {
        var time = TimeOnly.FromDateTime(passage);

        // start included, EndExclusive not. 06:30 belongs to the next band.
        foreach (var band in Bands)
            if (time >= band.Start && time < band.EndExclusive)
                return band.Amount;

        return 0;
    }

    public bool IsTaxFreeDate(DateOnly date) =>
        TaxFreeWeekdays.Contains(date.DayOfWeek) ||
        TaxFreeMonths.Contains(date.Month) ||
        TaxFreeDates.Contains(date);
}