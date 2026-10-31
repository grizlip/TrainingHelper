# TrainingHelper

TrainingHelper is a desktop application for planning exercise rounds and running
work/break countdowns. The app can show exercise images during work intervals
and play audio cues.

## Solution structure

The solution is [src/TrainingHelper.slnx](src/TrainingHelper.slnx), which
contains these projects:

| Project | Purpose |
| --- | --- |
| `TrainingHelper.App` | Avalonia desktop UI, application startup, and countdown coordination |
| `TrainingHelper.Common` | Shared settings and exercise/sound interfaces |
| `TrainingHelper.FreeExerciseDb` | Exercise data and per-round exercise/image selection |
| `TrainingHelper.Sdl` | Audio device support and SDL-based sound playback |
| `SDL2-CS` (`SDL2-CS.Core.csproj`) | SDL bindings used by the audio implementation |

The app composes the services at startup using dependency injection. Each
settings type is stored separately as JSON through the shared
`ISettingsService<TSettings>` implementation; this is not an `appsettings.json`
configuration setup.

## User configuration

Settings are stored in a per-user configuration directory. The application
creates this directory when it starts:

- **Windows:** `%APPDATA%\TraningHelper.App`
- **Linux:** `$XDG_CONFIG_HOME/TraningHelper.App`, or the platform application
  data directory if `XDG_CONFIG_HOME` is not set
- **Other platforms:** `UserConfig` relative to the application working
  directory

The directory name is spelled `TraningHelper.App` in the current implementation.
Each settings file is named after its settings type:

| File | Purpose |
| --- | --- |
| `AppSettings.json` | Default work, break, and countdown cue durations |
| `FreeExerciseDbConfig.json` | Paths to the exercise JSON data and image directory |
| `SoundConfig.json` | Selected audio device and optional MP3 cue files |

`AppSettings.json` and `SoundConfig.json` are optional; if either is missing,
the app uses that settings type's defaults. Loading defaults does not write a
file, so create the file in the configuration directory to customize values
that the app does not expose in its UI. `FreeExerciseDbConfig.json` must be
created and contain a valid exercise JSON file path for the app to start.

### Workout timing: `AppSettings.json`

The default values are 30 seconds of work, 3 seconds of break, and a 3-second
countdown cue window:

```json
{
  "WorkTime": "00:00:30",
  "BreakTime": "00:00:03",
  "CountdownStart": "00:00:03"
}
```

`WorkTime` and `BreakTime` set the initial interval and break durations for a
new workout plan. `CountdownStart` controls when the final countdown sound
starts. The settings tab currently lets you select an audio device; it does not
edit or save these timing values. Changes made to round durations in the plan
are also not saved between runs.

### Exercise data and images: `FreeExerciseDbConfig.json`

Configure the path to a Free Exercise DB JSON file and the base directory for
its images:

```json
{
  "FreeExercsieDbExercsieJson": "/path/to/exercises.json",
  "ImagesPathBase": "/path/to/exercise/images"
}
```

The property names above match the application exactly. The exercise file must
contain a top-level JSON array of exercise records with `id`, `name`, and
`images` fields (as well as the other fields in the Free Exercise DB format).
Image entries are combined with `ImagesPathBase`; use paths to files that
already exist. Absolute paths are recommended.

This configuration is required for startup and exercise selection: the app
checks that the exercise JSON file exists when it creates the exercise
provider. If the config file is missing, or the configured path is invalid,
startup fails. The app does not download the database or images.

### Audio: `SoundConfig.json`

Audio settings have these properties:

- `DefaultAudioDeviceSelection`: the exact audio device name to select. The app
  updates this value when a device is selected in the Settings tab. With an
  empty name, the audio backend selects its default device. If a saved name is
  no longer available, the built-in SDL backend chooses the first discovered
  device; the MP3 backend may fail to initialize until a valid device is
  selected.
- `SoundFiles`: optional paths to MP3 files keyed by sound type: `Work`,
  `Break`, `Start`, `End`, and `Countdown`.

The app uses MP3 playback only when `SoundFiles` contains all five sound types.
Otherwise, it uses the built-in synthesized SDL cues. When MP3 mode is active,
each configured path should point to an existing MP3 file; missing files are
logged and that cue will not play. MP3 file paths are used as supplied, so
absolute paths are recommended.

### Logging

`TrainingHelper.App/nlog.config` configures NLog to write Trace-and-above
messages to the console and to `all.log` in the application's working directory.
The app project copies this file to its output directory.
