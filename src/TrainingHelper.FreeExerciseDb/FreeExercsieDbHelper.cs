using Microsoft.Extensions.DependencyInjection;
using TrainingHelper.Common.Configuration;
using TrainingHelper.Common.Exercise;

namespace TrainingHelper.FreeExerciseDb;

public static class FreeExercsieDbHelper
{
    public static void AddFreeExercsieDbExercsieProvider(this IServiceCollection collection, Config configurationRoot)
    {
        var jsonFile = configurationRoot?.FreeExercsieDbExercsieJson ?? throw new Exception(@"Key FreeExercsieDbExercsieJson is not present.");

        collection.AddKeyedSingleton("jsonFile", jsonFile);
        collection.AddSingleton<IExerciseProvider, FreeExercsieDbExercsieProvider>();

    }
}