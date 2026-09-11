using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TrainingHelper.Common.Exercise;

namespace TrainingHelper.FreeExerciseDb;

public static class FreeExercsieDbHelper
{
    public static void AddFreeExercsieDbExercsieProvider(this IServiceCollection collection, IConfigurationBuilder configurationBuilder, string baseConfigDirectory)
    {
        configurationBuilder.AddJsonFile(Path.Combine(baseConfigDirectory, "freeExerciseDb.json"), optional: true, reloadOnChange: true);
        var config = configurationBuilder.Build().GetSection("FreeExercsieDbExercsieJson");
        var jsonFile = config.Value ?? throw new Exception(@"Key FreeExercsieDbExercsieJson is not present.");

        collection.AddKeyedSingleton("jsonFile", jsonFile);
        collection.AddSingleton<IExerciseProvider, FreeExercsieDbExercsieProvider>();

    }
}