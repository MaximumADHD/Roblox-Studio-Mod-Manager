namespace RobloxStudioModManager
{
    public class DeployLog
    {
        public string VersionGuid;

        public int MajorRev;
        public int Version;
        public int Patch;
        public int Changelist;

        public override string ToString()
        {
            return string.Join(".", MajorRev, Version, Patch, Changelist);
        }
    }
}