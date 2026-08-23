using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text;
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
        var state = new JsonStringScannerState();

        while (true)
        {
            var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), ct);

            if (bytesRead == 0)
            {
                break;
            }

            SearchBufferForIdAndName(onItemFound, buffer, bytesRead, state);
        }
    }

    private static void SearchBufferForIdAndName(Action<(string, string)> onItemFound, byte[] buffer, int bytesRead, JsonStringScannerState state)
    {
        for (var i = 0; i < bytesRead; i++)
        {
            var code = buffer[i];

            if (state.InString)
            {
                if (state.InUnicodeEscape)
                {
                    if (IsHexDigit(code))
                    {
                        state.UnicodeValue = (state.UnicodeValue << 4) | HexValue(code);
                        state.UnicodeDigitsRead++;
                        if (state.UnicodeDigitsRead == 4)
                        {
                            state.CurrentString.Append((char)state.UnicodeValue);
                            state.InUnicodeEscape = false;
                            state.UnicodeValue = 0;
                            state.UnicodeDigitsRead = 0;
                        }
                        continue;
                    }

                    state.CurrentString.Append('?');
                    state.InUnicodeEscape = false;
                    state.UnicodeValue = 0;
                    state.UnicodeDigitsRead = 0;
                }

                if (state.IsEscaped)
                {
                    switch ((char)code)
                    {
                        case '"':
                            state.CurrentString.Append('"');
                            break;
                        case '\\':
                            state.CurrentString.Append('\\');
                            break;
                        case '/':
                            state.CurrentString.Append('/');
                            break;
                        case 'b':
                            state.CurrentString.Append('\b');
                            break;
                        case 'f':
                            state.CurrentString.Append('\f');
                            break;
                        case 'n':
                            state.CurrentString.Append('\n');
                            break;
                        case 'r':
                            state.CurrentString.Append('\r');
                            break;
                        case 't':
                            state.CurrentString.Append('\t');
                            break;
                        case 'u':
                            state.InUnicodeEscape = true;
                            state.UnicodeValue = 0;
                            state.UnicodeDigitsRead = 0;
                            state.IsEscaped = false;
                            continue;
                        default:
                            state.CurrentString.Append((char)code);
                            break;
                    }

                    state.IsEscaped = false;
                    continue;
                }

                switch ((char)code)
                {
                    case '\\':
                        state.IsEscaped = true;
                        break;
                    case '"':
                        CompleteJsonStringValue(onItemFound, state);
                        break;
                    default:
                        state.CurrentString.Append((char)code);
                        break;
                }

                continue;
            }

            var character = (char)code;
            switch (character)
            {
                case '{':
                    state.ContainerStack.Push('{');
                    state.ExpectingPropertyName = true;
                    break;
                case '[':
                    state.ContainerStack.Push('[');
                    break;
                case '}':
                    if (state.ContainerStack.Count > 0 && state.ContainerStack.Peek() == '{')
                    {
                        state.ContainerStack.Pop();
                    }
                    state.ExpectingPropertyName = false;
                    state.ExpectingPropertyValue = false;
                    state.CurrentPropertyName = null;
                    break;
                case ']':
                    if (state.ContainerStack.Count > 0 && state.ContainerStack.Peek() == '[')
                    {
                        state.ContainerStack.Pop();
                    }
                    break;
                case ':':
                    state.ExpectingPropertyName = false;
                    state.ExpectingPropertyValue = true;
                    break;
                case ',':
                    if (state.ContainerStack.Count > 0 && state.ContainerStack.Peek() == '{')
                    {
                        state.ExpectingPropertyName = true;
                    }
                    state.ExpectingPropertyValue = false;
                    state.CurrentPropertyName = null;
                    break;
                case '"':
                    state.InString = true;
                    state.CurrentString.Clear();
                    break;
                case ' ':
                case '\r':
                case '\n':
                case '\t':
                    break;
                default:
                    if (state.ExpectingPropertyValue)
                    {
                        state.ExpectingPropertyValue = false;
                        state.CurrentPropertyName = null;
                    }
                    break;
            }
        }
    }

    private static void CompleteJsonStringValue(Action<(string, string)> onItemFound, JsonStringScannerState state)
    {
        state.InString = false;
        var value = state.CurrentString.ToString();
        state.CurrentString.Clear();

        if (state.ExpectingPropertyName)
        {
            state.CurrentPropertyName = value;
            state.ExpectingPropertyName = false;
            return;
        }

        if (state.ExpectingPropertyValue && state.CurrentPropertyName is not null)
        {
            if (state.CurrentPropertyName == "id")
            {
                state.PendingId = value;
            }
            else if (state.CurrentPropertyName == "name")
            {
                state.PendingName = value;
            }

            state.ExpectingPropertyValue = false;
            state.CurrentPropertyName = null;

            if (state.PendingId is not null && state.PendingName is not null)
            {
                onItemFound((state.PendingId, state.PendingName));
                state.PendingId = null;
                state.PendingName = null;
            }
        }
    }

    private static bool IsHexDigit(byte value)
    {
        return value is >= (byte)'0' and <= (byte)'9'
            or >= (byte)'a' and <= (byte)'f'
            or >= (byte)'A' and <= (byte)'F';
    }

    private static int HexValue(byte value)
    {
        if (value is >= (byte)'0' and <= (byte)'9')
        {
            return value - (byte)'0';
        }

        if (value is >= (byte)'a' and <= (byte)'f')
        {
            return 10 + (value - (byte)'a');
        }

        return 10 + (value - (byte)'A');
    }

    private sealed class JsonStringScannerState
    {
        public bool InString { get; set; }
        public bool IsEscaped { get; set; }
        public bool InUnicodeEscape { get; set; }
        public int UnicodeValue { get; set; }
        public int UnicodeDigitsRead { get; set; }
        public bool ExpectingPropertyName { get; set; }
        public bool ExpectingPropertyValue { get; set; }
        public string? CurrentPropertyName { get; set; }
        public string? PendingId { get; set; }
        public string? PendingName { get; set; }
        public readonly Stack<char> ContainerStack = new();
        public readonly StringBuilder CurrentString = new();
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
