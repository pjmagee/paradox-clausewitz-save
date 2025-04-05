using System.Text.RegularExpressions;


namespace MageeSoft.PDX.CE.SourceGenerator.Tests
{
    public static class AssertExtensions
    {
        public static void StatementsAreEqual(this Assert assert, string expected, string actual, string message = "")
        {
            var normalizedExpected = Regex.Replace(expected, @"\s+", " ").Trim();
            var normalizedActual = Regex.Replace(actual, @"\s+", " ").Trim();
            Assert.AreEqual(normalizedExpected, normalizedActual, message);
        }
    }
}
