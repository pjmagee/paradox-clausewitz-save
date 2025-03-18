using MageeSoft.Paradox.Clausewitz.Save.Parser;

namespace MageeSoft.Paradox.Clausewitz.Save.Binder.Aot;

/// <summary>
/// AOT-compatible binder for Clausewitz save files.
/// Uses source generation to create binding code that doesn't rely on reflection.
/// </summary>
public static class AOTBinder
{
    /// <summary>
    /// Binds a SaveObject to a model of type T.
    /// </summary>
    /// <typeparam name="T">The type of model to bind to.</typeparam>
    /// <param name="saveObject">The SaveObject to bind from.</param>
    /// <returns>A new instance of T with bound properties.</returns>
    public static T Bind<T>(SaveObject saveObject) where T : new()
    {
        // The source generator will create a static class named "TBinder" in the
        // same namespace as T but with ".Generated" appended.
        // For example, if T is "MageeSoft.Paradox.Clausewitz.Save.Reader.Games.Stellaris.Models.Achievements",
        // the generated binder will be "MageeSoft.Paradox.Clausewitz.Save.Reader.Games.Stellaris.Models.Generated.AchievementsBinder".
        
        // In a real implementation, this would dispatch to the generated binder based on T.
        // However, since we can't use reflection and we don't know all possible T types at compile time,
        // we'd need a manual mapping or registration system here.
        
        // For now, this is just a placeholder implementation that creates a default instance.
        return new T();
    }
    
    /// <summary>
    /// Example method that demonstrates how to use specific generated binders directly.
    /// In a real application, you'd have a mapping or registration system to dispatch to the correct binder.
    /// </summary>
    public static void Example()
    {
        // Parse a save file
        var parser = new Save.Parser.Parser("name=\"Test Empire\" capital=5");
        var saveObject = parser.Parse();
    }
} 