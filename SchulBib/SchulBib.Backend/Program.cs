using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateSlimBuilder(args);

builder.AddServiceDefaults();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "SchulBib API",
        Version = "v1",
        Description = "API für die datenschutzkonforme Bibliotheksverwaltung für Schulen",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "SchulBib Community",
            Email = "info@schulbib.de"
        },
        License = new Microsoft.OpenApi.Models.OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });
});

builder.Services.Configure<RouteOptions>(options =>
{
   options.SetParameterPolicy<RegexInlineRouteConstraint>("regex");
});

var app = builder.Build();

// Statische Dateien aktivieren (für YAML-Datei)
app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    // Verwende die YAML-Datei statt der generierten JSON
    options.SwaggerEndpoint("/swagger/openapi.yaml", "SchulBib API v1 (YAML)");
    // Behalte den Standardendpoint für Kompatibilität
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "SchulBib API v1");
    options.RoutePrefix = "swagger";
    options.DocumentTitle = "SchulBib API Dokumentation";
});

// Endpoint für den OpenAPI-Export
app.MapGet("/openapi.json", () => Results.Redirect("/swagger/v1/swagger.json"));

app.MapDefaultEndpoints();

var sampleTodos = new Todo[] {
    new(1, "Walk the dog"),
    new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
    new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
    new(4, "Clean the bathroom"),
    new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
};

var todosApi = app.MapGroup("/todos");
todosApi.MapGet("/", () => sampleTodos);
todosApi.MapGet("/{id}", (int id) =>
    sampleTodos.FirstOrDefault(a => a.Id == id) is { } todo
        ? Results.Ok(todo)
        : Results.NotFound());

app.Run();

public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);

[JsonSerializable(typeof(Todo[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}