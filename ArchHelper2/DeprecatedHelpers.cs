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
using static ArchHelper2.MainWindow;
using static ArchHelper2.DebugTools;
using static ArchHelper2.CSharpHelper;
using static ArchHelper2.XAMLHelper;
using System.Windows.Input;

namespace ArchHelper2
{
    class DeprecatedHelpers
    {
        /// <summary>
        /// Prints the settings in a list of ArchSetting to a file in the syntax "<setting name>=<setting value>"
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="archSettings"></param>
        //public static void PrintSettingsToFile(string filePath, List<ArchSetting> archSettings)
        //{
        //    List<string> settingsFileStrings = new List<string>();

        //    string archSettingString = "";
        //    foreach (ArchSetting archSetting in archSettings)
        //    {
        //        archSettingString = archSetting.Name + "=" + archSetting.Value;
        //        settingsFileStrings.Add(archSettingString);
        //    }

        //    settingsFileStrings.Sort();

        //    PrintStringsToFile(filePath, settingsFileStrings);
        //}

        /// <summary>
        /// Takes in and loads all the settings from a file with this syntax for each line: "(setting name)=(setting value)"
        /// </summary>
        /// <param name="filePath">The exact path to the settings text file</param>
        /// <param name="archSettings">The list of settings that you want updated</param>
        //public static void ParseSettingsFile(string filePath, List<ArchSetting> archSettings)
        //{
        //    if (!File.Exists(filePath))
        //    {
        //        ConsoleWriteLine("File \"" + filePath + "\" does not exist.", debugLoad);
        //        return;
        //    }

        //    List<string> fakeStrings = File.ReadAllLines(filePath).ToList();

        //    if (fakeStrings[0] == "null")
        //    {
        //        ConsoleWriteLine("File is null: " + filePath, debugLoad);
        //        return;
        //    }

        //    foreach (string line in fakeStrings)
        //    {
        //        string[] fakeSetting = line.Split('=');
        //        ArchSetting archSetting = new ArchSetting(fakeSetting[0], fakeSetting[1]);

        //        archSetting.Update(archSettings);
        //    }
        //}

        /// <summary>
        /// Returns the correct ArchSetting from a list of ArchSettings based on name. Returns an ArchSetting with null values if it didn't find the setting.
        /// </summary>
        /// <param name="archSettings"></param>
        /// <param name="settingName"></param>
        /// <returns></returns>
        //public static ArchSetting QuerySetting(List<ArchSetting> archSettings, string settingName)
        //{
        //    ArchSetting archSettingFind = new ArchSetting();
        //    foreach (ArchSetting archSetting in archSettings)
        //    {
        //        if (settingName == archSetting.Name)
        //        {
        //            //archSettingFind = archSetting;

        //            return archSetting;
        //        }
        //    }

        //    return archSettingFind;
        //}

        ///// <summary>
        ///// Returns the correct ArchSetting from the "settings" list of ArchSettings
        ///// </summary>
        ///// <param name="settingName"></param>
        ///// <returns></returns>
        //public static ArchSetting QuerySetting(string settingName)
        //{
        //    return QuerySetting(settings, settingName);
        //}

        //public static ArchSetting QuerySetting(List<ArchSetting> archSettings, ArchSetting archSetting)
        //{
        //    return QuerySetting(archSettings, archSetting.Name);
        //}

        //public static ArchSetting QuerySetting(ArchSetting archSetting)
        //{
        //    return QuerySetting(settings, archSetting.Name);
        //}

        ///// <summary>
        ///// Creates an ArchSetting from the provided string and int/string, then updates the provided list of ArchSettings with it.
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="archSettings"></param>
        ///// <param name="name"></param>
        ///// <param name="value">The value for the setting. Must be a string or int.</param>
        //public static void AddSetting<T>(List<ArchSetting> archSettings, string name, T value)
        //{
        //    ArchSetting archSetting = new ArchSetting(name, value.ToString());

        //    archSetting.Update(archSettings);
        //}

        ///// <summary>
        ///// Creates an ArchSetting from the provided string and int/string, then updates the list "settings" with it.
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="name"></param>
        ///// <param name="value">The value for the setting. Must be a string or int.</param>
        //public static void AddSetting<T>(string name, T value)
        //{
        //    AddSetting(settings, name, value.ToString());
        //}

        /// <summary>
        /// Filters a ListBox of strings based on a TextBox's text. Adds filtered strings to a list.
        /// </summary>
        /// <param name="listBox">The listBox of strings you want filtered.</param>
        /// <param name="sender">The sender corresponding to the TextBox.</param>
        /// <param name="listBoxItemsRemoved">The string list you want to send the filtered strings to.</param>
        public static void FilterListBox(ListBox listBox, string searchTerm, List<string> listBoxItemsRemoved, List<string> listBoxSelectedItemsRemoved)
        {
            List<string> listBoxItemsSelected = new List<string>(GetListBoxItems(listBox, 2));

            AddStringsToListBox(listBox, listBoxItemsRemoved);
            listBoxItemsRemoved.Clear();

            List<string> listBoxItems = new List<string>(GetListBoxItems(listBox, 1));
            List<int> listBoxItemsToRemove = new List<int>(FilterStringList(listBoxItems, StringToList(searchTerm)));
            RemoveStringsFromListBox(listBox, listBoxItemsToRemove, listBoxItemsRemoved);

            SortListBox(listBox);
            SetListBoxSelected(listBox, listBoxItemsSelected, listBoxItemsRemoved, listBoxSelectedItemsRemoved);
        }

        /// <summary>
        /// Takes all of the names out of a list of artefacts and returns them in a list of listBoxItems.
        /// </summary>
        /// <param name="artefactList">The list of artefacts you want the names removed from.</param>
        /// <returns></returns>
        public static List<listBoxItem> ExtractArteNamesToListBoxItems(List<artefact> artefactList)
        {
            List<listBoxItem> listBoxItems = new List<listBoxItem>();
            foreach (artefact currentItem in artefactList)
            {
                listBoxItem item = new listBoxItem(currentItem.arteName, 0, currentItem.URL);
                listBoxItems.Add(item);
            }

            return listBoxItems;
        }

        /// <summary>
        /// Converts a list of strings into a list of listBoxItems.
        /// </summary>
        /// <param name="strings">The strings you want converted.</param>
        /// <returns></returns>
        public static List<listBoxItem> ConvertStringsToListBoxItems(List<string> strings)
        {
            List<listBoxItem> listBoxItems = new List<listBoxItem>();
            foreach (string currentString in strings)
            {
                listBoxItem item = new listBoxItem(currentString, 0, URLConverter(currentString));
                listBoxItems.Add(item);
            }

            return listBoxItems;
        }

        /// <summary>
        /// Filter a list of strings based on another list of strings.
        /// </summary>
        /// <param name="stringListOG">The list being filtered.</param>
        /// <param name="filter">The list to filter against.</param>
        /// <returns></returns>
        public static List<int> FilterStringList(List<string> stringListOG, List<string> filter)
        {
            List<string> stringListTarget = new List<string>(stringListOG);
            List<int> stringListItemsToRemove = new List<int>();
            //Run through every string in filter
            for (int j = 0; j < filter.Count; ++j)
            {
                //Run through every string in the target list, for each string in the filter
                for (int i = 0; i < stringListTarget.Count; ++i)
                {
                    //Compare every string in the target list to the current string in the filter
                    switch (stringListTarget[i].Contains(filter[j], OrdinalIgnoreCase))
                    {
                        case false:
                            //If the substring is not found, run through every int in the list of strings to be removed
                            bool addedYet = false;
                            for (int k = 0; k < stringListItemsToRemove.Count; ++k)
                            {
                                //If this int has not been added to the list yet, add it
                                if (stringListItemsToRemove[k] == i)
                                {
                                    addedYet = true;
                                }
                            }

                            switch (addedYet)
                            {
                                case false:
                                    stringListItemsToRemove.Add(i);
                                    break;
                            }

                            //If the list of ints has not been given anything yet, this will start it
                            switch (stringListItemsToRemove.Count)
                            {
                                case 0:
                                    stringListItemsToRemove.Add(i);
                                    break;
                            }
                            break;
                    }
                }
            }

            return stringListItemsToRemove;
        }

        /// <summary>
        /// Adds the selected items of one ListBox to another ListBox. Also adds them to a Dictionary with a value provided by a TextBox. The selected items are removed from
        /// the first ListBox.
        /// </summary>
        /// <param name="listBoxOG">The ListBox losing its selected items.</param>
        /// <param name="listBoxTarget">The ListBox gaining items.</param>
        /// <param name="amountTextBox">The value each item should be assigned when moved to the Dictionary.</param>
        /// <param name="listBoxTargetDictionary">The dictionary the items are added to as well.</param>
        public static void ListBoxAddFunction(ListBox listBoxOG, ListBox listBoxTarget, TextBox amountTextBox, Dictionary<string, int> listBoxTargetDictionary)
        {
            int amount = 0;
            int.TryParse(amountTextBox.Text, out amount);

            if (amount != 0 && listBoxOG.SelectedItems.Count > 0)
            {
                List<string> listBoxOGSelectedItems = new List<string>(GetListBoxItems(listBoxOG, 2));

                AddStringsToDictionary(listBoxOGSelectedItems, amount, listBoxTargetDictionary);
                AddStringsToListBox(listBoxTarget, listBoxOGSelectedItems);

                for (int i = listBoxOG.SelectedItems.Count - 1; i >= 0; --i)
                {
                    listBoxOG.Items.Remove(listBoxOG.SelectedItems[i]);
                }
            }
        }

        /// <summary>
        /// Removes selected items from a target ListBox and adds them back into the original ListBox.
        /// </summary>
        /// <param name="listBoxOG">The original ListBox having items added back into.</param>
        /// <param name="listBoxTarget">The target ListBox having items removed.</param>
        /// <param name="listBoxTargetDictionary"></param>
        public static void ListBoxRemoveFunction(ListBox listBoxOG, ListBox listBoxTarget, Dictionary<string, int> listBoxTargetDictionary)
        {
            switch (listBoxTarget.SelectedItems.Count)
            {
                case 0:
                    break;

                default:
                    List<string> listBoxTargetSelectedItems = new List<string>(GetListBoxItems(listBoxTarget, 2));

                    AddStringsToListBox(listBoxOG, listBoxTargetSelectedItems);

                    for (int i = 0; i < listBoxTargetSelectedItems.Count; ++i)
                    {
                        listBoxTargetDictionary.Remove(listBoxTargetSelectedItems[i]);
                    }

                    for (int i = listBoxTarget.SelectedItems.Count - 1; i >= 0; --i)
                    {
                        listBoxTarget.Items.Remove(listBoxTarget.SelectedItems[i]);
                    }

                    break;
            }
        }

        /// <summary>
        /// Adds a list of strings to a ListBox.
        /// </summary>
        /// <param name="listBox">The listbox you want to add to.</param>
        /// <param name="strings">The list of strings you want to add.</param>
        public static void AddStringsToListBox(ListBox listBox, List<string> strings)
        {
            for (int i = 0; i < strings.Count; ++i)
            {
                listBox.Items.Add(strings[i]);
            }
        }

        /// <summary>
        /// Remove a list of items from a ListBox based on their index in the ListBox.
        /// </summary>
        /// <param name="listBox">The ListBox you are removing strings from.</param>
        /// <param name="listBoxItemsToRemove">The indexes of the items being removed.</param>
        /// <param name="listBoxItemsRemoved">A list of strings that have been removed from the ListBox.</param>
        public static void RemoveStringsFromListBox(ListBox listBox, List<int> listBoxItemsToRemove, List<string> listBoxItemsRemoved)
        {
            List<string> listBoxItems = new List<string>(GetListBoxItems(listBox, 1));
            for (int i = listBoxItemsToRemove.Count - 1; i >= 0; --i)
            {
                listBox.Items.RemoveAt(listBoxItemsToRemove[i]);
                listBoxItemsRemoved.Add(listBoxItems[listBoxItemsToRemove[i]]);
            }
        }

        /// <summary>
        /// Adds a list of strings to a Dictionary paired with a specific number..
        /// </summary>
        /// <param name="strings">The list of strings you want to add.</param>
        /// <param name="number">The number you want to assign each one.</param>
        /// <returns></returns>
        public static void AddStringsToDictionary(List<string> strings, int number, Dictionary<string, int> dictionary)
        {
            for (int i = 0; i < strings.Count; ++i)
            {
                dictionary.Add(strings[i], number);
            }
        }

        /// <summary>
        /// Sorts a LIstBox of strings by a
        /// </summary>
        /// <param name="listBox"></param>
        public static void SortListBox(ListBox listBox)
        {
            List<string> listBoxItems = new List<string>(GetListBoxItems(listBox, 1));
            List<string> listBoxSelectedItems = new List<string>(GetListBoxItems(listBox, 2));
            listBoxItems.Sort();

            listBox.Items.Clear();
            for (int i = 0; i < listBoxItems.Count; ++i)
            {
                listBox.Items.Add(listBoxItems[i]);
            }

            SetListBoxSelected(listBox, listBoxSelectedItems, null, null);
        }

        /// <summary>
        /// Grabs the Items or SelecedItems from a ListBox and converts them into a list of strings.
        /// </summary>
        /// <param name="listBox">The ListBox you're grabbing from.</param>
        /// <param name="mode">1 = Grab Items. 2 = Grab SelectedItems.</param>
        /// <returns></returns>
        public static List<string> GetListBoxItems(ListBox listBox, int mode)
        {
            List<string> items = new List<string>();
            switch (mode)
            {
                case 1:
                    items = listBox.Items.OfType<string>().ToList();
                    break;

                case 2:
                    items = listBox.SelectedItems.OfType<string>().ToList();
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
        public static void SetListBoxSelected(ListBox listBox, List<string> listBoxSelected, List<string> listBoxItemsRemoved, List<string> listBoxSelectedItemsRemoved)
        {
            for (int i = 0; listBoxSelectedItemsRemoved != null && i < listBoxSelectedItemsRemoved.Count; ++i)
            {
                if (listBox.Items.Contains(listBoxSelectedItemsRemoved[i]))
                {
                    listBox.SelectedItems.Add(listBoxSelectedItemsRemoved[i]);
                    listBoxSelectedItemsRemoved.RemoveAt(i);
                }
            }

            for (int i = 0; i < listBoxSelected.Count; ++i)
            {
                if (listBox.Items.Contains(listBoxSelected[i]))
                {
                    listBox.SelectedItems.Add(listBoxSelected[i]);
                }
                else if (listBoxItemsRemoved != null && listBoxItemsRemoved.Contains(listBoxSelected[i]))
                {
                    listBoxSelectedItemsRemoved.Add(listBoxSelected[i]);
                }
            }
        }

        /// <summary>
        /// Extracts a list of names from a list of artefacts.
        /// </summary>
        /// <param name="artefactList">The list of artefacts to extract names from.</param>
        /// <returns></returns>
        public static List<string> ExtractArteNameList(List<artefact> artefactList)
        {
            List<string> arteNameList = new List<string>();
            for (int i = 0; i < artefactList.Count; ++i)
            {
                arteNameList.Add(artefactList[i].arteName);
            }

            return arteNameList;
        }
    }
}
