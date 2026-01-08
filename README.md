# VoiceToText - Push-to-Talk Voice Transcription App

WPF System Tray application that records voice using a hotkey and transcribes it using OpenAI's Audio Transcriptions API, then pastes the text into the focused application.

## Features

- **Hold-to-talk**: Hold Ctrl+Alt+Space to record, release to transcribe
- **OpenAI Integration**: Uses gpt-4o-mini-transcribe (default) or whisper-1
- **Clipboard Paste**: Automatically pastes transcribed text via Ctrl+V
- **System Tray**: Runs silently in the background
- **Status Overlay**: Visual feedback during recording/processing
- **History**: View and copy recent transcriptions
- **Secure API Key Storage**: Uses Windows DPAPI encryption

## Requirements

- Windows 10/11
- .NET 9.0 SDK
- OpenAI API Key

## Quick Start

### Build and Run

```bash
cd C:\Users\be4rr\VoiceToText
dotnet build
dotnet run --project src/VoiceToText.App
```

### Publish (Self-contained)

```bash
dotnet publish src/VoiceToText.App -c Release -r win-x64 --self-contained -o ./publish
```

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
| Restore Clipboard | Restore original clipboard after paste | Yes |

## Project Structure

```
VoiceToText/
├── VoiceToText.sln
├── src/
│   ├── VoiceToText.App/           # WPF UI, System Tray, Settings
│   ├── VoiceToText.Core.Audio/    # NAudio recording, WAV encoding
│   ├── VoiceToText.Core.Hotkey/   # Global keyboard hook (WH_KEYBOARD_LL)
│   ├── VoiceToText.Core.OpenAI/   # OpenAI API client
│   └── VoiceToText.Core.Paste/    # Clipboard + SendInput
└── README.md
```

## Key Components

### Core.Hotkey
- Uses `WH_KEYBOARD_LL` low-level keyboard hook
- Detects both key press and release for hold-to-talk
- Configurable modifier keys and main key

### Core.Audio
- Records from default microphone using NAudio
- PCM 16-bit, Mono, 16kHz sample rate
- Outputs WAV format for API compatibility

### Core.OpenAI
- HTTP client with multipart/form-data
- Exponential backoff retry (1s, 2s, 4s)
- Error categorization (auth, rate limit, network, etc.)

### Core.Paste
- Saves and restores clipboard content
- Sends Ctrl+V via Windows SendInput API

## Known Limitations (MVP)

- Hold-to-talk only (no toggle mode)
- Default microphone only (no device selection)
- No streaming transcription
- Space long-press not implemented (planned for future)

## Future Enhancements

- Space long-press mode (separate from normal space input)
- Streaming transcription for real-time feedback
- Audio device selection
- Custom prompts for transcription
- Hotword detection

## Troubleshooting

### "API Key is not configured"
Open Settings and enter your OpenAI API key.

### Hotkey not working
- Ensure no other application is using the same hotkey
- Try running as Administrator
- Check that the app is running (visible in system tray)

### Transcription fails
- Verify your API key is valid
- Check your internet connection
- Review the error message in the balloon notification

## License

MIT License
