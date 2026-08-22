namespace TrainingHelper.FreeExerciseDb.JSON;

using System.Text.Json.Serialization;

public sealed class Exercise
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("force")]
    public ExerciseForce? Force { get; init; }

    [JsonPropertyName("level")]
    public required ExerciseLevel Level { get; init; }

    [JsonPropertyName("mechanic")]
    public required ExerciseMechanic? Mechanic { get; init; }

    [JsonPropertyName("equipment")]
    public required ExerciseEquipment? Equipment { get; init; }

    [JsonPropertyName("primaryMuscles")]
    public required List<ExerciseMuscle> PrimaryMuscles { get; init; }

    [JsonPropertyName("secondaryMuscles")]
    public required List<ExerciseMuscle> SecondaryMuscles { get; init; }

    [JsonPropertyName("instructions")]
    public required List<string> Instructions { get; init; }

    [JsonPropertyName("category")]
    public required ExerciseCategory Category { get; init; }

    [JsonPropertyName("images")]
    public required List<string> Images { get; init; }
}
