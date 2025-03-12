using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader;
using MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Tests.Stellaris;

[TestClass]
public class FirstContactsTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void FirstContact_ReturnsAllContacts()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-firstcontact"));
        var root = (SaveObject)gameState.Root;

        // Act
        var contacts = FirstContacts.Load(root);

        // Assert
        Assert.IsNotNull(contacts);
        Assert.IsTrue(contacts.Length > 0);
        TestContext.WriteLine($"Found {contacts.Length} first contacts");
    }

    [TestMethod]
    public void FirstContact_ParsesBasicPropertiesCorrectly()
    {
        // Arrange
        var gameState = GameStateDocument.Parse(File.ReadAllText("Stellaris/TestData/gamestate-firstcontact"));
        var root = (SaveObject)gameState.Root;

        // Act
        var contacts = FirstContact.Load(root);
        var firstContact = contacts[0];

        // Assert
        Assert.AreEqual(1, firstContact.Id);
        Assert.AreEqual(1, firstContact.Country);
        Assert.AreEqual(2, firstContact.Target);
        Assert.AreEqual(50, firstContact.Progress);
        Assert.IsTrue(firstContact.IsActive);
    }

    [TestMethod]
    public void FirstContacts_ReturnsAllFirstContacts()
    {
        // Act
        var contacts = StellarisTestData.Save.FirstContacts;

        // Assert
        Assert.IsNotNull(contacts, "Contacts should not be null");
        Assert.IsNotEmpty(contacts, "Contacts should not be empty");

        foreach (var contact in contacts)
        {
            Assert.AreNotEqual(0L, contact.Country, "Country should not be 0");
            Assert.AreNotEqual(0L, contact.Owner, "Owner should not be 0");
            Assert.AreNotEqual(0L, contact.Location, "Location should not be 0");
            Assert.AreNotEqual(0L, contact.Leader, "Leader should not be 0");
            Assert.IsFalse(string.IsNullOrEmpty(contact.Date), "Date should not be empty");
            Assert.IsFalse(string.IsNullOrEmpty(contact.Stage), "Stage should not be empty");
            TestContext.WriteLine($"Testing EventId: '{contact.EventId}'");
            Assert.IsFalse(string.IsNullOrEmpty(contact.EventId), "EventId should not be empty");

            TestContext.WriteLine($"First contact: Country={contact.Country}, Owner={contact.Owner}, Location={contact.Location}, Leader={contact.Leader}, Date={contact.Date}, Stage={contact.Stage}, EventId={contact.EventId}");
        }
    }

    [TestMethod]
    public void FirstContacts_ParsesLocalizedName()
    {
        // Act
        var contacts = StellarisTestData.Save.FirstContacts;

        // Assert
        Assert.IsNotNull(contacts, "Contacts should not be null");
        Assert.IsNotEmpty(contacts, "Contacts should not be empty");

        foreach (var contact in contacts)
        {
            Assert.IsNotNull(contact.Name, "Name should not be null");
            Assert.IsFalse(string.IsNullOrEmpty(contact.Name.Key), "Name key should not be empty");

            if (contact.Name.Variables != null && contact.Name.Variables.Count > 0)
            {
                TestContext.WriteLine($"Name variables: {string.Join(", ", contact.Name.Variables.Keys)}");
                foreach (var variable in contact.Name.Variables)
                {
                    Assert.IsFalse(string.IsNullOrEmpty(variable.Key), "Variable key should not be empty");
                    Assert.IsNotNull(variable.Value, "Variable value should not be null");
                    Assert.IsFalse(string.IsNullOrEmpty(variable.Value.Key), "Variable value key should not be empty");
                }
            }
        }
    }

    [TestMethod]
    public void FirstContacts_ParsesNumericFields()
    {
        // Act
        var contacts = StellarisTestData.Save.FirstContacts;

        // Assert
        Assert.IsNotNull(contacts, "Contacts should not be null");
        Assert.IsNotEmpty(contacts, "Contacts should not be empty");

        foreach (var contact in contacts)
        {
            TestContext.WriteLine($"LastRoll={contact.LastRoll}, DaysLeft={contact.DaysLeft}, Difficulty={contact.Difficulty}, Clues={contact.Clues}");
            Assert.IsTrue(contact.LastRoll >= 0, "LastRoll should be non-negative");
            Assert.IsTrue(contact.DaysLeft >= 0, "DaysLeft should be non-negative");
            Assert.IsTrue(contact.Difficulty >= 0, "Difficulty should be non-negative");
            Assert.IsTrue(contact.Clues >= 0, "Clues should be non-negative");
        }
    }

    [TestMethod]
    public void FirstContacts_ParsesStatus()
    {
        // Act
        var contacts = StellarisTestData.Save.FirstContacts;

        // Assert
        Assert.IsNotNull(contacts, "Contacts should not be null");
        Assert.IsNotEmpty(contacts, "Contacts should not be empty");

        foreach (var contact in contacts)
        {
            Assert.IsFalse(string.IsNullOrEmpty(contact.Status), "Status should not be empty");
            TestContext.WriteLine($"Status: {contact.Status}");
        }
    }

    [TestMethod]
    public void FirstContacts_ParsesEvents()
    {
        // Act
        var contacts = StellarisTestData.Save.FirstContacts;

        // Assert
        Assert.IsNotNull(contacts, "Contacts should not be null");
        Assert.IsNotEmpty(contacts, "Contacts should not be empty");

        foreach (var contact in contacts)
        {
            // Check main event
            Assert.IsNotNull(contact.Event, "Event should not be null");
            Assert.IsFalse(string.IsNullOrEmpty(contact.Event.EventId), "Event ID should not be empty");
            Assert.IsFalse(string.IsNullOrEmpty(contact.Event.Picture), "Event picture should not be empty");
            Assert.IsNotNull(contact.Event.Scope, "Event scope should not be null");

            // Check event scope
            var scope = contact.Event.Scope;
            Assert.IsFalse(string.IsNullOrEmpty(scope.Type), "Scope type should not be empty");
            Assert.IsTrue(scope.Random.Count > 0, "Scope should have random values");

            // Check event list
            Assert.IsNotNull(contact.Events, "Events list should not be null");
            Assert.IsTrue(contact.Events.Count > 0, "Should have at least one event");

            foreach (var evt in contact.Events)
            {
                Assert.IsFalse(string.IsNullOrEmpty(evt.EventId), "Event ID should not be empty");
                Assert.IsFalse(string.IsNullOrEmpty(evt.Picture), "Event picture should not be empty");
                Assert.IsNotNull(evt.Scope, "Event scope should not be null");

                TestContext.WriteLine($"Event: ID={evt.EventId}, Picture={evt.Picture}, Index={evt.Index}");
            }
        }
    }

    [TestMethod]
    public void FirstContacts_ParsesFlags()
    {
        // Act
        var contacts = StellarisTestData.Save.FirstContacts;

        // Assert
        Assert.IsNotNull(contacts, "Contacts should not be null");
        Assert.IsNotEmpty(contacts, "Contacts should not be empty");

        foreach (var contact in contacts)
        {
            Assert.IsNotNull(contact.Flags, "Flags should not be null");
            Assert.IsTrue(contact.Flags.Count > 0, "Should have at least one flag");

            foreach (var flag in contact.Flags)
            {
                Assert.IsFalse(string.IsNullOrEmpty(flag.Key), "Flag key should not be empty");
                Assert.AreNotEqual(0L, flag.Value, "Flag value should not be 0");
                TestContext.WriteLine($"Flag: {flag.Key}={flag.Value}");
            }
        }
    }

    [TestMethod]
    public void FirstContacts_ParsesCompletedStages()
    {
        // Act
        var contacts = StellarisTestData.Save.FirstContacts;

        // Assert
        Assert.IsNotNull(contacts, "Contacts should not be null");
        Assert.IsNotEmpty(contacts, "Contacts should not be empty");

        foreach (var contact in contacts)
        {
            Assert.IsNotNull(contact.Completed, "Completed stages should not be null");
            Assert.IsTrue(contact.Completed.Count > 0, "Should have at least one completed stage");

            foreach (var stage in contact.Completed)
            {
                Assert.IsFalse(string.IsNullOrEmpty(stage.Date), "Stage date should not be empty");
                Assert.IsFalse(string.IsNullOrEmpty(stage.Stage), "Stage name should not be empty");
                TestContext.WriteLine($"Completed stage: {stage.Stage} on {stage.Date}");
            }
        }
    }
} 