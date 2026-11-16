using System.Text.Json;
using TrainingHelper.FreeExerciseDbApi.Models;
using NLog;

var logger = LogManager.GetCurrentClassLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
// Load exercises once when application starts
var filePath = Path.Combine(app.Environment.ContentRootPath, "Data", "exercises.json");

var json = await File.ReadAllTextAsync(filePath);

var exercises = JsonSerializer.Deserialize<List<Exercise>>(json, new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true
}) ?? [];

logger.Info($"Loaded {exercises.Count} exercises");

app.MapGet("/", () => new
{
    message = "Exercise API is running",
    totalExercises = exercises.Count
});

app.MapGet("/api/exercises", (string? muscle, string? equipment, string? level, string? category, int page = 1, int pageSize = 20) =>
{
    var query = exercises.AsEnumerable();

    if (!string.IsNullOrWhiteSpace(muscle))
    {
        query = query.Where(e => e.PrimaryMuscles.Any(m => m.Contains(muscle, StringComparison.OrdinalIgnoreCase)));
    }

    if (!string.IsNullOrWhiteSpace(equipment))
    {
        query = query.Where(e => e.Equipment?.Contains(equipment, StringComparison.OrdinalIgnoreCase) == true);
    }

    if (!string.IsNullOrWhiteSpace(level))
    {
        query = query.Where(e => e.Level?.Equals(level, StringComparison.OrdinalIgnoreCase) == true);
    }

    if (!string.IsNullOrWhiteSpace(category))
    {
        query = query.Where(e => e.Category?.Equals(category, StringComparison.OrdinalIgnoreCase) == true);
    }

    page = Math.Max(page, 1);
    pageSize = Math.Clamp(pageSize, 1, 100);

    var total = query.Count();
    var results = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

    return Results.Ok(new
    {
        total,
        page,
        pageSize,
        data = results
    });
});

app.MapGet("/api/exercises/search", (string q) =>
{
    var results = exercises.Where(e => e.Name.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
    return Results.Ok(results);
});

app.MapGet("/api/exercises/{id}", (string id) =>
{
    var exercise = exercises.FirstOrDefault(e => e.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    return exercise is null
        ? Results.NotFound()
        : Results.Ok(exercise);
});

app.Run();
