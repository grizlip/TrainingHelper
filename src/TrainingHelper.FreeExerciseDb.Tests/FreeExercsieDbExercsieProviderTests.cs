using System.Text;
using Xunit;

namespace TrainingHelper.FreeExerciseDb.Tests;

public class FreeExercsieDbExercsieProviderTests
{
    [Fact]
    public async Task GetExercisesIdsAndNamesAsync_ParsesIdAndNamePairs()
    {
        var jsonFile = CreateJsonFile(
        [
            ("exercise-001", "Bench Press"),
            ("exercise-002", "Deadlift"),
            ("exercise-003", "Squat"),
        ]);

        var provider = new FreeExercsieDbExercsieProvider(jsonFile);

        var result = (await provider.GetExercisesIdsAndNamesAsync()).ToList();

        Assert.Equal(3, result.Count);
        Assert.Equal(("exercise-001", "Bench Press"), result[0]);
        Assert.Equal(("exercise-002", "Deadlift"), result[1]);
        Assert.Equal(("exercise-003", "Squat"), result[2]);
    }

    [Fact]
    public async Task GetExercisesIdsAndNamesAsync_KeepsIdAndNameAcross64KiBBoundary()
    {
        var items = 200;
        var boundaryName = new string('x', 128 * 1024);
        var pairs = new List<(string Id, string Name)>();

        for (var i = 0; i < items; i++)
        {
            pairs.Add(($"exercise-{i:000}", i % 2 == 0 ? boundaryName + i : $"Name-{i}"));
        }

        var jsonFile = CreateJsonFile(pairs);
        var provider = new FreeExercsieDbExercsieProvider(jsonFile);

        var result = (await provider.GetExercisesIdsAndNamesAsync()).ToList();

        Assert.Equal(items, result.Count);
        Assert.Equal(pairs[0], result[0]);
        Assert.Equal(pairs[^1], result[^1]);
        Assert.Contains(result, x => x.Id == "exercise-000" && x.Name == boundaryName + 0);
    }

    private static string CreateJsonFile(IEnumerable<(string Id, string Name)> items)
    {
        var path = Path.Combine(Path.GetTempPath(), $"exercise-provider-{Guid.NewGuid():N}.json");
        var builder = new StringBuilder();
        builder.Append('[');

        var first = true;
        foreach (var (id, name) in items)
        {
            if (!first)
            {
                builder.Append(',');
            }

            builder.Append('{');
            builder.Append("\"id\":\"").Append(EscapeJson(id)).Append("\",");
            builder.Append("\"name\":\"").Append(EscapeJson(name)).Append("\",");
            builder.Append("\"force\":\"pull\",");
            builder.Append("\"level\":\"beginner\",");
            builder.Append("\"mechanic\":\"compound\",");
            builder.Append("\"equipment\":\"dumbbell\",");
            builder.Append("\"primaryMuscles\":[\"chest\"],");
            builder.Append("\"secondaryMuscles\":[\"triceps\"],");
            builder.Append("\"instructions\":[\"step 1\"],");
            builder.Append("\"category\":\"strength\",");
            builder.Append("\"images\":[\"img.png\"]");
            builder.Append('}');
            first = false;
        }

        builder.Append(']');
        File.WriteAllText(path, builder.ToString());
        return path;
    }

    private static string EscapeJson(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n")
            .Replace("\t", "\\t");
    }
}
