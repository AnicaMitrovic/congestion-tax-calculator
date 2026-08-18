using CongestionTax.Domain.Rules;

namespace CongestionTax.Api;

/// <summary>Reads city tax parameters from JSON files in a directory on disk.</summary>
public sealed class JsonFileTaxRuleProvider : ITaxRuleProvider
{
    private readonly string _rulesDirectory;

    public JsonFileTaxRuleProvider(string rulesDirectory) => _rulesDirectory = rulesDirectory;

    public IReadOnlyCollection<string> AvailableCities => throw new NotImplementedException();

    public CityTaxRules? GetRules(string city) => throw new NotImplementedException();

    /// <summary>The on-disk shape of a rules file, e.g. rules/gothenburg.json.</summary>
    private sealed record CityTaxRulesDto(
        string? City,
        IReadOnlyList<TariffBandDto>? Bands,
        int DailyCapSek,
        int SingleChargeWindowMinutes,
        IReadOnlyList<string>? ExemptVehicles,
        IReadOnlyList<string>? TaxFreeWeekdays,
        IReadOnlyList<int>? TaxFreeMonths,
        IReadOnlyList<string>? TaxFreeDates);

    private sealed record TariffBandDto(string? Start, string? End, int Amount);
}
