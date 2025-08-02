using GenieWasm;
using GenieWasm.UserControls;

namespace TestGenieWasm;

[TestClass]
public class TestViewManager
{
    [TestMethod]
    public void CanInstantiateDefaultWindows()
    {
        ViewManager manager = ViewManager.Instance;
        IEnumerable<GameWindow> gameWindows = manager.InitializeDefaultWindows(true);
        Assert.IsTrue(gameWindows.Count() > 1,"Expected more than one default game window to be instantiated.");
        // See what happens if you re-add them- should remove the existing entry before adding again
        IEnumerable<GameWindow> gameWindows2 = manager.InitializeDefaultWindows(true);
        Assert.IsTrue(gameWindows2.Count() == gameWindows.Count(), "Expected same number of windows created as before.");
    }
}
