using TrainingHelper.Common.Sound;

namespace TrainingHelper.Sdl.Settings;

internal class SoundConfig
{
    public string DefaultAudioDeviceSelection { get; set; } = string.Empty;
    public Dictionary<SoundType, string> SoundFiles { get; set; } = [];
}
