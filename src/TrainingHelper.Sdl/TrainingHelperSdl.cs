using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TrainingHelper.Common.Sound;

namespace TrainingHelper.Sdl;

public static class TrainingHelperSdl
{
    public static void AddSdlSound(this IServiceCollection collection, IConfigurationBuilder configurationBuilder, string baseConfigDirectory)
    {
        configurationBuilder.AddJsonFile(Path.Combine(baseConfigDirectory, "soundconfig.json"), optional: true, reloadOnChange: true);
        var config = configurationBuilder.Build().Get<SoundConfig>();

        collection.AddKeyedSingleton("defaultSoundDeviceName", config?.DefaultAudioDeviceSelection ?? string.Empty);


        if (config?.SoundFiles.Count == Enum.GetValues(typeof(SoundType)).Length)
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