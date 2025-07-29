namespace GenieCoreLib;

    static class FileDirectory
    {
    public static string Path => AppGlobals.LocalDirectoryPath;
        public static string ConfigPath => System.IO.Path.Combine(Path, "Config");
        public static string MacrosFileName => System.IO.Path.Combine(ConfigPath, "macros.cfg");
        public static string ClassesFileName => System.IO.Path.Combine(ConfigPath, "classes.cfg");
        public static string SettingsFileName => System.IO.Path.Combine(ConfigPath, "settings.cfg");
        public static string UserDataDirectory => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        public static void CheckUserDirectory()
        {
            string dir = System.IO.Path.Combine(AppGlobals.LocalDirectoryPath, "Config");
            if (!System.IO.Directory.Exists(dir))
            {
                // No local settings, change to user data directory
                SetUserDataDirectory();
            }
        }

        public static void SetUserDataDirectory()
        {
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            dir = System.IO.Path.Combine(dir, AppGlobals.LocalDirectoryPath);
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }
        }

        public static string ValidateDirectory(string path)
        {
            try
            {
                if (!System.IO.Path.IsPathRooted(path)) path = Path + "\\" + path;
                DirectoryInfo directory = new DirectoryInfo(path);
                if (!directory.Exists)
                {
                    directory.Create();
                    return $"Directory Created: {directory.FullName}";
                }
                return $"Diretory Found: {directory.FullName}";
            }
            catch (Exception ex)
            {
                throw new Exception($"Error Setting Directory: {ex.Message}");
            }
        }
    }