using Microsoft.Extensions.DependencyInjection;
using TrainingHelper.Common;

namespace TrainingHelper.Sdl;

public static class TrainingHelperSdl
{
    public static void AddSdlSound(this IServiceCollection collection) => collection.AddSingleton<ISound, SDLSound>();
}