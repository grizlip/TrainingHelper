using NLog;
using SDL2;
using TrainingHelper.Common.Settings;
using TrainingHelper.Common.Sound;
using TrainingHelper.Sdl.Settings;

namespace TrainingHelper.Sdl;

internal sealed class Mp3Sound : ISound, IDisposable
{
    private const int DefaultMusicVolume = 64;
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private readonly List<string> _soundDevicesNames = [];
    private readonly Dictionary<SoundType, string> _sounds = [];
    private readonly SemaphoreSlim _playLock = new(1, 1);
    private bool _audioInitialized;
    private string? _selectedAudioDeviceName;
    private IntPtr _activeMusic;
    private string? _activeMusicFilePath;

    public IReadOnlyCollection<string> SoundDevicesNames => _soundDevicesNames;

    public string SelectedAudioDevice => _selectedAudioDeviceName ?? string.Empty;
    
    private readonly ISettingsService<SoundConfig> _settingsService;
    
    public Mp3Sound(ISettingsService<SoundConfig> settingsService)
    {
        _settingsService = settingsService;
        DiscoverAudioDevices();
        LoadMp3Files(settingsService.Load().SoundFiles);
        // Initialize the SDL_mixer audio system with the default or specified audio device (null means default)
        _selectedAudioDeviceName = settingsService.Load().DefaultAudioDeviceSelection;
        InitializeMixer(string.IsNullOrEmpty(_selectedAudioDeviceName) ? null : _selectedAudioDeviceName);
    }

    public void ChangeAudiDevice(string audioDeviceName)
    {
        if (string.IsNullOrWhiteSpace(audioDeviceName))
        {
            return;
        }

        if (string.Equals(_selectedAudioDeviceName, audioDeviceName, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        string? activeMusicFilePath = null;
        double activeMusicPosition = 0;

        _playLock.Wait();
        try
        {
            bool wasPlaying = _activeMusic != IntPtr.Zero && SDL_mixer.Mix_PlayingMusic() == 1;
            if (wasPlaying)
            {
                _logger.Trace("Pausing currently playing music before switching audio device.");
                activeMusicFilePath = _activeMusicFilePath;
                activeMusicPosition = SDL_mixer.Mix_GetMusicPosition(_activeMusic);
                SDL_mixer.Mix_HaltMusic();
                SDL_mixer.Mix_FreeMusic(_activeMusic);
                _activeMusic = IntPtr.Zero;
                _activeMusicFilePath = null;
            }

            _logger.Trace($"Switching SDL mixer audio device from '{_selectedAudioDeviceName ?? "default"}' to '{audioDeviceName}'.");
            SDL_mixer.Mix_CloseAudio();
            SDL_mixer.Mix_Quit();

            _selectedAudioDeviceName = audioDeviceName;
            InitializeMixer(audioDeviceName);

            if (wasPlaying && File.Exists(activeMusicFilePath))
            {
                _logger.Trace($"Resuming previously playing music '{activeMusicFilePath}' after switching audio device. {activeMusicPosition} seconds into the track.");
                _activeMusic = SDL_mixer.Mix_LoadMUS(activeMusicFilePath);
                if (_activeMusic != IntPtr.Zero)
                {
                    _activeMusicFilePath = activeMusicFilePath;
                    if (SDL_mixer.Mix_PlayMusic(_activeMusic, 0) < 0)
                    {
                        _logger.Warn($"Failed to play music '{activeMusicFilePath}': {SDL.SDL_GetError()}");
                    }
                    if (activeMusicPosition > 0)
                    {
                        if (SDL_mixer.Mix_SetMusicPosition(activeMusicPosition) < 0)
                        {
                            _logger.Warn($"Failed to set music position for '{activeMusicFilePath}': {SDL.SDL_GetError()}");
                        }
                    }


                }
            }
        }
        finally
        {
            _playLock.Release();
        }
        _settingsService.Save(new SoundConfig()
        {
            SoundFiles = _settingsService.Load().SoundFiles,
            DefaultAudioDeviceSelection = audioDeviceName
        });
    }

    public async Task PlayWorkJingleAsync(CancellationToken ct) => await PlayMp3Async(SoundType.Work, ct);
    public async Task PlayBreakJingleAsync(CancellationToken ct) => await PlayMp3Async(SoundType.Break, ct);
    public async Task PlayStartJingleAsyncAsync(CancellationToken ct) => await PlayMp3Async(SoundType.Start, ct);
    public async Task PlayEndJingleAsyncAsync(CancellationToken ct) => await PlayMp3Async(SoundType.End, ct);
    public async Task PlayCountdownJingleAsync(CancellationToken ct) => await PlayMp3Async(SoundType.Countdown, ct);

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

    private void InitializeMixer(string? deviceName)
    {
        if (SDL_mixer.OpenAudioDevice(44100, SDL.AUDIO_S16SYS, 2, 2048, deviceName, 1) < 0)
        {
            _logger.Warn($"Unable to open SDL_mixer audio device '{deviceName ?? "default"}': {SDL_mixer.Mix_GetError()}");
            _audioInitialized = false;
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

            _activeMusic = music;
            _activeMusicFilePath = filePath;

            try
            {
                SDL_mixer.Mix_PlayMusic(_activeMusic, 0);

                while (!ct.IsCancellationRequested && SDL_mixer.Mix_PlayingMusic() == 1)
                {
                    _playLock.Release();
                    await Task.Delay(50, ct).ConfigureAwait(false);
                    await _playLock.WaitAsync(ct).ConfigureAwait(false);
                }

                if (ct.IsCancellationRequested)
                {
                    SDL_mixer.Mix_HaltMusic();
                }
            }
            finally
            {

                _activeMusic = IntPtr.Zero;
                _activeMusicFilePath = null;


                SDL_mixer.Mix_FreeMusic(_activeMusic);
            }
        }
        catch (OperationCanceledException)
        {
            SDL_mixer.Mix_HaltMusic();
        }
        finally
        {
            if (_playLock.CurrentCount == 0)
            {
                _playLock.Release();
            }
        }
    }
}
