
using NLog;
using TrainingHelper.Common;

 namespace TrainingHelper.App.Model;

public class MainModel
{
    private readonly ISound _sound;
    private static ILogger _logger = LogManager.GetCurrentClassLogger();
    private readonly bool _noSound = false;
    public MainModel(ISound sound)
    {
        _sound = sound;
        if (_sound.SoundDevicesNames.Count == 0)
        {
            _logger.Warn($"Can't find any sound devices names. There will be no sound.");
            _noSound = true;
        }
    }
}