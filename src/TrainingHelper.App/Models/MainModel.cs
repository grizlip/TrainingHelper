using System;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using TrainingHelper.Common;

namespace TrainingHelper.App.Model;

public class MainModel
{
    public event EventHandler<(int SecondsPassed, int TotalSeconds, int CurrentRepet, bool Work)>? CountdownStateChanged;
    private CancellationTokenSource? _countdownCts = null;
    private readonly ISound _sound;
    private readonly bool _noSound = false;
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    public MainModel(ISound sound)
    {
        _sound = sound;
        if (_sound.SoundDevicesNames.Count == 0)
        {
            _logger.Warn($"Can't find any sound devices names. There will be no sound.");
            _noSound = true;
        }
    }

    public async Task StartCountdownAsync(TimeSpan work, TimeSpan @break, int repetCount)
    {
        _countdownCts?.Cancel();
        _countdownCts?.Dispose();

        _countdownCts = new CancellationTokenSource();
        await _sound.PlayStartJingleAsync();

        for (int currentRepet = 0; currentRepet < repetCount; currentRepet++)
        {
            if (_countdownCts.Token.IsCancellationRequested)
            {
                return;
            }
            try
            {
                if (!_noSound && currentRepet != 0)
                {
                    await _sound.PlayWorkJingleAsync();
                }
                await Countdown((int)work.TotalSeconds, true, currentRepet, _countdownCts.Token);
                if (!_noSound)
                {
                    await _sound.PlayBreakJingleAsync();
                }
                await Countdown((int)@break.TotalSeconds, false, currentRepet, _countdownCts.Token);
            }
            catch (TaskCanceledException) { }
        }
        await _sound.PlayEndJingleAsync();
    }

    public async Task StopCountdownAsync()
    {
        if (_countdownCts != null)
        {
            await _countdownCts.CancelAsync();
        }
    }

    private async Task Countdown(int totalSeconds, bool work, int currentRepet, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            return;
        }
        var secondsPassed = 0;
        while (secondsPassed < totalSeconds)
        {
            if (!_noSound && totalSeconds - secondsPassed <= 3)
            {
                await Task.WhenAll(Task.Delay(1000, token), _sound.PlayCountdownJingle());
            }
            else
            {
                await Task.Delay(1000, token);
            }
            secondsPassed++;
            if (token.IsCancellationRequested)
            {
                return;
            }
            CountdownStateChanged?.Invoke(this, (secondsPassed, totalSeconds, currentRepet, work));
        }
    }
}