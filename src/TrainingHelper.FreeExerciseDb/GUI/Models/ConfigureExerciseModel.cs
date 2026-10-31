using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using NLog;
using TrainingHelper.Common.Settings;
using TrainingHelper.FreeExerciseDb.JSON;
using TrainingHelper.FreeExerciseDb.Settings;

namespace TrainingHelper.FreeExerciseDb.GUI.Models;

internal class ConfigureExerciseModel
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private readonly Lazy<Task<ConcurrentDictionary<string, Exercise>>> _exercsies;
    private readonly string _jsonExercsieFile;
    private readonly string _imagesBasePath;

    public ConfigureExerciseModel(ISettingsService<FreeExerciseDbConfig> settingsService)
    {
        _jsonExercsieFile = settingsService.Load().FreeExercsieDbExercsieJson;
        _imagesBasePath = settingsService.Load().ImagesPathBase;

        if (!File.Exists(_jsonExercsieFile))
        {
            throw new ArgumentException(nameof(_jsonExercsieFile));
        }

        _exercsies = new(LoadExercisesAsync, true);
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
                List<string> imgagesPaths = GetFullPathsOfImages(exercise);
                result = new ExerciseDetails() { Name = exercise.Name, Id = exercise.Id, ImagesPaths = imgagesPaths };
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
            .Select(e => new ExerciseDetails() { Name = e.Name, Id = e.Id, ImagesPaths = GetFullPathsOfImages(e) })
            .ToList();
    }

    private async Task<ConcurrentDictionary<string, Exercise>> LoadExercisesAsync()
    {
        var result = new ConcurrentDictionary<string, Exercise>();

        await using var stream = File.OpenRead(_jsonExercsieFile);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
        };

        try
        {
            await foreach (var exercise in JsonSerializer.DeserializeAsyncEnumerable<Exercise>(stream, options, CancellationToken.None))
            {
                if (exercise is not null && !string.IsNullOrEmpty(exercise.Id))
                {
                    result.TryAdd(exercise.Id, exercise);
                }
            }
        }
        catch (JsonException je)
        {
            _logger.Warn("Error deserializing exercises from JSON file", je);
        }

        return result;
    }

    private List<string> GetFullPathsOfImages(Exercise exercise)
    {
        var imgagesPaths = new List<string>();
        foreach (var f in exercise.Images)
        {
            var iamgePath = Path.Combine(_imagesBasePath, f);
            if (File.Exists(iamgePath))
            {
                imgagesPaths.Add(iamgePath);
            }
            else
            {
                _logger.Warn($"File {iamgePath} does not exists");
            }
        }

        return imgagesPaths;
    }
}