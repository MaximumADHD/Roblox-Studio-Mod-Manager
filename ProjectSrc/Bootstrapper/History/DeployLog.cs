namespace RobloxStudioModManager
{
    public class DeployLog
    {
        public string VersionGuid;
        public string BuildType;

        public int MajorRev;
        public int Version;
        public int Patch;
        public int Changelist;

        public bool Is64Bit => BuildType.EndsWith("64");
        public string VersionId => ToString();

        public override string ToString()
        {
            return string.Join(".", MajorRev, Version, Patch, Changelist);
        }
    }
}