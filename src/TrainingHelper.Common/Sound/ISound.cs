namespace TrainingHelper.Common.Sound;

public interface ISound
{
    string SelectedAudioDevice { get; }
    IReadOnlyCollection<string> SoundDevicesNames { get; }
    void ChangeAudiDevice(string audioDeviceName);
    Task PlayWorkJingleAsync(CancellationToken ct);
    Task PlayBreakJingleAsync(CancellationToken ct);
    Task PlayStartJingleAsyncAsync(CancellationToken ct);
    Task PlayEndJingleAsyncAsync(CancellationToken ct);
    Task PlayCountdownJingleAsync(CancellationToken ct);
}