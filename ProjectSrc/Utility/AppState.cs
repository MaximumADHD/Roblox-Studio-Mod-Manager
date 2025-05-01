using System.Collections.Generic;
using RobloxDeployHistory;

namespace RobloxStudioModManager
{
    public class VersionManifest
    {
        public string Channel = "LIVE";
        public string Version = "";

        public string VersionGuid = "";
        public string VersionOverload = "";
        public string LastFlagScanVersion = "";
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
        SortedDictionary<string, string> FileManifest { get; }
        SortedDictionary<string, PackageState> PackageManifest { get; }
    }

    public class ModManagerState : IBootstrapperState
    {
        public string Channel { get; set; } = "LIVE";
        public string TargetVersion { get; set; } = "";

        public bool DisableFlagWarning { get; set; } = false;
        public VersionManifest VersionData { get; set; } = new VersionManifest();
        public SortedDictionary<string, string> FileManifest { get; set; } = new SortedDictionary<string, string>();
        public SortedDictionary<string, FVariable> FlagEditor { get; set; } = new SortedDictionary<string, FVariable>();
        public SortedDictionary<string, PackageState> PackageManifest { get; set; } = new SortedDictionary<string, PackageState>();
    }
}
