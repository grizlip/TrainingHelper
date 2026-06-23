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
    private readonly TimeSpan _oneSecond = TimeSpan.FromSeconds(1);
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
        await PlaySoundAsync(_sound.PlayStartJingleAsyncAsync, _countdownCts.Token);

        for (int currentRepet = 0; currentRepet < repetCount; currentRepet++)
        {
            if (_countdownCts.Token.IsCancellationRequested)
            {
                return;
            }
            try
            {
                if (currentRepet > 0)
                {
                    await PlaySoundAsync(_sound.PlayWorkJingleAsync, _countdownCts.Token);
                }

                await CountdownAsync((int)work.TotalSeconds, true, currentRepet, _countdownCts.Token);

                await PlaySoundAsync(_sound.PlayBreakJingleAsync, _countdownCts.Token);

                await CountdownAsync((int)@break.TotalSeconds, false, currentRepet, _countdownCts.Token);
            }
            catch (TaskCanceledException) { }
        }
        await PlaySoundAsync(_sound.PlayEndJingleAsyncAsync, _countdownCts.Token);

    }

    public async Task StopCountdownAsync()
    {
        if (_countdownCts != null)
        {
            await _countdownCts.CancelAsync();
        }
    }

    private async Task CountdownAsync(int totalSeconds, bool work, int currentRepet, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            return;
        }
        var secondsPassed = 0;
        while (secondsPassed < totalSeconds)
        {
            if (totalSeconds - secondsPassed <= 3)
            {
                await PlaySoundAsync(_sound.PlayCountdownJingleAsync, token, _oneSecond);
            }
            else
            {
                await Task.Delay(_oneSecond, token);
            }
            secondsPassed++;
            if (token.IsCancellationRequested)
            {
                return;
            }
            CountdownStateChanged?.Invoke(this, (secondsPassed, totalSeconds, currentRepet, work));
        }
    }

    private async Task PlaySoundAsync(Func<CancellationToken, Task> playFunction, CancellationToken ct) => await (!_noSound ? playFunction(ct) : Task.CompletedTask);

    private async Task PlaySoundAsync(Func<CancellationToken, Task> playFunction, CancellationToken ct, TimeSpan timeout)
    {
        if (!_noSound)
        {
            using CancellationTokenSource cts = new();
            await Task.WhenAll(Task.Delay(timeout, ct).ContinueWith(t => cts.Cancel()), playFunction(cts.Token));
        }
    }
}