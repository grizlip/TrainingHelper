namespace TrainingHelper.Sdl;

internal enum SoundType 
{
    Work,
    Break,
    Start,
    End,
    Countdown
}

internal class SoundConfig
{
    public Dictionary<SoundType, string> SoundFiles { get; set; } = [];
}
