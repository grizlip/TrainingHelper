namespace TrainingHelper.FreeExerciseDb.JSON;

using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ExerciseLevel
{
    [JsonStringEnumMemberName("beginner")]
    Beginner,

    [JsonStringEnumMemberName("intermediate")]
    Intermediate,

    [JsonStringEnumMemberName("expert")]
    Expert
}