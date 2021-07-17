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
using static ArchHelper2.ArchSetting;
using static ArchHelper2.Artefacts;
using static ArchHelper2.Materials;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Diagnostics;
using System.ComponentModel;

namespace ArchHelper2
{
    /*
    Inputs:
    All artefcat types and their material dependencies
    All material types

    User inputs:
    Artefacts wanted to be restored
    Materials already owned (optional)

    Outputs:
    Materials required to restore all artefacts (taking into account the artefacts already owned)
    Which artefacts there are already enough materials to restore
    How much experience will be gained per artifact, per group of similar artefacts, and
    */



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


        //Declaring application folder stuff
        public static TextBox importArtefactsTextBox = new TextBox();
        public static string appFolderPath = "";
        
        public static string defaultAppPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ArchHelper\\");
        public static string saveStateSpecificFilePath = "SaveState\\";
        public static string autoSaveStateSpecificFilePath = "AutoSaveState\\";

        public static ArchSetting appPath = new ArchSetting("AppPath", System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ArchHelper\\"));
        public static ArchSetting saveOnExit = new ArchSetting("SaveOnExit", "Ask");

        public static List<ArchSetting> settings = new List<ArchSetting> { appPath, saveOnExit };

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

            //Downloading resources
            Download("https://raw.githubusercontent.com/jsyocum/NewArcheology/master/ArchHelper2/Resources/icons8-settings.gif", System.IO.Path.Combine(QuerySetting("AppPath").Value, "Resources\\"));
            PopulateDebugConsole(System.IO.Path.Combine(QuerySetting("AppPath").Value, "Resources\\"));

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
            importArtefactsTextBox = ImportArtefactsTextBox;

            ImportArtefactsTextBox.Text = defaultAppPath;

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

            Load(ImportArtefactsTextBox.Text, artefactListBox, artefactsAddedListBox, materialListBox, materialsAddedListBox, artefactAddButton, materialAddButton, artefactsAddedButtons,
                materialsAddedButtons, allArtefacts, allMaterials);
        }

        public static void GetRequiredMaterialsMain()
        {
            GetRequiredMaterials(artefactsAddedListBox, materialsAddedListBox, materialsRequiredListBox, allMaterials, artefactAddBoxItemsRemoved,
                                 materialListBoxItemsRemoved, materialsRequiredListBoxItemsRemoved, materialsRequiredListBoxItemsEnough);
        }

        public static void UpdateTotalExperienceGainedMain()
        {
            UpdateTotalExperienceGained(artefactsAddedListBox, totalExpGained);
        }

        public static void AutoSave()
        {
            Save(importArtefactsTextBox.Text, autoSaveStateSpecificFilePath, artefactsAddedListBox, materialsAddedListBox, artefactAddBoxItemsRemoved, artefactAddBoxSelectedItemsRemoved,
                        materialAddBoxItemsRemoved, materialAddBoxSelectedItemsRemoved);
        }

        ///////////////////MainWindow stuff///////////////////
        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            AutoSave();

            MessageBoxResult saveBeforeClose = new MessageBoxResult();

            if (QuerySetting("Save on exit").Value == "Ask" || QuerySetting("Save on exit").Value == "null")
            {
                saveBeforeClose = MessageBox.Show("Save before exiting?", "Exit", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            }

            if (saveBeforeClose == MessageBoxResult.Yes || QuerySetting("Save on exit").Value == "Yes")
            {
                Save(ImportArtefactsTextBox.Text, artefactsAddedListBox, materialsAddedListBox, artefactAddBoxItemsRemoved, artefactAddBoxSelectedItemsRemoved,
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
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AddSetting<double>("MainWindowHeight", GrabOpenWindow<MainWindow>().ActualHeight);
            AddSetting<double>("MainWindowWidth", GrabOpenWindow<MainWindow>().ActualWidth);
        }

        ///////////////////Importing artefacts and materials stuff///////////////////

        //ImportArtefactsTextBox
        private void ImportArtefactsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HideSearchText(ImportArtefactsTextBox, ImportArtefactsTextBlock);
        }

        //ImportArtefacts button
        private void ImportArtefacts_Click(object sender, RoutedEventArgs e)
        {
            SelectFolder(ref appFolderPath);
            if(appFolderPath != "")
            {
                ImportArtefactsTextBox.Text = appFolderPath;
            }
        }

        //SaveButton
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Save(ImportArtefactsTextBox.Text, artefactsAddedListBox, materialsAddedListBox, artefactAddBoxItemsRemoved, artefactAddBoxSelectedItemsRemoved,
                            materialAddBoxItemsRemoved, materialAddBoxSelectedItemsRemoved);
        }

        //LoadButton
        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            Load(ImportArtefactsTextBox.Text, artefactListBox, artefactsAddedListBox, materialListBox, materialsAddedListBox, artefactAddButton, materialAddButton, artefactsAddedButtons,
            materialsAddedButtons, allArtefacts, allMaterials);
        }

        ///////////////////Artefact ListBox Stuff///////////////////

        //ArtefactTextBox (Artefact search box)
        private void ArtefactTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HideSearchText(ArtefactTextBox, ArtefactTextBlock);
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
            Process.Start(new ProcessStartInfo("cmd", $"/c start {artefactListBoxRightClicked.URL}") { CreateNoWindow = true });
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

                GetRequiredMaterials(ArtefactsAddedListBox, MaterialsAddedListBox, MaterialsRequiredListBox, allMaterials, artefactAddBoxItemsRemoved, 
                                     materialListBoxItemsRemoved, materialsRequiredListBoxItemsRemoved, materialsRequiredListBoxItemsEnough);
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

            GetRequiredMaterials(ArtefactsAddedListBox, MaterialsAddedListBox, MaterialsRequiredListBox, allMaterials, artefactAddBoxItemsRemoved, 
                                 materialListBoxItemsRemoved, materialsRequiredListBoxItemsRemoved, materialsRequiredListBoxItemsEnough);
            FilterListBoxItems(MaterialsRequiredListBox, MaterialsRequiredSearchBox.Text, materialsRequiredListBoxItemsRemoved, null);

            UpdateTotalExperienceGainedMain();
            ToggleButtonsBasedOnListBox(artefactListBox, artefactAddButton);

            ArtefactAddBox.Text = "1";
        }

        //ArtefactRemoveSearchBox
        private void ArtefactRemoveSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HideSearchText(ArtefactRemoveSearchBox, ArtefactRemoveSearchBlock);
            FilterListBoxItems(ArtefactsAddedListBox, ArtefactRemoveSearchBox.Text, artefactAddBoxItemsRemoved, artefactAddBoxSelectedItemsRemoved);
            ToggleButtonsBasedOnListBox(ArtefactsAddedListBox, artefactsAddedButtons);
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
            Process.Start(new ProcessStartInfo("cmd", $"/c start {artefactAddBoxRightClicked.URL}") { CreateNoWindow = true });
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

                GetRequiredMaterials(ArtefactsAddedListBox, MaterialsAddedListBox, MaterialsRequiredListBox, allMaterials, artefactAddBoxItemsRemoved, 
                                     materialListBoxItemsRemoved, materialsRequiredListBoxItemsRemoved, materialsRequiredListBoxItemsEnough);
                FilterListBoxItems(MaterialsRequiredListBox, MaterialsRequiredSearchBox.Text, materialsRequiredListBoxItemsRemoved, null);

                ToggleButtonsBasedOnListBox(ArtefactsAddedListBox, artefactsAddedButtons);
            }
            else if(e.Key == Key.Enter)
            {
                ListBoxRemoveItemsFunction<artefact>(ArtefactListBox, ArtefactsAddedListBox);
                FilterListBoxItems(ArtefactListBox, ArtefactTextBox.Text, artefactListBoxItemsRemoved, null);

                GetRequiredMaterials(ArtefactsAddedListBox, MaterialsAddedListBox, MaterialsRequiredListBox, allMaterials, artefactAddBoxItemsRemoved, 
                                     materialListBoxItemsRemoved, materialsRequiredListBoxItemsRemoved, materialsRequiredListBoxItemsEnough);
                FilterListBoxItems(MaterialsRequiredListBox, MaterialsRequiredSearchBox.Text, materialsRequiredListBoxItemsRemoved, null);

                ToggleButtonsBasedOnListBox(ArtefactsAddedListBox, artefactsAddedButtons);
            }
        }

        private void ArtefactRemoveBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HideSearchText(ArtefactRemoveBox, ArtefactAmountRemoveTextBlock);
            WhichButton(ArtefactRemoveBox, ArtefactRemoveButton, ArtefactChangeButton);
        }

        //ArtefactUpButton
        private void ArtefactUpButton_Click(object sender, RoutedEventArgs e)
        {
            ListBoxUpOrDownButton<artefact>(true, ArtefactsAddedListBox);
            FilterListBoxItems(ArtefactsAddedListBox, ArtefactRemoveSearchBox.Text, artefactAddBoxItemsRemoved, null);

            GetRequiredMaterials(ArtefactsAddedListBox, MaterialsAddedListBox, MaterialsRequiredListBox, allMaterials, artefactAddBoxItemsRemoved,
                                 materialListBoxItemsRemoved, materialsRequiredListBoxItemsRemoved, materialsRequiredListBoxItemsEnough);
            FilterListBoxItems(MaterialsRequiredListBox, MaterialsRequiredSearchBox.Text, materialsRequiredListBoxItemsRemoved, null);

            UpdateTotalExperienceGainedMain();
        }

        //ArtefactDownButton
        private void ArtefactDownButton_Click(object sender, RoutedEventArgs e)
        {
            ListBoxUpOrDownButton<artefact>(false, ArtefactsAddedListBox);
            FilterListBoxItems(ArtefactsAddedListBox, ArtefactRemoveSearchBox.Text, artefactAddBoxItemsRemoved, null);

            GetRequiredMaterials(ArtefactsAddedListBox, MaterialsAddedListBox, MaterialsRequiredListBox, allMaterials, artefactAddBoxItemsRemoved,
                                 materialListBoxItemsRemoved, materialsRequiredListBoxItemsRemoved, materialsRequiredListBoxItemsEnough);
            FilterListBoxItems(MaterialsRequiredListBox, MaterialsRequiredSearchBox.Text, materialsRequiredListBoxItemsRemoved, null);

            UpdateTotalExperienceGainedMain();
        }

        //ArtefactRemoveButton
        private void ArtefactRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            ListBoxRemoveItemsFunction<artefact>(ArtefactListBox, ArtefactsAddedListBox);
            FilterListBoxItems(ArtefactListBox, ArtefactTextBox.Text, artefactListBoxItemsRemoved, null);

            GetRequiredMaterials(ArtefactsAddedListBox, MaterialsAddedListBox, MaterialsRequiredListBox, allMaterials, artefactAddBoxItemsRemoved,
                                 materialListBoxItemsRemoved, materialsRequiredListBoxItemsRemoved, materialsRequiredListBoxItemsEnough);
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

            GetRequiredMaterials(ArtefactsAddedListBox, MaterialsAddedListBox, MaterialsRequiredListBox, allMaterials, artefactAddBoxItemsRemoved, 
                                 materialListBoxItemsRemoved, materialsRequiredListBoxItemsRemoved, materialsRequiredListBoxItemsEnough);
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
            FilterListBoxItems(MaterialListBox, MaterialSearchBox.Text, materialListBoxItemsRemoved, materialListBoxSelectedItemsRemoved);

            ToggleButtonsBasedOnListBox(materialListBox, materialAddButton);
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
            Process.Start(new ProcessStartInfo("cmd", $"/c start {materialListBoxRightClicked.URL}") { CreateNoWindow = true });
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

                GetRequiredMaterials(ArtefactsAddedListBox, MaterialsAddedListBox, MaterialsRequiredListBox, allMaterials, artefactAddBoxItemsRemoved,
                                     materialListBoxItemsRemoved, materialsRequiredListBoxItemsRemoved, materialsRequiredListBoxItemsEnough);
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

            GetRequiredMaterials(ArtefactsAddedListBox, MaterialsAddedListBox, MaterialsRequiredListBox, allMaterials, artefactAddBoxItemsRemoved,
                                 materialListBoxItemsRemoved, materialsRequiredListBoxItemsRemoved, materialsRequiredListBoxItemsEnough);
            FilterListBoxItems(MaterialsRequiredListBox, MaterialsRequiredSearchBox.Text, materialsRequiredListBoxItemsRemoved, null);

            ToggleButtonsBasedOnListBox(materialListBox, materialAddButton);

            MaterialAddBox.Text = "1";
        }

        //MaterialRemoveSearchBox
        private void MaterialRemoveSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HideSearchText(MaterialRemoveSearchBox, MaterialRemoveSearchBlock);
            FilterListBoxItems(MaterialsAddedListBox, MaterialRemoveSearchBox.Text, materialAddBoxItemsRemoved, materialAddBoxSelectedItemsRemoved);
            ToggleButtonsBasedOnListBox(MaterialsAddedListBox, materialsAddedButtons);
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
            Process.Start(new ProcessStartInfo("cmd", $"/c start {materialAddBoxRightClicked.URL}") { CreateNoWindow = true });
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

                GetRequiredMaterials(ArtefactsAddedListBox, MaterialsAddedListBox, MaterialsRequiredListBox, allMaterials, artefactAddBoxItemsRemoved, 
                                     materialListBoxItemsRemoved, materialsRequiredListBoxItemsRemoved, materialsRequiredListBoxItemsEnough);
                FilterListBoxItems(MaterialsRequiredListBox, MaterialsRequiredSearchBox.Text, materialsRequiredListBoxItemsRemoved, null);

                ToggleButtonsBasedOnListBox(MaterialsAddedListBox, materialsAddedButtons);
            }
            else if (e.Key == Key.Enter)
            {
                ListBoxRemoveItemsFunction<listBoxItem>(MaterialListBox, MaterialsAddedListBox);
                FilterListBoxItems(MaterialListBox, MaterialSearchBox.Text, materialListBoxItemsRemoved, null);

                GetRequiredMaterials(ArtefactsAddedListBox, MaterialsAddedListBox, MaterialsRequiredListBox, allMaterials, artefactAddBoxItemsRemoved, 
                                     materialListBoxItemsRemoved, materialsRequiredListBoxItemsRemoved, materialsRequiredListBoxItemsEnough);
                FilterListBoxItems(MaterialsRequiredListBox, MaterialsRequiredSearchBox.Text, materialsRequiredListBoxItemsRemoved, null);

                ToggleButtonsBasedOnListBox(MaterialsAddedListBox, materialsAddedButtons);
            }
        }

        private void MaterialRemoveBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HideSearchText(MaterialRemoveBox, MaterialAmountRemoveTextBlock);
            WhichButton(MaterialRemoveBox, MaterialRemoveButton, MaterialChangeButton);
        }

        //MaterialUpButton
        private void MaterialUpButton_Click(object sender, RoutedEventArgs e)
        {
            ListBoxUpOrDownButton<listBoxItem>(true, MaterialsAddedListBox);
            FilterListBoxItems(MaterialsAddedListBox, MaterialRemoveSearchBox.Text, materialAddBoxItemsRemoved, null);

            GetRequiredMaterials(ArtefactsAddedListBox, MaterialsAddedListBox, MaterialsRequiredListBox, allMaterials, artefactAddBoxItemsRemoved,
                                 materialListBoxItemsRemoved, materialsRequiredListBoxItemsRemoved, materialsRequiredListBoxItemsEnough);
            FilterListBoxItems(MaterialsRequiredListBox, MaterialsRequiredSearchBox.Text, materialsRequiredListBoxItemsRemoved, null);
        }

        //MaterialDownButton
        private void MaterialDownButton_Click(object sender, RoutedEventArgs e)
        {
            ListBoxUpOrDownButton<listBoxItem>(false, MaterialsAddedListBox);
            FilterListBoxItems(MaterialsAddedListBox, MaterialRemoveSearchBox.Text, materialAddBoxItemsRemoved, null);

            GetRequiredMaterials(ArtefactsAddedListBox, MaterialsAddedListBox, MaterialsRequiredListBox, allMaterials, artefactAddBoxItemsRemoved,
                                 materialListBoxItemsRemoved, materialsRequiredListBoxItemsRemoved, materialsRequiredListBoxItemsEnough);
            FilterListBoxItems(MaterialsRequiredListBox, MaterialsRequiredSearchBox.Text, materialsRequiredListBoxItemsRemoved, null);
        }

        //MaterialRemoveButton
        private void MaterialRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            ListBoxRemoveItemsFunction<listBoxItem>(MaterialListBox, MaterialsAddedListBox);
            FilterListBoxItems(MaterialListBox, MaterialSearchBox.Text, materialListBoxItemsRemoved, null);

            GetRequiredMaterials(ArtefactsAddedListBox, MaterialsAddedListBox, MaterialsRequiredListBox, allMaterials, artefactAddBoxItemsRemoved, 
                                 materialListBoxItemsRemoved, materialsRequiredListBoxItemsRemoved, materialsRequiredListBoxItemsEnough);
            FilterListBoxItems(MaterialsRequiredListBox, MaterialsRequiredSearchBox.Text, materialsRequiredListBoxItemsRemoved, null);

            ToggleButtonsBasedOnListBox(MaterialsAddedListBox, materialsAddedButtons);
            MaterialRemoveBox.Text = "";
        }

        //MaterialChangeButton
        private void MaterialChangeButton_Click(object sender, RoutedEventArgs e)
        {
            ListBoxChangeAmount<listBoxItem>(MaterialsAddedListBox, MaterialRemoveBox);
            FilterListBoxItems(MaterialsAddedListBox, MaterialRemoveSearchBox.Text, materialAddBoxItemsRemoved, null);

            GetRequiredMaterials(ArtefactsAddedListBox, MaterialsAddedListBox, MaterialsRequiredListBox, allMaterials, artefactAddBoxItemsRemoved, 
                                 materialListBoxItemsRemoved, materialsRequiredListBoxItemsRemoved, materialsRequiredListBoxItemsEnough);
            FilterListBoxItems(MaterialsRequiredListBox, MaterialsRequiredSearchBox.Text, materialsRequiredListBoxItemsRemoved, null);

            ToggleButtonsBasedOnListBox(MaterialsAddedListBox, materialsAddedButtons);
            MaterialRemoveBox.Text = "";
        }

        //MaterialsRequiredSearchBox
        private void MaterialsRequiredSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HideSearchText(MaterialsRequiredSearchBox, MaterialsRequiredSearchBlock);
            FilterListBoxItems(MaterialsRequiredListBox, MaterialsRequiredSearchBox.Text, materialsRequiredListBoxItemsRemoved, null);
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
        }

        private void MaterialsRequiredListBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        //MaterialsRequiredListBox ContextMenu
        private void MaterialsRequiredBoxWiki_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start {materialsRequiredListBoxRightClicked.URL}") { CreateNoWindow = true });
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        
    }
}