using Microsoft.UI.Xaml;

// Using RobloxStudioModManager.WinUI here instead of just RobloxStudioModManager
// to prevent shadowing since WinUI compiler generates its own Program class.
namespace RobloxStudioModManager.WinUI
{
    public partial class App : Application
    {
        private Window mainWindow;

        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            var rawArgs = System.Environment.GetCommandLineArgs();
            var unprocessedArgs = RobloxStudioModManager.Program.Initialize(rawArgs[1..]);

            System.AppDomain.CurrentDomain.ProcessExit += (sender, e) => RobloxStudioModManager.Program.SaveState();

            mainWindow = new MainWindow(unprocessedArgs);
            mainWindow.Activate();
        }
    }
}
