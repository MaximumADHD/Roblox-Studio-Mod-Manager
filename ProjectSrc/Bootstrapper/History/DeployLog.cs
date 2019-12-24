using System;

namespace RobloxStudioModManager
{
    public class DeployLog
    {
        public bool Is64Bit;
        public string VersionGuid;
        public DateTime TimeStamp;

        public int MajorRev;
        public int Version;
        public int Patch;
        public int Changelist;

        public string VersionId => string.Join(".", MajorRev, Version, Patch, Changelist);

        public override string ToString()
        {
            string date = TimeStamp.ToString("MMM dd");
            return $"{VersionId} ({date})";
        }
    }
}