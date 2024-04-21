using System.IO;
using Verse;

namespace ModlistQuickstart;

public class SaveManager
{
    public static void CopySave(string filePath)
    {
        // Check that the file exists
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"The file {filePath} does not exist.");
        }

        // Fetch our destination from Rimworlds save directory
        string saveDirectory = GenFilePaths.SaveDataFolderPath;
        string destinationPath = Path.Combine(saveDirectory, "Saves", Path.GetFileName(filePath));

        // Check that a file doesn't already exist at the destination
        if (File.Exists(destinationPath))
        {
            File.Delete(destinationPath);
        }

        // Copy the file
        File.Copy(filePath, destinationPath);
    }
}