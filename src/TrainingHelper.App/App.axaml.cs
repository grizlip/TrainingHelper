using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using TrainingHelper.App.ViewModels;
using TrainingHelper.App.Views;
using Microsoft.Extensions.DependencyInjection;
using TrainingHelper.Sdl;
using Microsoft.Extensions.Configuration;
using TrainingHelper.App.Configuration;
using TrainingHelper.App.Models;

namespace TrainingHelper.App;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var collection = new ServiceCollection();
        var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();
        collection.AddSingleton<IConfiguration>(config);
        collection.Configure<Config>(config);
        collection.Configure<ModelConfig>(config);
        collection.AddSdlSound();
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
