using CongestionTax.Domain;

namespace CongestionTax.Api;

public sealed record CalculateTaxRequest(string City, VehicleType VehicleType, DateTime[] Passages);

public sealed record ChargeDto(DateTime WindowStart, int Amount);

public sealed record DailyBreakdown(DateOnly Date, int Total, int SubtotalBeforeCap, ChargeDto[] Charges);

public sealed record CalculateTaxResponse(string City, int Total, DailyBreakdown[] Days);
