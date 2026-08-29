using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using TrainingHelper.Common.Exercise;
using TrainingHelper.FreeExerciseDb.JSON;

namespace TrainingHelper.FreeExerciseDb;

internal class FreeExercsieDbExercsieProvider : IExerciseProvider
{
    private readonly Lazy<Task<ConcurrentDictionary<string, Exercise>>> _exercsies;
    private readonly string _jsonExercsieFile;
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    public FreeExercsieDbExercsieProvider([FromKeyedServices("jsonFile")] string jsonFile)
    {
        if (!File.Exists(jsonFile))
        {
            throw new ArgumentException(nameof(jsonFile));
        }
        _jsonExercsieFile = jsonFile;
        _exercsies = new(LoadExercisesAsync);
    }

    public async Task<ExerciseDetails?> GetExerciseDetailsAsync(string exercsieId, CancellationToken ct)
    {
        ExerciseDetails? result = null;
        try
        {
            var exercises = await _exercsies.Value;
            ct.ThrowIfCancellationRequested();

            if (exercises.TryGetValue(exercsieId, out var exercise))
            {
                result = new ExerciseDetails() { Name = exercise.Name, Id = exercise.Id };
            }
            else
            {
                _logger.Warn($"No exercsie {exercsieId} in json file");
            }
        }
        catch (InvalidOperationException ioe)
        {
            _logger.Error($"More than one exercise with name {exercsieId}", ioe);
        }
        return result;
    }

    public async Task<IEnumerable<ExerciseDetails>> GetAllExerciseDetailsAsync(CancellationToken ct)
    {
        var exercises = await _exercsies.Value;
        ct.ThrowIfCancellationRequested();

        return exercises.Values
            .Select(e => new ExerciseDetails() { Name = e.Name, Id = e.Id })
            .ToList();
    }

    private async Task<ConcurrentDictionary<string, Exercise>> LoadExercisesAsync()
    {
        var result = new ConcurrentDictionary<string, Exercise>();

        await using var stream = File.OpenRead(_jsonExercsieFile);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        
        await foreach (var exercise in JsonSerializer.DeserializeAsyncEnumerable<Exercise>(stream, options, CancellationToken.None))
        {
            if (exercise is not null && !string.IsNullOrEmpty(exercise.Id))
            {
                result.TryAdd(exercise.Id, exercise);
            }
        }
        return result;
    }
}
