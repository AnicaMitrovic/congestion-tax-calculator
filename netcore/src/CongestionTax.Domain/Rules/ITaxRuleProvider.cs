namespace CongestionTax.Domain.Rules;

/// <summary>Supplies the tax parameters for a city from outside the application.</summary>
public interface ITaxRuleProvider
{
    CityTaxRules? GetRules(string city);

    IReadOnlyCollection<string> AvailableCities { get; }
}
