﻿using System;
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
using static ArchHelper2.Artefacts;
using static ArchHelper2.Materials;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Diagnostics;

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
        public static string appFolderPath = "";

        //Declaring allArtefact stuff
        public static List<artefact> allArtefacts = new List<artefact>();
        public static List<artefact> allArtefactsHardCoded = new List<artefact>(ImportArtefacts(artefactsHardCoded));
        public static List<artefact> allArtefactsFromFile = new List<artefact>();

        //Declaring Artefact ListBox stuff
        public static ListBox artefactListBox = new ListBox();
        public static ListBox artefactsAddedListBox = new ListBox();

        List<artefact> artefactListBoxItemsRemoved = new List<artefact>();
        List<artefact> artefactListBoxSelectedItemsRemoved = new List<artefact>();
        List<artefact> artefactListBoxSelectedItemsTrackedBefore = new List<artefact>();
        artefact artefactListBoxRightClicked = new artefact();

        List<artefact> artefactAddBoxItemsRemoved = new List<artefact>();
        List<artefact> artefactAddBoxSelectedItemsRemoved = new List<artefact>();
        List<artefact> artefactAddBoxSelectedItemsTrackedBefore = new List<artefact>();
        artefact artefactAddBoxRightClicked = new artefact();

        //Declaring allMaterial stuff
        public static List<listBoxItem> allMaterials = new List<listBoxItem>();
        public static List<listBoxItem> allMaterialsHardCoded = new List<listBoxItem>(ImportMaterials(materialsHardCoded));
        public static List<listBoxItem> allMaterialsFromFile = new List<listBoxItem>();

        //Declaring Material ListBox stuff
        public static ListBox materialListBox = new ListBox();
        public static ListBox materialsAddedListBox = new ListBox();

        List<listBoxItem> materialListBoxItemsRemoved = new List<listBoxItem>();
        List<listBoxItem> materialListBoxSelectedItemsRemoved = new List<listBoxItem>();
        List<listBoxItem> materialListBoxSelectedItemsTrackedBefore = new List<listBoxItem>();
        listBoxItem materialListBoxRightClicked = new listBoxItem();

        List<listBoxItem> materialAddBoxItemsRemoved = new List<listBoxItem>();
        List<listBoxItem> materialAddBoxSelectedItemsRemoved = new List<listBoxItem>();
        List<listBoxItem> materialAddBoxSelectedItemsTrackedBefore = new List<listBoxItem>();
        listBoxItem materialAddBoxRightClicked = new listBoxItem();

        //Declaring MaterialsRequired ListBox stuff
        List<listBoxItem> materialsRequiredListBoxItemsRemoved = new List<listBoxItem>();
        List<listBoxItem> materialsRequiredListBoxItemsEnough = new List<listBoxItem>();
        List<listBoxItem> materialsRequiredListBoxSelectedItemsTrackedBefore = new List<listBoxItem>();
        listBoxItem materialsRequiredListBoxRightClicked = new listBoxItem();

        public MainWindow()
        {
            InitializeComponent();
            ConsoleWriteLine("(" + DateTime.Now.ToString() + ") MainWindow opened", debugGeneral);

            artefactListBox = ArtefactListBox;
            artefactsAddedListBox = ArtefactsAddedListBox;
            materialListBox = MaterialListBox;
            materialsAddedListBox = MaterialsAddedListBox;

            ArtefactAddBox.Text = "1";
            MaterialAddBox.Text = "1";

            allArtefacts = allArtefactsHardCoded;
            BuildListBoxes(allArtefacts);

            allMaterials = allMaterialsHardCoded;
            BuildListBoxes(allMaterials);
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
            ImportArtefactsTextBox.Text = appFolderPath;
        }

        ///////////////////Artefact ListBox Stuff///////////////////

        //ArtefactTextBox (Artefact search box)
        private void ArtefactTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HideSearchText(ArtefactTextBox, ArtefactTextBlock);
            FilterListBoxItems(ArtefactListBox, ArtefactTextBox.Text, artefactListBoxItemsRemoved, artefactListBoxSelectedItemsRemoved);
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
        private void ArtefactsAddedListBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ToggleUpDownButtons(ArtefactsAddedListBox, ArtefactUpButton, ArtefactDownButton);
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
        }

        private void ArtefactListBoxUnselectAll_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ArtefactListBox.UnselectAll();
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

            ArtefactAddBox.Text = "1";
        }

        //ArtefactRemoveSearchBox
        private void ArtefactRemoveSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HideSearchText(ArtefactRemoveSearchBox, ArtefactRemoveSearchBlock);
            FilterListBoxItems(ArtefactsAddedListBox, ArtefactRemoveSearchBox.Text, artefactAddBoxItemsRemoved, artefactAddBoxSelectedItemsRemoved);
        }

        //ArtefactsAddedListBox
        private void ArtefactsAddedListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ToggleUpDownButtons(ArtefactsAddedListBox, ArtefactUpButton, ArtefactDownButton);
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
            ToggleUpDownButtons(ArtefactsAddedListBox, ArtefactUpButton, ArtefactDownButton);
        }

        private void ArtefactsAddedBoxUnselectAll_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ArtefactsAddedListBox.UnselectAll();
            ToggleUpDownButtons(ArtefactsAddedListBox, ArtefactUpButton, ArtefactDownButton);
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
            }
            else if(e.Key == Key.Enter)
            {
                ListBoxRemoveItemsFunction<artefact>(ArtefactListBox, ArtefactsAddedListBox);
                FilterListBoxItems(ArtefactListBox, ArtefactTextBox.Text, artefactListBoxItemsRemoved, null);

                GetRequiredMaterials(ArtefactsAddedListBox, MaterialsAddedListBox, MaterialsRequiredListBox, allMaterials, artefactAddBoxItemsRemoved, 
                                     materialListBoxItemsRemoved, materialsRequiredListBoxItemsRemoved, materialsRequiredListBoxItemsEnough);
                FilterListBoxItems(MaterialsRequiredListBox, MaterialsRequiredSearchBox.Text, materialsRequiredListBoxItemsRemoved, null);
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
        }

        //ArtefactDownButton
        private async void ArtefactDownButton_Click(object sender, RoutedEventArgs e)
        {
            ListBoxUpOrDownButton<artefact>(false, ArtefactsAddedListBox);
            FilterListBoxItems(ArtefactsAddedListBox, ArtefactRemoveSearchBox.Text, artefactAddBoxItemsRemoved, null);
        }

        //ArtefactRemoveButton
        private void ArtefactRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            ListBoxRemoveItemsFunction<artefact>(ArtefactListBox, ArtefactsAddedListBox);
            FilterListBoxItems(ArtefactListBox, ArtefactTextBox.Text, artefactListBoxItemsRemoved, null);

            GetRequiredMaterials(ArtefactsAddedListBox, MaterialsAddedListBox, MaterialsRequiredListBox, allMaterials, artefactAddBoxItemsRemoved,
                                 materialListBoxItemsRemoved, materialsRequiredListBoxItemsRemoved, materialsRequiredListBoxItemsEnough);
            FilterListBoxItems(MaterialsRequiredListBox, MaterialsRequiredSearchBox.Text, materialsRequiredListBoxItemsRemoved, null);

            ToggleUpDownButtons(ArtefactsAddedListBox, ArtefactUpButton, ArtefactDownButton);
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

            ToggleUpDownButtons(ArtefactsAddedListBox, ArtefactUpButton, ArtefactDownButton);
            ArtefactRemoveBox.Text = "";
        }


        ///////////////////Material ListBox Stuff///////////////////

        //MaterialSearchBox
        private void MaterialSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HideSearchText(MaterialSearchBox, MaterialSearchBlock);
            FilterListBoxItems(MaterialListBox, MaterialSearchBox.Text, materialListBoxItemsRemoved, materialListBoxSelectedItemsRemoved);
        }

        //MaterialListBox
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
        }

        private void MaterialListBoxUnselectAll_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MaterialListBox.UnselectAll();
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

            MaterialAddBox.Text = "1";
        }

        //MaterialRemoveSearchBox
        private void MaterialRemoveSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HideSearchText(MaterialRemoveSearchBox, MaterialRemoveSearchBlock);
            FilterListBoxItems(MaterialsAddedListBox, MaterialRemoveSearchBox.Text, materialAddBoxItemsRemoved, materialAddBoxSelectedItemsRemoved);
        }

        //MaterialsAddedListBox
        private void MaterialsAddedListBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ToggleUpDownButtons(MaterialsAddedListBox, MaterialUpButton, MaterialDownButton);
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
            ToggleUpDownButtons(MaterialsAddedListBox, MaterialUpButton, MaterialDownButton);
        }

        private void MaterialsAddedListBoxUnselectAll_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MaterialsAddedListBox.UnselectAll();
            ToggleUpDownButtons(MaterialsAddedListBox, MaterialUpButton, MaterialDownButton);
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
            }
            else if (e.Key == Key.Enter)
            {
                ListBoxRemoveItemsFunction<listBoxItem>(MaterialListBox, MaterialsAddedListBox);
                FilterListBoxItems(MaterialListBox, MaterialSearchBox.Text, materialListBoxItemsRemoved, null);

                GetRequiredMaterials(ArtefactsAddedListBox, MaterialsAddedListBox, MaterialsRequiredListBox, allMaterials, artefactAddBoxItemsRemoved, 
                                     materialListBoxItemsRemoved, materialsRequiredListBoxItemsRemoved, materialsRequiredListBoxItemsEnough);
                FilterListBoxItems(MaterialsRequiredListBox, MaterialsRequiredSearchBox.Text, materialsRequiredListBoxItemsRemoved, null);
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
        }

        //MaterialDownButton
        private void MaterialDownButton_Click(object sender, RoutedEventArgs e)
        {
            ListBoxUpOrDownButton<listBoxItem>(false, MaterialsAddedListBox);
            FilterListBoxItems(MaterialsAddedListBox, MaterialRemoveSearchBox.Text, materialAddBoxItemsRemoved, null);
        }

        //MaterialRemoveButton
        private void MaterialRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            ListBoxRemoveItemsFunction<listBoxItem>(MaterialListBox, MaterialsAddedListBox);
            FilterListBoxItems(MaterialListBox, MaterialSearchBox.Text, materialListBoxItemsRemoved, null);

            GetRequiredMaterials(ArtefactsAddedListBox, MaterialsAddedListBox, MaterialsRequiredListBox, allMaterials, artefactAddBoxItemsRemoved, 
                                 materialListBoxItemsRemoved, materialsRequiredListBoxItemsRemoved, materialsRequiredListBoxItemsEnough);
            FilterListBoxItems(MaterialsRequiredListBox, MaterialsRequiredSearchBox.Text, materialsRequiredListBoxItemsRemoved, null);

            ToggleUpDownButtons(MaterialsAddedListBox, MaterialUpButton, MaterialDownButton);
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

            ToggleUpDownButtons(MaterialsAddedListBox, MaterialUpButton, MaterialDownButton);
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

        
    }
}