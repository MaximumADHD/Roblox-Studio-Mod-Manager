namespace RobloxStudioModManager
{
    public delegate void ChangeEventHandler<T>(object sender, ChangeEventArgs<T> e);

    public class ChangeEventArgs<T>
    {
        public string Context { get; }
        public T Value { get; }

        public ChangeEventArgs(T value, string context = "N/A")
        {
            Value = value;
            Context = context;
        }
    }
}
