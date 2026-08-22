namespace TrainingHelper.FreeExerciseDb.JSON;

using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ExerciseCategory
{
    [JsonStringEnumMemberName("powerlifting")]
    Powerlifting,

    [JsonStringEnumMemberName("strength")]
    Strength,

    [JsonStringEnumMemberName("stretching")]
    Stretching,

    [JsonStringEnumMemberName("cardio")]
    Cardio,

    [JsonStringEnumMemberName("olympic weightlifting")]
    OlympicWeightlifting,

    [JsonStringEnumMemberName("strongman")]
    Strongman,

    [JsonStringEnumMemberName("plyometrics")]
    Plyometrics
}
