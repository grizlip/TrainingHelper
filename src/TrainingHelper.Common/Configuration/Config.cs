namespace TrainingHelper.Common.Configuration;

public class Config
{
    public TimeSpan WorkTime { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan BreakTime { get; set; } = TimeSpan.FromSeconds(3);
    public IEnumerable<string> Pictures { get; set; } = [];
    public string DefaultAudioDeviceSelection { get; set; } = string.Empty;
    public Dictionary<SoundType, string> SoundFiles { get; set; } = [];
    public TimeSpan CountdownStart { get; set; } = TimeSpan.FromSeconds(3);
    public string FreeExercsieDbExercsieJson { get; set; } = string.Empty;  
}
