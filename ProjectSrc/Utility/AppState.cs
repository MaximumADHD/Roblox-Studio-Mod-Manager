using System.Collections.Generic;
using RobloxDeployHistory;

namespace RobloxStudioModManager
{
    public class ExplorerIconManifest
    {
        public bool DarkTheme = false;
        public int ExtraItemSlots = 0;
        public bool ShowModifiedIcons = true;
        public string LastClassIconHash = "";
        public Dictionary<string, bool> EnabledIcons = new Dictionary<string, bool>();
    }

    public class VersionManifest
    {
        public string Channel = "LIVE";
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
        string Channel { get; set; }
        
        VersionManifest VersionData { get; set; }
        Dictionary<string, string> FileManifest { get; }
        Dictionary<string, PackageState> PackageManifest { get; }
    }

    public class ModManagerState : IBootstrapperState
    {
        public string Channel { get; set; } = "LIVE";
        public string TargetVersion { get; set; } = "";

        public bool DisableFlagWarning { get; set; } = false;
        public VersionManifest VersionData { get; set; } = new VersionManifest();
        public ExplorerIconManifest ExplorerIcons { get; set; } = new ExplorerIconManifest();
        public Dictionary<string, string> FileManifest { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, FVariable> FlagEditor { get; set; } = new Dictionary<string, FVariable>();
        public Dictionary<string, PackageState> PackageManifest { get; set; } = new Dictionary<string, PackageState>();
    }
}
