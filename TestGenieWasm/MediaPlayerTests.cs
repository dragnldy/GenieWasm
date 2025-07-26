using LibVLCSharp.Shared;

namespace TestGenieWasm;

[TestClass]
public class MediaPlayerTests
{
    [TestMethod]
    public void TestMethod1()
    {
        try
        {
            LibVLC MainLibVLC = new(enableDebugLogs: true);
            MediaPlayer MainMediaPlayer = new(MainLibVLC);
            MainMediaPlayer.TimeChanged += MediaPlayer_TimeChanged;
            Media media = new(MainLibVLC, new Uri("https://github.com/rafaelreis-hotmart/Audio-Sample-files/raw/refs/heads/master/sample.mp3"));
            MainMediaPlayer.Media = media;
            MainMediaPlayer.Play();
        }
        catch(Exception exc)
        {

        }
    }

    private void MediaPlayer_TimeChanged(object? sender, MediaPlayerTimeChangedEventArgs e)
    {
    }
}
