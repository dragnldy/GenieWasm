using GenieCoreLib;

namespace TestGenieWasm;

[TestClass]
public class TestGlobals
{
    [TestMethod]
    public void CanLoadGlobalPresets()
    {
        string variables = "presets";
        GenieWasm.Desktop.LocalSetup.InitLocalDirectory();
        string path = AppGlobals.LocalDirectoryPath;
        Presets Presetlist = Presets.Instance;
        // Check to make sure defaults installed
        Assert.IsTrue(Presetlist.Count > 0, $"Default {variables} should have been initialized");
        // Check to make sure can load the existing files
        Presetlist.Clear(false);
        Assert.IsTrue(Presetlist.Count == 0, $"Default {variables} should NOT have been initialized");
        Assert.IsTrue(Presetlist.Load()); // should supply the default filename and load
        Assert.IsTrue(Presetlist.Count > 0, $"Custom {variables} should have been loaded successfully.");
    }
    [TestMethod]
    public void CanSaveGlobalPresets()
    {
        string variables = "presets";
        GenieWasm.Desktop.LocalSetup.InitLocalDirectory();
        string path = AppGlobals.LocalDirectoryPath;
        Presets Presetlist = Presets.Instance;
        // Check to make sure defaults installed
        Assert.IsTrue(Presetlist.Count > 0, $"Default {variables} should have been initialized");
        Assert.IsTrue(Presetlist.Save($"{variables}.test.cfg"));
    }

    [TestMethod]
    public void CanLoadGlobalAliases()
    {
        string variables = "aliases";
        GenieWasm.Desktop.LocalSetup.InitLocalDirectory();
        string path = AppGlobals.LocalDirectoryPath;
        Aliases AliasList = Aliases.Instance;
        // No defaults to initialize
        Assert.IsTrue(AliasList.Count == 0, $"Default {variables} should have NOT been initialized");
        Assert.IsTrue(AliasList.Load()); // should supply the default filename and load
        Assert.IsTrue(AliasList.Count > 0, $"Custom {variables} should have been loaded successfully.");
    }
    [TestMethod]
    public void CanSaveGlobalAliases()
    {
        string variables = "aliases";
        GenieWasm.Desktop.LocalSetup.InitLocalDirectory();
        string path = AppGlobals.LocalDirectoryPath;
        Aliases Aliaslist = Aliases.Instance;
        // Check to make sure we have some installed
        Assert.IsTrue(Aliaslist.Load());
        Assert.IsTrue(Aliaslist.Count > 0, $"Default {variables} should have been initialized");
        Assert.IsTrue(Aliaslist.Save($"{variables}.test.cfg"));
    }
    [TestMethod]
    public void CanLoadGlobalNames()
    {
        string variables = "Names";
        GenieWasm.Desktop.LocalSetup.InitLocalDirectory();
        string path = AppGlobals.LocalDirectoryPath;
        Names NameList = Names.Instance;
        // No defaults to initialize
        Assert.IsTrue(NameList.Count == 0, $"Default {variables} should have NOT been initialized");
        Assert.IsTrue(NameList.Load()); // should supply the default filename and load
        Assert.IsTrue(NameList.Count > 0, $"Custom {variables} should have been loaded successfully.");
    }
    [TestMethod]
    public void CanSaveGlobalNames()
    {
        string variables = "Names";
        GenieWasm.Desktop.LocalSetup.InitLocalDirectory();
        string path = AppGlobals.LocalDirectoryPath;
        Names NameList = Names.Instance;
        // Check to make sure we have some installed
        Assert.IsTrue(NameList.Load());
        Assert.IsTrue(NameList.Count > 0, $"Default {variables} should have been initialized");
        Assert.IsTrue(NameList.Save($"{variables}.test.cfg"));
    }
    [TestMethod]
    public void CanLoadGlobalMacros()
    {
        string variables = "Macros";
        GenieWasm.Desktop.LocalSetup.InitLocalDirectory();
        string path = AppGlobals.LocalDirectoryPath;
        Macros MacroList = Macros.Instance;
        // No defaults to initialize
        Assert.IsTrue(MacroList.Count == 0, $"Default {variables} should have NOT been initialized");
        Assert.IsTrue(MacroList.Load()); // should supply the default fileMacro and load
        Assert.IsTrue(MacroList.Count > 0, $"Custom {variables} should have been loaded successfully.");
    }
    [TestMethod]
    public void CanSaveGlobalMacros()
    {
        string variables = "Macros";
        GenieWasm.Desktop.LocalSetup.InitLocalDirectory();
        string path = AppGlobals.LocalDirectoryPath;
        Macros MacroList = Macros.Instance;
        // Check to make sure we have some installed
        Assert.IsTrue(MacroList.Load());
        Assert.IsTrue(MacroList.Count > 0, $"Default {variables} should have been initialized");
        Assert.IsTrue(MacroList.Save($"{variables}.test.cfg"));
    }
    [TestMethod]
    public void CanLoadGlobalClasses()
    {
        string variables = "Classes";
        GenieWasm.Desktop.LocalSetup.InitLocalDirectory();
        string path = AppGlobals.LocalDirectoryPath;
        Classes ClassList = Classes.Instance;
        // No defaults to initialize
        Assert.IsTrue(ClassList.Count == 0, $"Default {variables} should have NOT been initialized");
        Assert.IsTrue(ClassList.Load()); // should supply the default fileMacro and load
        Assert.IsTrue(ClassList.Count > 0, $"Custom {variables} should have been loaded successfully.");
    }
    [TestMethod]
    public void CanSaveGlobalClasses()
    {
        string variables = "Classes";
        GenieWasm.Desktop.LocalSetup.InitLocalDirectory();
        string path = AppGlobals.LocalDirectoryPath;
        Classes ClassList = Classes.Instance;
        // Check to make sure we have some installed
        Assert.IsTrue(ClassList.Load());
        Assert.IsTrue(ClassList.Count > 0, $"Default {variables} should have been initialized");
        Assert.IsTrue(ClassList.Save($"{variables}.test.cfg"));
    }
    [TestMethod]
    public void CanLoadGlobalTriggers()
    {
        string variables = "Triggers";
        GenieWasm.Desktop.LocalSetup.InitLocalDirectory();
        string path = AppGlobals.LocalDirectoryPath;
        Triggers TriggerList = Triggers.Instance;
        // No defaults to initialize
        Assert.IsTrue(TriggerList.Count == 0, $"Default {variables} should have NOT been initialized");
        Assert.IsTrue(TriggerList.Load()); // should supply the default fileMacro and load
        Assert.IsTrue(TriggerList.Count > 0, $"Custom {variables} should have been loaded successfully.");
    }
    [TestMethod]
    public void CanSaveGlobalTriggers()
    {
        string variables = "Triggers";
        GenieWasm.Desktop.LocalSetup.InitLocalDirectory();
        string path = AppGlobals.LocalDirectoryPath;
        Triggers TriggerList = Triggers.Instance;
        // Check to make sure we have some installed
        Assert.IsTrue(TriggerList.Load());
        Assert.IsTrue(TriggerList.Count > 0, $"Default {variables} should have been initialized");
        Assert.IsTrue(TriggerList.Save($"{variables}.test.cfg"));
    }

    [TestMethod]
    public void CanLoadGlobalVariables()
    {
        string variables = "Variables";
        GenieWasm.Desktop.LocalSetup.InitLocalDirectory();
        string path = AppGlobals.LocalDirectoryPath;
        Variables VariableList = Variables.Instance;
        // Check to make sure defaults installed
        Assert.IsTrue(VariableList.Count > 0, $"Default {variables} should have been initialized");
        // Check to make sure can load the existing files
        VariableList.Clear(false);
        Assert.IsTrue(VariableList.Count == 0, $"Default {variables} should NOT have been initialized");
        Assert.IsTrue(VariableList.Load()); // should supply the default filename and load
        Assert.IsTrue(VariableList.Count > 0, $"Custom {variables} should have been loaded successfully.");
    }
    [TestMethod]
    public void CanSaveGlobalVariables()
    {
        string variables = "Variables";
        GenieWasm.Desktop.LocalSetup.InitLocalDirectory();
        string path = AppGlobals.LocalDirectoryPath;
        Variables VariableList = Variables.Instance;
        // Check to make sure defaults installed
        int iDefault = VariableList.Count;
        Assert.IsTrue(VariableList.Count > 0, $"Default {variables} should have been initialized");
        Assert.IsTrue(VariableList.Load()); // should supply the default filename and load
        Assert.IsTrue(VariableList.Count > iDefault, $"Custom {variables} should have been loaded successfully.");
        Assert.IsTrue(VariableList.Save($"{variables}.test.cfg"));
    }


}
