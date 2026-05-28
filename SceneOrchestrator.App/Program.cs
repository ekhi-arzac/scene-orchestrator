namespace SceneOrchestrator.App;

using SceneOrchestrator.UI;

/// <summary>
/// Application entry point that launches the editor UI.
/// </summary>
public class Program
{
    /// <summary>
    /// Application entry point.
    /// </summary>
    public static void Main(string[] args)
    {
        UserInterface ui = new();
        ui.Run();
    }
}
