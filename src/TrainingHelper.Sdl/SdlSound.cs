using TrainingHelper.Common;
using SDL2;
using NLog;

namespace TrainingHelper.Sdl;

internal class SDLSound : IDisposable, ISound
{
    public IReadOnlyCollection<string> SoundDevicesNames => _soundDevicesNames;

    private readonly List<string> _soundDevicesNames = [];
    private double _samplingIndex = 0;
    private SDL.SDL_AudioSpec _haveAudioSpec;
    private uint _soundDeviceId;

    private const double AMPLITUDE = 28000d;
    private const double SOUND_FREQUENCY = 261.63d;
    private static ILogger _logger = LogManager.GetCurrentClassLogger();

    public SDLSound()
    {
        // Initilizes SDL.
        if (SDL.SDL_Init(SDL.SDL_INIT_AUDIO) < 0)
        {
            _logger.Error($"Can't initalize SDL audio. {SDL.SDL_GetError()}");
        }

        int audioDevicesCount = SDL.SDL_GetNumAudioDevices(0);
        for (int i = 0; i < audioDevicesCount; i++)
        {
            _soundDevicesNames.Add(SDL.SDL_GetAudioDeviceName(i, 0));
        }

        if (_soundDevicesNames.Count != 0)
        {
            OpenAudioDevice(_soundDevicesNames[0]);
        }
        else
        {
            _logger.Warn("No audio device suitable for playback.");
        }
    }

    public void ChangeAudiDevice(string audioDeviceName)
    {
        if (_soundDeviceId != 0)
        {
            SDL.SDL_CloseAudioDevice(_soundDeviceId);
        }
        OpenAudioDevice(audioDeviceName);
    }

    public void ChangeSoundState(bool active)
    {
        SDL.SDL_PauseAudioDevice(_soundDeviceId, active ? 0 : 1);
    }

    public void Dispose()
    {
        ChangeSoundState(false);
        SDL.SDL_CloseAudioDevice(_soundDeviceId);
        SDL.SDL_Quit();
    }

    private void OpenAudioDevice(string deviceName)
    {
        SDL.SDL_AudioSpec want = new()
        {
            freq = 44100,
            format = SDL.AUDIO_S16SYS,
            channels = 1,
            samples = 512,
            callback = AudioCallback
        };
        _soundDeviceId = SDL.SDL_OpenAudioDevice(deviceName, 0, ref want, out _haveAudioSpec, 0);

        if (_soundDeviceId == 0)
        {
            _logger.Error($"Failed to open audio: {SDL.SDL_GetError()}");
        }
        else if (want.format != _haveAudioSpec.format)
        {
            _logger.Error($"Failed to get the desired AudioSpec. Instead got: frequency: {_haveAudioSpec.freq}, format: {_haveAudioSpec.format}, channels: {_haveAudioSpec.channels}, samples: {_haveAudioSpec.samples}, sizes: {_haveAudioSpec.size}");
        }

    }

    private void AudioCallback(IntPtr userdata, IntPtr stream, int lenInBytes)
    {
        Span<short> streamSpan;
        unsafe
        {
            short* buffer = (short*)stream.ToPointer();
            streamSpan = new Span<short>(buffer, lenInBytes / sizeof(short));
        }
        for (int sample = 0; sample < _haveAudioSpec.samples; sample++)
        {
            short data = GetSoundDataForSample(sample);

            for (int channelId = 0; channelId < _haveAudioSpec.channels; channelId++)
            {
                int offset = (sample * _haveAudioSpec.channels) + channelId;
                streamSpan[offset] = data;
            }
        }
    }

    private short GetSoundDataForSample(int sample)
    {
        short result = (short)(AMPLITUDE * Math.Sin(_samplingIndex));
        _samplingIndex += (2.0d * Math.PI * SOUND_FREQUENCY) / _haveAudioSpec.freq;
        //we want to wrap around after we make full circle
        if (_samplingIndex >= (2.0d * Math.PI))
        {
            _samplingIndex -= 2.0d * Math.PI;
        }
        return result;
    }
}
