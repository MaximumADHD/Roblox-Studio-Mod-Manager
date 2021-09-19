using System.Diagnostics.Contracts;

namespace RobloxStudioModManager
{
    public class ClientVersionInfo
    {
        public string Version { get; private set; }
        public string VersionGuid { get; private set; }

        public ClientVersionInfo(string version, string versionGuid)
        {
            Version = version;
            VersionGuid = versionGuid;
        }

        public ClientVersionInfo(DeployLog log)
        {
            Contract.Requires(log != null);
            VersionGuid = log.VersionGuid;
            Version = log.VersionId;
        }
    }
}
