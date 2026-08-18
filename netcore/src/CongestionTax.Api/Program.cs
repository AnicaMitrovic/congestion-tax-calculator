using System.Text.Json.Serialization;
using CongestionTax.Api;
using CongestionTax.Domain.Rules;

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
    IResult (CalculateTaxRequest request) => throw new NotImplementedException());

app.Run();
