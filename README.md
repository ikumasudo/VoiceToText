# oto - Push-to-Talk Voice Transcription App

WPF System Tray application that records voice using a hotkey and transcribes it using OpenAI's Audio Transcriptions API, then pastes the text into the focused application.

## Features

- **Hold-to-talk**: Hold Ctrl+Alt+Space to record, release to transcribe
- **OpenAI Integration**: Uses gpt-4o-mini-transcribe (default) or whisper-1
- **Clipboard Paste**: Automatically pastes transcribed text via Ctrl+V
- **System Tray**: Runs silently in the background
- **Status Indicator**: Visual feedback with audio level meter during recording
- **Sound Effects**: Audio feedback for recording start/stop
- **History**: View and copy recent transcriptions
- **Secure API Key Storage**: Uses Windows DPAPI encryption

## Installation

### Download (Recommended)

1. Go to [Releases](https://github.com/ikumasudo/oto/releases/latest)
2. Download `oto-Setup-X.X.X.exe`
3. Run the installer
4. Launch oto from Start Menu or Desktop

**No .NET runtime installation required** - the application is fully self-contained.

### Build from Source

```bash
git clone https://github.com/ikumasudo/oto.git
cd oto
dotnet build
dotnet run --project src/oto.App
```

## Requirements

- Windows 10/11 (64-bit)
- OpenAI API Key
- .NET 9.0 SDK (only for building from source)

## Usage

1. **First Run**: Right-click the tray icon > Settings > Enter your OpenAI API Key
2. **Recording**: Hold `Ctrl+Alt+Space` to record
3. **Release**: Release the keys to stop recording and transcribe
4. **Result**: Text is automatically pasted into the focused application

## Configuration

Right-click the tray icon and select **Settings** to configure:

| Setting | Description | Default |
|---------|-------------|---------|
| API Key | Your OpenAI API key | (required) |
| Model | Transcription model | gpt-4o-mini-transcribe |
| Hotkey | Key combination for recording | Ctrl+Alt+Space |
| Max Duration | Maximum recording time (seconds) | 60 |
| Language | Language hint (e.g., "ja", "en") | Auto-detect |
| Add Punctuation | Include punctuation in output | Yes |
| Preserve Newlines | Keep line breaks | Yes |
| Sound Effects | Audio feedback for recording | Yes |

## Project Structure

```
oto/
├── oto.slnx
├── src/
│   ├── oto.App/           # WPF UI, System Tray, Settings
│   ├── oto.Core.Audio/    # NAudio recording, WAV encoding
│   ├── oto.Core.Hotkey/   # Global keyboard hook (WH_KEYBOARD_LL)
│   ├── oto.Core.OpenAI/   # OpenAI API client
│   └── oto.Core.Paste/    # Clipboard + SendInput
└── README.md
```

## Uninstall

1. Open **Settings** > **Apps** > **Apps & features**
2. Find **oto** and click **Uninstall**

Settings are stored in `%APPDATA%\oto\` (delete manually to remove all data).

## License

MIT License
