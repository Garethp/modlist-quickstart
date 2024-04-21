namespace ModlistQuickstart.ModlistManager;

public struct ModData
{
    public ModData(string name = "", string packageId = "", string fileId = "")
    {
        Name = name;
        PackageId = packageId;
        FileId = fileId;
    }

    public string Name;
    public string PackageId;
    public string FileId;
}