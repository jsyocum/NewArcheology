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
using System.IO;
using XamlAnimatedGif;
using System.Runtime.InteropServices;

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
            if (textBlockName != null)
            {
                textBlockName.Visibility = Visibility.Hidden;
                switch (textBoxName.Text.Length)
                {
                    case 0:
                        textBlockName.Visibility = Visibility.Visible;
                        break;
                }
            }
        }

        public static void HideClearButton(TextBox textBox, Button clearButton)
        {
            if (textBox.Text.Length > 0)
            {
                clearButton.Visibility = Visibility.Visible;
            }
            else
            {
                clearButton.Visibility = Visibility.Hidden;
            }
        }

        public static void ClearSearchBoxes()
        {
            artefactTextBox.Text = "";
            artefactRemoveSearchBox.Text = "";
            materialSearchBox.Text = "";
            materialRemoveSearchBox.Text = "";
            materialsRequiredSearchBox.Text = "";
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
        /// Figures out whether or not a ListBox contains an item (artefact or material) based on the provided item's name. Changes foundItem into the matched item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listBox">The ListBox being searched</param>
        /// <param name="item">The item you're looking to match (must be the same type as the ListBox)</param>
        /// <returns></returns>
        public static bool ListBoxContains<T>(ListBox listBox, T item, ref T foundItem, ref bool selected)
        {
            bool contains = false;
            
            if (typeof(T) == typeof(artefact))
            {
                List<artefact> listBoxItems = new List<artefact>(GetItemsFromListBox<artefact>(listBox, 1));
                List<artefact> listBoxItemsSelected = new List<artefact>(GetItemsFromListBox<artefact>(listBox, 2));
                artefact itemArte = (artefact)(object)item;

                foreach (artefact arte in listBoxItems)
                {
                    if (arte.arteName == itemArte.arteName)
                    {
                        foundItem = (T)(object)arte;
                        contains = true;

                        if (listBoxItemsSelected.Contains(arte))
                        {
                            selected = true;
                        }
                    }
                }
            }
            else if (typeof(T) == typeof(listBoxItem))
            {
                List<listBoxItem> listBoxItems = new List<listBoxItem>(GetItemsFromListBox<listBoxItem>(listBox, 1));
                List<listBoxItem> listBoxItemsSelected = new List<listBoxItem>(GetItemsFromListBox<listBoxItem>(listBox, 2));
                listBoxItem itemMat = (listBoxItem)(object)item;

                foreach (listBoxItem mat in listBoxItems)
                {
                    if (mat.ItemName == itemMat.ItemName)
                    {
                        foundItem = (T)(object)mat;
                        contains = true;

                        if (listBoxItemsSelected.Contains(mat))
                        {
                            selected = true;
                        }
                    }
                }
            }

            return contains;
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
                listBoxItem listBoxItemRightClickedToListBoxItem = (listBoxItem)(object)listBoxItemRightClicked;

                if (listBoxItemRightClicked != null)
                {
                    string debugLine = "(" + DateTime.Now.ToString() + ") Material right clicked: " + listBoxItemRightClickedToListBoxItem.ItemName + "; URL: " + listBoxItemRightClickedToListBoxItem.URL;
                    ConsoleWriteLine(debugLine, debugLines);
                }
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

        public static void ListBoxUpOrDownButton<T>(bool upButton, ListBox listBox)
        {
            if (typeof(T) == typeof(listBoxItem))
            {
                List<listBoxItem> changedListBoxItems = new List<listBoxItem>(GetItemsFromListBox<listBoxItem>(listBox, 2));
                foreach (listBoxItem material in changedListBoxItems)
                {
                    listBox.Items.Remove(material);

                    if (upButton)
                    {
                        material.ItemAmount = ++material.ItemAmount;
                    }
                    else if (material.ItemAmount > 0)
                    {
                        material.ItemAmount = --material.ItemAmount;
                    }

                    listBox.Items.Add(material);
                    listBox.SelectedItems.Add(material);
                }
            }
            else if (typeof(T) == typeof(artefact))
            {
                List<artefact> changedArtefacts = new List<artefact>(GetItemsFromListBox<artefact>(listBox, 2));
                List<artefact> newChangedArtefacts = new List<artefact>();
                foreach(artefact arte in changedArtefacts)
                {
                    int newAmount = arte.amountNeeded;

                    if (upButton)
                    {
                        newAmount = arte.amountNeeded + 1;
                    }
                    else if (arte.amountNeeded > 0)
                    {
                        newAmount = arte.amountNeeded - 1;
                    }

                    listBox.Items.Remove(arte);
                    artefact changedArte = new artefact(arte.arteName, arte.experience, arte.matAmountsNeeded, arte.matsNeeded, newAmount, arte.URL);

                    listBox.Items.Add(changedArte);
                    listBox.SelectedItems.Add(changedArte);
                }
            }
            else
            {
                ConsoleWriteLine("The type " + typeof(T) + " is not compatible with the \"ListBoxUpOrDownButton\" function.", debugGeneral);
                return;
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

            if (amountTextBox.Text.Length > 0)
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

        public static void ListBoxWhichMenuItems<T>(MenuItem selectAll, MenuItem unselectAll, ListBox listBox, MenuItem goToWiki, MenuItem add, MenuItem subtract, MenuItem gotEnough, T listBoxRightClickedItem)
        {
            if (selectAll != null && unselectAll != null && listBox.Items.Count > 0)
            {
                selectAll.IsEnabled = true;
                unselectAll.IsEnabled = true;
            }
            else if (selectAll != null && unselectAll != null)
            {
                selectAll.IsEnabled = false;
                unselectAll.IsEnabled = false;
            }

            bool itemRightClicked = false;
            if (listBoxRightClickedItem != null)
            {
                if (typeof(T) == typeof(artefact))
                {
                    artefact listBoxRightClickedItemArte = (artefact)(object)listBoxRightClickedItem;
                    if (listBoxRightClickedItemArte.URL != null && listBoxRightClickedItemArte.URL.Length > 0)
                    {
                        itemRightClicked = true;
                    }
                }
                else if (typeof(T) == typeof(listBoxItem))
                {
                    listBoxItem listBoxRightClickedItemMat = (listBoxItem)(object)listBoxRightClickedItem;
                    if (listBoxRightClickedItemMat.URL != null && listBoxRightClickedItemMat.URL.Length > 0)
                    {
                        itemRightClicked = true;
                    }
                }
            }

            if (goToWiki != null && itemRightClicked)
            {
                goToWiki.IsEnabled = true;
            }
            else if (goToWiki != null)
            {
                goToWiki.IsEnabled = false;
            }

            if (add != null && subtract != null && itemRightClicked)
            {
                add.IsEnabled = true;
                subtract.IsEnabled = true;
            }
            else if (add != null && subtract != null)
            {
                add.IsEnabled = false;
                subtract.IsEnabled = false;
            }

            if (gotEnough != null && itemRightClicked)
            {
                gotEnough.IsEnabled = true;
            }
            else if (gotEnough != null)
            {
                gotEnough.IsEnabled = false;
            }
        }

        public static void EnableAddOrSubtract(MenuItem menuItemAdd, MenuItem menuItemSubtract, TextBox numberBox)
        {
            if (StringToInt(numberBox.Text) != 0)
            {
                if (menuItemAdd != null)
                {
                    menuItemAdd.IsEnabled = true;
                }

                if (menuItemSubtract != null)
                {
                    menuItemSubtract.IsEnabled = true;
                }
            }
            else
            {
                if (menuItemAdd != null)
                {
                    menuItemAdd.IsEnabled = false;
                }

                if (menuItemSubtract != null)
                {
                    menuItemSubtract.IsEnabled = false;
                }
            }
        }

        public static void AddOrSubtract<T>(TextBox numberBox, T rightClickedItem, ListBox listBox, bool add, ContextMenu contextMenu)
        {
            int amountBeingAdded = StringToInt(numberBox.Text);
            if (!add)
            {
                amountBeingAdded *= -1;
            }

            bool wasSelected = false;
            if (listBox.SelectedItems.Contains(rightClickedItem))
            {
                wasSelected = true;
            }

            numberBox.Text = "";
            listBox.Items.Remove(rightClickedItem);

            if (typeof(T) == typeof(artefact))
            {
                artefact rightClickedItemArtefact = (artefact)(object)rightClickedItem;

                if (rightClickedItemArtefact.amountNeeded + amountBeingAdded > 0)
                {
                    artefact artefactAdded = new artefact(rightClickedItemArtefact.arteName, rightClickedItemArtefact.experience, rightClickedItemArtefact.matAmountsNeeded, rightClickedItemArtefact.matsNeeded, rightClickedItemArtefact.amountNeeded + amountBeingAdded, rightClickedItemArtefact.URL);
                    listBox.Items.Add(artefactAdded);

                    if (wasSelected)
                    {
                        listBox.SelectedItems.Add(artefactAdded);
                    }
                }
                else
                {
                    artefact artefactAdded = new artefact(rightClickedItemArtefact.arteName, rightClickedItemArtefact.experience, rightClickedItemArtefact.matAmountsNeeded, rightClickedItemArtefact.matsNeeded, 1, rightClickedItemArtefact.URL);
                    listBox.Items.Add(artefactAdded);

                    if (wasSelected)
                    {
                        listBox.SelectedItems.Add(artefactAdded);
                    }
                }
            }
            else if (typeof(T) == typeof(listBoxItem))
            {
                listBoxItem rightClickedItemMaterial = (listBoxItem)(object)rightClickedItem;
                
                if (rightClickedItemMaterial.ItemAmount + amountBeingAdded > 0)
                {
                    rightClickedItemMaterial.ItemAmount += amountBeingAdded;
                }
                else
                {
                    rightClickedItemMaterial.ItemAmount = 1;
                }

                listBox.Items.Add(rightClickedItemMaterial);

                if (wasSelected)
                {
                    listBox.SelectedItems.Add(rightClickedItemMaterial);
                }
            }

            if (contextMenu != null)
            {
                contextMenu.IsOpen = false;
            }
        }

        public static void AddOrSubtract<T>(TextBox numberBox, T rightClickedItem, ListBox listBox, bool add)
        {
            AddOrSubtract<T>(numberBox, rightClickedItem, listBox, add, null);
        }

        /// <summary>
        /// Determines whether or not a list of buttons should be active based on whether or not there are any selected items in a listbox
        /// </summary>
        /// <param name="listBox"></param>
        /// <param name="upButton"></param>
        /// <param name="downButton"></param>
        public static void ToggleButtonsBasedOnListBox(ListBox listBox, List<Button> buttons)
        {
            if (listBox.SelectedItems.Count > 0)
            {
                foreach (Button button in buttons)
                {
                    button.IsEnabled = true;
                }
            }
            else
            {
                foreach (Button button in buttons)
                {
                    button.IsEnabled = false;
                }
            }
        }

        public static void ToggleButtonsBasedOnListBox(ListBox listBox, Button button)
        {
            List<Button> buttons = new();
            buttons.Add(button);

            ToggleButtonsBasedOnListBox(listBox, buttons);
        }

        /// <summary>
        /// Adds the selected items of one ListBox to another ListBox. Also adds them to a Dictionary with a value provided by a TextBox. The selected items are removed from the first ListBox.
        /// </summary>
        /// <param name="listBoxOG">The ListBox losing its selected items.</param>
        /// <param name="listBoxTarget">The ListBox gaining items.</param>
        /// <param name="amountTextBox">The amount each item should be assigned when moved to the new ListBox.</param>
        public static void ListBoxAddItemsFunction<T>(ListBox listBoxOG, ListBox listBoxTarget, TextBox amountTextBox)
        {
            int amount = -1;
            int.TryParse(amountTextBox.Text, out amount);

            if (amount != -1 && listBoxOG.SelectedItems.Count > 0)
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
        /// Adds a list of items to a ListBox's selected items
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listBox"></param>
        /// <param name="listBoxItems"></param>
        public static void AddSelectedItemsToListBox<T>(ListBox listBox, List<T> listBoxItems)
        {
            foreach (T item in listBoxItems)
            {
                listBox.SelectedItems.Add(item);
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
                                                List<listBoxItem> materialsAddedListBoxItemsRemoved, TextBox artefactsAddedSearchBox, TextBox materialsAddedSearchBox,
                                                List<listBoxItem> materialsRequiredListBoxItemsRemoved,
                                                List<listBoxItem> materialsRequiredListBoxItemsEnough)
        {
            //AddItemsToListBox(artefactsAddedListBox, artefactsAddedListBoxItemsRemoved);
            //AddItemsToListBox(materialsAddedListBox, materialsAddedListBoxItemsRemoved);
            string artefactsAddedSearch = artefactsAddedSearchBox.Text;
            string materialsAddedSearch = materialsAddedSearchBox.Text;
            artefactsAddedSearchBox.Text = "";
            materialsAddedSearchBox.Text = "";

            List<artefact> artefactsAdded = new List<artefact>(GetItemsFromListBox<artefact>(artefactsAddedListBox, 1));
            List<listBoxItem> materialsAdded = new List<listBoxItem>(GetItemsFromListBox<listBoxItem>(materialsAddedListBox, 1));

            //RemoveListBoxItemsFromListBox(artefactsAddedListBox, artefactsAddedListBoxItemsRemoved);
            //RemoveListBoxItemsFromListBox(materialsAddedListBox, materialsAddedListBoxItemsRemoved);
            artefactsAddedSearchBox.Text = artefactsAddedSearch;
            materialsAddedSearchBox.Text = materialsAddedSearch;

            List<listBoxItem> materialsRequired = new List<listBoxItem>(CalculateRequiredMaterials(artefactsAdded, materialsAdded, allMaterials, materialsRequiredListBoxItemsEnough));
            
            materialsRequiredListBox.Items.Clear();
            materialsRequiredListBoxItemsRemoved.Clear();
            materialsRequiredListBoxItemsEnough.Clear();

            AddItemsToListBox(materialsRequiredListBox, materialsRequired);
            SortListBoxItems<listBoxItem>(materialsRequiredListBox);
        }

        public static void GotEnough(listBoxItem rightClickedItem, ListBox materialsListBox, ListBox materialsAddedListBox, TextBox materialSearchBox, TextBox materialsAddedSearchBox)
        {
            string materialSearch = materialSearchBox.Text;
            string materialsAddedSearch = materialsAddedSearchBox.Text;

            materialSearchBox.Text = "";
            materialsAddedSearchBox.Text = "";

            listBoxItem foundItem = new listBoxItem();
            bool selected = false;
            bool foundInAdded = ListBoxContains<listBoxItem>(materialsAddedListBox, rightClickedItem, ref foundItem, ref selected);
            if (foundInAdded)
            {
                materialsAddedListBox.Items.Remove(foundItem);
                foundItem.ItemAmount += rightClickedItem.ItemAmount;
                
                materialsAddedListBox.Items.Add(foundItem);
                if (selected)
                {
                    materialsAddedListBox.SelectedItems.Add(foundItem);
                }
            }
            else if (ListBoxContains<listBoxItem>(materialsListBox, rightClickedItem, ref foundItem, ref selected))
            {
                materialsListBox.Items.Remove(foundItem);
                foundItem.ItemAmount = rightClickedItem.ItemAmount;
                materialsAddedListBox.Items.Add(foundItem);
            }
            else
            {
                PopulateDebugConsole("Could not find " + rightClickedItem.ItemName + " in either ListBox");
            }

            materialSearchBox.Text = materialSearch;
            materialsAddedSearchBox.Text = materialsAddedSearch;

            SortListBoxItems<listBoxItem>(materialsAddedListBox);
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

        /// <summary>
        /// Saves the artefact and material ListBox's current states to four files.
        /// </summary>
        /// <param name="savePath"></param>
        /// <param name="artefactListBox"></param>
        /// <param name="materialListBox"></param>
        /// <returns></returns>
        public static void Save(string appDirectoryPath, string savePathSpecific, ListBox artefactListBox, ListBox materialListBox, List<artefact> artefactsRemoved, List<artefact> selectedArtefactsRemoved, List<listBoxItem> materialsRemoved, List<listBoxItem> selectedMaterialsRemoved)
        {
            string savePath = System.IO.Path.Combine(appDirectoryPath, savePathSpecific);

            System.IO.Directory.CreateDirectory(savePath);

            AddItemsToListBox<artefact>(artefactListBox, artefactsRemoved);
            AddSelectedItemsToListBox<artefact>(artefactListBox, selectedArtefactsRemoved);

            AddItemsToListBox<listBoxItem>(materialListBox, materialsRemoved);
            AddSelectedItemsToListBox<listBoxItem>(materialListBox, selectedMaterialsRemoved);

            SortListBoxItems<artefact>(artefactListBox);
            SortListBoxItems<listBoxItem>(materialListBox);


            SaveArtefacts(savePath, artefactListBox);
            SaveMaterials(savePath, materialListBox);


            RemoveListBoxItemsFromListBox<artefact>(artefactListBox, artefactsRemoved);

            RemoveListBoxItemsFromListBox<listBoxItem>(materialListBox, materialsRemoved);
        }

        public static void Save(string appDirectoryPath, ListBox artefactListBox, ListBox materialListBox, List<artefact> artefactsRemoved, List<artefact> selectedArtefactsRemoved, List<listBoxItem> materialsRemoved, List<listBoxItem> selectedMaterialsRemoved)
        {
            string savePathSpecific = "SaveState\\";
            Save(appDirectoryPath, savePathSpecific, artefactListBox, materialListBox, artefactsRemoved, selectedArtefactsRemoved, materialsRemoved, selectedMaterialsRemoved);
        }

        /// <summary>
        /// Prints the artefacts, selected artefacts, and their amounts from a ListBox
        /// </summary>
        /// <param name="saveFolderPath"></param>
        /// <param name="listBox"></param>
        public static void SaveArtefacts(string saveFolderPath, ListBox listBox)
        {
            List<artefact> artefacts = new List<artefact>(GetItemsFromListBox<artefact>(listBox, 1));
            List<string> artefactsStrings = new List<string>();
            foreach (artefact arte in artefacts)
            {
                artefactsStrings.Add(arte.arteName);
                artefactsStrings.Add(arte.amountNeeded.ToString());
            }

            List<artefact> selectedArtefacts = new List<artefact>(GetItemsFromListBox<artefact>(listBox, 2));
            List<string> selectedArtefactsStrings = new List<string>();
            foreach (artefact arte in selectedArtefacts)
            {
                selectedArtefactsStrings.Add(arte.arteName);
            }

            string artefactsSavePath = System.IO.Path.Combine(saveFolderPath, "artefactsAdded.txt");
            PrintStringsToFile(artefactsSavePath, artefactsStrings);

            string selectedArtefactsSavePath = System.IO.Path.Combine(saveFolderPath, "selectedArtefactsAdded.txt");
            PrintStringsToFile(selectedArtefactsSavePath, selectedArtefactsStrings);
        }

        /// <summary>
        /// Prints the materials, selected materials, and their amounts from a ListBox
        /// </summary>
        /// <param name="saveFolderPath"></param>
        /// <param name="listBox"></param>
        public static void SaveMaterials(string saveFolderPath, ListBox listBox)
        {
            List<listBoxItem> materials = new List<listBoxItem>(GetItemsFromListBox<listBoxItem>(listBox, 1));
            List<string> materialsStrings = new List<string>();
            foreach (listBoxItem mat in materials)
            {
                materialsStrings.Add(mat.ItemName);
                materialsStrings.Add(mat.ItemAmount.ToString());
            }

            List<listBoxItem> selectedMaterials = new List<listBoxItem>(GetItemsFromListBox<listBoxItem>(listBox, 2));
            List<string> selectedMaterialsStrings = new List<string>();
            foreach (listBoxItem mat in selectedMaterials)
            {
                selectedMaterialsStrings.Add(mat.ItemName);
            }

            string materialsSavePath = System.IO.Path.Combine(saveFolderPath, "materialsAdded.txt");
            PrintStringsToFile(materialsSavePath, materialsStrings);

            string selectedMaterialsSavePath = System.IO.Path.Combine(saveFolderPath, "selectedMaterialsAdded.txt");
            PrintStringsToFile(selectedMaterialsSavePath, selectedMaterialsStrings);
        }


        

        /// <summary>
        /// Loads everything it can for the ListBoxes and settings
        /// </summary>
        /// <param name="loadPath"></param>
        /// <param name="artefactListBox"></param>
        /// <param name="materialListBox"></param>
        /// <returns></returns>
        public static bool Load(string appDirectoryPath, ListBox artefactListBox, ListBox artefactsAddedListBox, ListBox materialListBox, 
            ListBox materialsAddedListBox, Button artefactAddButtonLoad, Button materialAddButtonLoad, List<Button> artefactsAddedButtonsLoad, List<Button> materialsAddedButtonsLoad, 
            List<artefact> artefactsReference, List<listBoxItem> materialsReference)
        {
            string loadPath = System.IO.Path.Combine(appDirectoryPath, "SaveState\\");

            if (!System.IO.Directory.Exists(loadPath))
            {
                string defaultLoadPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ArchHelper\\SaveState\\");
                ConsoleWriteLine("Load path directory \"" + loadPath + "\" does not exist. Try the default: \"" + defaultLoadPath + "\"", debugLoad);
                return false;
            }

            artefactListBox.Items.Clear();
            artefactsAddedListBox.Items.Clear();
            materialListBox.Items.Clear();
            materialsAddedListBox.Items.Clear();

            ClearSearchBoxes();

            BuildListBoxes(artefactsReference);
            BuildListBoxes(materialsReference);

            LoadArtefacts(loadPath, artefactListBox, artefactsAddedListBox, artefactsReference);
            LoadMaterials(loadPath, materialListBox, materialsAddedListBox, materialsReference);

            SortListBoxItems<artefact>(artefactsAddedListBox);
            SortListBoxItems<listBoxItem>(materialsAddedListBox);

            ToggleButtonsBasedOnListBox(artefactListBox, artefactAddButtonLoad);
            ToggleButtonsBasedOnListBox(artefactsAddedListBox, artefactsAddedButtonsLoad);
            ToggleButtonsBasedOnListBox(materialListBox, materialAddButtonLoad);
            ToggleButtonsBasedOnListBox(materialsAddedListBox, materialsAddedButtonsLoad);

            GetRequiredMaterialsMain();
            UpdateTotalExperienceGainedMain();
            artefactAddBoxSelectedItemsTrackedBefore = GetItemsFromListBox<artefact>(artefactsAddedListBox, 2);
            materialAddBoxSelectedItemsTrackedBefore = GetItemsFromListBox<listBoxItem>(materialsAddedListBox, 2);

            return true;
        }

        public static void LoadArtefacts(string loadPath, ListBox listBox, ListBox addedListBox, List<artefact> artefactsReference)
        {
            string artefactLoadPath = Path.Combine(loadPath, "artefactsAdded.txt");
            List<string> artefactNamesLoaded = new List<string>();
            List<int> artefactAmountsLoaded = new List<int>();
            List<artefact> realLoadedArtefacts = new List<artefact>();
            List<artefact> artefactsWithNeeded = new List<artefact>();
            if (File.Exists(artefactLoadPath))
            {
                ParseAlternatingTextFile(artefactLoadPath, artefactNamesLoaded, artefactAmountsLoaded);

                //Create and populate a list of all the actual artefacts based on their names
                foreach (string fakeArte in artefactNamesLoaded)
                {
                    foreach (artefact arte in artefactsReference)
                    {
                        if (fakeArte == arte.arteName)
                        {
                            realLoadedArtefacts.Add(arte);
                        }
                    }
                }

                //Create another list of all the actual artefacts plus their amount needed
                int i = 0;
                foreach (artefact arte in realLoadedArtefacts)
                {
                    artefact arteWithNeeded = new artefact(arte.arteName, arte.experience, arte.matAmountsNeeded, arte.matsNeeded, artefactAmountsLoaded[i], arte.URL);
                    artefactsWithNeeded.Add(arteWithNeeded);

                    ++i;
                }
            }

            string selectedArtefactsLoadPath = Path.Combine(loadPath, "selectedArtefactsAdded.txt");
            List<artefact> realSelectedArtes = new List<artefact>();
            if (File.Exists(selectedArtefactsLoadPath))
            {
                //Create a list of all the selected artefacts in string form
                List<string> fakeSelectedArtes = File.ReadAllLines(selectedArtefactsLoadPath).ToList();
                foreach (string fakeArte in fakeSelectedArtes)
                {
                    foreach (artefact arte in artefactsWithNeeded)
                    {
                        if (fakeArte == arte.arteName)
                        {
                            realSelectedArtes.Add(arte);
                        }
                    }
                }
            }

            if (File.Exists(artefactLoadPath))
            {
                RemoveListBoxItemsFromListBox(listBox, realLoadedArtefacts);
                AddItemsToListBox(addedListBox, artefactsWithNeeded);
            }
            
            if (File.Exists(selectedArtefactsLoadPath))
            {
                AddSelectedItemsToListBox(addedListBox, realSelectedArtes);
            }
        }

        public static void LoadMaterials(string loadPath, ListBox listBox, ListBox addedListBox, List<listBoxItem> materialsReference)
        {
            string materialLoadPath = Path.Combine(loadPath, "materialsAdded.txt");
            List<string> materialNamesLoaded = new List<string>();
            List<int> materialAmountsLoaded = new List<int>();
            List<listBoxItem> realLoadedMaterials = new List<listBoxItem>();
            List<listBoxItem> materialsWithNeeded = new List<listBoxItem>();
            if (File.Exists(materialLoadPath))
            {
                ParseAlternatingTextFile(materialLoadPath, materialNamesLoaded, materialAmountsLoaded);

                //Create and populate a list of all the actual artefacts based on their names
                foreach (string fakeMat in materialNamesLoaded)
                {
                    foreach (listBoxItem mat in materialsReference)
                    {
                        if (fakeMat == mat.ItemName)
                        {
                            realLoadedMaterials.Add(mat);
                        }
                    }
                }

                //Create another list of all the actual artefacts plus their amount needed
                int i = 0;
                foreach (listBoxItem mat in realLoadedMaterials)
                {
                    listBoxItem matWithNeeded = mat;
                    matWithNeeded.ItemAmount = materialAmountsLoaded[i];
                    materialsWithNeeded.Add(matWithNeeded);

                    ++i;
                }
            }

            string selectedMaterialsLoadPath = Path.Combine(loadPath, "selectedMaterialsAdded.txt");
            List<listBoxItem> realSelectedMats = new List<listBoxItem>();
            if (File.Exists(selectedMaterialsLoadPath))
            {
                //Create a list of all the selected artefacts in string form
                List<string> fakeSelectedMats = File.ReadAllLines(selectedMaterialsLoadPath).ToList();
                foreach (string fakeMat in fakeSelectedMats)
                {
                    foreach (listBoxItem mat in materialsWithNeeded)
                    {
                        if (fakeMat == mat.ItemName)
                        {
                            realSelectedMats.Add(mat);
                        }
                    }
                }
            }

            if (File.Exists(materialLoadPath))
            {
                RemoveListBoxItemsFromListBox(listBox, realLoadedMaterials);
                AddItemsToListBox(addedListBox, materialsWithNeeded);
            }

            if (File.Exists(selectedMaterialsLoadPath))
            {
                AddSelectedItemsToListBox(addedListBox, realSelectedMats);
            }
        }

        /// <summary>
        /// Updates the TotalExperienceGained textblock based on the artefacts added.
        /// </summary>
        /// <param name="artefactsAddedListBox"></param>
        /// <param name="totalExperienceGained"></param>
        public static void UpdateTotalExperienceGained(ListBox artefactsAddedListBox, TextBlock totalExperienceGained)
        {
            if(artefactsAddedListBox.Items.Count < 1)
            {
                totalExperienceGained.Text = "Total exp. gained: 0";
            }
            else
            {
                double expGained = 0;
                foreach (artefact arte in artefactsAddedListBox.Items)
                {
                    expGained += arte.totalExperience;
                }

                expGained = Math.Round(expGained, 1);
                totalExperienceGained.Text = "Total exp. gained: " + expGained.ToString();
            }
        }

        /// <summary>
        /// Uses the XamlAnimatedGif library to play a gif.
        /// </summary>
        /// <param name="gif"></param>
        public static void PlayGif(Image gif, string source, bool allowInterrupt)
        {
            Animator aGif = AnimationBehavior.GetAnimator(gif);
            if ((!allowInterrupt && (aGif.CurrentFrameIndex == 0 || aGif.CurrentFrameIndex == aGif.FrameCount - 1)) || allowInterrupt)
            {
                Uri uri = new Uri(source, UriKind.RelativeOrAbsolute);
                AnimationBehavior.SetSourceUri(gif, uri);
                AnimationBehavior.GetAnimator(gif).Play();
            }
        }

        public static async void PlayGifHideButton(Image gif, string source, bool allowInterrupt, Button button, TextBox textBox)
        {
            textBox.Text = "";
            textBox.Focus();

            button.Visibility = Visibility.Visible;

            PlayGif(gif, source, allowInterrupt);

            Animator aGif = AnimationBehavior.GetAnimator(gif);
            while (aGif.CurrentFrameIndex != aGif.FrameCount - 1)
            {
                await Task.Delay(1);
            }

            if (textBox.Text == "")
            {
                button.Visibility = Visibility.Hidden;
            }
        }

        public static async void StopGif(Image gif, string source)
        {
            Animator aGif = AnimationBehavior.GetAnimator(gif);
            while (aGif.CurrentFrameIndex != 0 && aGif.CurrentFrameIndex != aGif.FrameCount - 1)
            {
                await Task.Delay(1);
            }

            Uri uri = new Uri(source, UriKind.RelativeOrAbsolute);
            AnimationBehavior.SetSourceUri(gif, uri);
        }

        public static async void ReplaceButtonWithGif(Image gif, string source, Button textButton, Button gifButton, Window window)
        {
            textButton.Visibility = Visibility.Hidden;
            gifButton.Visibility = Visibility.Visible;

            PlayGif(gif, source, true);

            Animator aGif = AnimationBehavior.GetAnimator(gif);
            while (aGif.CurrentFrameIndex != aGif.FrameCount - 1)
            {
                await Task.Delay(1);
            }

            gifButton.Visibility = Visibility.Hidden;
            textButton.Visibility = Visibility.Visible;

            if (window != null)
            {
                window.Close();
            }
        }

        public static async void ReplaceButtonWithGif(Image gif, string source, Button textButton, Button gifButton)
        {
            ReplaceButtonWithGif(gif, source, textButton, gifButton, null);
        }

        public static void SetWindowProperties(Window window, double windowHeight, double windowWidth, double windowLeft, double windowTop, bool setSize, bool setLocation)
        {
            if (setSize)
            {
                window.Height = windowHeight;
                window.Width = windowWidth;
            }

            if (setLocation)
            {
                window.Left = windowLeft;
                window.Top = windowTop;
            }
        }
    }
}
