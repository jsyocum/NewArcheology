﻿using System;
using System.Collections.Generic;
using System.IO;
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
using static ArchHelper2.XAMLHelper;
using static ArchHelper2.DebugConsole;
using static ArchHelper2.DebugConsoleTools;
using static ArchHelper2.ArchDebugConsoleTools;
using System.Windows.Input;

namespace ArchHelper2
{
    public class CSharpHelper
    {
        /// <summary>
        /// Changes a generic (T) variable/object's type.
        /// </summary>
        /// <typeparam name="T">The generic type T (Must be of type T).</typeparam>
        /// <typeparam name="U">The type you want it changed to.</typeparam>
        /// <param name="variable">The variable you want converted.</param>
        /// <returns></returns>
        public static U ChangeType<T, U>(T variable)
        {
            List<T> variableToList = new List<T>();
            variableToList.Add(variable);

            List<U> variableToUList = new List<U>(variableToList.OfType<U>());
            return variableToUList[0];
        }

        /// <summary>
        /// Takes a string and creates a list where that string is the only item.
        /// </summary>
        /// <param name="stringIn">The string you want to make into a list.</param>
        /// <returns></returns>
        public static List<string> StringToList(string stringIn)
        {
            List<string> stringListOut = new List<string>();
            stringListOut.Add(stringIn);

            return stringListOut;
        }

        /// <summary>
        /// Filter a list of items based on a list of strings.
        /// </summary>
        /// <param name="itemList">The list being filtered.</param>
        /// <param name="filter">The list to filter against.</param>
        /// <returns></returns>
        public static List<int> FilterItemList<T>(List<T> itemList, List<string> filter)
        {
            List<string> itemListToString = new List<string>();
            if(typeof(T) == typeof(listBoxItem))
            {
                List<listBoxItem> itemListAsListBoxItems = new List<listBoxItem>(itemList.OfType<listBoxItem>().ToList());

                foreach (listBoxItem item in itemListAsListBoxItems)
                {
                    itemListToString.Add(item.ItemName);
                }
            }
            else if (typeof(T) == typeof(artefact))
            {
                List<artefact> itemListAsArtefacts = new List<artefact>(itemList.OfType<artefact>().ToList());

                foreach (artefact arte in itemListAsArtefacts)
                {
                    itemListToString.Add(arte.arteName);
                }
            }

            List<string> itemListTarget = new List<string>(itemListToString);
            List<int> itemListItemsToRemove = new List<int>();
            //Run through every string in filter
            for (int j = 0; j < filter.Count; ++j)
            {
                //Run through every item in the target list, for each string in the filter
                for (int i = 0; i < itemListTarget.Count; ++i)
                {
                    //Compare every item's ItemName in the target list to the current string in the filter
                    switch (itemListTarget[i].Contains(filter[j], OrdinalIgnoreCase))
                    {
                        case false:
                            //If the substring is not found, run through every int in the list of strings to be removed
                            bool addedYet = false;
                            for (int k = 0; k < itemListItemsToRemove.Count; ++k)
                            {
                                //If this int has not been added to the list yet, add it
                                if (itemListItemsToRemove[k] == i)
                                {
                                    addedYet = true;
                                }
                            }

                            switch (addedYet)
                            {
                                case false:
                                    itemListItemsToRemove.Add(i);
                                    break;
                            }

                            //If the list of ints has not been given anything yet, this will start it
                            switch (itemListItemsToRemove.Count)
                            {
                                case 0:
                                    itemListItemsToRemove.Add(i);
                                    break;
                            }
                            break;
                    }
                }
            }

            return itemListItemsToRemove;
        }

        /// <summary>
        /// Imports a list of artefacts from a text file.
        /// </summary>
        /// <param name="fileName">The pathway to the text file.</param>
        /// <returns></returns>
        public static List<artefact> ImportArtefacts(string[] stringArray)
        {
            List<string> stringList = new List<string>(stringArray.ToList());

            int lineType = 1;
            List<artefact> artefacts = new List<artefact>();

            artefact newArtefact = new artefact();
            List<int> matsNeededListOfInts = new List<int>();
            List<int> matAmountsNeededListOfInts = new List<int>();
            newArtefact.matsNeeded = matsNeededListOfInts;
            newArtefact.matAmountsNeeded = matAmountsNeededListOfInts;

            //Run through each line of the artefact text file
            for (int i = 0; i < stringList.Count; ++i)
            {

                switch (lineType)
                {
                    case 1:
                        newArtefact.arteName = stringList[i];
                        newArtefact.URL = URLConverter(newArtefact.arteName);

                        ++lineType;
                        break;

                    case 2:
                        int numberType = 1;
                        int character = 1;
                        char[] materialsLine = stringList[i].ToCharArray();
                        StringBuilder matRequired = new StringBuilder();
                        foreach (char ch in materialsLine)
                        {
                            if (ch != ' ' && character == materialsLine.Length)
                            {
                                matRequired.Append(ch);

                                string matRequiredString = matRequired.ToString();
                                int materialRequired = 0;
                                int.TryParse(matRequiredString, out materialRequired);

                                if (materialRequired > 0 && numberType == 1)
                                {
                                    newArtefact.matsNeeded.Add(materialRequired);
                                    ++numberType;
                                }
                                else if (materialRequired > 0 && numberType == 2)
                                {
                                    newArtefact.matAmountsNeeded.Add(materialRequired);
                                    numberType = 1;
                                }

                                matRequired.Clear();
                            }
                            else if (ch != ' ')
                            {
                                matRequired.Append(ch);
                            }
                            else if (ch == ' ')
                            {
                                string matRequiredString = matRequired.ToString();
                                int materialRequired = 0;
                                int.TryParse(matRequiredString, out materialRequired);

                                if (materialRequired > 0 && numberType == 1)
                                {
                                    newArtefact.matsNeeded.Add(materialRequired);
                                    ++numberType;
                                }
                                else if (materialRequired > 0 && numberType == 2)
                                {
                                    newArtefact.matAmountsNeeded.Add(materialRequired);
                                    numberType = 1;
                                }

                                matRequired.Clear();
                            }

                            ++character;
                        }

                        ++lineType;
                        break;

                    case 3:
                        double currentExperience = 0;
                        double.TryParse(stringList[i], out currentExperience);
                        newArtefact.experience = currentExperience;

                        artefacts.Add(newArtefact);

                        string artefactImported = "(" + DateTime.Now.ToString() + ") Artefact imported: " + newArtefact.arteName + "; Size of matsNeeded: " + newArtefact.matsNeeded.Count + "; Size of matAmountsNeeded: " + newArtefact.matAmountsNeeded.Count + "; Experience: " + newArtefact.experience + "; URL: " + newArtefact.URL;
                        ConsoleWriteLine(artefactImported, debugImportArtefacts);

                        List<int> newMatsNeededListOfInts = new List<int>();
                        List<int> newMatAmountsNeededListOfInts = new List<int>();
                        string blankURL = new string("");

                        newArtefact.matsNeeded = newMatsNeededListOfInts;
                        newArtefact.matAmountsNeeded = newMatAmountsNeededListOfInts;
                        newArtefact.URL = blankURL;

                        lineType = 1;
                        break;
                }
            }

            return artefacts;
        }

        /// <summary>
        /// Imports a list of materials from a text file.
        /// </summary>
        /// <param name="fileName">The path to the text file.</param>
        /// <returns></returns>
        public static List<listBoxItem> ImportMaterials(string[] stringArray)
        {
            List<string> stringList = new List<string>(stringArray.ToList());

            List<listBoxItem> materials = new List<listBoxItem>();

            foreach (string line in stringList)
            {
                listBoxItem newMaterial = new listBoxItem(line, 0, URLConverter(line));
                materials.Add(newMaterial);
            }

            return materials;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="artefactsAdded"></param>
        /// <param name="materialsAdded"></param>
        /// <param name="allArtefacts"></param>
        /// <param name="allMaterials"></param>
        /// <returns></returns>
        public static List<listBoxItem> CalculateRequiredMaterials(List<artefact> artefactsAdded, List<listBoxItem> materialsAdded,
                                                                   List<listBoxItem> allMaterials, List<listBoxItem> materialsRequiredListBoxItemsEnough)
        {
            List<listBoxItem> materialsRequiredMessy = new List<listBoxItem>();
            foreach (artefact artefact in artefactsAdded)
            {
                int i = 0;

                foreach (int matNeeded in artefact.matsNeeded)
                {
                    listBoxItem matBeingAdded = new listBoxItem();
                    matBeingAdded.ItemName = allMaterials[matNeeded - 1].ItemName;
                    matBeingAdded.ItemAmount = artefact.matAmountsNeeded[i] * artefact.amountNeeded;
                    matBeingAdded.URL = allMaterials[matNeeded - 1].URL;

                    materialsRequiredMessy.Add(matBeingAdded);
                    ++i;
                }
            }

            List<listBoxItem> materialsRequired = new List<listBoxItem>();
            foreach (listBoxItem messyMaterial in materialsRequiredMessy)
            {
                bool addedYet = false;

                foreach (listBoxItem material in materialsRequired)
                {
                    if (material.ItemName == messyMaterial.ItemName)
                    {
                        material.ItemAmount += messyMaterial.ItemAmount;
                        addedYet = true;
                    }
                }

                if(addedYet == false)
                {
                    materialsRequired.Add(messyMaterial);
                }
            }

            List<listBoxItem> materialsRequiredToRemove = new List<listBoxItem>();
            foreach (listBoxItem hadMaterial in materialsAdded)
            {
                foreach (listBoxItem material in materialsRequired)
                {
                    if (hadMaterial.ItemName == material.ItemName)
                    {
                        material.ItemAmount -= hadMaterial.ItemAmount;
                    }

                    if (material.ItemAmount <= 0)
                    {
                        materialsRequiredToRemove.Add(material);
                    }
                }
            }

            foreach (listBoxItem material in materialsRequiredToRemove)
            {
                materialsRequiredListBoxItemsEnough.Add(material);
                materialsRequired.Remove(material);
            }

            return materialsRequired;
        }

        /// <summary>
        /// Converts an artefact or material name to a runescape wiki URL.
        /// </summary>
        /// <param name="itemName">The artefact or material name you want converted.</param>
        /// <returns></returns>
        public static string URLConverter(string itemName)
        {
            StringBuilder itemNameConverted = new StringBuilder("https://www.runescape.wiki/w/");

            foreach (char ch in itemName)
            {
                if (ch == ' ')
                {
                    itemNameConverted.Append('_');
                }
                else if (ch == '\'')
                {
                    itemNameConverted.Append("%27");
                }
                else
                {
                    itemNameConverted.Append(ch);
                }
            }

            string URL = itemNameConverted.ToString();
            return URL;
        }

        public static string FixFilePath(string filePath)
        {
            StringBuilder fixingFilePath = new StringBuilder(filePath);

            for (int i = fixingFilePath.Length - 1; i >= 0 && !File.Exists(fixingFilePath.ToString()) && !Directory.Exists(fixingFilePath.ToString()); --i)
            {
                fixingFilePath.Remove(i, 1);
            }

            return fixingFilePath.ToString();
        }
    }
}