using MageeSoft.Paradox.Clausewitz.SaveReader.Model;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Tests.Stellaris.Models;

[TestClass]
public class AchievementsTests
{
    [TestMethod]
    public void Binder_AchievementsTest()
    {
        var document = GameStateDocument.Parse(new FileInfo(Path.Combine("Stellaris", "TestData", "achievement.so")));
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Root);

        var achievements = Binder.Bind<Achievements>(document.Root);
        Assert.IsNotNull(achievements);
        Assert.IsTrue(achievements.Values.Count > 0, "Achievements should have items");

        Assert.AreEqual(191, achievements.Values.Max());
        Assert.AreEqual(22, achievements.Values.Min());
    }
}