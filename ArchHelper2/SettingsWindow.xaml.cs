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

            ImportArtefactsTextBox.Text = defSettings.AppPath;
        }



        ///////////////////Importing artefacts and materials stuff///////////////////

        //ImportArtefactsTextBox
        private void ImportArtefactsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HideSearchText(ImportArtefactsTextBox, ImportArtefactsTextBlock);
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

                ImportArtefactsTextBox.Text = defSettings.AppPath;
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
    }
}
