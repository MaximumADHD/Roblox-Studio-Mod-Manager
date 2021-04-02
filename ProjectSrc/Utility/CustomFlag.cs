namespace RobloxStudioModManager
{
    public class CustomFlag
    {
        public string Type { get; private set; }
        public string Name { get; private set; }

        public CustomFlag(string type, string name)
        {
            Type = type;
            Name = name;
        }
    }
}
