using System;
using System.Globalization;

namespace RobloxStudioModManager
{
    public class DeployLog
    {
        public bool Is64Bit { get; set; }
        public string VersionGuid { get; set; }
        public DateTime TimeStamp { get; set; }

        public int MajorRev { get; set; }
        public int Version { get; set; }
        public int Patch { get; set; }
        public int Changelist { get; set; }

        public string VersionId => string.Join(".", MajorRev, Version, Patch, Changelist);

        public override string ToString()
        {
            string date = TimeStamp.ToString("MMM dd", CultureInfo.InvariantCulture);
            return $"{VersionId} ({date})";
        }
    }
}