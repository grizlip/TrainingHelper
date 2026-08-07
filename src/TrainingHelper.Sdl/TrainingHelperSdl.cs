using Microsoft.Extensions.DependencyInjection;
using TrainingHelper.Common;

namespace TrainingHelper.Sdl;

public static class TrainingHelperSdl
{
    public static void AddSdlSound(this IServiceCollection collection, SoundConfig soundConfig)
    {
        if (soundConfig.SoundFiles.Count == Enum.GetValues(typeof(SoundType)).Length)
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