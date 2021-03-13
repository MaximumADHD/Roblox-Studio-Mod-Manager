namespace RobloxStudioModManager
{
    internal class Package
    {
        public string Name { get; set; }
        public string Signature { get; set; }
        public int PackedSize { get; set; }
        public int Size { get; set; }

        public bool Exists { get; set; }
        public bool ShouldInstall { get; set; }

        internal byte[] Data { get; set; }

        public override string ToString()
        {
            return $"[{Signature}] {Name}";
        }
    }
}
