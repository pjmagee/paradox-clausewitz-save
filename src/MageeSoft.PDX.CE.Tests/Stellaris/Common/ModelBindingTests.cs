using MageeSoft.PDX.CE.Models;
using MageeSoft.PDX.CE.Reader;

namespace MageeSoft.PDX.CE.Tests.Stellaris.Common;

[TestClass]
public class ModelBindingTests
{   
    
    [TestMethod]
    public void SourceGen_Binder_AchievementsTest()
    {
        var document = GameStateDocument.Parse(new FileInfo(Path.Combine("Stellaris", "TestData", "achievement.so")));
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Root);
        
        var gamestate = Gamestate.Bind(document.Root);
        Assert.IsNotNull(gamestate);
        Assert.IsTrue(gamestate.Achievement!.Count > 0, "Achievements should have items");

        Assert.AreEqual(191, gamestate.Achievement.Max());
        Assert.AreEqual(22, gamestate.Achievement.Min());
    }
    
    [TestMethod]
    public void SourceGen_Binder_CountryTest()
    {
        var document = GameStateDocument.Parse(new FileInfo(Path.Combine("Stellaris", "TestData", "country.so")));
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Root);

        var gamestate = Gamestate.Bind(document.Root);

        Assert.IsNotNull(gamestate!.Country);
        Assert.IsTrue(gamestate.Country.Count > 0, "Country should have items");
    }
}