using CongestionTax.Domain;
using CongestionTax.Domain.Calculation;
using CongestionTax.Domain.Rules;

namespace CongestionTax.Tests;

public class CongestionTaxCalculatorTests
{
    // rules are built here , not loaded from JSON
    private static readonly CityTaxRules Gothenburg2013 = new(
        City: "gothenburg",
        Bands:
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
        ],
        DailyCapSek: 60,
        SingleChargeWindow: TimeSpan.FromMinutes(60),
        ExemptVehicles: new HashSet<VehicleType>
        {
            VehicleType.Motorcycle, VehicleType.Bus, VehicleType.Emergency,
            VehicleType.Diplomat, VehicleType.Military, VehicleType.Foreign
        },
        TaxFreeWeekdays: new HashSet<DayOfWeek> { DayOfWeek.Saturday, DayOfWeek.Sunday },
        TaxFreeMonths: new HashSet<int> { 7 },
        TaxFreeDates: new HashSet<DateOnly>
        {
            new(2013, 1, 1),
            new(2013, 3, 28), new(2013, 3, 29),
            new(2013, 4, 1),  new(2013, 4, 30),
            new(2013, 5, 1),  new(2013, 5, 8),  new(2013, 5, 9),
            new(2013, 6, 5),  new(2013, 6, 6),  new(2013, 6, 21),
            new(2013, 11, 1),
            new(2013, 12, 24), new(2013, 12, 25),
            new(2013, 12, 26), new(2013, 12, 31),
        });

    // dates from the post-it note, for 2013-02-08.
    private static readonly DateTime[] SampleDay =
    [
        new(2013, 2, 8, 6, 20, 27),
        new(2013, 2, 8, 6, 27, 0),
        new(2013, 2, 8, 14, 35, 0),
        new(2013, 2, 8, 15, 29, 0),
        new(2013, 2, 8, 15, 47, 0),
        new(2013, 2, 8, 16, 1, 0),
        new(2013, 2, 8, 16, 48, 0),
        new(2013, 2, 8, 17, 49, 0),
        new(2013, 2, 8, 18, 29, 0),
        new(2013, 2, 8, 18, 35, 0),
    ];

    private static TaxCalculationResult Calculate(
        VehicleType vehicle, params DateTime[] passages) =>
        CongestionTaxCalculator.Calculate(Gothenburg2013, vehicle, passages);

    [Theory]
    [InlineData(5, 59, 0)]    // before charging hours
    [InlineData(6, 0, 8)]
    [InlineData(6, 29, 8)]
    [InlineData(6, 30, 13)]
    [InlineData(7, 0, 18)]
    [InlineData(8, 0, 13)]
    [InlineData(8, 29, 13)]
    [InlineData(8, 30, 8)]
    [InlineData(9, 15, 8)]    // was 0 before the fix , the fix was to make the end time exclusive, not inclusive
    [InlineData(12, 10, 8)]
    [InlineData(14, 59, 8)]
    [InlineData(15, 0, 13)]
    [InlineData(15, 29, 13)]
    [InlineData(15, 30, 18)]
    [InlineData(16, 59, 18)]
    [InlineData(17, 0, 13)]
    [InlineData(18, 0, 8)]
    [InlineData(18, 29, 8)]
    [InlineData(18, 30, 0)]   // after charging hours
    public void SinglePassage_ChargesTheBandAmount(int hour, int minute, int expected)
    {
        var result = Calculate(VehicleType.Car, new DateTime(2013, 2, 8, hour, minute, 0));

        Assert.Equal(expected, result.Total);
    }

    [Fact]
    public void SampleDay_ChargesFiveWindowsAndHitsTheCap()
    {
        var result = Calculate(VehicleType.Car, SampleDay);
        var day = Assert.Single(result.Days);

        // 8 + 13 + 18 + 18 + 13 = 70, capped to 60. Asserting only the capped
        // figure would pass even if the windows were grouped wrongly, since
        // anything above 60 comes out as 60.
        Assert.Equal(5, day.Charges.Count);
        Assert.Equal(70, day.SumOfCharges);
        Assert.Equal(60, day.Total);
    }

    [Fact]
    public void PassagesExactly61MinutesApart_AreTwoWindows()
    {
        // 16:48 and 17:49 from the sample day: 61 minutes, so a new window opens.
        var result = Calculate(VehicleType.Car,
            new DateTime(2013, 2, 8, 16, 48, 0),
            new DateTime(2013, 2, 8, 17, 49, 0));

        Assert.Equal(2, result.Days.Single().Charges.Count);
        Assert.Equal(31, result.Total);   // 18 + 13
    }

    [Fact]
    public void PassagesExactly60MinutesApart_AreOneWindow()
    {
        // "within 60 minutes" read as inclusive. See questions.md.
        var result = Calculate(VehicleType.Car,
            new DateTime(2013, 2, 8, 15, 47, 0),
            new DateTime(2013, 2, 8, 16, 47, 0));

        Assert.Equal(1, result.Days.Single().Charges.Count);
        Assert.Equal(18, result.Total);
    }

    [Theory]
    [InlineData(VehicleType.Motorcycle)]
    [InlineData(VehicleType.Bus)]
    [InlineData(VehicleType.Emergency)]
    [InlineData(VehicleType.Diplomat)]
    [InlineData(VehicleType.Military)]
    [InlineData(VehicleType.Foreign)]
    public void ExemptVehicles_PayNothing(VehicleType vehicle)
    {
        Assert.Equal(0, Calculate(vehicle, SampleDay).Total);
    }

    [Theory]
    [InlineData(2013, 2, 9)]    // Saturday
    [InlineData(2013, 2, 10)]   // Sunday
    [InlineData(2013, 7, 15)]   // July
    [InlineData(2013, 12, 25)]  // Christmas Day
    [InlineData(2013, 12, 24)]  // day before a public holiday
    public void TaxFreeDays_PayNothing(int year, int month, int day)
    {
        var result = Calculate(VehicleType.Car, new DateTime(year, month, day, 15, 47, 0));

        Assert.Equal(0, result.Total);
    }

    [Fact]
    public void SecondsAreIgnored()
    {
        // 06:20:27 and 06:20:59 are the same minute, so the same amount.
        var result = Calculate(VehicleType.Car, new DateTime(2013, 2, 8, 6, 20, 27));

        Assert.Equal(8, result.Total);
    }
}