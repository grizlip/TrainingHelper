using Microsoft.Extensions.DependencyInjection;
using TrainingHelper.Common.Configuration;
using TrainingHelper.Common.Sound;

namespace TrainingHelper.Sdl;

public static class TrainingHelperSdl
{
    public static void AddSdlSound(this IServiceCollection collection, Config config)
    {
        var defaultSoundDeviceName = config.DefaultAudioDeviceSelection;
        if(defaultSoundDeviceName != null)
        {
            collection.AddKeyedSingleton("defaultSoundDeviceName", defaultSoundDeviceName);
        }
        if (config.SoundFiles.Count == Enum.GetValues(typeof(SoundType)).Length)
        {
            collection.AddKeyedSingleton("soundFiles", config.SoundFiles);
            collection.AddSingleton<ISound, Mp3Sound>();
        }
        else
        {
            collection.AddSingleton<ISound, SDLSound>();
        }

    }
}