namespace MageeSoft.PDX.CE.Tests;

public static class Extensions
{
    /// <summary>
    /// Compares two strings for equality, ignoring whitespace differences (spaces, tabs, newlines, carriage returns).
    /// Useful for comparing Paradox save file contents where formatting may differ.
    /// </summary>
    public static void AreClausewitzStringsEqual(this Assert assert, string? expected, string? actual, string? message = null)
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