using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace CSAnalyzer_Preview.Structures
{
    public class ExplorerFunctions
    {
        [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        public static void SetAssociation(string Extension, string KeyName, string OpenWith, string FileDescription)
        {
            RegistryKey BaseKey;
            RegistryKey OpenMethod;
            RegistryKey Shell;
            RegistryKey CurrentUser;

            BaseKey = Registry.ClassesRoot.CreateSubKey(Extension);
            BaseKey.SetValue("", KeyName);

            OpenMethod = Registry.ClassesRoot.CreateSubKey("OpenWithProgids");
            OpenMethod.SetValue("", FileDescription);
            OpenMethod.CreateSubKey("DefaultIcon").SetValue("", "\"" + OpenWith + "\",0");
            Shell = OpenMethod.CreateSubKey("Shell");
            Shell.CreateSubKey("open").CreateSubKey("command").SetValue("", "\"" + OpenWith + "\"" + " \"--open %1\"");
            BaseKey.Close();
            OpenMethod.Close();
            Shell.Close();

            CurrentUser = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\" + Extension, true);
            CurrentUser.DeleteSubKey("UserChoice", false);
            CurrentUser.Close();

            SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
        }
    }
}
