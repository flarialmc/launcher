using System;
using System.Drawing;
using System.IO;

namespace Flarial.Launcher.Handlers.Functions
{
    public static class FontManager
    {
        public static bool IsFontInstalled(string fontName)
        {
            var testFont = new Font(fontName, 8);
            return 0 == string.Compare(fontName, testFont.Name, StringComparison.InvariantCultureIgnoreCase);
        }

        public static void InstallFont(string fontSourcePath)
        {
            var shellAppType = Type.GetTypeFromProgID("Shell.Application");
            var shell = Activator.CreateInstance(shellAppType);
            var fontFolder = (Shell32.Folder)shellAppType.InvokeMember("NameSpace", System.Reflection.BindingFlags.InvokeMethod, null, shell, new object[] { Environment.GetFolderPath(Environment.SpecialFolder.Fonts) });

            if (File.Exists(fontSourcePath))
            {
                fontFolder.CopyHere(fontSourcePath);
            }
        }
    }
}
