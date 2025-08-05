using GenieCoreLib;

namespace TestGenieWasm;

[TestClass]
public class TestConnect
{
    [TestMethod]
    public void CanSendConnectRequest()
    {
        // # connect account password character game"
        CharacterProfile profile = new CharacterProfile("testAccount", "testCharacter", "testPassword", "DRT", string.Empty);
        GameConnection.Instance.Connect(profile, false, true);
    }

}
