using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.String;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static ArchHelper2.CSharpHelper;
using static ArchHelper2.XAMLHelper;
using static ArchHelper2.DeprecatedHelpers;
using static ArchHelper2.DebugConsole;
using static ArchHelper2.DebugConsoleTools;
using static ArchHelper2.ArchDebugConsoleTools;
using static ArchHelper2.SettingsWindow;
using static ArchHelper2.SettingsWindowTools;
using static ArchHelper2.Artefacts;
using static ArchHelper2.Materials;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Diagnostics;
using System.ComponentModel;
using XamlAnimatedGif;
using System.Windows.Media.Animation;
using System.Globalization;

namespace ArchHelper2
{
    public partial class MainWindow : Window
    {

        public struct artefact
        {

            public artefact(string ArteName, double Experience, List<int> MatAmountsNeeded, List<int> MatsNeeded, int AmountNeeded, string url)
            {
                arteName = ArteName;
                experience = Experience;
                matAmountsNeeded = MatAmountsNeeded;
                matsNeeded = MatsNeeded;
                amountNeeded = AmountNeeded;
                totalExperience = experience * amountNeeded;
                URL = url;
                experienceToolTip = "Experience gained by restoring " + amountNeeded + " of these: " + totalExperience;
            }

            public string arteName { get; set; }
            public double experience { get; set; }
            public List<int> matAmountsNeeded { get; set; }
            public List<int> matsNeeded { get; set; }
            public int amountNeeded { get; set; }
            public double totalExperience { get; }
            public string URL { get; set; }
            public string experienceToolTip { get; set; }
        }

        public class listBoxItem
        {
            public string ItemName { get; set; }
            public int ItemAmount { get; set; }
            public string URL { get; set; }
            public listBoxItem(string itemName, int itemAmount, string url)
            {
                ItemName = itemName;
                ItemAmount = itemAmount;
                URL = url;
            }

            public listBoxItem()
            {
                ItemName = "UnknownItem";
                ItemAmount = 0;
                URL = "";
            }
        }

        //Gif source strings
        string XButtonSource = "/ArchHelper2;component/Resources/icons8-delete.gif";

        //Declaring application folder stuff
        public static TextBox importArtefactsTextBox = new TextBox();
        public static string appFolderPath = "";
        public static Properties.Settings defSettings = ArchHelper2.Properties.Settings.Default;

        public static string saveStateSpecificFilePath = "SaveState\\";
        public static string autoSaveStateSpecificFilePath = "AutoSaveState\\";

        //Declaring allArtefact stuff
        public static List<artefact> allArtefacts = new List<artefact>();
        public static List<artefact> allArtefactsHardCoded = new List<artefact>(ImportArtefacts(artefactsHardCoded));
        public static List<artefact> allArtefactsFromFile = new List<artefact>();

        //Declaring Artefact ListBox stuff
        public static ListBox artefactListBox = new ListBox();
        public static ListBox artefactsAddedListBox = new ListBox();
        public static Button artefactAddButton = new Button();
        public static Button artefactUpButton = new Button();
        public static Button artefactDownButton = new Button();
        public static Button artefactRemoveButton = new Button();
        public static Button artefactChangeButton = new Button();
        public static List<Button> artefactsAddedButtons = new List<Button>();

        List<artefact> artefactListBoxItemsRemoved = new List<artefact>();
        List<artefact> artefactListBoxSelectedItemsRemoved = new List<artefact>();
        List<artefact> artefactListBoxSelectedItemsTrackedBefore = new List<artefact>();
        artefact artefactListBoxRightClicked = new artefact();

        public static List <artefact> artefactAddBoxItemsRemoved = new List<artefact>();
        public static List <artefact> artefactAddBoxSelectedItemsRemoved = new List<artefact>();
        public static List <artefact> artefactAddBoxSelectedItemsTrackedBefore = new List<artefact>();
        artefact artefactAddBoxRightClicked = new artefact();

        //Declaring allMaterial stuff
        public static List<listBoxItem> allMaterials = new List<listBoxItem>();
        public static List<listBoxItem> allMaterialsHardCoded = new List<listBoxItem>(ImportMaterials(materialsHardCoded));
        public static List<listBoxItem> allMaterialsFromFile = new List<listBoxItem>();

        //Declaring Material ListBox stuff
        public static ListBox materialListBox = new ListBox();
        public static ListBox materialsAddedListBox = new ListBox();
        public static Button materialAddButton = new Button();
        public static Button materialUpButton = new Button();
        public static Button materialDownButton = new Button();
        public static Button materialRemoveButton = new Button();
        public static Button materialChangeButton = new Button();
        public static List<Button> materialsAddedButtons = new List<Button>();

        public static List<listBoxItem> materialListBoxItemsRemoved = new List<listBoxItem>();
        List<listBoxItem> materialListBoxSelectedItemsRemoved = new List<listBoxItem>();
        List<listBoxItem> materialListBoxSelectedItemsTrackedBefore = new List<listBoxItem>();
        listBoxItem materialListBoxRightClicked = new listBoxItem();

        public static List<listBoxItem> materialAddBoxItemsRemoved = new List<listBoxItem>();
        public static List<listBoxItem> materialAddBoxSelectedItemsRemoved = new List<listBoxItem>();
        public static List<listBoxItem> materialAddBoxSelectedItemsTrackedBefore = new List<listBoxItem>();
        listBoxItem materialAddBoxRightClicked = new listBoxItem();

        //Declaring MaterialsRequired ListBox stuff
        public static ListBox materialsRequiredListBox = new ListBox();
        public static List<listBoxItem> materialsRequiredListBoxItemsRemoved = new List<listBoxItem>();
        public static List<listBoxItem> materialsRequiredListBoxItemsEnough = new List<listBoxItem>();
        List<listBoxItem> materialsRequiredListBoxSelectedItemsTrackedBefore = new List<listBoxItem>();
        listBoxItem materialsRequiredListBoxRightClicked = new listBoxItem();

        //Declaring total experience gained stuff
        public static TextBlock totalExpGained = new TextBlock();

        //Declaring search boxes
        public static TextBox artefactTextBox = new TextBox();
        public static TextBox artefactRemoveSearchBox = new TextBox();
        public static TextBox materialSearchBox = new TextBox();
        public static TextBox materialRemoveSearchBox = new TextBox();
        public static TextBox materialsRequiredSearchBox = new TextBox();

        public MainWindow()
        {
            InitializeComponent();
            ConsoleWriteLine("(" + DateTime.Now.ToString() + ") MainWindow opened", debugGeneral);

            //Setting default app path
            if (defSettings.AppPath == "")
            {
                defSettings.AppPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ArchHelper\\");
            }

            //ListBox "bindings"
            artefactListBox = ArtefactListBox;
            artefactsAddedListBox = ArtefactsAddedListBox;
            materialListBox = MaterialListBox;
            materialsAddedListBox = MaterialsAddedListBox;
            materialsRequiredListBox = MaterialsRequiredListBox;

            totalExpGained = totalExperienceGained;

            //TextBoxes
            artefactTextBox = ArtefactTextBox;
            artefactRemoveSearchBox = ArtefactRemoveSearchBox;
            materialSearchBox = MaterialSearchBox;
            materialRemoveSearchBox = MaterialRemoveSearchBox;
            materialsRequiredSearchBox = MaterialsRequiredSearchBox;

            //Buttons
            artefactAddButton = ArtefactAddButton;
            artefactUpButton = ArtefactUpButton;
            artefactDownButton = ArtefactDownButton;
            artefactRemoveButton = ArtefactRemoveButton;
            artefactChangeButton = ArtefactChangeButton;

            artefactsAddedButtons.Add(artefactUpButton);
            artefactsAddedButtons.Add(artefactDownButton);
            artefactsAddedButtons.Add(artefactRemoveButton);
            artefactsAddedButtons.Add(artefactChangeButton);

            materialAddButton = MaterialAddButton;
            materialUpButton = MaterialUpButton;
            materialDownButton = MaterialDownButton;
            materialRemoveButton = MaterialRemoveButton;
            materialChangeButton = MaterialChangeButton;

            materialsAddedButtons.Add(materialUpButton);
            materialsAddedButtons.Add(materialDownButton);
            materialsAddedButtons.Add(materialRemoveButton);
            materialsAddedButtons.Add(materialChangeButton);


            allArtefacts = allArtefactsHardCoded;
            BuildListBoxes(allArtefacts);

            allMaterials = allMaterialsHardCoded;
            BuildListBoxes(allMaterials);

            UpdateTotalExperienceGainedMain();

            Load(defSettings.AppPath, artefactListBox, artefactsAddedListBox, materialListBox, materialsAddedListBox, artefactAddButton, materialAddButton, artefactsAddedButtons,
                materialsAddedButtons, allArtefacts, allMaterials);

            SetWindowProperties(GrabOpenWindow<MainWindow>(), defSettings.MainWindowHeight, defSettings.MainWindowWidth, defSettings.MainWindowLeft, defSettings.MainWindowTop, defSettings.SaveWindowSize, defSettings.SaveWindowLocation);
        }

        public static void GetRequiredMaterialsMain()
        {
            GetRequiredMaterials(artefactsAddedListBox, materialsAddedListBox, materialsRequiredListBox, allMaterials, artefactAddBoxItemsRemoved,
                                 materialListBoxItemsRemoved, artefactRemoveSearchBox, materialRemoveSearchBox, materialsRequiredListBoxItemsRemoved, materialsRequiredListBoxItemsEnough);
        }

        public static void UpdateTotalExperienceGainedMain()
        {
            UpdateTotalExperienceGained(artefactsAddedListBox, totalExpGained);
        }

        public static void AutoSave()
        {
            Save(defSettings.AppPath, autoSaveStateSpecificFilePath, artefactsAddedListBox, materialsAddedListBox, artefactAddBoxItemsRemoved, artefactAddBoxSelectedItemsRemoved,
                        materialAddBoxItemsRemoved, materialAddBoxSelectedItemsRemoved);
        }

        public static void DeleteAutoSave()
        {
            string fullPath = System.IO.Path.Combine(defSettings.AppPath, autoSaveStateSpecificFilePath);
            if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, true);
            }
        }

        public static void SaveWindowProperties()
        {
            defSettings.MainWindowHeight = GrabOpenWindow<MainWindow>().ActualHeight;
            defSettings.MainWindowWidth = GrabOpenWindow<MainWindow>().ActualWidth;
            defSettings.MainWindowLeft = GrabOpenWindow<MainWindow>().Left;
            defSettings.MainWindowTop = GrabOpenWindow<MainWindow>().Top;

            defSettings.Save();
        }

        ///////////////////MainWindow stuff///////////////////
        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            AutoSave();
            SaveWindowProperties();

            MessageBoxResult saveBeforeClose = new MessageBoxResult();

            if (defSettings.SaveOnExit == "Ask")
            {
                saveBeforeClose = MessageBox.Show("Save before exiting?", "Exit", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            }

            if (saveBeforeClose == MessageBoxResult.Yes || defSettings.SaveOnExit == "Always")
            {
                Save(defSettings.AppPath, artefactsAddedListBox, materialsAddedListBox, artefactAddBoxItemsRemoved, artefactAddBoxSelectedItemsRemoved,
                        materialAddBoxItemsRemoved, materialAddBoxSelectedItemsRemoved);
            }
            else if (saveBeforeClose == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }

            if (IsWindowOpen<DebugConsole>() && saveBeforeClose != MessageBoxResult.Cancel)
            {
                GrabOpenWindow<DebugConsole>().Close();
            }

            if (IsWindowOpen<SettingsWindow>() && saveBeforeClose != MessageBoxResult.Cancel)
            {
                GrabOpenWindow<SettingsWindow>().Close();
            }

            //DeleteAutoSave();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (defSettings.SaveWindowLocation)
            {
                defSettings.MainWindowHeight = GrabOpenWindow<MainWindow>().ActualHeight;
                defSettings.MainWindowWidth = GrabOpenWindow<MainWindow>().ActualWidth;
            }
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            if (defSettings.SaveWindowLocation)
            {
                defSettings.MainWindowTop = GrabOpenWindow<MainWindow>().Top;
                defSettings.MainWindowLeft = GrabOpenWindow<MainWindow>().Left;
            }
        }

        ///////////////////Save, load, and settings buttons stuff///////////////////

        //SaveButton
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Save(defSettings.AppPath, artefactsAddedListBox, materialsAddedListBox, artefactAddBoxItemsRemoved, artefactAddBoxSelectedItemsRemoved,
                            materialAddBoxItemsRemoved, materialAddBoxSelectedItemsRemoved);

            PlayGif(SaveButtonImage, "/ArchHelper2;component/Resources/icons8-save-ezgif.gif", true);
        }

        //LoadButton
        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            Load(defSettings.AppPath, artefactListBox, artefactsAddedListBox, materialListBox, materialsAddedListBox, artefactAddButton, materialAddButton, artefactsAddedButtons,
                materialsAddedButtons, allArtefacts, allMaterials);

            PlayGif(LoadButtonImage, "/ArchHelper2;component/Resources/icons8-load.gif", true);
        }

        //SettingsButton
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            OpenSettingsWindow();
        }

        private void SettingsButton_MouseEnter(object sender, MouseEventArgs e)
        {
            AnimationBehavior.GetAnimator(SettingsButtonImage).Play();
        }

        private void SettingsButton_MouseLeave(object sender, MouseEventArgs e)
        {
            StopGif(SettingsButtonImage, "/ArchHelper2;component/Resources/icons8-settings.gif");
        }

        ///////////////////Artefact ListBox Stuff///////////////////

        //ArtefactTextBox (Artefact search box)
        private void ArtefactTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HideSearchText(ArtefactTextBox, ArtefactTextBlock);
            HideClearButton(ArtefactTextBox, ArtefactTextBoxClearButton);
            FilterListBoxItems(ArtefactListBox, ArtefactTextBox.Text, artefactListBoxItemsRemoved, artefactListBoxSelectedItemsRemoved);

            ToggleButtonsBasedOnListBox(artefactListBox, artefactAddButton);
        }

        private void ArtefactTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter && ArtefactTextBox.Text.ToLower() == "debug")
            {
                ToggleConsole();

                ArtefactTextBox.Text = "";
            }
        }

        private void ArtefactTextBoxClearButton_Click(object sender, RoutedEventArgs e)
        {
            PlayGifHideButton(ArtefactTextBoxClearButtonImage, XButtonSource, true, ArtefactTextBoxClearButton, ArtefactTextBox);
        }

        //ArtefactListBox
        private void ArtefactListBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ToggleButtonsBasedOnListBox(artefactListBox, artefactAddButton);
        }

        private void ArtefactListBox_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            artefactListBoxSelectedItemsTrackedBefore = GetItemsFromListBox<artefact>(ArtefactListBox, 2);
        }

        private void ArtefactListBox_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ListBoxItemRightClickFunction<artefact>(ArtefactListBox, artefactListBoxSelectedItemsTrackedBefore, ref artefactListBoxRightClicked, debugRightClicks);

            ListBoxWhichMenuItems(ArtefactListBoxSelectAll, ArtefactListBoxUnselectAll, ArtefactListBox, ArtefactListBoxWiki, null, null, null, artefactListBoxRightClicked);
        }

        //ArtefactListBox ContextMenu
        private void ArtefactListBoxSelectAll_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ArtefactListBox.SelectAll();
            ToggleButtonsBasedOnListBox(artefactListBox, artefactAddButton);
        }

        private void ArtefactListBoxUnselectAll_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ArtefactListBox.UnselectAll();
            ToggleButtonsBasedOnListBox(artefactListBox, artefactAddButton);
        }

        private void ArtefactListBoxWiki_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenURL(artefactListBoxRightClicked.URL);
        }

        //ArtefactAddBox
        private void ArtefactAddBox_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void ArtefactAddBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            RestrictToNumbers(e);

            if(e.Key == Key.Enter)
            {
                ListBoxAddItemsFunction<artefact>(ArtefactListBox, ArtefactsAddedListBox, ArtefactAddBox);
                FilterListBoxItems(ArtefactsAddedListBox, ArtefactRemoveSearchBox.Text, artefactAddBoxItemsRemoved, null);

                GetRequiredMaterialsMain();
                FilterListBoxItems(MaterialsRequiredListBox, MaterialsRequiredSearchBox.Text, materialsRequiredListBoxItemsRemoved, null);

                ToggleButtonsBasedOnListBox(artefactListBox, artefactAddButton);
            }
        }

        private void ArtefactAddBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HideSearchText(ArtefactAddBox, ArtefactAmountTextBlock);
        }

        //ArtefactAddButton
        private void ArtefactAddButton_Click(object sender, RoutedEventArgs e)
        {
            ListBoxAddItemsFunction<artefact>(ArtefactListBox, ArtefactsAddedListBox, ArtefactAddBox);
            FilterListBoxItems(ArtefactsAddedListBox, ArtefactRemoveSearchBox.Text, artefactAddBoxItemsRemoved, null);

            GetRequiredMaterialsMain();
            FilterListBoxItems(MaterialsRequiredListBox, MaterialsRequiredSearchBox.Text, materialsRequiredListBoxItemsRemoved, null);

            UpdateTotalExperienceGainedMain();
            ToggleButtonsBasedOnListBox(artefactListBox, artefactAddButton);

            ArtefactAddBox.Text = "1";
        }

        //ArtefactRemoveSearchBox
        private void ArtefactRemoveSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HideSearchText(ArtefactRemoveSearchBox, ArtefactRemoveSearchBlock);
            HideClearButton(ArtefactRemoveSearchBox, ArtefactRemoveSearchBoxClearButton);

            FilterListBoxItems(ArtefactsAddedListBox, ArtefactRemoveSearchBox.Text, artefactAddBoxItemsRemoved, artefactAddBoxSelectedItemsRemoved);
            ToggleButtonsBasedOnListBox(ArtefactsAddedListBox, artefactsAddedButtons);
        }

        //ArtefactRemoveSearchBoxClearButton
        private void ArtefactRemoveSearchBoxClearButton_Click(object sender, RoutedEventArgs e)
        {
            PlayGifHideButton(ArtefactRemoveSearchBoxClearButtonImage, XButtonSource, true, ArtefactRemoveSearchBoxClearButton, ArtefactRemoveSearchBox);
        }

        //ArtefactsAddedListBox
        private void ArtefactsAddedListBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ToggleButtonsBasedOnListBox(ArtefactsAddedListBox, artefactsAddedButtons);
        }

        private void ArtefactsAddedListBox_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            artefactAddBoxSelectedItemsTrackedBefore = GetItemsFromListBox<artefact>(ArtefactsAddedListBox, 2);
        }

        private void ArtefactsAddedListBox_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ListBoxItemRightClickFunction<artefact>(ArtefactsAddedListBox, artefactAddBoxSelectedItemsTrackedBefore, ref artefactAddBoxRightClicked, debugRightClicks);

            ListBoxWhichMenuItems(ArtefactsAddedBoxSelectAll, ArtefactsAddedBoxUnselectAll, ArtefactsAddedListBox, ArtefactsAddedBoxWiki, ArtefactsAddedBoxAdd, ArtefactsAddedBoxSubtract, null, artefactAddBoxRightClicked);
        }

        //ArtefactsAddedListBox ContextMenu
        private void ArtefactsAddedBoxSelectAll_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ArtefactsAddedListBox.SelectAll();
            ToggleButtonsBasedOnListBox(ArtefactsAddedListBox, artefactsAddedButtons);
        }

        private void ArtefactsAddedBoxUnselectAll_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ArtefactsAddedListBox.UnselectAll();
            ToggleButtonsBasedOnListBox(ArtefactsAddedListBox, artefactsAddedButtons);
        }

        private void ArtefactsAddedBoxWiki_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenURL(artefactAddBoxRightClicked.URL);
        }

        private void ArtefactsAddedBoxAdd_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AddOrSubtract<artefact>(ArtefactRemoveBox, artefactAddBoxRightClicked, ArtefactsAddedListBox, true, ArtefactsAddedBoxContextMenu);

            GetRequiredMaterialsMain();
            FilterListBoxItems(ArtefactsAddedListBox, ArtefactRemoveSearchBox.Text, artefactAddBoxItemsRemoved, artefactAddBoxSelectedItemsRemoved);
        }

        private void ArtefactsAddedBoxSubtract_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AddOrSubtract<artefact>(ArtefactRemoveBox, artefactAddBoxRightClicked, ArtefactsAddedListBox, false, ArtefactsAddedBoxContextMenu);

            GetRequiredMaterialsMain();
            FilterListBoxItems(ArtefactsAddedListBox, ArtefactRemoveSearchBox.Text, artefactAddBoxItemsRemoved, artefactAddBoxSelectedItemsRemoved);
        }

        //ArtefactRemoveBox
        private void ArtefactRemoveBox_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void ArtefactRemoveBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            RestrictToNumbers(e);

            int artefactRemoveBoxAmount = 0;
            int.TryParse(ArtefactRemoveBox.Text, out artefactRemoveBoxAmount);
            if (e.Key == Key.Enter && artefactRemoveBoxAmount > 0)
            {
                ListBoxChangeAmount<artefact>(ArtefactsAddedListBox, ArtefactRemoveBox);
                FilterListBoxItems(ArtefactsAddedListBox, ArtefactRemoveSearchBox.Text, artefactAddBoxItemsRemoved, null);

                GetRequiredMaterialsMain();
                FilterListBoxItems(MaterialsRequiredListBox, MaterialsRequiredSearchBox.Text, materialsRequiredListBoxItemsRemoved, null);

                ToggleButtonsBasedOnListBox(ArtefactsAddedListBox, artefactsAddedButtons);
            }
            else if(e.Key == Key.Enter)
            {
                ListBoxRemoveItemsFunction<artefact>(ArtefactListBox, ArtefactsAddedListBox);
                FilterListBoxItems(ArtefactListBox, ArtefactTextBox.Text, artefactListBoxItemsRemoved, null);

                GetRequiredMaterialsMain();
                FilterListBoxItems(MaterialsRequiredListBox, MaterialsRequiredSearchBox.Text, materialsRequiredListBoxItemsRemoved, null);

                ToggleButtonsBasedOnListBox(ArtefactsAddedListBox, artefactsAddedButtons);
            }
        }

        private void ArtefactRemoveBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HideSearchText(ArtefactRemoveBox, ArtefactAmountRemoveTextBlock);
            WhichButton(ArtefactRemoveBox, ArtefactRemoveButton, ArtefactChangeButton);
            EnableAddOrSubtract(ArtefactsAddedBoxAdd, ArtefactsAddedBoxSubtract, ArtefactRemoveBox);
        }

        //ArtefactUpButton
        private void ArtefactUpButton_Click(object sender, RoutedEventArgs e)
        {
            ListBoxUpOrDownButton<artefact>(true, ArtefactsAddedListBox);
            FilterListBoxItems(ArtefactsAddedListBox, ArtefactRemoveSearchBox.Text, artefactAddBoxItemsRemoved, null);

            GetRequiredMaterialsMain();
            FilterListBoxItems(MaterialsRequiredListBox, MaterialsRequiredSearchBox.Text, materialsRequiredListBoxItemsRemoved, null);

            UpdateTotalExperienceGainedMain();
        }

        //ArtefactDownButton
        private void ArtefactDownButton_Click(object sender, RoutedEventArgs e)
        {
            ListBoxUpOrDownButton<artefact>(false, ArtefactsAddedListBox);
            FilterListBoxItems(ArtefactsAddedListBox, ArtefactRemoveSearchBox.Text, artefactAddBoxItemsRemoved, null);

            GetRequiredMaterialsMain();
            FilterListBoxItems(MaterialsRequiredListBox, MaterialsRequiredSearchBox.Text, materialsRequiredListBoxItemsRemoved, null);

            UpdateTotalExperienceGainedMain();
        }

        //ArtefactRemoveButton
        private void ArtefactRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            ListBoxRemoveItemsFunction<artefact>(ArtefactListBox, ArtefactsAddedListBox);
            FilterListBoxItems(ArtefactListBox, ArtefactTextBox.Text, artefactListBoxItemsRemoved, null);

            GetRequiredMaterialsMain();
            FilterListBoxItems(MaterialsRequiredListBox, MaterialsRequiredSearchBox.Text, materialsRequiredListBoxItemsRemoved, null);

            UpdateTotalExperienceGainedMain();
            ToggleButtonsBasedOnListBox(ArtefactsAddedListBox, artefactsAddedButtons);
            ArtefactRemoveBox.Text = "";
        }

        //ArtefactChangeButton
        private void ArtefactChangeButton_Click(object sender, RoutedEventArgs e)
        {
            ListBoxChangeAmount<artefact>(ArtefactsAddedListBox, ArtefactRemoveBox);
            FilterListBoxItems(ArtefactsAddedListBox, ArtefactRemoveSearchBox.Text, artefactAddBoxItemsRemoved, null);

            GetRequiredMaterialsMain();
            FilterListBoxItems(MaterialsRequiredListBox, MaterialsRequiredSearchBox.Text, materialsRequiredListBoxItemsRemoved, null);

            UpdateTotalExperienceGainedMain();
            ToggleButtonsBasedOnListBox(ArtefactsAddedListBox, artefactsAddedButtons);
            ArtefactRemoveBox.Text = "";
        }


        ///////////////////Material ListBox Stuff///////////////////

        //MaterialSearchBox
        private void MaterialSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HideSearchText(MaterialSearchBox, MaterialSearchBlock);
            HideClearButton(MaterialSearchBox, MaterialSearchBoxClearButton);
            
            FilterListBoxItems(MaterialListBox, MaterialSearchBox.Text, materialListBoxItemsRemoved, materialListBoxSelectedItemsRemoved);
            ToggleButtonsBasedOnListBox(materialListBox, materialAddButton);
        }

        //MaterialSearchBoxClearButton
        private void MaterialSearchBoxClearButton_Click(object sender, RoutedEventArgs e)
        {
            PlayGifHideButton(MaterialSearchBoxClearButtonImage, XButtonSource, true, MaterialSearchBoxClearButton, MaterialSearchBox);
        }

        //MaterialListBox
        private void MaterialListBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ToggleButtonsBasedOnListBox(materialListBox, materialAddButton);
        }
        private void MaterialListBox_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            materialListBoxSelectedItemsTrackedBefore = GetItemsFromListBox<listBoxItem>(MaterialListBox, 2);
        }

        private void MaterialListBox_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ListBoxItemRightClickFunction<listBoxItem>(MaterialListBox, materialListBoxSelectedItemsTrackedBefore, ref materialListBoxRightClicked, debugRightClicks);

            ListBoxWhichMenuItems(MaterialListBoxSelectAll, MaterialListBoxUnselectAll, MaterialListBox, MaterialListBoxWiki, null, null, null, materialListBoxRightClicked);
        }

        //MaterialListBox ContextMenu
        private void MaterialListBoxSelectAll_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MaterialListBox.SelectAll();
            ToggleButtonsBasedOnListBox(materialListBox, materialAddButton);
        }

        private void MaterialListBoxUnselectAll_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MaterialListBox.UnselectAll();
            ToggleButtonsBasedOnListBox(materialListBox, materialAddButton);
        }

        private void MaterialListBoxWiki_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenURL(materialListBoxRightClicked.URL);
        }

        //MaterialAddBox
        private void MaterialAddBox_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void MaterialAddBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            RestrictToNumbers(e);

            if(e.Key == Key.Enter)
            {
                ListBoxAddItemsFunction<listBoxItem>(MaterialListBox, MaterialsAddedListBox, MaterialAddBox);
                FilterListBoxItems(MaterialsAddedListBox, MaterialSearchBox.Text, materialAddBoxItemsRemoved, null);

                GetRequiredMaterialsMain();
                FilterListBoxItems(MaterialsRequiredListBox, MaterialsRequiredSearchBox.Text, materialsRequiredListBoxItemsRemoved, null);

                ToggleButtonsBasedOnListBox(materialListBox, materialAddButton);
            }
        }

        private void MaterialAddBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HideSearchText(MaterialAddBox, MaterialAmountTextBlock);
        }

        //MaterialAddButton
        private void MaterialAddButton_Click(object sender, RoutedEventArgs e)
        {
            ListBoxAddItemsFunction<listBoxItem>(MaterialListBox, MaterialsAddedListBox, MaterialAddBox);
            FilterListBoxItems(MaterialsAddedListBox, MaterialRemoveSearchBox.Text, materialAddBoxItemsRemoved, null);

            GetRequiredMaterialsMain();
            FilterListBoxItems(MaterialsRequiredListBox, MaterialsRequiredSearchBox.Text, materialsRequiredListBoxItemsRemoved, null);

            ToggleButtonsBasedOnListBox(materialListBox, materialAddButton);

            MaterialAddBox.Text = "1";
        }

        //MaterialRemoveSearchBox
        private void MaterialRemoveSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HideSearchText(MaterialRemoveSearchBox, MaterialRemoveSearchBlock);
            HideClearButton(MaterialRemoveSearchBox, MaterialRemoveSearchBoxClearButton);

            FilterListBoxItems(MaterialsAddedListBox, MaterialRemoveSearchBox.Text, materialAddBoxItemsRemoved, materialAddBoxSelectedItemsRemoved);
            ToggleButtonsBasedOnListBox(MaterialsAddedListBox, materialsAddedButtons);
        }

        //MaterialRemoveSearchBoxClearButton
        private void MaterialRemoveSearchBoxClearButton_Click(object sender, RoutedEventArgs e)
        {
            PlayGifHideButton(MaterialRemoveSearchBoxClearButtonImage, XButtonSource, true, MaterialRemoveSearchBoxClearButton, MaterialRemoveSearchBox);
        }

        //MaterialsAddedListBox
        private void MaterialsAddedListBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ToggleButtonsBasedOnListBox(MaterialsAddedListBox, materialsAddedButtons);
        }

        private void MaterialsAddedListBox_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            materialAddBoxSelectedItemsTrackedBefore = GetItemsFromListBox<listBoxItem>(MaterialsAddedListBox, 2);
        }

        private void MaterialsAddedListBox_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ListBoxItemRightClickFunction<listBoxItem>(MaterialsAddedListBox, materialAddBoxSelectedItemsTrackedBefore, ref materialAddBoxRightClicked, debugRightClicks);

            ListBoxWhichMenuItems(MaterialsAddedListBoxSelectAll, MaterialsAddedListBoxUnselectAll, MaterialsAddedListBox, MaterialsAddedBoxWiki, MaterialsAddedBoxAdd, MaterialsAddedBoxSubtract, null, materialAddBoxRightClicked);
        }

        //MaterialsAddedListBox ContextMenu
        private void MaterialsAddedListBoxSelectAll_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MaterialsAddedListBox.SelectAll();
            ToggleButtonsBasedOnListBox(MaterialsAddedListBox, materialsAddedButtons);
        }

        private void MaterialsAddedListBoxUnselectAll_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MaterialsAddedListBox.UnselectAll();
            ToggleButtonsBasedOnListBox(MaterialsAddedListBox, materialsAddedButtons);
        }

        private void MaterialsAddedBoxWiki_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenURL(materialAddBoxRightClicked.URL);
        }

        private void MaterialsAddedBoxAdd_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AddOrSubtract<listBoxItem>(MaterialRemoveBox, materialAddBoxRightClicked, MaterialsAddedListBox, true, MaterialsAddedListBoxContextMenu);

            GetRequiredMaterialsMain();
            FilterListBoxItems(MaterialsAddedListBox, MaterialRemoveSearchBox.Text, materialAddBoxItemsRemoved, materialAddBoxSelectedItemsRemoved);
        }

        private void MaterialsAddedBoxSubtract_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AddOrSubtract<listBoxItem>(MaterialRemoveBox, materialAddBoxRightClicked, MaterialsAddedListBox, false, MaterialsAddedListBoxContextMenu);

            GetRequiredMaterialsMain();
            FilterListBoxItems(MaterialsAddedListBox, MaterialRemoveSearchBox.Text, materialAddBoxItemsRemoved, materialAddBoxSelectedItemsRemoved);
        }

        //MaterialRemoveBox
        private void MaterialRemoveBox_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void MaterialRemoveBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            RestrictToNumbers(e);

            int materialRemoveBoxAmount = 0;
            int.TryParse(MaterialRemoveBox.Text, out materialRemoveBoxAmount);
            if (e.Key == Key.Enter && materialRemoveBoxAmount > 0)
            {
                ListBoxChangeAmount<listBoxItem>(MaterialsAddedListBox, MaterialRemoveBox);
                FilterListBoxItems(MaterialsAddedListBox, MaterialRemoveSearchBox.Text, materialAddBoxItemsRemoved, null);

                GetRequiredMaterialsMain();
                FilterListBoxItems(MaterialsRequiredListBox, MaterialsRequiredSearchBox.Text, materialsRequiredListBoxItemsRemoved, null);

                ToggleButtonsBasedOnListBox(MaterialsAddedListBox, materialsAddedButtons);
            }
            else if (e.Key == Key.Enter)
            {
                ListBoxRemoveItemsFunction<listBoxItem>(MaterialListBox, MaterialsAddedListBox);
                FilterListBoxItems(MaterialListBox, MaterialSearchBox.Text, materialListBoxItemsRemoved, null);

                GetRequiredMaterialsMain();
                FilterListBoxItems(MaterialsRequiredListBox, MaterialsRequiredSearchBox.Text, materialsRequiredListBoxItemsRemoved, null);

                ToggleButtonsBasedOnListBox(MaterialsAddedListBox, materialsAddedButtons);
            }
        }

        private void MaterialRemoveBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HideSearchText(MaterialRemoveBox, MaterialAmountRemoveTextBlock);
            WhichButton(MaterialRemoveBox, MaterialRemoveButton, MaterialChangeButton);
            EnableAddOrSubtract(MaterialsAddedBoxAdd, MaterialsAddedBoxSubtract, MaterialRemoveBox);
        }

        //MaterialUpButton
        private void MaterialUpButton_Click(object sender, RoutedEventArgs e)
        {
            ListBoxUpOrDownButton<listBoxItem>(true, MaterialsAddedListBox);
            FilterListBoxItems(MaterialsAddedListBox, MaterialRemoveSearchBox.Text, materialAddBoxItemsRemoved, null);

            GetRequiredMaterialsMain();
            FilterListBoxItems(MaterialsRequiredListBox, MaterialsRequiredSearchBox.Text, materialsRequiredListBoxItemsRemoved, null);
        }

        //MaterialDownButton
        private void MaterialDownButton_Click(object sender, RoutedEventArgs e)
        {
            ListBoxUpOrDownButton<listBoxItem>(false, MaterialsAddedListBox);
            FilterListBoxItems(MaterialsAddedListBox, MaterialRemoveSearchBox.Text, materialAddBoxItemsRemoved, null);

            GetRequiredMaterialsMain();
            FilterListBoxItems(MaterialsRequiredListBox, MaterialsRequiredSearchBox.Text, materialsRequiredListBoxItemsRemoved, null);
        }

        //MaterialRemoveButton
        private void MaterialRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            ListBoxRemoveItemsFunction<listBoxItem>(MaterialListBox, MaterialsAddedListBox);
            FilterListBoxItems(MaterialListBox, MaterialSearchBox.Text, materialListBoxItemsRemoved, null);

            GetRequiredMaterialsMain();
            FilterListBoxItems(MaterialsRequiredListBox, MaterialsRequiredSearchBox.Text, materialsRequiredListBoxItemsRemoved, null);

            ToggleButtonsBasedOnListBox(MaterialsAddedListBox, materialsAddedButtons);
            MaterialRemoveBox.Text = "";
        }

        //MaterialChangeButton
        private void MaterialChangeButton_Click(object sender, RoutedEventArgs e)
        {
            ListBoxChangeAmount<listBoxItem>(MaterialsAddedListBox, MaterialRemoveBox);
            FilterListBoxItems(MaterialsAddedListBox, MaterialRemoveSearchBox.Text, materialAddBoxItemsRemoved, null);

            GetRequiredMaterialsMain();
            FilterListBoxItems(MaterialsRequiredListBox, MaterialsRequiredSearchBox.Text, materialsRequiredListBoxItemsRemoved, null);

            ToggleButtonsBasedOnListBox(MaterialsAddedListBox, materialsAddedButtons);
            MaterialRemoveBox.Text = "";
        }

        //MaterialsRequiredSearchBox
        private void MaterialsRequiredSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HideSearchText(MaterialsRequiredSearchBox, MaterialsRequiredSearchBlock);
            HideClearButton(MaterialsRequiredSearchBox, MaterialsRequiredSearchBoxClearButton);

            FilterListBoxItems(MaterialsRequiredListBox, MaterialsRequiredSearchBox.Text, materialsRequiredListBoxItemsRemoved, null);
        }

        //MaterialsRequiredSearchBoxClearButton
        private void MaterialsRequiredSearchBoxClearButton_Click(object sender, RoutedEventArgs e)
        {
            PlayGifHideButton(MaterialsRequiredSearchBoxClearButtonImage, XButtonSource, true, MaterialsRequiredSearchBoxClearButton, MaterialsRequiredSearchBox);
        }

        //MaterialsRequiredListBox
        private void MaterialsRequiredListBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DisableSelection(MaterialsRequiredListBox);
        }

        private void MaterialsRequiredListBox_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            materialsRequiredListBoxSelectedItemsTrackedBefore = GetItemsFromListBox<listBoxItem>(MaterialsRequiredListBox, 2);
        }

        private void MaterialsRequiredListBox_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ListBoxItemRightClickFunction<listBoxItem>(MaterialsRequiredListBox, materialsRequiredListBoxSelectedItemsTrackedBefore, ref materialsRequiredListBoxRightClicked, debugRightClicks);

            ListBoxWhichMenuItems(null, null, null, MaterialsRequiredBoxWiki, null, null, GotEnoughButton, materialsRequiredListBoxRightClicked);
        }

        private void MaterialsRequiredListBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        //MaterialsRequiredListBox ContextMenu
        private void MaterialsRequiredBoxWiki_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenURL(materialsRequiredListBoxRightClicked.URL);
        }

        private void GotEnoughButton_Click(object sender, RoutedEventArgs e)
        {
            GotEnough(materialsRequiredListBoxRightClicked, MaterialListBox, MaterialsAddedListBox, MaterialSearchBox, MaterialRemoveSearchBox);
            GetRequiredMaterialsMain();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        
    }
}