using System.IO;

using Microsoft.Win32;

namespace RobloxStudioModManager
{
    public static class RegistryExtensions
    {
        public static RegistryKey GetSubKey(this RegistryKey key, params string[] path)
        {
            string constructedPath = Path.Combine(path);
            return key.CreateSubKey(constructedPath, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryOptions.None);
        }
    }
}
