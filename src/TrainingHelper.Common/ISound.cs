namespace TrainingHelper.Common;

public interface ISound
{
    IReadOnlyCollection<string> SoundDevicesNames { get; }
    void ChangeAudiDevice(string audioDeviceName);
    Task PlayWorkJingleAsync();
    Task PlayBreakJingleAsync();
    Task PlayStartJingleAsyncAsync();
    Task PlayEndJingleAsyncAsync();
    Task PlayCountdownJingleAsync();
}