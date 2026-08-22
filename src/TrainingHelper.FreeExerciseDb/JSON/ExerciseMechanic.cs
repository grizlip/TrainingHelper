namespace TrainingHelper.FreeExerciseDb.JSON;

using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ExerciseMechanic
{
    [JsonStringEnumMemberName("isolation")]
    Isolation,

    [JsonStringEnumMemberName("compound")]
    Compound
}