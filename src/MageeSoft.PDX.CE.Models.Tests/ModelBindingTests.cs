using MageeSoft.PDX.CE.Reader;

namespace MageeSoft.PDX.CE.Models.Tests;

[TestClass]
public class ModelBindingTests
{
    [TestMethod]
    public void GameState_Binding_Achievement()
    {
        // Arrange
        var document = GameStateDocument.Parse(new FileInfo(Path.Combine("Stellaris", "TestData", "achievement.so")));
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Root);

        // Act
        var gamestate = Gamestate.Load(document.Root);

        // Assert
        Assert.IsNotNull(gamestate);
        Assert.IsNotNull(gamestate.Achievement, "Achievement list should not be null");
        Assert.IsTrue(gamestate.Achievement.Count > 0, "Achievements should have items");

        Assert.AreEqual(191, gamestate.Achievement.Max(v => v));
        Assert.AreEqual(22, gamestate.Achievement.Min(v => v));
    }

    [TestMethod]
    public void GameState_Binding_Country()
    {
        // Arrange
        var document = GameStateDocument.Parse(new FileInfo(Path.Combine("Stellaris", "TestData", "country.so")));
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Root);

        // Act
        var gamestate = Gamestate.Load(document.Root);

        // Assert
        Assert.IsNotNull(gamestate!.Country);
        Assert.IsTrue(gamestate.Country.Count > 0, "Country should have items");
    }

    [TestMethod]
    public void Meta_Binding_Info()
    {
        // Arrange
        var document = GameStateDocument.Parse(new FileInfo(Path.Combine("Stellaris", "TestData", "meta")));
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Root);

        // Act
        var meta = Meta.Load(document.Root);

        // Assert
        Assert.IsNotNull(meta);
        Assert.IsNotNull(meta.Date);
        Assert.AreEqual(new DateTime(2250, 11, 15), meta.Date);
        Assert.AreEqual("Circinus v3.14.15926", meta.Version);
        Assert.AreEqual("United Nations of Earth", meta.Name);
        Assert.AreEqual(25, meta.RequiredDlcs!.Count);
        Assert.IsTrue(meta.Ironman);
    }

    [TestMethod]
    public void GameState_Binding_Intel_Manager_Intel()
    {
        var document = GameStateDocument.Parse("""
                                               {
                                                   intel=
                                                   {
                                                       {
                                                           67108916 
                                                           {
                                                               intel=10.123
                                                               stale_intel=
                                                               {
                                                               }
                                                           }
                                                       }
                                                   
                                                       {
                                                           218103860 
                                                           {
                                                               intel=10
                                                               stale_intel=
                                                               {
                                                               }
                                                           }
                                                       }
                                                   }
                                               }
                                               """
        );

        var intelmanager = Gamestate.GamestateCountryValue.GamestateCountryValueIntelManager.Load(document.Root);

        Assert.IsNotNull(intelmanager);
        Assert.IsNotNull(intelmanager.Intel);

        // This is broken!
        Assert.IsTrue(intelmanager.Intel.ContainsKey(67108916));
        Assert.IsTrue(intelmanager.Intel.ContainsKey(218103860));

        var item1 = intelmanager.Intel[67108916];
        var item2 = intelmanager.Intel[218103860];
        Assert.IsNotNull(item1);
        Assert.IsNotNull(item2);

        Assert.IsNotNull(item1.Intel);
        Assert.IsNotNull(item2.Intel);
        Assert.IsInstanceOfType(item1.Intel, typeof(float));
        Assert.IsInstanceOfType(item2.Intel, typeof(float));

        Assert.AreEqual(10.123f, item1.Intel);
        Assert.AreEqual(10.0f, item2.Intel);
    }
}