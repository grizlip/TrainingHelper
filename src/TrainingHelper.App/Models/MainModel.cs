using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using TrainingHelper.Common.Sound;
using TrainingHelper.App.Settings;
using TrainingHelper.Common.Settings;
using System.Linq;

namespace TrainingHelper.App.Models;

internal class MainModel
{
    public event EventHandler<(int SecondsPassed, int TotalSeconds, int CurrentRound, bool Work, string ImagPath)>? CountdownStateChanged;
    public IReadOnlyCollection<string> AudioDevices => _sound.SoundDevicesNames;

    public string SelectedAudioDevice => _sound.SelectedAudioDevice;

    private CancellationTokenSource? _countdownCts = null;
    private readonly ISound _sound;

    private readonly bool _noSound = false;
    private bool _pause = false;
    private readonly TimeSpan _countdownStart = TimeSpan.Zero;
    private static readonly TimeSpan _oneSecond = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan _maxBeepTime = TimeSpan.FromMilliseconds(200);
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    public MainModel(ISound sound, ISettingsService<AppSettings> settingsService)
    {
        _sound = sound;
        _countdownStart = settingsService.Load().CountdownStart;
        if (_sound.SoundDevicesNames.Count == 0)
        {
            _logger.Warn($"Can't find any sound devices names. There will be no sound.");
            _noSound = true;
        }
    }

    public void ChangeSelectedAudioDevice(string deviceName)
    {
        _sound.ChangeAudiDevice(deviceName);
    }

    public async Task StartCountdownAsync(IEnumerable<IRoundDefinition> roundDefinitions)
    {
        _countdownCts?.Cancel();
        _countdownCts?.Dispose();

        _countdownCts = new CancellationTokenSource();
        await PlaySoundAsync(_sound.PlayStartJingleAsyncAsync, _countdownCts.Token);

        int roundNumber = 1;
        foreach (var round in roundDefinitions)
        {
            for (int i = 0; i < round.Repetitions; i++)
            {
                if (!await DoIntervalAsync(round.Interval, round.Break, roundNumber, round.RoundImagesPaths))
                {
                    return;
                }
                roundNumber++;
            }
        }

        await PlaySoundAsync(_sound.PlayEndJingleAsyncAsync, _countdownCts.Token);

        async Task<bool> DoIntervalAsync(TimeSpan workTime, TimeSpan breakTime, int currentRound, IEnumerable<string> currentImages)
        {
            if (_countdownCts.Token.IsCancellationRequested)
            {
                return false;
            }

            try
            {
                if (!await PlaySoundAsync(_sound.PlayWorkJingleAsync, _countdownCts.Token) ||
                   !await CountdownAsync((int)workTime.TotalSeconds, true, currentRound, currentImages, _countdownCts.Token) ||
                   !await PlaySoundAsync(_sound.PlayBreakJingleAsync, _countdownCts.Token) ||
                   !await CountdownAsync((int)breakTime.TotalSeconds, false, currentRound, Array.Empty<string>(), _countdownCts.Token))
                {
                    return false;
                }
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
        _pause = false;
    }

    public void PauseResumeCountdown()
    {
        _pause = !_pause;
    }

    private async Task<bool> CountdownAsync(int totalSeconds, bool work, int currentRound, IEnumerable<string> imagesPaths, CancellationToken token)
    {
        var secondsPassed = 0;
        using PeriodicTimer periodicTimer = new(_oneSecond);
        var imagesList = imagesPaths.ToList();
        try
        {
            while (await periodicTimer.WaitForNextTickAsync(token) && secondsPassed < totalSeconds)
            {
                if (_pause)
                {
                    continue;
                }
                secondsPassed++;

                CountdownStateChanged?.Invoke(this, (secondsPassed, totalSeconds, currentRound, work, imagesList.Count > 0 ? imagesList[secondsPassed % imagesList.Count] : string.Empty));

                if (totalSeconds - secondsPassed <= _countdownStart.TotalSeconds)
                {
                    using var oneSecondCts = new CancellationTokenSource();
                    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token, oneSecondCts.Token);
                    oneSecondCts.CancelAfter(_maxBeepTime);
                    await PlaySoundAsync(_sound.PlayCountdownJingleAsync, linkedCts.Token);
                }
            }
        }
        catch (OperationCanceledException) { return false; }

        return true;
    }

    private async Task<bool> PlaySoundAsync(Func<CancellationToken, Task> playFunction, CancellationToken ct)
    {
        await (!_noSound ? playFunction(ct) : Task.CompletedTask);
        return !ct.IsCancellationRequested;
    }
}
