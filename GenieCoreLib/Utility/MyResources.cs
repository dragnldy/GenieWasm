using System.Reflection;
using System.Resources;

namespace GenieCoreLib;

public static class MyResources
{
    public static ResourceManager rm = new ResourceManager("Genie4Lib.Properties.Resources", Assembly.GetExecutingAssembly());

    public static string GetApplicationName()
    {
        return Globals.AppName;
    }
    public static string GetApplicationVersion()
    {
        return Globals.AppVersion;
    }
    public static string StringResource(string resourceName)
    {
        return rm.GetObject(resourceName)?.ToString() ?? string.Empty;
    }
    //public static Image ImageResource(string resourceName)
    //{
    //    //find the resource "Genie4Lib.Properties.Resources.resources" among the resources "GenieClient.ComponentBars.resources", "GenieClient.ComponentIconBar.resources", "Genie4Lib.Forms.Components.ComponentIcons.resources", "GenieClient.ComponentPluginItem.resources", "GenieClient.ComponentRichTextBox.resources", "GenieClient.ComponentRoundtime.resources", "GenieClient.UCAliases.resources", "GenieClient.UCClasses.resources", "GenieClient.UCHighlightStrings.resources", "GenieClient.UCIgnore.resources", ... embedded in the assembly "Genie4Lib", nor among the resources in any satellite assemblies for the specified culture.Perhaps the resources were embedded with an incorrect name.'
        
    //    object resource = rm.GetObject(resourceName);
    //    if ( resource is not null)
    //    {
    //        return (Image)resource;
    //    }
    //    return null;
    //}
    public static object Resources(string resourceName)
    {
        return rm.GetObject(resourceName);
    }
    public static object FormResource(string resourceName, bool createifnotopen = false,bool show = false)
    {
        return null;
    }
    public static object CreateForm(string formType)
    {
        return null;
    }
}
