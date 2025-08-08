using System;
using System.Security.Principal;
using System.Text;
using System.Text.Json;

namespace GenieCoreLib;

public class CharacterProfile
{
    public string Character { get; set; } = string.Empty; // Character name
    public string Account { get; set; } = string.Empty; // Account name
    public string EncryptedPassword { get; set; } = string.Empty; // Encrypted password
    public string Game { get; set; } = string.Empty; // Default game
    public string Layout { get; set; } = string.Empty; // Layout settings

    public CharacterProfile()
    {
    }
    
    public CharacterProfile(string account, string character, string password, string game, string layout)
    {
        Account = account;
        Character = character;
        EncryptedPassword = password;
        Game = game;
        Layout = layout;
    }
    public static string GetDecryptedPassword(string account, string encryptedPassword)
    {
        try
        {
            string argsPassword = "G3" + account.ToUpper();
            string decryptedPassword = Utility.DecryptString(argsPassword, encryptedPassword);
            return decryptedPassword; // Placeholder, replace with actual decryption logic
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error decrypting password: {ex.Message}");
            return string.Empty; // Return empty string if decryption fails
        }
    }
    public bool CheckValid()
    {
        // Encrypted password can be null- maybe account as well
        return !string.IsNullOrEmpty(Character) && 
               !string.IsNullOrEmpty(Account) && 
               !string.IsNullOrEmpty(Game);
    }

    public bool LoadLayout(string layoutFile, out string layoutContent)
    {
        layoutContent = string.Empty;
        if (string.IsNullOrEmpty(layoutFile) || !File.Exists(layoutFile))
        {
            return false; // Layout file does not exist
        }
        try
        {
            layoutContent = File.ReadAllText(layoutFile);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading layout: {ex.Message}");
            return false;
        }
    }
    public bool LoadCustomConfig(string character, string account, string game, out string layoutContent)
    {
        layoutContent = string.Empty;
        string layoutFile = Path.Combine(ConfigSettings.Instance.ConfigProfileDir, $"{character}_{account}_{game}.json");
        if (File.Exists(layoutFile))
        {
            try
            {
                layoutContent = File.ReadAllText(layoutFile);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading custom config: {ex.Message}");
                return false;
            }
        }
        return false; // Custom config file does not exist
    }

    public static string GetEncryptedPassword(string accountName, string password)
    {
        string argsPassword = "G3" + accountName.ToUpper();
        string argsText = password;
        return Utility.EncryptString(argsPassword, argsText);
    }
}

public class CharacterProfiles
{
    public List<CharacterProfile> Profiles { get; set; } = new List<CharacterProfile>();
    public CharacterProfiles(bool useLegacy = false)
    {
        LoadExistingProfiles(useLegacy);
    }

    public bool LoadExistingProfiles(bool useLegacy = false)
    {
        Profiles.Clear();

        string baseDir = Path.Combine(AppGlobals.LocalDirectoryPath, ConfigSettings.Instance.ConfigDir);

        // Might want to load from legacy files if testing or if new format not found
        string currentProfiles = Path.Combine(baseDir, "profiles.json");
        useLegacy = useLegacy | !File.Exists(currentProfiles);

        if (!useLegacy)
        {
            try
            {
                string json = File.ReadAllText(currentProfiles);
                IEnumerable<CharacterProfile> profiles = JsonSerializer.Deserialize<List<CharacterProfile>>(json);
                if (profiles is not null)
                {
                    Profiles.AddRange(profiles);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading profiles: {ex.Message}");
                return false;
            }
        }
        string legacyProfiles = Path.Combine(AppGlobals.LocalDirectoryPath, ConfigSettings.Instance.ConfigProfileDir);
        try
        {
            // If we fall through, need to load invidiual files
            foreach (var file in Directory.GetFiles(legacyProfiles).Where(n=>n.EndsWith(".xml")))
            {
                CharacterProfile? profile = LoadProfileFromFile(file);
                if (profile is not null)
                {
                    AddProfile(profile);
                }
            }
            SaveProfiles(Profiles); // Save back to ensure the format is correct
            return Profiles.Count > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading legacy profiles: {ex.Message}");
            return false; // Return false if there was an error
        }
    }
    private CharacterProfile? LoadProfileFromFile(string file)
    {
        CharacterProfile? profile = new CharacterProfile();

        try
        {
            // Load legacy profile from XML file
            XMLConfig config = new XMLConfig(file);
            if (!config.HasData)
            {
                return null; // No data in the file
            }
            profile.Account = config.GetValue("Genie/Profile", "Account", string.Empty);
            profile.Character = config.GetValue("Genie/Profile", "Character", string.Empty);
            profile.Game = config.GetValue("Genie/Profile", AppGlobals.MainWindow, string.Empty);
            profile.EncryptedPassword = config.GetValue("Genie/Profile", "Password", string.Empty);
            profile.Layout = config.GetValue("Genie/Profile/Layout", "FileName", string.Empty);

            //sProfile = FileName.Substring(FileName.LastIndexOf(@"\") + 1).Replace(".xml", "");
            //m_oGlobals.Config.sConfigDirProfile = m_oGlobals.Config.ConfigDir + @"\Profiles\" + sProfile;
            return profile;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading profile from file {file}: {ex.Message}");
            return null; // Return null if there was an error
        }
    }

    public void AddProfile(CharacterProfile profile)
    {
        if (profile == null) return;
        // Remove any existing profile
        RemoveProfile(profile.Character, profile.Account, profile.Game);
        // Add new profile
        Profiles.Add(profile);
    }
    public void RemoveProfile(string name, string account, string game)
    {
        var profile = Profiles.FirstOrDefault(p => p.Character == name && p.Account == account && p.Game == game);
        if (profile != null)
        {
            Profiles.Remove(profile);
        }
    }
    public static bool SaveProfiles(IEnumerable<CharacterProfile> profiles)
    {
        try
        {
            string baseDir = Path.Combine(AppGlobals.LocalDirectoryPath, ConfigSettings.Instance.ConfigDir);
            string currentProfiles = Path.Combine(baseDir, "profiles.json");
            string json = JsonSerializer.Serialize(profiles, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(currentProfiles, json);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving profiles: {ex.Message}");
            return false; // Return false if there was an error
        }
    }
}
