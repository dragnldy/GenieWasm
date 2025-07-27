using GenieCoreLib;
using LibVLCSharp.Shared;
using System;

namespace GenieWasm.Desktop;

public static class PlatformSpecificEventHandlers
{
    public static LibVLC MainLibVLC { get; set; } = new LibVLC();
    public static MediaPlayer MainMediaPlayer { get; set; } = new MediaPlayer(MainLibVLC);
    /// <summary>
    /// Subscribes to the PlaySoundRequested event in GenieCoreLib.EventCallBacks.
    /// </summary>
    public static void Register()
    {
        EventCallBacks.PlaySoundRequested += OnPlaySoundRequested;
    }

    /// <summary>
    /// Receives the request to play a sound.
    /// </summary>
    private static void OnPlaySoundRequested(object? sender, PlaySoundRequestedEventArgs e)
    {
        string sound = e.SoundName;
        if (string.IsNullOrEmpty(sound))
        {
            // stop the media player if no sound is specified
            MainMediaPlayer.Stop();
        }
        else
        {
            Media media = new(MainLibVLC, new Uri("https://github.com/rafaelreis-hotmart/Audio-Sample-files/raw/refs/heads/master/sample.mp3"));
            MainMediaPlayer.Media = media;
            MainMediaPlayer.Play();
        }
    }
}
