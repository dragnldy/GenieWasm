using GenieCoreLib;
using static GenieCoreLib.CommandQueue;
[assembly: DoNotParallelize]

namespace TestGenieWasm;

[TestClass]
public class TestGlobals
{
    #region Testing Presets
    [TestMethod]
    public void CanLoadGlobalPresets()
    {
        string variables = "presets";
        LoadPresets(variables);
        Assert.IsTrue(Presets.Instance.Count > 0, $"Custom {variables} should have been loaded successfully.");
    }
    internal void LoadPresets(string variables)
    {
        GenieWasm.Desktop.LocalSetup.InitLocalDirectory();
        string path = AppGlobals.LocalDirectoryPath;
        Presets Presetlist = Presets.Instance;
        // Check to make sure defaults installed
        Assert.IsTrue(Presetlist.Count > 0, $"Default {variables} should have been initialized");
        // Check to make sure can load the existing files
        Presetlist.Clear(false);
        Assert.IsTrue(Presetlist.Count == 0, $"Default {variables} should NOT have been initialized");
        Assert.IsTrue(Presetlist.Load()); // should supply the default filename and load

    }
    [TestMethod]
    public void CanSaveGlobalPresets()
    {
        string variables = "presets";
        LoadPresets(variables);
        Assert.IsTrue(Presets.Instance.Save($"{variables}.test.cfg"));
    }
    [TestMethod]
    public void CanListGlobalPresets()
    {   string variables = "presets";
        LoadPresets(variables);
        string allvars = Presets.Instance.ListAll("");
        Assert.IsTrue(allvars.Length > 0, $"Custom {variables} should have been listed successfully.");
        allvars = Command.Instance.ListPresets("");
        Assert.IsTrue(allvars.Length > 0, $"Custom {variables} should have been listed successfully.");
        string justwhispers = Command.Instance.ListPresets("whispers");
        Assert.IsTrue(justwhispers.Length > 0, $"Custom {variables} 'Whispers' should have been listed successfully.");
    }
    #endregion Presets

    #region Aliases
    [TestMethod]
    public void CanLoadGlobalAliases()
    {
        string variables = "aliases";
        LoadAliases(variables);
        Assert.IsTrue(Aliases.Instance.Count > 0, $"Custom {variables} should have been loaded successfully.");
    }
    internal void LoadAliases(string variables)
    {
        GenieWasm.Desktop.LocalSetup.InitLocalDirectory();
        string path = AppGlobals.LocalDirectoryPath;
        Aliases AliasList = Aliases.Instance;
        Assert.IsTrue(AliasList.Load()); // should supply the default filename and load
    }
    [TestMethod]
    public void CanSaveGlobalAliases()
    {
        string variables = "aliases";
        LoadAliases(variables);
        Assert.IsTrue(Aliases.Instance.Save($"{variables}.test.cfg"));
    }
    [TestMethod]
    public void CanListGlobalAliases()
    {
        string variables = "Aliases";
        LoadAliases(variables);
        string allvars = Aliases.Instance.ListAll("");
        Assert.IsTrue(allvars.Length > 0, $"Custom {variables} should have been listed successfully.");
        allvars = Command.Instance.ListAliases("");
        Assert.IsTrue(allvars.Length > 0, $"Custom {variables} should have been listed successfully.");
        string filtered = Command.Instance.ListAliases("oy");
        Assert.IsTrue(filtered.Length > 0, $"Custom {variables} 'oy' should have been listed successfully.");
    }
    #endregion Aliases

    #region Names
    [TestMethod]
    public void CanLoadGlobalNames()
    {
        string variables = "Names";
        LoadNames(variables);
        Assert.IsTrue(Names.Instance.Count > 0, $"Custom {variables} should have been loaded successfully.");
    }
    internal void LoadNames(string variables)
    {
        GenieWasm.Desktop.LocalSetup.InitLocalDirectory();
        string path = AppGlobals.LocalDirectoryPath;
        Names NameList = Names.Instance;
        // No defaults to initialize
        Assert.IsTrue(NameList.Count == 0, $"Default {variables} should have NOT been initialized");
        Assert.IsTrue(NameList.Load()); // should supply the default filename and load
    }
    [TestMethod]
    public void CanSaveGlobalNames()
    {
        string variables = "Names";
        LoadNames(variables);
        Assert.IsTrue(Names.Instance.Save($"{variables}.test.cfg"));
    }
    [TestMethod]
    public void CanListGlobalNames()
    {
        string variables = "Names";
        LoadNames(variables);
        string allvars = Names.Instance.ListAll("");
        Assert.IsTrue(allvars.Length > 0, $"Custom {variables} should have been listed successfully.");
        allvars = Command.Instance.ListNames("");
        Assert.IsTrue(allvars.Length > 0, $"Custom {variables} should have been listed successfully.");
        string filtered = Command.Instance.ListNames("Zephyria");
        Assert.IsTrue(filtered.Length > 0, $"Custom {variables} 'Zephyria' should have been listed successfully.");
    }
    #endregion Names

    #region Macros
    [TestMethod]
    public void CanLoadGlobalMacros()
    {
        string variables = "Macros";
        LoadMacros(variables);
        Assert.IsTrue(Macros.Instance.Count > 0, $"Custom {variables} should have been loaded successfully.");
    }
    internal void LoadMacros(string variables)
    {
        GenieWasm.Desktop.LocalSetup.InitLocalDirectory();
        string path = AppGlobals.LocalDirectoryPath;
        Macros MacroList = Macros.Instance;
        // No defaults to initialize
        Assert.IsTrue(MacroList.Count == 0, $"Default {variables} should have NOT been initialized");
        Assert.IsTrue(MacroList.Load()); // should supply the default fileMacro and load
    }
    [TestMethod]
    public void CanSaveGlobalMacros()
    {
        string variables = "Macros";
        LoadMacros(variables);
        Assert.IsTrue(Macros.Instance.Save($"{variables}.test.cfg"));
    }
    [TestMethod]
    public void CanListGlobalMacros()
    {
        string variables = "Macros";
        LoadMacros(variables);
        string allvars = Macros.Instance.ListAll("");
        Assert.IsTrue(allvars.Length > 0, $"Custom {variables} should have been listed successfully.");
        allvars = Command.Instance.ListMacros("");
        Assert.IsTrue(allvars.Length > 0, $"Custom {variables} should have been listed successfully.");
        string filtered = Command.Instance.ListMacros("Escape");
        Assert.IsTrue(filtered.Length > 0, $"Custom {variables} 'Escape' should have been listed successfully.");
    }
    #endregion Macros

    #region Classes
    [TestMethod]
    public void CanLoadGlobalClasses()
    {
        string variables = "Classes";
        LoadClasses(variables);
        Assert.IsTrue(Classes.Instance.Count > 0, $"Custom {variables} should have been loaded successfully.");
    }
    internal void LoadClasses(string variables)
    {
        GenieWasm.Desktop.LocalSetup.InitLocalDirectory();
        string path = AppGlobals.LocalDirectoryPath;
        Classes ClassList = Classes.Instance;
        // No defaults to initialize
        Assert.IsTrue(ClassList.Count == 0, $"Default {variables} should have NOT been initialized");
        Assert.IsTrue(ClassList.Load()); // should supply the default fileMacro and load
    }
    [TestMethod]
    public void CanSaveGlobalClasses()
    {
        string variables = "Classes";
        LoadClasses(variables);
        Assert.IsTrue(Classes.Instance.Save($"{variables}.test.cfg"));
    }
    [TestMethod]
    public void CanListGlobalClasses ()
    {
        string variables = "Classes";
        LoadClasses(variables);
        string allvars = Classes.Instance.ListAll("");
        allvars = Command.Instance.ListClasses("");
        Assert.IsTrue(allvars.Length > 0, $"Custom {variables} should have been listed successfully.");
        string filtered = Command.Instance.ListClasses("default");
        Assert.IsFalse(filtered.Contains("None"), $"Custom {variables} 'default' should have been listed successfully.");
    }
    #endregion Classes

    #region Triggers
    [TestMethod]
    public void CanLoadGlobalTriggers()
    {
        string variables = "Triggers";
        LoadTriggers(variables);
        Assert.IsTrue(Triggers.Instance.Count > 0, $"Custom {variables} should have been loaded successfully.");
    }
    internal void LoadTriggers(string variables)
    {
        GenieWasm.Desktop.LocalSetup.InitLocalDirectory();
        string path = AppGlobals.LocalDirectoryPath;
        Triggers TriggerList = Triggers.Instance;
        // No defaults to initialize
        Assert.IsTrue(TriggerList.Count == 0, $"Default {variables} should have NOT been initialized");
        Assert.IsTrue(TriggerList.Load()); // should supply the default file
    }   
    [TestMethod]
    public void CanSaveGlobalTriggers()
    {
        string variables = "Triggers";
        LoadTriggers(variables);
        Assert.IsTrue(Triggers.Instance.Save($"{variables}.test.cfg"));
    }
    [TestMethod]
    public void CanListGlobalTriggers()
    {
        string variables = "Triggers";
        LoadTriggers(variables);
        string allvars = Triggers.Instance.ListAll("");
        Assert.IsTrue(allvars.Length > 0, $"Custom {variables} should have been listed successfully.");
        allvars = Command.Instance.ListTriggers("");
        Assert.IsTrue(allvars.Length > 0, $"Custom {variables} should have been listed successfully.");
        string filtered = Command.Instance.ListTriggers("Your spell pattern snaps");
        Assert.IsTrue(filtered.Length > 0, $"Custom {variables} 'Escape' should have been listed successfully.");
    }
    #endregion Triggers

    #region Gags
    [TestMethod]
    public void CanLoadGlobalGags()
    {
        string variables = "Gags";
        LoadGags(variables);
        Assert.IsTrue(GagRegExp.Instance.Count > 0, $"Custom {variables} should have been loaded successfully.");
    }
    internal void LoadGags(string variables)
    {
        GenieWasm.Desktop.LocalSetup.InitLocalDirectory();
        string path = AppGlobals.LocalDirectoryPath;
        GagRegExp GagList = GagRegExp.Instance;
        // No defaults to initialize
        Assert.IsTrue(GagList.Count == 0, $"Default {variables} should have NOT been initialized");
        Assert.IsTrue(GagList.Load()); // should supply the default file and load
    }
    [TestMethod]
    public void CanSaveGlobalGags()
    {
        string variables = "Gags";
        LoadGags(variables);
        Assert.IsTrue(GagRegExp.Instance.Save($"{variables}.test.cfg"));
    }

    [TestMethod]
    public void CanListGlobalGags()
    {
        string variables = "Gags";
        LoadGags(variables);
        string filtered = Command.Instance.ListGags("roots around");
        Assert.IsTrue(filtered.Length > 0, $"Custom {variables} 'Escape' should have been listed successfully.");
        string filtered2 = Command.Instance.ListGags("junk");
        Assert.IsTrue(filtered2.Length > 0, $"Custom {variables} 'Escape' should NOT have been listed successfully.");
        string allvars = GagRegExp.Instance.ListAll("");
        Assert.IsTrue(allvars.Length > 0, $"Custom {variables} should have been listed successfully.");
        allvars = Command.Instance.ListGags("");
        Assert.IsTrue(allvars.Length > 0, $"Custom {variables} should have been listed successfully.");
    }
    #endregion Gags

    #region Variables
    [TestMethod]
    public void CanLoadGlobalVariables()
    {
        string variables = "Variables";
        int iDefaults = LoadVariables(variables);
        Assert.IsTrue(Variables.Instance.Count > iDefaults, $"Custom {variables} should have been loaded successfully.");
    }
    internal int LoadVariables(string variables)
    {
        GenieWasm.Desktop.LocalSetup.InitLocalDirectory();
        string path = AppGlobals.LocalDirectoryPath;
        Variables VariableList = Variables.Instance;
        int iDefault = VariableList.Count; // this is the number of defaults just loaded
        // No defaults to initialize
        Assert.IsTrue(VariableList.Count > 0, $"Default {variables} should have been initialized");
        Assert.IsTrue(VariableList.Load()); // should supply the default filename and load
        return iDefault;
    }

    [TestMethod]
    public void CanSaveGlobalVariables()
    {
        string variables = "Variables";
        int iDefault = LoadVariables(variables);
        Assert.IsTrue(Variables.Instance.Load()); // should supply the default filename and load
        Assert.IsTrue(Variables.Instance.Count > iDefault, $"Custom {variables} should have been loaded successfully.");
        Assert.IsTrue(Variables.Instance.Save($"{variables}.test.cfg"));
    }
    [TestMethod]
    public void CanListGlobalVariables()
    {
        string variables = "Variables";
        LoadVariables(variables);
        string allvars = Variables.Instance.ListAll("");
        Assert.IsTrue(allvars.Length > 0, $"Custom {variables} should have been listed successfully.");
        allvars = Command.Instance.ListVariables("");
        Assert.IsTrue(allvars.Length > 0, $"Custom {variables} should have been listed successfully.");
        string filtered = Command.Instance.ListVariables("Your spell pattern snaps");
        Assert.IsTrue(filtered.Length > 0, $"Custom {variables} 'Escape' should have been listed successfully.");
        string filtered2 = Command.Instance.ListVariables("junkxx");
        Assert.IsTrue(filtered2.Length > 0, $"Custom {variables} 'Escape' should have been listed successfully.");
    }
    #endregion Variables


    #region Events
    [TestMethod]
    public void CanLoadGlobalEvents()
    {
        string variables = "Events";
        LoadEvents(variables);
        Assert.IsTrue(QueueList.Instance.Count > 0, $"Custom {variables} should have been loaded successfully.");
    }
    internal void LoadEvents(string variables)
    {
        // There is nothing in a file that represents an event- they are memory only
        QueueList queueList = QueueList.Instance;
        // No defaults to initialize
        Assert.IsTrue(queueList.Count == 0, $"Default {variables} should have NOT been initialized");
        queueList.Add(1000.0, "move", new CommandRestrictions(false, true, true));
        queueList.Add(2000.0, "look", new CommandRestrictions(true, false, true));
        queueList.Add(3000.0, "forage", new CommandRestrictions(true, true, false));
    }
    [TestMethod]
    public void CanListGlobalEvents()
    {
        string variables = "Events";
        LoadEvents(variables);
        string filtered = Command.Instance.ListEvents("look");
        Assert.IsTrue(filtered.Length > 0, $"Custom {variables} 'Escape' should have been listed successfully.");
        string filtered2 = Command.Instance.ListEvents("junk");
        Assert.IsTrue(filtered2.Contains("None"), $"Custom {variables} 'Escape' should NOT have been listed successfully.");
        string allvars = QueueList.Instance.ListAll("");
        Assert.IsTrue(allvars.Length > 0, $"Custom {variables} should have been listed successfully.");
        allvars = Command.Instance.ListEvents("");
        Assert.IsTrue(allvars.Length > 0, $"Custom {variables} should have been listed successfully.");
    }
    #endregion Events

    #region Substitutes
    [TestMethod]
    public void CanLoadGlobalSubstitutes()
    {
        string variables = "Substitutes";
        LoadSubstitutes(variables);
        Assert.IsTrue(SubstituteRegExp.Instance.Count > 0, $"Custom {variables} should have been loaded successfully.");
    }
    internal void LoadSubstitutes(string variables)
    {
        GenieWasm.Desktop.LocalSetup.InitLocalDirectory();
        string path = AppGlobals.LocalDirectoryPath;
        SubstituteRegExp SubstituteList = SubstituteRegExp.Instance;
        // No defaults to initialize
        Assert.IsTrue(SubstituteList.Count == 0, $"Default {variables} should have NOT been initialized");
        Assert.IsTrue(SubstituteList.Load()); // should supply the default file and load
    }
    [TestMethod]
    public void CanSaveGlobalSubstitutes()
    {
        string variables = "Substitutes";
        LoadSubstitutes(variables);
        Assert.IsTrue(SubstituteRegExp.Instance.Save($"{variables}.test.cfg"));
    }

    [TestMethod]
    public void CanListGlobalSubstitutes()
    {
        string variables = "Substitutes";
        LoadSubstitutes(variables);
        string filtered = Command.Instance.ListSubstitutes("stow");
        Assert.IsTrue(filtered.Length > 0, $"Custom {variables} 'stow' should have been listed successfully.");
        string filtered2 = Command.Instance.ListSubstitutes("junk");
        Assert.IsTrue(filtered2.Contains("None"), $"Custom {variables} 'junk' should not have been listed successfully.");
        string allvars = SubstituteRegExp.Instance.ListAll("");
        Assert.IsTrue(allvars.Length > 0, $"Custom {variables} should have been listed successfully.");
        allvars = Command.Instance.ListSubstitutes("");
        Assert.IsTrue(allvars.Length > 0, $"Custom {variables} should have been listed successfully.");
    }
    #endregion Substitutes

    #region Testing Hightlights
    [TestMethod]
    public void CanLoadGlobalHightlights()
    {
        string variables = "Highlights";
        LoadHighlights(variables);
        Assert.IsTrue(HighlightsList.Instance.Count > 0, $"Custom {variables} should have been loaded successfully.");
    }
    internal void LoadHighlights(string variables)
    {
        GenieWasm.Desktop.LocalSetup.InitLocalDirectory();
        string path = AppGlobals.LocalDirectoryPath;
        HighlightsList highlightlist = HighlightsList.Instance;
        // Check to make sure can load the existing files
        Assert.IsTrue(highlightlist.Load()); // should supply the default filename and load

    }
    [TestMethod]
    public void CanSaveGlobalHightlights()
    {
        string variables = "Hightlights";
        LoadHighlights(variables);
        Assert.IsTrue(HighlightsList.Instance.Save($"{variables}.test.cfg"));
    }
    [TestMethod]
    public void CanListGlobalHightlights()
    {
        string variables = "Hightlights";
        LoadHighlights(variables);
        string filtered = Command.Instance.ListHighlights("you open");
        Assert.IsTrue(filtered.Length > 0, $"Custom {variables} 'Whispers' should have been listed successfully.");
        string filtered2 = Command.Instance.ListHighlights("junkxx");
        Assert.IsTrue(filtered2.Contains("None"), $"Custom {variables} 'junk' should NOT have been listed successfully.");
        string allvars = HighlightsList.Instance.ListAll("");
        Assert.IsTrue(allvars.Length > 0, $"Custom {variables} should have been listed successfully.");
        allvars = Command.Instance.ListHighlights("");
        Assert.IsTrue(allvars.Length > 0, $"Custom {variables} should have been listed successfully.");
    }
    #endregion Hightlights

}
