namespace JobPortal.Settings
{
    /// <summary>
    /// Holds the absolute path to the App_Data directory.
    /// Registered as a singleton in Program.cs and injected into controllers.
    /// </summary>
    public class FileStorageSettings
    {
        public string AppDataPath { get; set; } = string.Empty;
    }
}
