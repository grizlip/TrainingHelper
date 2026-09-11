namespace TrainingHelper.Common.Settings;

public interface ISettingsService<TSettings> where TSettings : class, new()
{
    TSettings Load();
    void Save(TSettings settings);

  
}
