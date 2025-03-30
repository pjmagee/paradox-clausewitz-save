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
        
        var gamestate = Gamestate.Bind(document.Root);
        Assert.IsNotNull(gamestate);
        Assert.IsTrue(gamestate.Achievement!.Count > 0, "Achievements should have items");

        Assert.AreEqual(191, gamestate.Achievement.Max());
        Assert.AreEqual(22, gamestate.Achievement.Min());
    }
    
    [TestMethod]
    public void GameState_Binding_Country()
    {
        var document = GameStateDocument.Parse(new FileInfo(Path.Combine("Stellaris", "TestData", "country.so")));
        Assert.IsNotNull(document);
        Assert.IsNotNull(document.Root);

        var gamestate = Gamestate.Bind(document.Root);

        Assert.IsNotNull(gamestate!.Country);
        Assert.IsTrue(gamestate.Country.Count > 0, "Country should have items");
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
        """);

        var intelmanager = Gamestate.GamestateCountryitem.GamestateCountryitemIntelmanager.Bind(document.Root);

        Assert.IsNotNull(intelmanager);
        Assert.IsNotNull(intelmanager.Intel);

        // Check for the expected keys
        Assert.IsTrue(intelmanager.Intel.ContainsKey(67108916));
        Assert.IsTrue(intelmanager.Intel.ContainsKey(218103860));
        
        // Verify the items exist
        var item1 = intelmanager.Intel[67108916];
        var item2 = intelmanager.Intel[218103860];
        
        Assert.IsNotNull(item1);
        Assert.IsNotNull(item2);
        
        // Ensure the Intel property is defined correctly and values can be accessed
        Assert.IsNotNull(item1.Intel);
        Assert.IsNotNull(item2.Intel);
        
        // Verify both values are now typed as float despite one being originally an int
        // To verify this, we'll compare the runtime types of the values
        Assert.IsTrue(item1.Intel is float, "Intel property for floating-point value should be of type float");
        Assert.IsTrue(item2.Intel is float, "Intel property for integer value should also be of type float");

        // Also verify the actual values are preserved correctly
        Assert.AreEqual(10.123f, (float)item1.Intel, 0.0001f);
        Assert.AreEqual(10.0f, (float)item2.Intel, 0.0001f);
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

        var intelmanager = Gamestate.GamestateCountryitem.GamestateCountryitemIntelmanager.Bind(document.Root);
        Assert.IsNotNull(intelmanager);
        
        // Get the intel item with ID 14
        var item = intelmanager.Intel[14];
        Assert.IsNotNull(item);
        
        // Get the StaleIntel object
        var staleIntel = item.StaleIntel;
        Assert.IsNotNull(staleIntel);
        
        // Access all the properties directly to confirm they exist in the generated model
        // This will cause a compilation error if the properties don't exist in the generated class
        
        // Check RelativeEconomy
        var relativeEconomy = staleIntel.RelativeEconomy;
        Assert.IsNotNull(relativeEconomy);
        
        // Check the properties on RelativeEconomy
        Assert.AreEqual(1.65469f, relativeEconomy.RelativePower, 0.0001f);
        Assert.AreEqual(0.60431f, relativeEconomy.ReverseRelativePower, 0.0001f);
        
        // Check IntelTechRelativePower
        var intelTechRelativePower = staleIntel.IntelTechRelativePower;
        Assert.IsNotNull(intelTechRelativePower);
        
        // Check the properties on IntelTechRelativePower
        Assert.AreEqual(1.46497f, intelTechRelativePower.RelativePower, 0.0001f);
        Assert.AreEqual(0.68259f, intelTechRelativePower.ReverseRelativePower, 0.0001f);
        
        // Check RelativeFleet
        var relativeFleet = staleIntel.RelativeFleet;
        Assert.IsNotNull(relativeFleet);
        
        // Check the properties on RelativeFleet
        Assert.AreEqual(2.30643f, relativeFleet.RelativePower, 0.0001f);
        Assert.AreEqual(0.43356f, relativeFleet.ReverseRelativePower, 0.0001f);
    }
}