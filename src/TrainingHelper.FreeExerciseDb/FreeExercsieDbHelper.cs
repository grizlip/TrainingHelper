using Microsoft.Extensions.DependencyInjection;
using TrainingHelper.Common.Exercise;
using TrainingHelper.Common.Settings;
using TrainingHelper.Common.Settings.JSON;
using TrainingHelper.FreeExerciseDb.Settings;

namespace TrainingHelper.FreeExerciseDb;

public static class FreeExercsieDbHelper
{
    public static void AddFreeExercsieDbExercsieProvider(this IServiceCollection collection)
    {
        collection.AddSingleton<ISettingsService<FreeExerciseDbConfig>>(new JsonSettingsService<FreeExerciseDbConfig>());
        collection.AddSingleton<IExerciseProvider, FreeExercsieDbExercsieProvider>();

    }
}