using NLog;
using SDL2;
using TrainingHelper.Common;

namespace TrainingHelper.Sdl;

internal sealed class Mp3Sound : ISound, IDisposable
{
    private const int DefaultMusicVolume = 64;
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private readonly List<string> _soundDevicesNames = [];
    private readonly Dictionary<SoundType, string> _sounds = [];
    private readonly SemaphoreSlim _playLock = new(1, 1);
    private bool _audioInitialized;

    public IReadOnlyCollection<string> SoundDevicesNames => _soundDevicesNames;

    public Mp3Sound(SoundConfig soundConfig)
    {
        DiscoverAudioDevices();
        LoadMp3Files(soundConfig.SoundFiles);
        InitializeMixer();
    }

    public void ChangeAudiDevice(string audioDeviceName)
    {
        if (string.IsNullOrWhiteSpace(audioDeviceName))
        {
            return;
        }

        _logger.Info($"MP3 sound implementation is using the default SDL mixer device; selected device '{audioDeviceName}' was recorded for configuration purposes.");
    }

    public Task PlayWorkJingleAsync(CancellationToken ct) => PlayMp3Async(SoundType.Work, ct);
    public Task PlayBreakJingleAsync(CancellationToken ct) => PlayMp3Async(SoundType.Break, ct);
    public Task PlayStartJingleAsyncAsync(CancellationToken ct) => PlayMp3Async(SoundType.Start, ct);
    public Task PlayEndJingleAsyncAsync(CancellationToken ct) => PlayMp3Async(SoundType.End, ct);
    public Task PlayCountdownJingleAsync(CancellationToken ct) => PlayMp3Async(SoundType.Countdown, ct);

    public void Dispose()
    {
        SDL_mixer.Mix_HaltMusic();
        SDL_mixer.Mix_CloseAudio();
        SDL_mixer.Mix_Quit();
        SDL.SDL_Quit();
        _playLock.Dispose();
    }

    private void DiscoverAudioDevices()
    {
        if (SDL.SDL_Init(SDL.SDL_INIT_AUDIO) < 0)
        {
            _logger.Warn($"Unable to initialize SDL audio for device discovery: {SDL.SDL_GetError()}");
            return;
        }

        int deviceCount = SDL.SDL_GetNumAudioDevices(0);
        for (int i = 0; i < deviceCount; i++)
        {
            string deviceName = SDL.SDL_GetAudioDeviceName(i, 0);
            if (!string.IsNullOrWhiteSpace(deviceName))
            {
                _soundDevicesNames.Add(deviceName);
            }
        }
    }

    private void LoadMp3Files(Dictionary<SoundType, string> soundFiles)
    {
        foreach (var soundFile in soundFiles)
        {
            string filePath = soundFile.Value;
            if (string.IsNullOrWhiteSpace(filePath))
            {
                _logger.Warn($"No MP3 file configured for sound type '{soundFile.Key}'.");
                continue;
            }

            string configuredPath = filePath.Trim();
            if (!File.Exists(configuredPath))
            {
                _logger.Warn($"Configured MP3 file does not exist for sound type '{soundFile.Key}': '{configuredPath}'");
                continue;
            }

            _sounds[soundFile.Key] = configuredPath;
        }
    }

    private void InitializeMixer()
    {
        if (SDL_mixer.Mix_OpenAudio(44100, SDL.AUDIO_S16SYS, 2, 2048) < 0)
        {
            _logger.Warn($"Unable to open SDL_mixer audio device: {SDL_mixer.Mix_GetError()}");
            return;
        }

        int initialized = SDL_mixer.Mix_Init(SDL_mixer.MIX_InitFlags.MIX_INIT_MP3);
        if ((initialized & (int)SDL_mixer.MIX_InitFlags.MIX_INIT_MP3) == 0)
        {
            _logger.Warn($"MP3 decoder support is not available. {SDL_mixer.Mix_GetError()}");
        }

        SDL_mixer.Mix_AllocateChannels(8);
        SDL_mixer.Mix_VolumeMusic(DefaultMusicVolume);
        _audioInitialized = true;
    }

    private async Task PlayMp3Async(SoundType soundType, CancellationToken ct)
    {
        if (!_audioInitialized || ct.IsCancellationRequested)
        {
            return;
        }

        if (!_sounds.TryGetValue(soundType, out string? filePath))
        {
            _logger.Warn($"No mp3 file configured for sound type '{soundType}'.");
            return;
        }

        await _playLock.WaitAsync(ct).ConfigureAwait(false);

        try
        {
            IntPtr music = SDL_mixer.Mix_LoadMUS(filePath);
            if (music == IntPtr.Zero)
            {
                _logger.Warn($"Failed to load '{filePath}'. {SDL_mixer.Mix_GetError()}");
                return;
            }

            try
            {
                SDL_mixer.Mix_PlayMusic(music, 0);

                while (!ct.IsCancellationRequested && SDL_mixer.Mix_PlayingMusic() == 1)
                {
                    await Task.Delay(50, ct).ConfigureAwait(false);
                }

                if (ct.IsCancellationRequested)
                {
                    SDL_mixer.Mix_HaltMusic();
                }
            }
            finally
            {
                SDL_mixer.Mix_FreeMusic(music);
            }
        }
        catch (OperationCanceledException)
        {
            SDL_mixer.Mix_HaltMusic();
        }
        finally
        {
            _playLock.Release();
        }
    }
}
