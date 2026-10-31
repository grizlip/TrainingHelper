namespace TrainingHelper.FreeExerciseDb.GUI.Models;

internal class ExerciseDetails
{
    public required string Name { get;  set; }
    public required string Id { get;  set; }
    public List<string> ImagesPaths { get;  set; } = new List<string>();
    public override string ToString() => Name;
}