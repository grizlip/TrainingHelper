using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using TrainingHelper.Common;

namespace TrainingHelper.App.Models;

public class MainModel
{
    public event EventHandler<(int SecondsPassed, int TotalSeconds, int CurrentRepet, bool Work, string ImagPath)>? CountdownStateChanged;
    private CancellationTokenSource? _countdownCts = null;
    private readonly ISound _sound;
    private readonly bool _noSound = false;
    private static readonly TimeSpan _oneSecond = TimeSpan.FromSeconds(1);
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

    public async Task StartCountdownAsync(TimeSpan workTime, TimeSpan breakTime, int repetCount, IEnumerable<string> images)
    {
        _countdownCts?.Cancel();
        _countdownCts?.Dispose();

        _countdownCts = new CancellationTokenSource();
        await PlaySoundAsync(_sound.PlayStartJingleAsyncAsync, _countdownCts.Token);
        Func<TimeSpan, TimeSpan, int, Task<bool>> repetCountDownFuncAsync = images.Any() ?
        async (work, @break, repet) =>
            {
                bool result = true;
                foreach (var currentImage in images)
                {
                    if (!(result = await DoIntervalAsync(work, @break, repet, currentImage)))
                    {
                        break;
                    }
                }
                return result;
            }
        :
        async (work, @break, repet) => await DoIntervalAsync(work, @break, repet);

        for (int currentRepet = 0; currentRepet < repetCount; currentRepet++)
        {
            if (!await repetCountDownFuncAsync(workTime, breakTime, currentRepet))
            {
                return;
            }
        }

        await PlaySoundAsync(_sound.PlayEndJingleAsyncAsync, _countdownCts.Token);

        async Task<bool> DoIntervalAsync(TimeSpan workTime, TimeSpan breakTime, int currentRepet, string currentImage = "")
        {
            if (_countdownCts.Token.IsCancellationRequested)
            {
                return false;
            }

            try
            {
                await PlaySoundAsync(_sound.PlayWorkJingleAsync, _countdownCts.Token);

                await CountdownAsync((int)workTime.TotalSeconds, true, currentRepet, currentImage, _countdownCts.Token);

                await PlaySoundAsync(_sound.PlayBreakJingleAsync, _countdownCts.Token);

                await CountdownAsync((int)breakTime.TotalSeconds, false, currentRepet, currentImage, _countdownCts.Token);
            }
            catch (TaskCanceledException) { return false; }

            return true;
        }
    }

    public async Task StopCountdownAsync()
    {
        if (_countdownCts != null)
        {
            await _countdownCts.CancelAsync();
        }
    }

    private async Task CountdownAsync(int totalSeconds, bool work, int currentRepet, string imagePath, CancellationToken token)
    {
        using PeriodicTimer periodicTimer = new(_oneSecond);
        var secondsPassed = 0;
        try
        {
            while (await periodicTimer.WaitForNextTickAsync(token) && secondsPassed < totalSeconds)
            {
                if (totalSeconds - secondsPassed <= 3)
                {
                    await PlaySoundAsync(_sound.PlayCountdownJingleAsync, token);
                }
                secondsPassed++;
                CountdownStateChanged?.Invoke(this, (secondsPassed, totalSeconds, currentRepet, work, imagePath));
            }
        }
        catch (OperationCanceledException) { /*do nothing*/ }
    }

    private async Task PlaySoundAsync(Func<CancellationToken, Task> playFunction, CancellationToken ct) => await (!_noSound ? playFunction(ct) : Task.CompletedTask);
}