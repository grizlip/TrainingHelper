using Microsoft.Extensions.DependencyInjection;
using TrainingHelper.Common.Settings;
using TrainingHelper.Common.Settings.JSON;
using TrainingHelper.Common.Sound;
using TrainingHelper.Sdl.Settings;

namespace TrainingHelper.Sdl;

public static class TrainingHelperSdl
{
    public static void AddSdlSound(this IServiceCollection collection)
    {
        var soundConfig = new JsonSettingsService<SoundConfig>();
        collection.AddSingleton<ISettingsService<SoundConfig>>(soundConfig);
        
        if (soundConfig.Load().SoundFiles.Count == Enum.GetValues(typeof(SoundType)).Length)
        {
            collection.AddSingleton<ISound, Mp3Sound>();
        }
        else
        {
            collection.AddSingleton<ISound, SDLSound>();
        }
    }
}