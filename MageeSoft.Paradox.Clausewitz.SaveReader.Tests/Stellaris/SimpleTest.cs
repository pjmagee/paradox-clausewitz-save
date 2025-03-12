using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Tests.Stellaris;

[TestClass]
public class SimpleTest
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void TestSimpleParse()
    {
        try
        {
            var filePath = Path.Combine("Stellaris", "TestData", "gamestate-market");
            TestContext.WriteLine($"File path: {filePath}");
            TestContext.WriteLine($"File exists: {File.Exists(filePath)}");
            
            if (File.Exists(filePath))
            {
                var fileInfo = new FileInfo(filePath);
                TestContext.WriteLine($"File size: {fileInfo.Length}");
                
                var document = GameStateDocument.Parse(fileInfo);
                Assert.IsNotNull(document);
                Assert.IsNotNull(document.Root);
                
                TestContext.WriteLine("Document parsed successfully");
            }
            else
            {
                Assert.Fail("Test file does not exist");
            }
        }
        catch (Exception ex)
        {
            TestContext.WriteLine($"Exception: {ex.GetType().Name}");
            TestContext.WriteLine($"Message: {ex.Message}");
            TestContext.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    [TestMethod]
    public void TestSimpleArrayParse()
    {
        try
        {
            var filePath = Path.Combine("Stellaris", "TestData", "gamestate-achievement");
            TestContext.WriteLine($"File path: {filePath}");
            TestContext.WriteLine($"File exists: {File.Exists(filePath)}");
            
            if (File.Exists(filePath))
            {
                var fileInfo = new FileInfo(filePath);
                TestContext.WriteLine($"File size: {fileInfo.Length}");
                
                var document = GameStateDocument.Parse(fileInfo);
                Assert.IsNotNull(document);
                Assert.IsNotNull(document.Root);
                
                TestContext.WriteLine("Document parsed successfully");
                
                // Check if the root is a SaveObject
                var root = document.Root as SaveObject;
                Assert.IsNotNull(root, "Root should be a SaveObject");
                
                // Check if there's a single property with key "achievement="
                Assert.AreEqual(1, root.Properties.Length, "Root should have exactly one property");
                var achievementProp = root.Properties[0];
                Assert.AreEqual("achievement", achievementProp.Key, "Property key should be 'achievement'");
                
                // Check if the value is a SaveArray
                var achievementArray = achievementProp.Value as SaveArray;
                Assert.IsNotNull(achievementArray, "Achievement value should be a SaveArray");
                
                // Check if the array has items
                Assert.IsTrue(achievementArray.Items.Length > 0, "Achievement array should have items");
                
                // Output the first few items
                for (int i = 0; i < Math.Min(5, achievementArray.Items.Length); i++)
                {
                    TestContext.WriteLine($"Item {i}: {achievementArray.Items[i]}");
                }
            }
            else
            {
                Assert.Fail("Test file does not exist");
            }
        }
        catch (Exception ex)
        {
            TestContext.WriteLine($"Exception: {ex.GetType().Name}");
            TestContext.WriteLine($"Message: {ex.Message}");
            TestContext.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    [TestMethod]
    public void TestDirectParserArray()
    {
        try
        {
            // Create a simple string with an array
            string input = "test={ 1 2 3 4 5 }";
            TestContext.WriteLine($"Input: {input}");
            
            // Parse it directly
            var parser = new Parser.Parser(input);
            var result = parser.Parse();
            TestContext.WriteLine($"Result type: {result.GetType().Name}");
            
            // Check if the result is a SaveObject
            var root = result as SaveObject;
            Assert.IsNotNull(root, "Root should be a SaveObject");
            TestContext.WriteLine($"Root properties count: {root.Properties.Length}");
            
            // Check if there's a single property with key "test="
            Assert.AreEqual(1, root.Properties.Length, "Root should have exactly one property");
            var testProp = root.Properties[0];
            TestContext.WriteLine($"Property key: {testProp.Key}");
            TestContext.WriteLine($"Property value type: {testProp.Value.GetType().Name}");
            Assert.AreEqual("test", testProp.Key, "Property key should be 'test'");
            
            // Check if the value is a SaveArray
            var testArray = testProp.Value as SaveArray;
            Assert.IsNotNull(testArray, "Test value should be a SaveArray");
            TestContext.WriteLine($"Array items count: {testArray.Items.Length}");
            
            // Check if the array has 5 items
            Assert.AreEqual(5, testArray.Items.Length, "Test array should have 5 items");
            
            // Check the values of the items
            for (int i = 0; i < testArray.Items.Length; i++)
            {
                TestContext.WriteLine($"Item {i} type: {testArray.Items[i].GetType().Name}");
                var item = testArray.Items[i] as Scalar<int>;
                Assert.IsNotNull(item, $"Item {i} should be a Scalar<int>");
                TestContext.WriteLine($"Item {i} value: {item.Value}");
                Assert.AreEqual(i + 1, item.Value, $"Item {i} should have value {i + 1}");
            }
        }
        catch (Exception ex)
        {
            TestContext.WriteLine($"Exception: {ex.GetType().Name}");
            TestContext.WriteLine($"Message: {ex.Message}");
            TestContext.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }
} 