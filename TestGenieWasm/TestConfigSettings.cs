using GenieCoreLib;
using System.Text.Json;

namespace TestGenieWasm;

[TestClass]
public class TestConfigSettings
{
    [TestMethod]
    public void CanLoadLegacySettings()
    {
        GenieWasm.Desktop.LocalSetup.InitLocalDirectory();
        string path = AppGlobals.LocalDirectoryPath;
        string configPath = System.IO.Path.Combine(path, "config","settings.cfg");
        Assert.IsTrue(File.Exists(configPath));
        string configData = File.ReadAllText(configPath);
        Assert.IsFalse(string.IsNullOrEmpty(configData));
        ConfigSettings? settings = ConfigSettings.Instance;
        Assert.IsNotNull(settings);
        settings.IgnoreMonsterList = "JUNK"; // Set it so we can make sure is changed after load

        ConfigSettings.LoadLegacySettings(settings, configData);
        Assert.IsFalse(settings.IgnoreMonsterList.Equals("JUNK"));
    }
    readonly JsonSerializerOptions jsonOptions = new() { WriteIndented = true };
    [TestMethod]
    public void CanSaveSettings()
    {

        GenieWasm.Desktop.LocalSetup.InitLocalDirectory();
        string path = AppGlobals.LocalDirectoryPath;
        string configPath = System.IO.Path.Combine(path, "config", "settings.cfg");
        ConfigSettings? settings = ConfigSettings.Instance;
        Assert.IsNotNull(settings);
        settings.LoadSettings(configPath);
        string jsonBeforeWrite = JsonSerializer.Serialize(settings, jsonOptions);
        settings.SaveSettings(configPath.Replace("settings","settings.test"));
        settings.LoadSettings(configPath.Replace("settings","settings.test"));
        string jsonAfterWrite = JsonSerializer.Serialize(settings, jsonOptions);
        Assert.IsTrue(jsonBeforeWrite.Equals(jsonAfterWrite), "Settings were not saved correctly.");
    }
}
