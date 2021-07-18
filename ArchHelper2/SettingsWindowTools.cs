using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ArchHelper2.MainWindow;
using static ArchHelper2.CSharpHelper;
using static ArchHelper2.XAMLHelper;
using static ArchHelper2.DeprecatedHelpers;
using static ArchHelper2.DebugConsoleTools;
using static ArchHelper2.ArchDebugConsoleTools;
using static ArchHelper2.SettingsWindow;

namespace ArchHelper2
{
    class SettingsWindowTools
    {
        public static void ToggleSettingsWindow()
        {
            if (IsWindowOpen<SettingsWindow>())
            {
                GrabOpenWindow<SettingsWindow>().Close();
            }
            else
            {
                SettingsWindow settingsWindow = new SettingsWindow();
                settingsWindow.Show();
            }
        }

        public static void OpenSettingsWindow()
        {
            if (IsWindowOpen<SettingsWindow>())
            {
                GrabOpenWindow<SettingsWindow>().Activate();
            }
            else
            {
                SettingsWindow settingsWindow = new SettingsWindow();
                settingsWindow.Show();
            }
        }
    }
}
