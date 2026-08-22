namespace TrainingHelper.FreeExerciseDb.JSON;

using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ExerciseForce
{
    [JsonStringEnumMemberName("static")]
    Static,

    [JsonStringEnumMemberName("pull")]
    Pull,

    [JsonStringEnumMemberName("push")]
    Push
}