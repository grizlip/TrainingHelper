namespace TrainingHelper.Common.Exercise;

public interface IRoundDefinition
{
    int RoundNumber { get; }
    TimeSpan Interval { get; }
    TimeSpan Break { get; }
    bool Work { get; }
    string RoundImagePath { get; }
}