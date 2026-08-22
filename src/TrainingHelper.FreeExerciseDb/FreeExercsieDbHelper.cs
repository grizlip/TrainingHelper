using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TrainingHelper.Common.Exercise;

namespace TrainingHelper.FreeExerciseDb;

public static class FreeExercsieDbHelper
{
    public static void AddFreeExercsieDbExercsieProvider(this IServiceCollection collection, IConfigurationRoot? configurationRoot)
    {
        var jsonFile = configurationRoot?.GetSection("FreeExercsieDbExercsieJson").Value ?? throw new Exception(@"Key FreeExercsieDbExercsieJson is not present.");

        collection.AddKeyedSingleton("jsonFile", jsonFile);
        collection.AddSingleton<IExerciseProvider, FreeExercsieDbExercsieProvider>();

    }
}