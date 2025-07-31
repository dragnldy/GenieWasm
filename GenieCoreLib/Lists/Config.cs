using System.ComponentModel;
using System.Drawing;
using static GenieCoreLib.Names;

namespace GenieCoreLib;

public interface IConfig
{
}

public class Config: IConfig, INotifyPropertyChanged
{
    public static Config Instance => _m_oConfig  ?? new Config();
    private static Config _m_oConfig;

    public delegate void ConfigChangedEventHandler(ConfigFieldUpdated oField);
    public event PropertyChangedEventHandler? PropertyChanged;


    public Config()
    {
        _m_oConfig = this;
        ConfigSettings.Instance.ConnectString = "FE:GENIE /VERSION:" + MyResources.GetApplicationVersion() + " /P:WIN_XP /XML";
        ConfigSettings.Instance.PropertyChanged +=ConfigSettings_PropertyChanged;
    }

    private void ConfigSettings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, e);
    }
    private void Config_Changed(ConfigFieldUpdated oField)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(oField.ToString()));
    }


    public int[] iPickerColors = new int[17];
    public int[] PickerColors
    {
        get
        {
            if (iPickerColors[0] == 0)
            {
                iPickerColors[0] = ColorCode.ColorToColorref(Color.DimGray);
                iPickerColors[1] = ColorCode.ColorToColorref(Color.DarkRed);
                iPickerColors[2] = ColorCode.ColorToColorref(Color.Green);
                iPickerColors[3] = ColorCode.ColorToColorref(Color.Olive);
                iPickerColors[4] = ColorCode.ColorToColorref(Color.DarkBlue);
                iPickerColors[5] = ColorCode.ColorToColorref(Color.Purple);
                iPickerColors[6] = ColorCode.ColorToColorref(Color.DarkCyan);
                iPickerColors[7] = ColorCode.ColorToColorref(Color.Silver);
                iPickerColors[8] = ColorCode.ColorToColorref(Color.Gray);
                iPickerColors[9] = ColorCode.ColorToColorref(Color.Red);
                iPickerColors[10] = ColorCode.ColorToColorref(Color.Lime);
                iPickerColors[11] = ColorCode.ColorToColorref(Color.Yellow);
                iPickerColors[12] = ColorCode.ColorToColorref(Color.Blue);
                iPickerColors[13] = ColorCode.ColorToColorref(Color.Magenta);
                iPickerColors[14] = ColorCode.ColorToColorref(Color.Cyan);
                iPickerColors[15] = ColorCode.ColorToColorref(Color.WhiteSmoke);
            }

            return iPickerColors;
        }

        set
        {
            iPickerColors = value;
        }
    }
    
    public enum ConfigFieldUpdated
    {
        MonoFont,
        InputFont,
        Reconnect,
        Autolog,
        KeepInput,
        Muted,
        AutoMapper,
        LogDir,
        CheckForUpdates,
        AutoUpdate,
        AutoUpdateLamp,
        ClassicConnect,
        ImagesEnabled,
        SizeInputToGame,
        AlwaysOnTop,
        UpdateMapperScripts
    }
}
