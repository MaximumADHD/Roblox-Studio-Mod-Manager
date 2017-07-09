using System;
using System.Net;
using System.Windows.Forms;

namespace RobloxModManager
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true; // Unlocks https for the WebClient

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Launcher(args));
        }
    }
}
