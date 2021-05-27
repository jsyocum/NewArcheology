using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Linq;
using System.Text;
using static System.String;
using static System.StringComparison;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using static ArchHelper2.MainWindow;
using static ArchHelper2.CSharpHelper;
using static ArchHelper2.DeprecatedHelpers;
using static ArchHelper2.DebugConsole;
using static ArchHelper2.DebugConsoleTools;
using static ArchHelper2.ArchDebugConsoleTools;
using System.Windows.Input;

namespace ArchHelper2
{
    class XAMLHelper
    {
        /// <summary>
        /// Hides a TextBlock's text when a TextBox has text in it.
        /// </summary>
        /// <param name="textBoxName">The TextBox receiving inputs from the user.</param>
        /// <param name="textBlockName">The TextBlock wanting to be hidden.</param>
        public static void HideSearchText(TextBox textBoxName, TextBlock textBlockName)
        {
            textBlockName.Visibility = Visibility.Hidden;
            switch (textBoxName.Text.Length)
            {
                case 0:
                    textBlockName.Visibility = Visibility.Visible;
                    break;
            }
        }

        /// <summary>
        /// Makes a TextBox refuse input if any item in a specific ListBox is selected.
        /// </summary>
        /// <param name="textBox">The TextBox you want to refuse input in.</param>
        /// <param name="listBox">The ListBox you want to monitor for selections.</param>
        public static void ToggleSearchBar(TextBox textBox, ListBox listBox)
        {
            switch (listBox.SelectedItems.Count)
            {
                case 0:
                    textBox.IsReadOnly = false;
                    break;
                default:
                    textBox.IsReadOnly = true;
                    break;
            }
        }

        public static void BuildListBoxes<T>(List<T> allItems)
        {
            if (typeof(T) == typeof(artefact))
            {
                artefactListBox.Items.Clear();
                artefactsAddedListBox.Items.Clear();

                AddItemsToListBox(artefactListBox, allItems);
                SortListBoxItems<artefact>(artefactListBox);
            }
            else if(typeof(T) == typeof(listBoxItem))
            {
                materialListBox.Items.Clear();
                materialsAddedListBox.Items.Clear();

                AddItemsToListBox(materialListBox, allItems);
                SortListBoxItems<listBoxItem>(materialListBox);
            }
        }

        /// <summary>
        /// Filters a ListBox's item contents based on a user's search term, while still preserving their selected items.
        /// </summary>
        /// <param name="listBox">The ListBox that will be filtered.</param>
        /// <param name="searchTerm">The search term the user inputs.</param>
        /// <param name="listBoxItemsRemoved">The running list of filtered list box items.</param>
        /// <param name="listBoxSelectedItemsRemoved">The running list of list box items that were filtered while they were still selected by the user.</param>
        public static void FilterListBoxItems<T>(ListBox listBox, string searchTerm, List<T> listBoxItemsRemoved, List<T> listBoxSelectedItemsRemoved)
        {
            //Grab the ListBox's currently selected items.
            List<T> listBoxItemsSelected = new List<T>(GetItemsFromListBox<T>(listBox, 2));

            //Add any items that were previously removed from the ListBox back in, then clear that list.
            AddItemsToListBox(listBox, listBoxItemsRemoved);
            listBoxItemsRemoved.Clear();

            //Grab the ListBox's items now that everything has been added back in.
            List<T> listBoxItems = new List<T>(GetItemsFromListBox<T>(listBox, 1));

            //Determine which items should be removed based on the search term, remove them, and add them to the listBoxItemsRemoved list.
            List<int> listBoxItemsToRemove = new List<int>(FilterItemList(listBoxItems, StringToList(searchTerm)));
            RemoveItemsFromListBox(listBox, listBoxItemsToRemove, listBoxItemsRemoved);

            //Sort the ListBox and set the user's selected items back if they are still an option.
            SortListBoxItems<T>(listBox);
            SetListBoxSelectedItems(listBox, listBoxItemsSelected, listBoxItemsRemoved, listBoxSelectedItemsRemoved);
        }

        /// <summary>
        /// When an item in a ListBox is right clicked, this function figures out which one.
        /// </summary>
        /// <typeparam name="T">The type of items stored in the ListBox.</typeparam>
        /// <param name="listBox">The ListBox being right clicked.</param>
        /// <param name="listBoxSelectedBefore">The selected items that should have been recorded before the right click occurred.</param>
        /// <param name="listBoxItemRightClicked">The variable which contains the right clicked item.</param>
        /// <param name="debugLines">The relevant list of strings that the debugger will take for this function.</param>
        public static void ListBoxItemRightClickFunction<T>(ListBox listBox, List<T> listBoxSelectedBefore, ref T listBoxItemRightClicked, List<string> debugLines)
        {
            List<T> listBoxSelectedAfter = new List<T>(GetItemsFromListBox<T>(listBox, 2));
            listBoxItemRightClicked = GetRightClickedItem(listBoxSelectedBefore, listBoxSelectedAfter);

            if (listBoxSelectedBefore.IndexOf(listBoxItemRightClicked) == -1)
            {
                listBox.SelectedItems.Remove(listBoxItemRightClicked);
            }
            else
            {
                listBox.SelectedItems.Add(listBoxItemRightClicked);
            }

            if (typeof(T) == typeof(artefact))
            {
                artefact listBoxItemRightClickedToArtefact = ChangeType<T, artefact>(listBoxItemRightClicked);

                string debugLine = "(" + DateTime.Now.ToString() + ") Artefact right clicked: " + listBoxItemRightClickedToArtefact.arteName + "; URL: " + listBoxItemRightClickedToArtefact.URL;
                ConsoleWriteLine(debugLine, debugLines);
            }
            else if (typeof(T) == typeof(listBoxItem))
            {
                listBoxItem listBoxItemRightClickedToListBoxItem = ChangeType<T, listBoxItem>(listBoxItemRightClicked);

                string debugLine = "(" + DateTime.Now.ToString() + ") Material right clicked: " + listBoxItemRightClickedToListBoxItem.ItemName + "; URL: " + listBoxItemRightClickedToListBoxItem.URL;
                ConsoleWriteLine(debugLine, debugLines);
            }
        }

        /// <summary>
        /// Note: Wherever this function is implemented, it will have to be followed by a FilterListBoxItems function.
        /// </summary>
        /// <param name="listBoxOG"></param>
        /// <param name="listBoxTarget"></param>
        /// <param name="amountTextBox"></param>
        public static void ListBoxChangeAmount<T>(ListBox listBoxTarget, TextBox amountTextBox)
        {
            if(listBoxTarget.SelectedItems.Count > 0)
            {
                int amount = 0;
                int.TryParse(amountTextBox.Text, out amount);

                if(typeof(T) == typeof(listBoxItem))
                {
                    List<listBoxItem> changedListBoxItems = new List<listBoxItem>(GetItemsFromListBox<listBoxItem>(listBoxTarget, 2));

                    //Remove all of the items that are having their amount changed from the ListBox.
                    for (int i = 0; i < changedListBoxItems.Count; ++i)
                    {
                        listBoxTarget.Items.Remove(changedListBoxItems[i]);
                    }

                    //Change all of the removed items' amounts to the amount specified by the user.
                    for (int i = 0; i < changedListBoxItems.Count; ++i)
                    {
                        changedListBoxItems[i].ItemAmount = amount;
                    }

                    //Add the newly changed items back in.
                    AddItemsToListBox(listBoxTarget, changedListBoxItems);
                }
                else if(typeof(T) == typeof(artefact))
                {
                    List<artefact> changedListBoxItems = new List<artefact>(GetItemsFromListBox<artefact>(listBoxTarget, 2));
                    List<artefact> newChangedListBoxItems = new List<artefact>();

                    //Remove all of the items that are having their amount changed from the ListBox.
                    for (int i = 0; i < changedListBoxItems.Count; ++i)
                    {
                        listBoxTarget.Items.Remove(changedListBoxItems[i]);
                    }

                    //Change all of the removed items' amounts to the amount specified by the user.
                    foreach (artefact arte in changedListBoxItems)
                    {
                        newChangedListBoxItems.Add(new artefact(arte.arteName, arte.experience, arte.matAmountsNeeded, arte.matsNeeded, amount, arte.URL));
                    }

                    //Add the newly changed items back in.
                    AddItemsToListBox(listBoxTarget, newChangedListBoxItems);
                }
            }
        }

        /// <summary>
        /// Decides which button should be showed based on whether or not there is text in a TextBox.
        /// </summary>
        /// <param name="amountTextBox">The TextBox you're monitoring.</param>
        /// <param name="removeButton">The button that will lose visibility if there is text.</param>
        /// <param name="changeButton">The button that will gain visibility if there is text.</param>
        public static void WhichButton(TextBox amountTextBox, Button removeButton, Button changeButton)
        {
            int amount = 0;
            int.TryParse(amountTextBox.Text, out amount);

            if (amountTextBox.Text.Length > 0 && amount != 0)
            {
                removeButton.Visibility = Visibility.Hidden;
                changeButton.Visibility = Visibility.Visible;
            }
            else
            {
                removeButton.Visibility = Visibility.Visible;
                changeButton.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Adds the selected items of one ListBox to another ListBox. Also adds them to a Dictionary with a value provided by a TextBox. The selected items are removed from the first ListBox.
        /// </summary>
        /// <param name="listBoxOG">The ListBox losing its selected items.</param>
        /// <param name="listBoxTarget">The ListBox gaining items.</param>
        /// <param name="amountTextBox">The amount each item should be assigned when moved to the new ListBox.</param>
        public static void ListBoxAddItemsFunction<T>(ListBox listBoxOG, ListBox listBoxTarget, TextBox amountTextBox)
        {
            int amount = 0;
            int.TryParse(amountTextBox.Text, out amount);

            if (amount != 0 && listBoxOG.SelectedItems.Count > 0)
            {
                if(typeof(T) == typeof(listBoxItem))
                {
                    List<listBoxItem> listBoxOGSelectedItems = new List<listBoxItem>(GetItemsFromListBox<listBoxItem>(listBoxOG, 2));

                    for (int i = 0; i < listBoxOGSelectedItems.Count; ++i)
                    {
                        listBoxOGSelectedItems[i].ItemAmount = amount;
                    }

                    AddItemsToListBox(listBoxTarget, listBoxOGSelectedItems);
                    for (int i = listBoxOG.SelectedItems.Count - 1; i >= 0; --i)
                    {
                        listBoxOG.Items.Remove(listBoxOG.SelectedItems[i]);
                    }
                }
                else if(typeof(T) == typeof(artefact))
                {
                    List<artefact> listBoxOGSelectedItems = new List<artefact>(GetItemsFromListBox<artefact>(listBoxOG, 2));
                    List<artefact> newListBoxOGSelectedItems = new List<artefact>();

                    foreach (artefact arte in listBoxOGSelectedItems)
                    {
                        newListBoxOGSelectedItems.Add(new artefact(arte.arteName, arte.experience, arte.matAmountsNeeded, arte.matsNeeded, amount, arte.URL));
                    }

                    AddItemsToListBox(listBoxTarget, newListBoxOGSelectedItems);
                    for (int i = listBoxOG.SelectedItems.Count - 1; i >= 0; --i)
                    {
                        listBoxOG.Items.Remove(listBoxOG.SelectedItems[i]);
                    }
                }

                
            }
        }

        /// <summary>
        /// Removes selected items from a target ListBox and adds them back into the original ListBox.
        /// </summary>
        /// <param name="listBoxOG">The original ListBox having items added back into.</param>
        /// <param name="listBoxTarget">The target ListBox having items removed.</param>
        public static void ListBoxRemoveItemsFunction<T>(ListBox listBoxOG, ListBox listBoxTarget)
        {
            switch (listBoxTarget.SelectedItems.Count)
            {
                case 0:
                    break;

                default:
                    List<T> listBoxTargetSelectedItems = new List<T>(GetItemsFromListBox<T>(listBoxTarget, 2));

                    AddItemsToListBox(listBoxOG, listBoxTargetSelectedItems);

                    for (int i = listBoxTarget.SelectedItems.Count - 1; i >= 0; --i)
                    {
                        listBoxTarget.Items.Remove(listBoxTarget.SelectedItems[i]);
                    }

                    break;
            }
        }

        /// <summary>
        /// Adds a list of listBoxItems to a ListBox.
        /// </summary>
        /// <param name="listBox">The ListBox you want to add the items to.</param>
        /// <param name="listBoxItems">The list of items you want to add.</param>
        public static void AddItemsToListBox<T>(ListBox listBox, List<T> listBoxItems)
        {
            foreach (T item in listBoxItems)
            {
                listBox.Items.Add(item);
            }
        }

        /// <summary>
        /// Removed a list of listBoxItems from a ListBox. Only use if you know for certain that the ListBox contains the list of items you want removed.
        /// </summary>
        /// <param name="listBox">The ListBox you want the items removed from.</param>
        /// <param name="listBoxItems">The items you want removed from the ListBox.</param>
        public static void RemoveListBoxItemsFromListBox<T>(ListBox listBox, List<T> listBoxItems)
        {
            foreach (T item in listBoxItems)
            {
                listBox.Items.Remove(item);
            }
        }

        /// <summary>
        /// Remove a list of items from a ListBox based on their index in the ListBox.
        /// </summary>
        /// <param name="listBox">The ListBox you are removing strings from.</param>
        /// <param name="listBoxItemsToRemove">The indexes of the items being removed.</param>
        /// <param name="listBoxItemsRemoved">A list of listBoxItems that have been removed from the ListBox.</param>
        public static void RemoveItemsFromListBox<T>(ListBox listBox, List<int> listBoxItemsToRemove, List<T> listBoxItemsRemoved)
        {
            List<T> listBoxItems = new List<T>(GetItemsFromListBox<T>(listBox, 1));
            for (int i = listBoxItemsToRemove.Count - 1; i >= 0; --i)
            {
                listBox.Items.RemoveAt(listBoxItemsToRemove[i]);
                listBoxItemsRemoved.Add(listBoxItems[listBoxItemsToRemove[i]]);
            }
        }

        /// <summary>
        /// Sorts a ListBox of listBoxItems by their ItemNames.
        /// </summary>
        /// <param name="listBox">The ListBox you want sorted.</param>
        public static void SortListBoxItems<T>(ListBox listBox)
        {
            List<T> listBoxSelectedItems = GetItemsFromListBox<T>(listBox, 2);
            
            if (typeof(T) == typeof(listBoxItem))
            {
                List<listBoxItem> listBoxItems = new List<listBoxItem>(GetItemsFromListBox<listBoxItem>(listBox, 1));
                List<listBoxItem> orderedListBoxItems;
                orderedListBoxItems = listBoxItems.OrderBy(listBoxItem => listBoxItem.ItemName).ToList();

                listBox.Items.Clear();
                AddItemsToListBox(listBox, orderedListBoxItems);
            }
            else if (typeof(T) == typeof(artefact))
            {
                List<artefact> listBoxItems = new List<artefact>(GetItemsFromListBox<artefact>(listBox, 1));
                List<artefact> orderedListBoxItems;
                orderedListBoxItems = listBoxItems.OrderBy(artefact => artefact.arteName).ToList();

                listBox.Items.Clear();
                AddItemsToListBox(listBox, orderedListBoxItems);
            }

            SetListBoxSelectedItems(listBox, listBoxSelectedItems, null, null);
        }

        /// <summary>
        /// Grabs the Items or SelecedItems from a ListBox and converts them into a list of listBoxItems.
        /// </summary>
        /// <param name="listBox">The ListBox you're grabbing from.</param>
        /// <param name="mode">1 = Grab Items. 2 = Grab SelectedItems.</param>
        /// <returns></returns>
        public static List<T> GetItemsFromListBox<T>(ListBox listBox, int mode)
        {
            List<T> items = new List<T>();
            switch (mode)
            {
                case 1:
                    items = listBox.Items.OfType<T>().ToList();
                    break;

                case 2:
                    items = listBox.SelectedItems.OfType<T>().ToList();
                    break;
            }

            return items;
        }

        /// <summary>
        /// Sets a ListBox's SelectedItems based on lists of selected items passed to it.
        /// </summary>
        /// <param name="listBox">The ListBox you want to set the selected items in.</param>
        /// <param name="listBoxSelected">A list of the ListBox's previously selected items.</param>
        /// <param name="listBoxItemsRemoved">The ListBox's list of items that have already been removed. Included here to be checked against in case the user filtered out a selected item.</param>
        /// <param name="listBoxSelectedItemsRemoved">If a ListBox has selected items removed, they are added to this list. This list is then checked against the current items to see if they are to be added back in.</param>
        public static void SetListBoxSelectedItems<T>(ListBox listBox, List<T> listBoxSelected, List<T> listBoxItemsRemoved, List<T> listBoxSelectedItemsRemoved)
        {
            //If there have selected items have been removed, check them against all the items in the ListBox at the moment. Add them back if they match.
            if(listBoxSelectedItemsRemoved != null)
            {
                for (int i = listBoxSelectedItemsRemoved.Count - 1; i >= 0; --i)
                {
                    if (listBox.Items.Contains(listBoxSelectedItemsRemoved[i]))
                    {
                        listBox.SelectedItems.Add(listBoxSelectedItemsRemoved[i]);
                        listBoxSelectedItemsRemoved.RemoveAt(i);
                    }
                }
            }

            //Runs through the list of previously selected items handed to the function.
            for (int i = 0; i < listBoxSelected.Count; ++i)
            {
                //If the ListBox currently has the item, it will select it.
                if (listBox.Items.Contains(listBoxSelected[i]))
                {
                    listBox.SelectedItems.Add(listBoxSelected[i]);
                }

                //Otherwise, it will check the removed items. If it finds it there, it will add the selected item to the list of selected items removed for future reference.
                else if (listBoxItemsRemoved != null && listBoxItemsRemoved.Contains(listBoxSelected[i]))
                {
                    listBoxSelectedItemsRemoved.Add(listBoxSelected[i]);
                }
            }
        }

        public static void GetRequiredMaterials(ListBox artefactsAddedListBox, ListBox materialsAddedListBox, ListBox materialsRequiredListBox, 
                                                List<listBoxItem> allMaterials, List<artefact> artefactsAddedListBoxItemsRemoved, 
                                                List<listBoxItem> materialsAddedListBoxItemsRemoved, List<listBoxItem> materialsRequiredListBoxItemsRemoved,
                                                List<listBoxItem> materialsRequiredListBoxItemsEnough)
        {
            AddItemsToListBox(artefactsAddedListBox, artefactsAddedListBoxItemsRemoved);
            AddItemsToListBox(materialsAddedListBox, materialsAddedListBoxItemsRemoved);

            List<artefact> artefactsAdded = new List<artefact>(GetItemsFromListBox<artefact>(artefactsAddedListBox, 1));
            List<listBoxItem> materialsAdded = new List<listBoxItem>(GetItemsFromListBox<listBoxItem>(materialsAddedListBox, 1));

            RemoveListBoxItemsFromListBox(artefactsAddedListBox, artefactsAddedListBoxItemsRemoved);
            RemoveListBoxItemsFromListBox(materialsAddedListBox, materialsAddedListBoxItemsRemoved);

            List<listBoxItem> materialsRequired = new List<listBoxItem>(CalculateRequiredMaterials(artefactsAdded, materialsAdded, allMaterials, materialsRequiredListBoxItemsEnough));
            
            materialsRequiredListBox.Items.Clear();
            materialsRequiredListBoxItemsRemoved.Clear();
            materialsRequiredListBoxItemsEnough.Clear();

            AddItemsToListBox(materialsRequiredListBox, materialsRequired);
        }

        /// <summary>
        /// Restricts a TextBox's input to numbers, backspace, and enter. Must be used inside of a PreviewKeyUp/Down, not a regular KeyUp/Down.
        /// </summary>
        /// <param name="e">The TextBox's PreviewKeyUp/Down's KeyEventArgs parameter.</param>
        public static void RestrictToNumbers(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.NumPad0:
                case Key.NumPad1:
                case Key.NumPad2:
                case Key.NumPad3:
                case Key.NumPad4:
                case Key.NumPad5:
                case Key.NumPad6:
                case Key.NumPad7:
                case Key.NumPad8:
                case Key.NumPad9:
                case Key.D0:
                case Key.D1:
                case Key.D2:
                case Key.D3:
                case Key.D4:
                case Key.D5:
                case Key.D6:
                case Key.D7:
                case Key.D8:
                case Key.D9:
                case Key.Back:
                case Key.Enter:
                    e.Handled = false;
                    break;
                case Key.Space:
                    e.Handled = true;
                    break;
                default:
                    e.Handled = true;
                    break;
            }
        }

        public static void DisableSelection(ListBox listBox)
        {
            List<listBoxItem> listBoxSelectedItems = new List<listBoxItem>(GetItemsFromListBox<listBoxItem>(listBox, 2));
            foreach (listBoxItem item in listBoxSelectedItems)
            {
                listBox.SelectedItems.Remove(item);
            }
        }

        public static T GetRightClickedItem<T>(List<T> itemsTrackedBefore, List<T> itemsTrackedAfter)
        {
            T rightClickedItem = default;

            if(itemsTrackedBefore.Count < itemsTrackedAfter.Count)
            {
                foreach (T item in itemsTrackedAfter)
                {
                    if(itemsTrackedBefore.IndexOf(item) == -1)
                    {
                        rightClickedItem = item;
                    }
                }
            }
            else
            {
                foreach (T item in itemsTrackedBefore)
                {
                    if(itemsTrackedAfter.IndexOf(item) == -1)
                    {
                        rightClickedItem = item;
                    }
                }
            }

            return rightClickedItem;
        }

        public static void SelectFolder(ref string path)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            CommonFileDialogResult result = dialog.ShowDialog();

            if(result == CommonFileDialogResult.Ok)
            {
                path = dialog.FileName;
            }
        }
    }
}
