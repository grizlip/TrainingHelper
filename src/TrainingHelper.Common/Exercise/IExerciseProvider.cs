namespace TrainingHelper.Common.Exercise;

public interface IExerciseProvider
{
    Task<IEnumerable<ExerciseDetails>> GetAllExerciseDetailsAsync(CancellationToken ct = default);
    Task<ExerciseDetails?> GetExerciseDetailsAsync(string exercsieId, CancellationToken ct = default);
}