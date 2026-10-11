namespace TrainingHelper.Common.Exercise;

public interface IRoundDefinition
{
    TimeSpan Interval { get; }
    TimeSpan Break { get; }
    bool Work { get; }
    int Repetitions { get; }
    string RoundImagePath { get; }
}