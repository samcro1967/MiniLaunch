using System;
using System.IO;

namespace MiniLaunch.Profiles
{
    public static class ProfilePaths
    {
        public static string BaseDir =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MiniLaunch"
            );

        public static string ProfilesDir =>
            Path.Combine(BaseDir, "profiles");

        public static void Ensure()
        {
            Directory.CreateDirectory(ProfilesDir);
        }
    }
}
