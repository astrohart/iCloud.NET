using System.Reflection;

namespace AppleICloudDotNet.Tests
{
    internal static class ReflectionUtilities
    {
        public static string GetProgramName()
        {
            return Assembly.GetCallingAssembly().GetName().Name;
        }

        public static string GetProgramPath()
        {
            return Assembly.GetCallingAssembly().Location;
        }
    }
}