# TrainingHelper

TrainingHelper is a lightweight desktop workout timer for organizing exercise rounds, tracking work/break periods, and playing audio cues. The app is built with Avalonia and targets .NET 10.

The project centers around a configurable workout plan: each round has an interval, break length, repetition count, and optional custom exercise configuration. During a workout, the app shows the current round state, can display exercise images, and emits a countdown beep as the interval ends.

## Project structure

The solution entry point is [src/TrainingHelper.slnx](src/TrainingHelper.slnx). It contains these projects:

- `TrainingHelper.App` - Avalonia desktop UI, dependency injection, startup, and the countdown loop
- `TrainingHelper.Common` - shared contracts for settings, plugins, and exercise configuration
- `TrainingHelper.FreeExerciseDb` - Free Exercise DB integration and round configuration UI
- `TrainingHelper.FreeExerciseDbApi` - ASP.NET Core sample API that serves exercise data
- `TrainingHelper.Sdl` - sound backend selection and SDL/MP3 integration
- `SDL2-CS` - managed SDL bindings used by the sound layer

## What the app does

- Creates a multi-round workout plan from one or more rounds
- Displays the current work/break state and elapsed time in the UI
- Supports optional exercise images per round
- Plays start, work, break, countdown, and end cues
- Lets the user pick an audio device from detected system devices
- Allows round behavior to be extended through a simple plugin system

At startup, the app loads round custom-config factories from a plugin directory, then registers them with the dependency injection container. The built-in Free Exercise DB factory adds exercise lookup and image selection to the round configuration flow.

## Runtime architecture

The app is organized around a few key pieces:

- `App` wires up dependency injection, loads plugins, and creates the main window
- `MainModel` owns the countdown loop and sound playback logic
- `MainWindowViewModel` exposes the UI state and commands for starting, pausing, and stopping workouts
- `RoundViewModel` holds per-round settings and custom configuration UI
- `ISettingsService<T>` loads and saves JSON settings for each settings type
- `PluginLoader` discovers plugin directories and loads types implementing `IPlugin`

## Configuration

The app stores JSON settings in a per-user config directory. The current implementation creates a directory named `TrainingHelper.App`.

### Configuration root

- Windows: `%APPDATA%\TrainingHelper.App`
- Linux: `$XDG_CONFIG_HOME/TrainingHelper.App`, or the platform application data directory when `XDG_CONFIG_HOME` is not set
- Other platforms: `UserConfig` relative to the working directory

### Files created there

- `AppSettings.json`
- `SoundConfig.json`

### `AppSettings.json`

This file controls the default workout timings and plugin path:

```json
{
  "WorkTime": "00:00:30",
  "BreakTime": "00:00:03",
  "CountdownStart": "00:00:03",
  "PluginsPath": "Plugins"
}
```

Defaults in the current code are:

- `WorkTime`: 30 seconds
- `BreakTime`: 3 seconds
- `CountdownStart`: 3 seconds
- `PluginsPath`: `Plugins`

`AppSettings.json` is optional; if it is missing, the app falls back to the in-code defaults. The project does not use a standard ASP.NET `appsettings.json` file pattern for the desktop app.

### `FreeExerciseDbConfig.json`

The Free Exercise DB integration expects this shape:

```json
{
  "FreeExercsieDbExercsieJson": "/path/to/exercises.json",
  "ImagesPathBase": "/path/to/exercise/images"
}
```

The values are used to point to the source exercise JSON and the root folder for exercise images. The code checks that the configured exercise file exists before creating the exercise provider. If the config is missing or invalid, startup can fail.

### `SoundConfig.json`

This file stores audio configuration:

```json
{
  "DefaultAudioDeviceSelection": "",
  "SoundFiles": {
    "Work": "/path/to/work.mp3",
    "Break": "/path/to/break.mp3",
    "Start": "/path/to/start.mp3",
    "End": "/path/to/end.mp3",
    "Countdown": "/path/to/countdown.mp3"
  }
}
```

`DefaultAudioDeviceSelection` selects an audio device by name. If it is empty, the backend chooses the default device. `SoundFiles` is optional; when all five sound entries are present, the app switches to the MP3-backed sound path. Otherwise it falls back to the built-in synthesized SDL cues.

## Free Exercise DB data

The project includes a separate exercise database model and configuration UI in `TrainingHelper.FreeExerciseDb`. This module defines the exercise schema used by the workout round custom config and is designed to work with Free Exercise DB JSON data.

The API project, `TrainingHelper.FreeExerciseDbApi`, is a companion ASP.NET Core service that reads a local `Data/exercises.json` file and exposes endpoints such as:

- `/api/exercises`
- `/api/exercises/search`
- `/api/exercises/{id}`

This server is not the desktop app itself; it is a standalone API for serving exercise metadata.

## Building and running

Requirements:

- .NET 10 SDK
- SDL development dependencies if you are building the audio stack on the target platform

From the repository root:

```bash
dotnet restore
dotnet build src/TrainingHelper.slnx
dotnet run --project src/TrainingHelper.App/TrainingHelper.App.csproj
```

The solution file is `src/TrainingHelper.slnx`, and the desktop app entry point is `src/TrainingHelper.App/TrainingHelper.App.csproj`.

## Notes

- The project is currently structured as a desktop utility rather than a web application.
- The app is intentionally configurable via JSON files and a plugin model rather than a single central `appsettings.json` file.
