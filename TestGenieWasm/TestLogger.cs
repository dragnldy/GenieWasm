using GenieCoreLib;

namespace TestGenieWasm;

[TestClass]
public class TestLogger
{
    [TestMethod]
    public void CanStartAndStopLogger()
    {
        if (string.IsNullOrEmpty(AppGlobals.LocalDirectoryPath))
        {
            GenieWasm.Desktop.LocalSetup.InitSettings();
        }
        AppGlobals.LogQueue.Clear(); // Clear any existing log entries
        Log logger = Log.Instance;
        Log.IsTesting = true;
        Log.LogText("Test log entry", "TestCharacter", "DRT");
        Thread.Sleep(1200); // Wait for the log entry to be processed
        Assert.IsTrue(Log.LogBuffer.Length > 0, "Log buffer should contain log entries after logging.");
        Log.StopLogging();
        Log.LogBuffer.Clear();
        Log.LogText("Test log entry", "TestCharacter", "DRT");
        Thread.Sleep(1200); // Wait for the log entry to not be processed
        Assert.IsTrue(Log.LogBuffer.Length == 0, "Log buffer should not contain log entries after stop logging.");
        Log.IsTesting = false;
    }
    [TestMethod]
    public void CanWriteToFileWithLogger()
    {
        // Don't run this very often as it opens and writes to an actual file
        if (string.IsNullOrEmpty(AppGlobals.LocalDirectoryPath))
        {
            GenieWasm.Desktop.LocalSetup.InitSettings();
        }
        AppGlobals.LogQueue.Clear(); // Clear any existing log entries
        Log logger = Log.Instance;
        Log.IsTesting = false;
        Log.LogText("Test log entry", "TestCharacter", "DRT");
        Log.LogLine("Test logline entry", Log.GetFileName("TestCharacter", "DRT"));
        Log.LogLine("Test logline entry2", "TestCharacter", "DRT");
        Thread.Sleep(1200); // Wait for the log entry to be processed
        Assert.IsTrue(Log.LogBuffer.Length == 0, "Log buffer should NOT contain log entries after logging.");
        Log.StopLogging();
        Log.IsTesting = false;
    }
}
