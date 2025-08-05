namespace GenieCoreLib;

public class ConnectionRequest
{
    // Properties supplied by user during connection
    public string Account { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Character { get; set; } = string.Empty;
    public string Game { get; set; } = string.Empty;
    public bool IsValid { get; set; } = false; // Indicates if the connection request is valid

    // Properties supplied by system during connection
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 0;
    public bool IsSecure { get; set; } = false;
    public string AuthenticationToken { get; set; } = string.Empty;

    // Instructions for saving credentials
    public bool SaveAccount { get; set; } = false;
    public bool SavePassword { get; set; } = false;

    public ConnectionRequest() { }
    public ConnectionRequest(string account, string password, string character, string game, bool saveAccount=false, bool savePassword=false)
    {
        Account = account;
        Password = password;
        Character = character;
        Game = game;
        SaveAccount = saveAccount;
        SavePassword = savePassword;
    }
}
