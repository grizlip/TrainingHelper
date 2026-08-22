using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TrainingHelper.Common.Sound;

namespace TrainingHelper.Sdl;

public static class TrainingHelperSdl
{
    public static void AddSdlSound(this IServiceCollection collection, IConfigurationRoot configurationRoot)
    {
        var soundConfig = configurationRoot.GetSection("SoundConfig").Get<SoundConfig>();
        if (soundConfig?.SoundFiles.Count == Enum.GetValues(typeof(SoundType)).Length)
        {
            collection.AddSingleton(soundConfig);
            collection.AddSingleton<ISound, Mp3Sound>();
        }
        else
        {
            collection.AddSingleton<ISound, SDLSound>();
        }

    }
}