namespace TrainingHelper.Common;

public enum SoundType 
{
    Work,
    Break,
    Start,
    End,
    Countdown
}

public class SoundConfig
{
    public Dictionary<SoundType, string> SoundFiles { get; set; } = [];
}
