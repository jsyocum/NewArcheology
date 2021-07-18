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
using static ArchHelper2.DebugConsole;
using static ArchHelper2.ArchDebugConsoleTools;
using System.IO;

namespace ArchHelper2
{
    class DebugConsoleTools
    {


        /// <summary>
        /// Opens the DebugConsole if it is not opened already.
        /// </summary>
        public static void OpenConsole()
        {
            if (!IsWindowOpen<DebugConsole>())
            {
                DebugConsole debugConsole = new DebugConsole();
                debugConsole.Show();
            }
        }

        /// <summary>
        /// Closes the DebugConsole if it is open.
        /// </summary>
        public static void CloseConsole()
        {
            if (IsWindowOpen<DebugConsole>())
            {
                GrabOpenWindow<DebugConsole>().Close();
            }
        }

        /// <summary>
        /// Toggles the DebugConsole open or closed.
        /// </summary>
        public static void ToggleConsole()
        {
            if (IsWindowOpen<DebugConsole>())
            {
                GrabOpenWindow<DebugConsole>().Close();
            }
            else
            {
                DebugConsole debugConsole = new DebugConsole();
                debugConsole.Show();
            }
        }

        /// <summary>
        /// Checks to see if a window of the specified type is open or not.
        /// </summary>
        /// <typeparam name="T">The type of window. For example, DebugConsole or MainWindow.</typeparam>
        /// <returns></returns>
        public static bool IsWindowOpen<T>()
        {
            return Application.Current.Windows.OfType<T>().Any();
        }

        public static T GrabOpenWindow<T>()
        {
            return Application.Current.Windows.OfType<T>().ToList()[0];
        }

        /// <summary>
        /// Writes a line to the DebugConsole if it is open. Also adds the line to the running list of debug lines for that specific type of debug.
        /// </summary>
        /// <param name="line">The line you want to write.</param>
        /// <param name="linesForTypeofDebug">The list of lines for the type of debug.</param>
        public static void ConsoleWriteLine(string line, List<string> linesForTypeofDebug)
        {
            if (linesForTypeofDebug[1] == "true" && IsWindowOpen<DebugConsole>())
            {
                PopulateDebugConsole(line);
            }
            else if (linesForTypeofDebug[1] == "true" && !IsWindowOpen<DebugConsole>())
            {
                populatedWhileClosed.Add(line);
            }

            linesForTypeofDebug.Add(line);
        }

        public static void Help()
        {
            PopulateDebugConsole("The following are all of the commands accepted by the DebugConsole:");

            foreach (HelpItem item in helpItems)
            {
                PopulateDebugConsole(item.Command + ": " + item.BasicDescription);
            }
        }

        public static void Help(string helpItem)
        {
            foreach (HelpItem item in helpItems)
            {
                if(helpItem == item.Command.ToLower())
                {

                    PopulateDebugConsole(item.Command + ": " + item.BasicDescription);

                    if (item.AdvancedDescription != "")
                    {
                        PopulateDebugConsole("Advanced description: " + item.AdvancedDescription);
                    }
                    else
                    {
                        PopulateDebugConsole("The command requested does not have an advanced description.");
                    }
                    break;
                }
            }
        }

        public static void MoveThroughInputHistory(TextBox UserInput, List<string> inputHistory, ref int inputHistoryTracker, bool KeyUp)
        {
            if (KeyUp)
            {
                if (inputHistoryTracker == -2 || inputHistoryTracker == 0)
                {
                    inputHistoryTracker = inputHistory.Count - 1;
                }
                else
                {
                    --inputHistoryTracker;
                }
            }
            else
            {
                if (inputHistoryTracker == -2 || inputHistoryTracker == inputHistory.Count - 1)
                {
                    inputHistoryTracker = 0;
                }
                else
                {
                    ++inputHistoryTracker;
                }
            }

            UserInput.Text = inputHistory[inputHistoryTracker];
            UserInput.CaretIndex = UserInput.Text.Count();
        }

        /// <summary>
        /// Prints all of the debug types to the DebugConsole.
        /// </summary>
        public static void PrintDebugTypes()
        {
            PopulateDebugConsole("The following are all of the debug types available:");

            foreach (List<string> debugType in allDebugTypes)
            {
                PopulateDebugConsole(debugType[0]);
            }
        }

        /// <summary>
        /// Adds the list of lines to the DebugConsole.
        /// </summary>
        /// <param name="debugLines">The list of lines being added.</param>
        public static void PopulateDebugConsole(List<string> debugLines)
        {
            if (IsWindowOpen<DebugConsole>())
            {

                if (IsADebugType(debugLines[0]) != -1)
                {
                    for (int i = 2; i < debugLines.Count; ++i)
                    {
                        consoleStackPanel.Items.Add(debugLines[i]);
                        history.Add(debugLines[i]);
                        consoleScrollViewer.ScrollToBottom();
                    }
                }
                else if (debugLines[0] == "history")
                {
                    for (int i = 1; i < debugLines.Count; ++i)
                    {
                        consoleStackPanel.Items.Add(debugLines[i]);
                        consoleScrollViewer.ScrollToBottom();
                    }
                }
                else
                {
                    foreach (string line in debugLines)
                    {
                        consoleStackPanel.Items.Add(line);
                        history.Add(line);
                        consoleScrollViewer.ScrollToBottom();
                    }
                }
            }
        }

        /// <summary>
        /// Adds the line to the DebugConsole.
        /// </summary>
        /// <param name="debugLine">The line being added.</param>
        public static void PopulateDebugConsole(string debugLine)
        {
            if (IsWindowOpen<DebugConsole>())
            {
                consoleStackPanel.Items.Add(debugLine);
                history.Add(debugLine);
                consoleScrollViewer.ScrollToBottom();
            }
        }

        /// <summary>
        /// Checks to see if the string is a name of one of the types of debug.
        /// </summary>
        /// <param name="isType">The string you're checking.</param>
        /// <returns></returns>
        public static int IsADebugType(string isType)
        {
            int isADebugType = -1;

            foreach (List<string> debugTypeList in allDebugTypes)
            {
                if (isType.ToLower() == debugTypeList[0].ToLower())
                {
                    isADebugType = allDebugTypes.IndexOf(debugTypeList);
                }
            }

            return isADebugType;
        }

        /// <summary>
        /// Enables or disables the debug type provided. If modifier is "-history" then it prints all of the debug messages sent to the DebugConsole while the debug type was disabled.
        /// </summary>
        /// <param name="debugType">The debug type you want enabled or disabled.</param>
        /// <param name="modifier">An option which enables or disables something.</param>
        public static void EnableDebugType(string debugType, string modifier)
        {
            if (IsADebugType(debugType) != -1)
            {
                string debugTypeName = allDebugTypes[IsADebugType(debugType)][0];
                string debugTypeStatus = allDebugTypes[IsADebugType(debugType)][1];
                List<string> debugTypeHistory = new List<string>(allDebugTypes[IsADebugType(debugType)]);

                //Print all the debug messages previously sent of the type specified, even if the type was disabled when they were sent.
                if (modifier == "-history")
                {
                    PopulateDebugConsole("History of debug type " + debugTypeName + ":");
                    PopulateDebugConsole(debugTypeHistory);
                }
                else if(modifier != "")
                {
                    PopulateDebugConsole("Unknown modifier: " + modifier);
                }
                //Check if the debug type is currently being printed
                else if (debugTypeStatus == "false")
                {
                    //If not, allow it to be printed
                    allDebugTypes[IsADebugType(debugType)][1] = "true";

                    PopulateDebugConsole(debugTypeName + " debug type enabled.");
                }
                //If the debug type is currently being printed, set its status to false.
                else if(debugTypeStatus == "true")
                {
                    allDebugTypes[IsADebugType(debugType)][1] = "false";

                    PopulateDebugConsole(debugTypeName + " debug type disabled.");
                }
            }
            else if (debugType == "enableall")
            {
                PopulateDebugConsole("Enabled all debug types.");
                foreach (List<string> debugTypeCurrent in allDebugTypes)
                {
                    debugTypeCurrent[1] = "true";
                }
            }
            else if (debugType == "disableall")
            {
                PopulateDebugConsole("Disabled all debug types.");
                foreach (List<string> debugTypeCurrent in allDebugTypes)
                {
                    debugTypeCurrent[1] = "false";
                }
            }
            else
            {
                PopulateDebugConsole("Unknown debug type: " + debugType + ". For a list of all debug types, use the command \"DebugTypes\".");
            }
        }

        public static void UserImport(string importType, string filePath)
        {
            string maybeRealFilePath = FixFilePath(filePath);

            if(maybeRealFilePath != filePath)
            {
                PopulateDebugConsole("Could not find the file path or directory given. Searching \"" + maybeRealFilePath + "\" (excluding subdirectories) for " + importType + ".txt instead.");
            }

            string[] realFilePaths;
            string realFilePath = "";

            if (!File.Exists(maybeRealFilePath) && !Directory.Exists(maybeRealFilePath))
            {
                PopulateDebugConsole("Could not find a matching file.");
            }
            else if (!File.Exists(maybeRealFilePath))
            {
                if (importType == "artefacts")
                {
                    realFilePaths = Directory.GetFiles(maybeRealFilePath, "artefacts.txt");
                }
                else
                {
                    realFilePaths = Directory.GetFiles(maybeRealFilePath, "materials.txt");
                }

                if (realFilePaths.Any())
                {
                    realFilePath = realFilePaths[0];
                }
            }

            if (realFilePath != "")
            {
                if (importType == "artefacts")
                {
                    allArtefactsFromFile = ImportArtefacts(File.ReadAllLines(realFilePath));
                    allArtefacts = allArtefactsFromFile;
                    BuildListBoxes(allArtefacts);

                    PopulateDebugConsole("Imported " + allArtefactsFromFile.Count + " artefacts from \"" + realFilePath + "\"");
                }
                else if (importType == "materials")
                {
                    allMaterialsFromFile = ImportMaterials(File.ReadAllLines(realFilePath));
                    allMaterials = allMaterialsFromFile;
                    BuildListBoxes(allMaterials);

                    PopulateDebugConsole("Imported " + allMaterialsFromFile.Count + " materials from \"" + realFilePath + "\"");
                }
            }

        }

    }
}
