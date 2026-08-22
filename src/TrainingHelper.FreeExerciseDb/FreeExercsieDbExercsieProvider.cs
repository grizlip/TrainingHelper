using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using TrainingHelper.Common.Exercise;
using TrainingHelper.FreeExerciseDb.JSON;

namespace TrainingHelper.FreeExerciseDb;

internal class FreeExercsieDbExercsieProvider : IExerciseProvider
{
    private readonly ConcurrentDictionary<string, Exercise> _exercsiessDetails = [];
    private readonly List<(string Id, string Name)> _availableExercises = [];
    private readonly string _jsonExercsieFile;
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    public FreeExercsieDbExercsieProvider([FromKeyedServices("jsonFile")] string jsonFile)
    {
        if (!File.Exists(jsonFile))
        {
            throw new ArgumentException(nameof(jsonFile));
        }
        _jsonExercsieFile = jsonFile;
    }

    public async Task<ExerciseDetails?> GetExerciseDetailsAsync(string exercsieId, CancellationToken ct)
    {
        try
        {
            if (!_exercsiessDetails.TryGetValue(exercsieId, out var result))
            {
                result = await GetExercisesAsync(ct).SingleOrDefaultAsync(e => e?.Id == exercsieId);
                if (result != null)
                {
                    _exercsiessDetails.TryAdd(exercsieId, result);
                }
            }
            ct.ThrowIfCancellationRequested();
            if (result != null)
            {
                //TODO: add more details
                return new ExerciseDetails() { Name = result.Name, Id = result.Id };
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
        return null;
    }

    public async Task<IEnumerable<(string Id, string Name)>> GetExercisesIdsAndNamesAsync(CancellationToken ct = default)
    {
        if (_availableExercises.Count == 0)
        {
            await ReadIdsAndNamesAsync(_availableExercises.Add, ct);
        }
        return _availableExercises;
    }

    private async Task ReadIdsAndNamesAsync(Action<(string, string)> onItemFound, CancellationToken ct = default)
    {
        await using var stream = File.OpenRead(_jsonExercsieFile);

        byte[] buffer = new byte[64 * 1024];
        var state = new JsonReaderState();

        int bytesInBuffer = 0;

        while (true)
        {
            var bytesRead = await stream.ReadAsync(buffer.AsMemory(bytesInBuffer), ct);

            if (bytesRead == 0)
            {
                break;
            }

            var totalBytes = bytesInBuffer + bytesRead;

            Utf8JsonReader reader;
            SearchBufferForIdAndName(onItemFound, buffer, state, totalBytes, out reader);

            // Preserve incomplete token at the end of the buffer.
            int consumed = (int)reader.BytesConsumed;
            int remaining = totalBytes - consumed;

            if (remaining > 0)
            {
                Buffer.BlockCopy(buffer, consumed, buffer, 0, remaining);
            }

            bytesInBuffer = remaining;
            state = reader.CurrentState;
        }

        // Process the final buffer.
        if (bytesInBuffer > 0)
        {
            SearchBufferForIdAndName(onItemFound, buffer, state, bytesInBuffer, out _);
        }
    }

    private static void SearchBufferForIdAndName(Action<(string, string)> onItemFound, byte[] buffer, JsonReaderState state, int totalBytes, out Utf8JsonReader reader)
    {
        reader = new Utf8JsonReader(buffer.AsSpan(0, totalBytes), isFinalBlock: false, state);
        string? name = null;
        string? id = null;
        while (reader.Read())
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                continue;
            }

            if (reader.ValueTextEquals("name"))
            {
                if (reader.Read() && reader.TokenType == JsonTokenType.String)
                {
                    name = reader.GetString();
                }
            }
            else if (reader.ValueTextEquals("id"))
            {
                if (reader.Read() && reader.TokenType == JsonTokenType.String)
                {
                    id = reader.GetString();
                }
            }

            // Both found — no need to inspect the rest of this chunk
            if (name != null && id != null)
            {
                onItemFound((id, name));
                name = null;
                id = null;
            }
        }
    }

    private async IAsyncEnumerable<Exercise?> GetExercisesAsync([EnumeratorCancellation] CancellationToken ct)
    {
        await using var stream = File.OpenRead(_jsonExercsieFile);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        await foreach (var exercise in JsonSerializer.DeserializeAsyncEnumerable<Exercise>(stream, options, ct))
        {
            yield return exercise;
        }
    }
}
