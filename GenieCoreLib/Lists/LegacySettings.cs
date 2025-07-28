using System.Reflection;

namespace GenieCoreLib;

public partial class ConfigSettings
{
    public static string SetSetting(string sKey, string sValue, ConfigSettings settings,
        List<PropertyInfo> properties)
    {
        if (sKey.Length == 0 || sValue == null)
        {
            return "Key or value cannot be empty.";
        }
        string errors = string.Empty;
        // Set a property based on its name... return error message if it fails
        // Use reflection to get the property by name and it's type
        PropertyInfo propertyInfo = properties.FirstOrDefault(p=>p.Name.Equals(sKey, StringComparison.OrdinalIgnoreCase));
        if (propertyInfo is null)
        {
            return $"Property '{sKey}' not found in ConfigSettings.";
        }
        // Check if the property is writable
        if (!propertyInfo.CanWrite)
        {
            return $"Property '{sKey}' is read-only and cannot be set.";
        }
        try
        {
            switch (propertyInfo.PropertyType)
            {
                case Type t when t == typeof(char):
                    char cValue = Conversions.ToChar(sValue.ToCharArray().GetValue(0));
                    propertyInfo.SetValue(settings, cValue);
                    break;
                case Type t when t == typeof(bool):
                    bool bValue = sValue.ToLower() switch
                    {
                        "on" => true,
                        "true" => true,
                        "1" => true,
                        _ => false
                    };
                    propertyInfo.SetValue(settings, bValue);
                    break;
                case Type t when t == typeof(int):
                    int iValue = Utility.StringToInteger(sValue);
                    propertyInfo.SetValue(settings, iValue);
                    break;
                case Type t when t == typeof(double):
                    double dValue = Utility.StringToDouble(sValue);
                    propertyInfo.SetValue(settings, dValue);
                    break;
                case Type t when t == typeof(string):
                    propertyInfo.SetValue(settings, sValue);
                    break;
                default:
                    return "Unknown property type: " + propertyInfo.PropertyType.Name;
            }
        }
        catch (Exception ex)
        {
            errors += $"Error setting property '{sKey}': {ex.Message}\n";
        }
        return errors;
    }
    private static string SetPropertyValue(PropertyInfo propertyInfo, string sValue, ConfigSettings settings)
    {
        try
        {
            propertyInfo.SetValue(settings, Convert.ChangeType(sValue, propertyInfo.PropertyType), null);
        }
        catch (Exception ex)
        {
            return $"Couldn't set property {propertyInfo.Name} to value {sValue}";
        }
        return string.Empty;
    }
}