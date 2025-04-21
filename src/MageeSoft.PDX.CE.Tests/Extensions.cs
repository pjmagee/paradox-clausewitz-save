namespace MageeSoft.PDX.CE.Tests;

public static class Extensions
{    
    // This is a custom assertion method that compares clausewitz plaintext strings
    extension(Assert assert)
    {
        public static void AreClausewitzStringsEqual(string? expected, string? actual, string? message = null)
        {
            string Normalize(string? s) =>
            new string((s ?? string.Empty)
                .Where(c => !char.IsWhiteSpace(c))
                .ToArray());

            var normExpected = Normalize(expected);
            var normActual = Normalize(actual);

            Assert.AreEqual(normExpected, normActual, message);
        }
    }
}