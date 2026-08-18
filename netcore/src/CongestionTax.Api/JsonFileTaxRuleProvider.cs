using System.Text.Json;
using System.Text.Json.Serialization;
using CongestionTax.Domain;
using CongestionTax.Domain.Rules;

namespace CongestionTax.Api;

public sealed class JsonFileTaxRuleProvider : ITaxRuleProvider
{
    private readonly Dictionary<string, CityTaxRules> _rules;

    public JsonFileTaxRuleProvider(string rulesDirectory)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        // Load all JSON files in the directory and deserialize them into CityRulesDto objects, then convert to domain objects and store in a dictionary
        _rules = Directory
            .EnumerateFiles(rulesDirectory, "*.json")
            .Select(path => JsonSerializer.Deserialize<CityRulesDto>(
                File.ReadAllText(path), options)
                ?? throw new InvalidOperationException($"Could not read rules from {path}."))
            .Select(ToDomain)
            .ToDictionary(r => r.City, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyCollection<string> AvailableCities => _rules.Keys;

    public CityTaxRules? GetRules(string city) =>
        _rules.TryGetValue(city, out var rules) ? rules : null;

    private static CityTaxRules ToDomain(CityRulesDto dto) => new(
        City: dto.City,
        Bands: dto.Bands
            .Select(b => (TimeOnly.Parse(b.Start), TimeOnly.Parse(b.EndExclusive), b.Amount))
            .ToList(),
        DailyCapSek: dto.DailyCapSek,
        SingleChargeWindow: TimeSpan.FromMinutes(dto.SingleChargeWindowMinutes),
        ExemptVehicles: dto.ExemptVehicles.ToHashSet(),
        TaxFreeWeekdays: dto.TaxFreeWeekdays.ToHashSet(),
        TaxFreeMonths: dto.TaxFreeMonths.ToHashSet(),
        TaxFreeDates: dto.TaxFreeDates.Select(DateOnly.Parse).ToHashSet());

    // hapes the file format, so the JSON is not tied to the domain types.
    private sealed record CityRulesDto(
        string City,
        List<BandDto> Bands,
        int DailyCapSek,
        int SingleChargeWindowMinutes,
        List<VehicleType> ExemptVehicles,
        List<DayOfWeek> TaxFreeWeekdays,
        List<int> TaxFreeMonths,
        List<string> TaxFreeDates);

    private sealed record BandDto(string Start, string EndExclusive, int Amount);
}