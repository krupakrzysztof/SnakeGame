using Microsoft.Win32;
using System.Linq;
using System.Reflection;

namespace SnakeGame.Helpers
{
    public static class RegistryHelper
    {
        public static string AssemblyName { get; } = Assembly.GetExecutingAssembly().GetName().Name;

        public static void WriteRegistry(string nodeName, string value)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);
            key = !key.GetSubKeyNames().Contains(AssemblyName) ? key.CreateSubKey(AssemblyName, true) : key.OpenSubKey(AssemblyName, true);

            key.SetValue(nodeName, value, RegistryValueKind.String);
        }

        public static string GetRegistry(string nodeName)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true).CreateSubKey(AssemblyName, true);

            return key != null ? key.GetValue(nodeName, string.Empty).ToString() : string.Empty;
        }
    }
}
