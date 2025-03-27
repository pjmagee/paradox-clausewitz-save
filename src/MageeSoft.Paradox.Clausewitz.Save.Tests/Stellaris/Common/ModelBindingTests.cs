using MageeSoft.Paradox.Clausewitz.Save.Binder.Reflection;
using MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;
using MageeSoft.Paradox.Clausewitz.Save.Reader;

namespace MageeSoft.Paradox.Clausewitz.Save.Tests.Stellaris.Common;

[TestClass]
public class ModelBindingTests
{
    [TestMethod]
    public void Reflection_Binder_AchievementsTest()
    {
        var document = GameStateDocument.Parse(new FileInfo(Path.Combine("Stellaris", "TestData", "achievement.so")));
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Root);

        var achievements = ReflectionBinder.Bind<Achievements>(document.Root);
        Assert.IsNotNull(achievements);
        Assert.IsTrue(achievements.Values!.Count > 0, "Achievements should have items");

        Assert.AreEqual(191, achievements.Values.Max());
        Assert.AreEqual(22, achievements.Values.Min());
    }
    
    [TestMethod]
    public void SourceGen_Binder_AchievementsTest()
    {
        var document = GameStateDocument.Parse(new FileInfo(Path.Combine("Stellaris", "TestData", "achievement.so")));
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Root);

        var achievements = Achievements.Bind(document.Root);
        Assert.IsNotNull(achievements);
        Assert.IsTrue(achievements.Values!.Count > 0, "Achievements should have items");

        Assert.AreEqual(191, achievements.Values.Max());
        Assert.AreEqual(22, achievements.Values.Min());
    }
    
    [TestMethod]
    public void SourceGen_Binder_CountryTest()
    {
        var document = GameStateDocument.Parse(new FileInfo(Path.Combine("Stellaris", "TestData", "country.so")));
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Root);

        var country = Country.Bind(document.Root);
        Assert.IsNotNull(country);
    }
}