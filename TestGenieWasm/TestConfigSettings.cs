using GenieCoreLib;

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
        Assert.IsTrue(!string.IsNullOrEmpty(configData));
        ConfigSettings settings = ConfigSettings.GetInstance();
        Assert.IsNotNull(settings);
        settings.IgnoreMonsterList = "JUNK"; // Set it so we can make sure is changed after load

        ConfigSettings.LoadLegacySettings(settings, configData);
        Assert.IsTrue(!settings.IgnoreMonsterList.Equals("JUNK"));
    }
}
