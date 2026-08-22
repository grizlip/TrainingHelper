namespace TrainingHelper.FreeExerciseDb.JSON;

using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ExerciseEquipment
{
    [JsonStringEnumMemberName("medicine ball")]
    MedicineBall,

    [JsonStringEnumMemberName("dumbbell")]
    Dumbbell,

    [JsonStringEnumMemberName("body only")]
    BodyOnly,

    [JsonStringEnumMemberName("bands")]
    Bands,

    [JsonStringEnumMemberName("kettlebells")]
    Kettlebells,

    [JsonStringEnumMemberName("foam roll")]
    FoamRoll,

    [JsonStringEnumMemberName("cable")]
    Cable,

    [JsonStringEnumMemberName("machine")]
    Machine,

    [JsonStringEnumMemberName("barbell")]
    Barbell,

    [JsonStringEnumMemberName("exercise ball")]
    ExerciseBall,

    [JsonStringEnumMemberName("e-z curl bar")]
    EZCurlBar,

    [JsonStringEnumMemberName("other")]
    Other
}
