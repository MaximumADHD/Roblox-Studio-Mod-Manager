using System.Collections.Generic;

namespace RobloxStudioModManager
{
    public class ClassImageManifest
    {
        public string LastClassIconHash = "";
        public string SourceLocation = "";
    }

    public class ExplorerIconManifest
    {
        public bool DarkTheme = false;
        public int ExtraItemSlots = 0;
        public bool ShowModifiedIcons = true;

        public ClassImageManifest ClassImagesInfo = new ClassImageManifest();
        public Dictionary<string, bool> EnabledIcons = new Dictionary<string, bool>();
    }

    public class VersionManifest
    {
        public string Version = "";
        public string VersionGuid = "";
        public string VersionOverload = "";
        public string LastExecutedVersion = "";

        public string LatestGuid_x64 = "";
        public string LatestGuid_x86 = "";
    }

    public class PackageState
    {
        public int NumFiles = 0;
        public string Signature = "";
    }

    public interface IBootstrapperState
    {
        bool DeprecateMD5 { get; set; }
        string BuildBranch { get; set; }
        
        VersionManifest VersionData { get; set; }
        Dictionary<string, string> FileManifest { get; }
        Dictionary<string, PackageState> PackageManifest { get; }
    }

    public class ModManagerState : IBootstrapperState
    {
        public string TargetVersion { get; set; } = "";
        public string BuildBranch { get; set; } = "roblox";

        public bool DisableFlagWarning { get; set; } = false;
        public bool DeprecateMD5 { get; set; } = true;

        public VersionManifest VersionData { get; set; } = new VersionManifest();
        public ExplorerIconManifest ExplorerIcons { get; set; } = new ExplorerIconManifest();
        public Dictionary<string, string> FileManifest { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, FVariable> FlagEditor { get; set; } = new Dictionary<string, FVariable>();
        public Dictionary<string, PackageState> PackageManifest { get; set; } = new Dictionary<string, PackageState>();
    }
}
