using GenieCoreLib;

namespace TestGenieWasm;

[TestClass]
public class TestConnect
{
    [TestMethod]
    public void CanSendConnectRequest()
    {
        // # connect account password character game"
        Command command = Command.Instance;
        string account = "testAccount";
        string password = "testPassword";
        string character = "testCharacter";
        string targetgame = "DRT";
        var oArgs = new ArrayList();
        oArgs = Utility.ParseArgs($"connect {account} {password} {character} {targetgame}");
        command.EventConnect += Command_EventConnect;
        command.Connect(oArgs,false);
    }

    private void Command_EventConnect(string sAccountName, string sPassword, string sCharacter, string sGame, bool isLich)
    {
        Game game = Game.Instance;
        // game.Connect(string.Empty, account, password, character, targetgame);    
        //        Assert.IsFalse(command.IsConnected, "Connection should be established.");
    }
}
