namespace TrainingHelper.Common;

public interface ISound
{
    IReadOnlyCollection<string> SoundDevicesNames { get; }
    void ChangeAudiDevice(string audioDeviceName);
    void ChangeSoundState(bool active);
}