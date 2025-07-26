namespace GenieCoreLib;
/// <summary>
/// Event arguments for requesting a sound to be played.
/// </summary>
public class PlaySoundRequestedEventArgs : EventArgs
{
    public string SoundName { get; }

    public PlaySoundRequestedEventArgs(string soundName)
    {
        SoundName = soundName;
    }
}

// Callbacks to implement platform specific logic
// For now we need to supply a callback for playing sounds
public static class EventCallBacks
{
    /// <summary>
    /// Delegate for requesting a sound to be played.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event arguments containing sound information.</param>
    public delegate void PlaySoundRequestedEventHandler(object? sender, PlaySoundRequestedEventArgs e);



    /// <summary>
    /// Event to request playing a sound.
    /// </summary>
    public static event PlaySoundRequestedEventHandler? PlaySoundRequested;

    /// <summary>
    /// Raises the PlaySoundRequested event.
    /// </summary>
    /// <param name="soundName">The name of the sound to play.</param>
    public static void OnPlaySoundRequested(string soundName)
    {
        PlaySoundRequested?.Invoke(null, new PlaySoundRequestedEventArgs(soundName));
    }
}
