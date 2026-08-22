namespace TrainingHelper.FreeExerciseDb.JSON;

using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ExerciseMuscle
{
    [JsonStringEnumMemberName("abdominals")]
    Abdominals,

    [JsonStringEnumMemberName("abductors")]
    Abductors,

    [JsonStringEnumMemberName("adductors")]
    Adductors,

    [JsonStringEnumMemberName("biceps")]
    Biceps,

    [JsonStringEnumMemberName("calves")]
    Calves,

    [JsonStringEnumMemberName("chest")]
    Chest,

    [JsonStringEnumMemberName("forearms")]
    Forearms,

    [JsonStringEnumMemberName("glutes")]
    Glutes,

    [JsonStringEnumMemberName("hamstrings")]
    Hamstrings,

    [JsonStringEnumMemberName("lats")]
    Lats,

    [JsonStringEnumMemberName("lower back")]
    LowerBack,

    [JsonStringEnumMemberName("middle back")]
    MiddleBack,

    [JsonStringEnumMemberName("neck")]
    Neck,

    [JsonStringEnumMemberName("quadriceps")]
    Quadriceps,

    [JsonStringEnumMemberName("shoulders")]
    Shoulders,

    [JsonStringEnumMemberName("traps")]
    Traps,

    [JsonStringEnumMemberName("triceps")]
    Triceps
}