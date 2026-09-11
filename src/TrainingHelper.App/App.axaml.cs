using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using TrainingHelper.App.ViewModels;
using TrainingHelper.App.Views;
using Microsoft.Extensions.DependencyInjection;
using TrainingHelper.Sdl;
using Microsoft.Extensions.Configuration;
using TrainingHelper.App.Models;
using TrainingHelper.FreeExerciseDb;
using System;
using System.IO;
using TrainingHelper.App.Configuration;

namespace TrainingHelper.App;

public partial class App : Application
{
    private static IConfigurationRoot? _configurationRoot;
    private static Lazy<string> ConfigDirectory = new Lazy<string>(static () =>
       {
           if (OperatingSystem.IsWindows())
           {
               return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TraningHelper.App");
           }

           if (OperatingSystem.IsLinux())
           {
               var xdgConfigHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");

               var basePath = !string.IsNullOrWhiteSpace(xdgConfigHome)
                   ? xdgConfigHome
                   : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");

               return Path.Combine(basePath, "TraningHelper.App");
           }

           return "UserConfig";
       });

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        if (Directory.Exists(ConfigDirectory.Value) == false)
        {
            Directory.CreateDirectory(ConfigDirectory.Value);
        }
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var collection = new ServiceCollection();
        var configurationBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        configurationBuilder.AddJsonFile(Path.Combine(ConfigDirectory.Value, "appsettings.json"), optional: true, reloadOnChange: true);
        _configurationRoot = configurationBuilder.Build();
        collection.AddSingleton<IConfiguration>(_configurationRoot);
        collection.Configure<Config>(_configurationRoot);

        collection.AddSdlSound(configurationBuilder, ConfigDirectory.Value);
        collection.AddFreeExercsieDbExercsieProvider(configurationBuilder, ConfigDirectory.Value);

        collection.AddSingleton<MainModel>();
        collection.AddSingleton<MainWindowViewModel>();
        var services = collection.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = services.GetRequiredService<MainWindowViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
