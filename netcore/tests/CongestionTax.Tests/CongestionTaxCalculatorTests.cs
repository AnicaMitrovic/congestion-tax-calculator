using CongestionTax.Domain;
using CongestionTax.Domain.Calculation;
using System.Runtime.ConstrainedExecution;

namespace CongestionTax.Tests;

public class CongestionTaxCalculatorTests
{
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

    [Fact]
    public void SampleDay_2013_02_08_Returns60()
    {
        var calculator = new CongestionTaxCalculator();
        Assert.Equal(60, calculator.GetTax(VehicleType.Car, SampleDay));
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
        var calculator = new CongestionTaxCalculator();
        Assert.Equal(0, calculator.GetTax(vehicle, SampleDay));
    }

    [Fact]
    public void Car_IsNotExempt()
    {
        var calculator = new CongestionTaxCalculator();
        Assert.Equal(60, calculator.GetTax(VehicleType.Car, SampleDay));
    }
}
