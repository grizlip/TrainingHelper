namespace TrainingHelper.Common.Exercise;

public interface IExerciseProvider
{
    Task<IEnumerable<(string Id, string Name)>> GetExercisesIdsAndNamesAsync(CancellationToken ct = default);
    Task<ExerciseDetails?> GetExerciseDetailsAsync(string exercsieId, CancellationToken ct = default);
}