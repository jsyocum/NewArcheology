using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static ArchHelper2.MainWindow;
using static ArchHelper2.CSharpHelper;
using static ArchHelper2.XAMLHelper;
using static ArchHelper2.DeprecatedHelpers;
using static ArchHelper2.DebugConsoleTools;
using static ArchHelper2.ArchDebugConsoleTools;
using static ArchHelper2.SettingsWindowTools;
using XamlAnimatedGif;
using System.Diagnostics;

namespace ArchHelper2
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();

            ReloadSettingsPage();
        }



        /////////////////// SettingsWindow stuff ///////////////////

        //SettingsWindow
        private void SettingsWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            defSettings.Reload();
        }

        /////////////////// Reload/reset settings ///////////////////

        /// <summary>
        /// Reloads all of the elements in the settings page that correspond to a setting. Does not change any settings.
        /// </summary>
        private void ReloadSettingsPage()
        {
            AppFolderTextBox.Text = defSettings.AppPath;
            DecideSaveOnExitButton();
            DecideSaveWindowPropertiesCheckBoxes();
        }

        /// <summary>
        /// This resets settings that vary based on user
        /// </summary>
        private void ResetOddSettings()
        {
            defSettings.AppPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ArchHelper\\");
        }

        /////////////////// Application folder stuff (AppFolder) ///////////////////

        //AppFolderTextBox
        private void AppFolderTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HideSearchText(AppFolderTextBox, AppFolderTextBlock);
        }

        //AppFolderDefaultButton
        private void AppFolderDefaultButton_Click(object sender, RoutedEventArgs e)
        {
            defSettings.AppPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ArchHelper\\");
            ReloadSettingsPage();

            PlayGif(AppFolderDefaultButtonImage, "/ArchHelper2;component/Resources/icons8-refresh-default.gif", true);
        }

        //AppFolderChooseButton
        private void AppFolderChooseButton_MouseEnter(object sender, MouseEventArgs e)
        {
            AnimationBehavior.GetAnimator(AppFolderChooseButtonImage).Play();
        }

        private void AppFolderChooseButton_MouseLeave(object sender, MouseEventArgs e)
        {
            StopGif(AppFolderChooseButtonImage, "/ArchHelper2;component/Resources/icons8-dots-loading.gif");
        }

        private void AppFolderChooseButton_Click(object sender, RoutedEventArgs e)
        {
            SelectFolder(ref appFolderPath);
            GrabOpenWindow<SettingsWindow>().Activate();
            if (appFolderPath != "")
            {
                defSettings.AppPath = appFolderPath;

                AppFolderTextBox.Text = defSettings.AppPath;
            }
        }

        //AppFolderOpenButton
        private void AppFolderOpenButton_MouseEnter(object sender, MouseEventArgs e)
        {
            AnimationBehavior.GetAnimator(AppFolderOpenButtonImage).Play();
        }

        private void AppFolderOpenButton_MouseLeave(object sender, MouseEventArgs e)
        {
            StopGif(AppFolderOpenButtonImage, "/ArchHelper2;component/Resources/icons8-folderopen.gif");
        }

        private void AppFolderOpenButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", defSettings.AppPath);
        }

        //AppFolderCopyButton
        private void AppFolderCopyButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Clipboard.SetText(defSettings.AppPath);
            PlayGif(AppFolderCopyButtonImage, "/ArchHelper2;component/Resources/icons8-copy.gif", true);
        }

        /////////////////// Save on closing stuff (SaveOnExit) ///////////////////

        private void DecideSaveOnExitButton()
        {
            if (defSettings.SaveOnExit == "Always")
            {
                SaveOnExitAlways.IsChecked = true;
            }
            else if (defSettings.SaveOnExit == "Never")
            {
                SaveOnExitNever.IsChecked = true;
            }
            else
            {
                SaveOnExitAsk.IsChecked = true;
            }
        }

        //SaveOnExitDefaultButton
        private void SaveOnExitDefaultButton_Click(object sender, RoutedEventArgs e)
        {
            defSettings.SaveOnExit = "Ask";

            ReloadSettingsPage();

            PlayGif(SaveOnExitDefaultButtonImage, "/ArchHelper2;component/Resources/icons8-refresh-default.gif", true);
        }

        //SaveOnExitAlways
        private void SaveOnExitAlways_Checked(object sender, RoutedEventArgs e)
        {
            defSettings.SaveOnExit = "Always";
        }

        //SaveOnExitNever
        private void SaveOnExitNever_Checked(object sender, RoutedEventArgs e)
        {
            defSettings.SaveOnExit = "Never";
        }

        //SaveOnExitAsk
        private void SaveOnExitAsk_Checked(object sender, RoutedEventArgs e)
        {
            defSettings.SaveOnExit = "Ask";
        }

        /////////////////// Save window properties on exit ///////////////////

        //DecideSaveWindowPropertiesCheckBoxes
        private void DecideSaveWindowPropertiesCheckBoxes()
        {
            if (defSettings.SaveWindowSize)
            {
                SaveWindowSizeCheckBox.IsChecked = true;
            }

            if (defSettings.SaveWindowLocation)
            {
                SaveWindowLocationCheckBox.IsChecked = true;
            }
        }

        //SaveWindowPropertiesDefaultButton
        private void SaveWindowPropertiesDefaultButton_Click(object sender, RoutedEventArgs e)
        {
            defSettings.SaveWindowSize = true;
            defSettings.SaveWindowLocation = true;

            ReloadSettingsPage();

            PlayGif(SaveWindowPropertiesDefaultButtonImage, "/ArchHelper2;component/Resources/icons8-refresh-default.gif", true);
        }

        //SaveWindowSizeCheckBox
        private void SaveWindowSizeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            defSettings.SaveWindowSize = true;
        }

        private void SaveWindowSizeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            defSettings.SaveWindowSize = false;
        }

        //SaveWindowLocationCheckBox
        private void SaveWindowLocationCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            defSettings.SaveWindowLocation = true;
        }

        private void SaveWindowLocationCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            defSettings.SaveWindowLocation = false;
        }

        /////////////////// Reset all defaults ///////////////////

        //SettingsDefaultButton
        private void SettingsDefaultButton_Click(object sender, RoutedEventArgs e)
        {
            defSettings.Reset();
            ResetOddSettings();
            defSettings.Save();

            ReloadSettingsPage();
            PlayGif(SettingsDefaultButtonImage, "/ArchHelper2;component/Resources/icons8-refresh-all-defaults.gif", true);
        }

        /////////////////// Save/cancel buttons ///////////////////
        
        //SettingsSaveButton
        private void SettingsSaveButton_Click(object sender, RoutedEventArgs e)
        {
            defSettings.Save();

            ReplaceButtonWithGif(SettingsSaveButtonImage, "/ArchHelper2;component/Resources/icons8-save-ezgif.gif", SettingsSaveButton, SettingsSaveButtonFake);
        }

        //SettingsCancelButton
        private async void SettingsCancelButton_Click(object sender, RoutedEventArgs e)
        {
            defSettings.Reload();
            ReloadSettingsPage();

            ReplaceButtonWithGif(SettingsCancelButtonImage, "/ArchHelper2;component/Resources/icons8-delete.gif", SettingsCancelButton, SettingsCancelButtonFake, GrabOpenWindow<SettingsWindow>());
        }

        
    }
}
