using GenieCoreLib;

namespace TestGenieWasm;

[TestClass]
public class TestConnections
{
    [TestMethod]
    public void CanLoadLegacyCharacterProfiles()
    {
        GenieWasm.Desktop.LocalSetup.InitLocalDirectory();
        var _ = AppGlobals.LocalDirectoryPath; // Ensure the LocalDirectoryPath is initialized
        CharacterProfiles profiles = new CharacterProfiles(useLegacy: true);
        Assert.IsTrue(profiles.Profiles.Count > 0);
        IEnumerable<CharacterProfile> validProfiles = profiles.Profiles.Where(profile => profile.CheckValid());
        IEnumerable<CharacterProfile> invalidProfiles = profiles.Profiles.Where(profile => !profile.CheckValid());
        Assert.IsTrue(profiles.Profiles.Count == validProfiles.Count());
    }
    [TestMethod]
    public void CanSaveProfiles()
    {
        GenieWasm.Desktop.LocalSetup.InitLocalDirectory();
        var _ = AppGlobals.LocalDirectoryPath; // Ensure the LocalDirectoryPath is initialized
        CharacterProfiles profiles = new CharacterProfiles(useLegacy: true);
        Assert.IsTrue(profiles.Profiles.Count > 0);
        Assert.IsTrue(CharacterProfiles.SaveProfiles(profiles.Profiles));
    }
    [TestMethod]
    public void CanLoadCurrentCharacterProfiles()
    {
        GenieWasm.Desktop.LocalSetup.InitLocalDirectory();
        var _ = AppGlobals.LocalDirectoryPath; // Ensure the LocalDirectoryPath is initialized
        CharacterProfiles profiles = new CharacterProfiles(useLegacy: false);
        Assert.IsTrue(profiles.Profiles.Count > 0);
        IEnumerable<CharacterProfile> validProfiles = profiles.Profiles.Where(profile => profile.CheckValid());
        IEnumerable<CharacterProfile> invalidProfiles = profiles.Profiles.Where(profile => !profile.CheckValid());
        Assert.IsTrue(profiles.Profiles.Count == validProfiles.Count());
    }
}
