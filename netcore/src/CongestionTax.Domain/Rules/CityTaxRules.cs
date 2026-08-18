namespace CongestionTax.Domain.Rules;

/// <summary>A tariff window and the amount charged for a passage inside it.</summary>
public sealed record TariffBand(TimeOnly Start, TimeOnly EndExclusive, int Amount)
{
    public bool Contains(TimeOnly time) => throw new NotImplementedException();
}

/// <summary>The complete set of congestion tax parameters for one city.</summary>
public sealed record CityTaxRules(
    string City,
    IReadOnlyList<TariffBand> Bands,
    int DailyCapSek,
    TimeSpan SingleChargeWindow,
    IReadOnlySet<VehicleType> ExemptVehicles,
    IReadOnlySet<DayOfWeek> TaxFreeWeekdays,
    IReadOnlySet<int> TaxFreeMonths,
    IReadOnlySet<DateOnly> TaxFreeDates)
{
    public int FeeAt(DateTime passage) => throw new NotImplementedException();

    public bool IsTaxFreeDate(DateOnly date) => throw new NotImplementedException();
}
