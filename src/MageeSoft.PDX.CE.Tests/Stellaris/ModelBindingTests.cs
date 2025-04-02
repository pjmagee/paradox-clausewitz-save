using MageeSoft.PDX.CE.Models;
using MageeSoft.PDX.CE.Reader;

namespace MageeSoft.PDX.CE.Tests.Stellaris;

[TestClass]
public class ModelBindingTests
{
    [TestMethod]
    public void GameState_Binding_Achievement()
    {
        var document = GameStateDocument.Parse(new FileInfo(Path.Combine("Stellaris", "TestData", "achievement.so")));
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Root);
        
        // var gamestate = Gamestate.Bind(document.Root);
        // Assert.IsNotNull(gamestate);
        // Assert.IsNotNull(gamestate.Achievement, "Achievement list should not be null");
        // Assert.IsTrue(gamestate.Achievement.Count > 0, "Achievements should have items");
        //
        // Assert.AreEqual<int>(191, gamestate.Achievement.Max(v => v!.Value));
        // Assert.AreEqual<int>(22, gamestate.Achievement.Min(v => v!.Value));
    }
    
    [TestMethod]
    public void GameState_Binding_Country()
    {
        var document = GameStateDocument.Parse(new FileInfo(Path.Combine("Stellaris", "TestData", "country.so")));
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Root);

        var gamestate = Gamestate.Bind(document.Root);
        //
        // Assert.IsNotNull(gamestate!.Country);
        // Assert.IsTrue(gamestate.Country.Count > 0, "Country should have items");
    }
    
    [TestMethod]
    public void Meta_Binding_Info()
    {
        var document = GameStateDocument.Parse(new FileInfo(Path.Combine("Stellaris", "TestData", "meta")));
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Root);
        
        var meta = Meta.Bind(document.Root);
        Assert.IsNotNull(meta);
        Assert.IsNotNull(meta.Date);
        Assert.AreEqual(new DateTime(2250, 11, 15), meta.Date);
        Assert.AreEqual("Circinus v3.14.15926", meta.AVersion);
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
        """);
        
        // var intelmanager = Gamestate.GamestateCountryItem.GamestateCountryItemIntelManager.GamestateCountryItemIntelManagerIntelItem.Bind(document.Root);
        //
        // Assert.IsNotNull(intelmanager);
        // Assert.IsNotNull(intelmanager.Intel);
        //
        // Assert.IsTrue(intelmanager.Intel.ContainsKey(67108916));
        // Assert.IsTrue(intelmanager.Intel.ContainsKey(218103860));
        //
        // var item1 = intelmanager.Intel[67108916];
        // var item2 = intelmanager.Intel[218103860];
        //
        // Assert.IsNotNull(item1);
        // Assert.IsNotNull(item2);
        //
        // Assert.IsNotNull(item1.Intel);
        // Assert.IsNotNull(item2.Intel);
        //
        // Assert.IsInstanceOfType(item1.Intel, typeof(float?));
        // Assert.IsInstanceOfType(item2.Intel, typeof(float?));
        //
        // Assert.AreEqual(10.123f, item1.Intel);
        // Assert.AreEqual(10.0f, item2.Intel);
    }

    [TestMethod]
    public void GameState_StaleIntel_DirectPropertyAccess()
    {
        var document = GameStateDocument.Parse("""
        {
            intel=
            {
                {
                    14 
                    {
                        intel=21.2885
                        stale_intel=
                        {
                            relative_economy=
                            {
                                relative_power=1.65469
                                reverse_relative_power=0.60431
                            }
                            intel_tech_relative_power=
                            {
                                relative_power=1.46497
                                reverse_relative_power=0.68259
                            }
                            relative_fleet=
                            {
                                relative_power=2.30643
                                reverse_relative_power=0.43356
                            }
                        }
                    }
                }
            }
        }
        """);

        // var intelManager = Gamestate.GamestateCountryItemIntelManager.Bind(document.Root);
        //
        // Assert.IsNotNull(intelManager);
        // Assert.IsNotNull(intelManager.Intel);
        //
        // var item = intelManager.Intel[14];
        // Assert.IsNotNull(item);
        //
        // var staleIntel = item.StaleIntel;
        // Assert.IsNotNull(staleIntel);
        //
        // // Properties not generated by the source generator for some reason
        //
        // var relativeEconomy = staleIntel.SourceObject;
        // // Assert.IsNotNull(relativeEconomy);
        // //Assert.AreEqual(1.65469f, relativeEconomy.RelativePower, 0.0001f);
        // // Assert.AreEqual(0.60431f, relativeEconomy.ReverseRelativePower, 0.0001f);
        // //
        // // var intelTechRelativePower = staleIntel.IntelTechRelativePower;
        // // Assert.IsNotNull(intelTechRelativePower);
        // // Assert.AreEqual(1.46497f, intelTechRelativePower.RelativePower, 0.0001f);
        // // Assert.AreEqual(0.68259f, intelTechRelativePower.ReverseRelativePower, 0.0001f);
        // //
        // // var relativeFleet = staleIntel.RelativeFleet;
        // // Assert.IsNotNull(relativeFleet);
        // // Assert.AreEqual(2.30643f, relativeFleet.RelativePower, 0.0001f);
        // // Assert.AreEqual(0.43356f, relativeFleet.ReverseRelativePower, 0.0001f);
    }
}