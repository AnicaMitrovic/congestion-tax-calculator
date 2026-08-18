using CongestionTax.Api;
using CongestionTax.Domain.Calculation;
using CongestionTax.Domain.Rules;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddSingleton<ITaxRuleProvider>(_ =>
    new JsonFileTaxRuleProvider(Path.Combine(AppContext.BaseDirectory, "rules")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/api/tax/calculate",
    IResult (CalculateTaxRequest request, ITaxRuleProvider provider) =>
    {
        var errors = new Dictionary<string, string[]>();

        var rules = provider.GetRules(request.City);
        if (rules is null)
            errors["city"] = [$"Unknown city. Available: {string.Join(", ", provider.AvailableCities)}."];

        if (request.Passages is null || request.Passages.Length == 0)
            errors["passages"] = ["At least one passage timestamp is required."];
        else if (request.Passages.Any(p => p.Year != 2013))
            errors["passages"] = ["Only 2013 is supported; the tax-free calendar is not defined for other years."];

        if (errors.Count > 0)
            return Results.ValidationProblem(errors);

        var result = CongestionTaxCalculator.Calculate(
            rules!, request.VehicleType, request.Passages);

        return Results.Ok(new CalculateTaxResponse(
            rules!.City,
            result.Total,
            result.Days
                .Select(d => new DailyBreakdown(
                    d.Date,
                    d.Total,
                    d.SumOfCharges,
                    d.Charges.Select(c => new ChargeDto(c.WindowStart, c.Amount)).ToArray()))
                .ToArray()));
    });

app.MapGet("/api/cities", (ITaxRuleProvider provider) => provider.AvailableCities);

app.Run();
