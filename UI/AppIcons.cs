using System.Drawing;
using System.Reflection;

namespace MiniLaunch.UI
{
    public static class AppIcons
    {
        private static Icon? _app;
        private static Icon? _about;
        private static Icon? _exit;
        private static Icon? _settings;
        private static Icon? _profile;
        private static Icon? _help;

        private static Icon Load(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var resourceName = assembly
                .GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));

            if (resourceName == null)
                throw new InvalidOperationException($"Icon resource '{fileName}' not found.");

            using var stream = assembly.GetManifestResourceStream(resourceName)!;
            return new Icon(stream);
        }

        public static Icon App =>
            _app ??= Load("MiniLaunch.ico");

        public static Icon About =>
            _about ??= Load("about.ico");

        public static Icon Exit =>
            _exit ??= Load("exit.ico");

        public static Icon Settings =>
            _settings ??= Load("settings.ico");

        public static Icon Profile =>
            _profile ??= Load("profile.ico");

        public static Icon Help =>
            _help ??= Load("help.ico");
    }
}