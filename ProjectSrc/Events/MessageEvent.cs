namespace RobloxStudioModManager
{
    public delegate void MessageEventHandler(object sender, MessageEventArgs e);
    
    public class MessageEventArgs
    {
        public string Message { get; }

        public MessageEventArgs(string message)
        {
            Message = message;
        }
    }
}
