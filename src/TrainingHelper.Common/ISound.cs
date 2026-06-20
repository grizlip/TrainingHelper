namespace TrainingHelper.Common;

public interface ISound
{
    IReadOnlyCollection<string> SoundDevicesNames { get; }
    void ChangeAudiDevice(string audioDeviceName);
    Task PlayWorkJingleAsync();
    Task PlayBreakJingleAsync();
    Task PlayStartJingleAsync();
    Task PlayEndJingleAsync();
    Task PlayCountdownJingle();
}