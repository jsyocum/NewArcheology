using System;
using System.Collections.Generic;
using System.IO;
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

namespace ArchHelper2
{
    /// <summary>
    /// Interaction logic for DebugConsole.xaml
    /// </summary>
    public partial class DebugConsole : Window
    {
        public class HelpItem
        {
            public string Command { get; set; }
            public string BasicDescription { get; set; }
            public string AdvancedDescription { get; set; }

            public HelpItem(string command, string basicDescription, string advancedDescription)
            {
                Command = command;
                BasicDescription = basicDescription;
                AdvancedDescription = advancedDescription;
            }

            public HelpItem(string command, string basicDescription)
            {
                Command = command;
                BasicDescription = basicDescription;
                AdvancedDescription = "";
            }

            public HelpItem()
            {
                Command = "";
                BasicDescription = "";
            }

            public HelpItem(HelpItem existingHelpItem)
            {
                Command = existingHelpItem.Command;
                BasicDescription = existingHelpItem.BasicDescription;
                AdvancedDescription = existingHelpItem.AdvancedDescription;
            }
        }

        //Declaring debug types
        public static List<string> debugGeneral = new List<string> { "General", "false" };
        public static List<string> debugImportArtefacts = new List<string> { "ImportArtefacts", "false" };
        public static List<string> debugRightClicks = new List<string> { "RightClicks", "false" };
        public static List<string> debugLoad = new List<string> { "Load", "false" };
        public static List<List<string>> allDebugTypes = new List<List<string>> { debugGeneral, debugImportArtefacts, debugRightClicks, debugLoad };

        public static List<string> history = new List<string> { "history" };
        public static List<string> inputHistory = new List<string>();
        public static List<string> populatedWhileClosed = new List<string>();
        int inputHistoryTracker = -2;

        //Declaring stack panel, scroll viewer
        public static ItemsControl consoleStackPanel = new ItemsControl();
        public static ScrollViewer consoleScrollViewer = new ScrollViewer();

        //Declaring help items
        public static List<HelpItem> helpItems = new List<HelpItem>
        {
            new HelpItem("Exit", "Closes the DebugConsole."),
            new HelpItem("History", "Displays all lines that have been printed to the DebugConsole previously."),
            new HelpItem("Help", "Gives information on various commands. You can specify a command for greater detail like so: \"Help History\"."),
            new HelpItem("DebugTypes", "Prints a list of all the debug types available.", "A debug type is a method of organizing debug messages sent to the DebugConsole. For example, any time a user right clicks an artefact or material a debug message of type \"RightClicks\" is sent to the console. However, these messages are only printed if the user has previously used the \"Debug\" command and called out that type."),
            new HelpItem("Debug", "Enables or disables a debug type.", "When a debug type is enabled, any debug messages sent to the DebugConsole of that type will be printed. Use the -History modifier to view all of the debug messages of that type which have been sent to the console."),
            new HelpItem("Import", "Allows the user to import their own artefacts or materials.", "The command takes two arguments. First, you must specify whether you are importing artefacts or materials. Next, you must provide a file path to import from (must not require administrator permissions). If your file is named \"artefacts.txt\" or \"materials.txt\", you can simply provide the directory the file lives in. If it has a different name, you must provide the full path to the file. Example inputs: \"Import Artefacts C:\\Program Files (x86)\\ArchHelper\\myArtefacts.txt\", \"Import Materials C:\\Users\\Archeology Major\".")
        };


        public DebugConsole()
        {
            InitializeComponent();

            //Assigning stack panel and scroll viewer
            consoleStackPanel = debugStack;
            consoleScrollViewer = debugScrollViewer;


            PopulateDebugConsole("Welcome to the Debug Console. Type \"Help\" for a list of commands.");

            if(populatedWhileClosed.Count > 0)
            {
                PopulateDebugConsole("The following active debug messages were sent while the console was closed:");
                PopulateDebugConsole(populatedWhileClosed);
                populatedWhileClosed.Clear();
            }

            UserInput.Focus();
        }


        ///////////////////XAML Interaction Stuff///////////////////

        //debugScrollViewer stuff
        private void debugScrollViewer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            UserInput.Focus();
        }

        private void debugScrollViewer_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            UserInput.Focus();
        }

        //UserInput stuff
        private void UserInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            

            if (e.Key == Key.Enter && UserInput.Text.Length > 0)
            {
                inputHistory.Add(UserInput.Text);

                string[] userInput = UserInput.Text.ToLower().Split(' ');
                bool knownCommand = false;
                PopulateDebugConsole("");
                PopulateDebugConsole("(" + DateTime.Now.ToString() + ") User input: " + UserInput.Text);

                //Check if the user's input matches one of our custom-defined console functions.
                switch (userInput[0])
                {
                    case "exit":
                        CloseConsole();
                        knownCommand = true;
                        break;

                    case "history":
                        PopulateDebugConsole(history);
                        knownCommand = true;
                        break;

                    case "help":
                        if(userInput.Count() == 2)
                        {
                            Help(userInput[1]);
                        }
                        else
                        {
                            Help();
                        }

                        knownCommand = true;
                        break;

                    case "debugtypes":
                        PrintDebugTypes();
                        knownCommand = true;
                        break;

                    case "debug":
                        if (userInput.Count() == 3)
                        {
                            EnableDebugType(userInput[1], userInput[2]);
                        }
                        else if(userInput.Count() == 2)
                        {
                            EnableDebugType(userInput[1], "");
                        }
                        else if(userInput.Count() == 1)
                        {
                            PopulateDebugConsole("This command must be followed by a debug type. For a list of all debug types, input \"Help DebugTypes\".");
                        }

                        knownCommand = true;
                        break;

                    case "import":
                        if (userInput.Count() >= 3 && (userInput[1] == "artefacts" || userInput[1] == "materials"))
                        {
                            string importFilePath = string.Join("", userInput.Skip(2));
                            UserImport(userInput[1], @importFilePath);
                        }
                        else if (userInput.Count() >= 3)
                        {
                            PopulateDebugConsole("\"" + userInput[2] + "\" is not an accepted argument for this command. Do \"Help Import\" for details.");
                        }
                        else if (userInput.Count() == 2 && (userInput[1] == "artefacts" || userInput[1] == "materials"))
                        {
                            PopulateDebugConsole("You must provide a file path with the use of this command. Do \"Help Import\" for details.");
                        }
                        else if (userInput.Count() == 2)
                        {
                            PopulateDebugConsole("\"" + userInput[2] + "\" is not an accepted argument for this command. Do \"Help Import\" for details.");
                        }
                        else if (userInput.Count() == 1)
                        {
                            PopulateDebugConsole("This command requires two more arguments. Input \"Help Import\" for details.");
                        }

                        knownCommand = true;
                        break;

                    case "folder":

                        string allArteNamesPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ArchHelper\\Imported.txt");
                        //string allArteNamesPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Imported.txt");
                        PopulateDebugConsole(allArteNamesPath);

                        //Directory.CreateDirectory(allArteNamesPath);

                        //if (Directory.Exists(allArteNamesPath))
                        //{
                        //System.IO.Path.Combine(allArteNamesPath, "Imported.txt");
                        //StreamWriter sw = File.CreateText(allArteNamesPath);

                        //foreach (artefact arte in allArtefacts)
                        //{
                        //    sw.WriteLine(arte.arteName);
                        //}

                        //sw.Close();

                        //PopulateDebugConsole("Arte names saved to: " + allArteNamesPath);
                        //}
                        //else
                        //{
                        //PopulateDebugConsole("\"" + allArteNamesPath + "\" could not be found.");
                        //}

                        using (var fs = File.Create(allArteNamesPath))
                        {
                            using (var sw = new StreamWriter(fs))
                            {
                                foreach (artefact arte in allArtefacts)
                                {
                                    sw.WriteLine(arte.arteName);
                                }
                            }
                        }

                        knownCommand = true;
                        break;

                    case "current":
                        PopulateDebugConsole(Directory.GetCurrentDirectory());

                        knownCommand = true;
                        break;

                    case "save":
                        string savePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ArchHelper\\SaveState\\");
                        Save(savePath, artefactsAddedListBox, materialsAddedListBox);

                        PopulateDebugConsole("Attempted to save to: " + savePath);

                        knownCommand = true;
                        break;

                    case "load":
                        string loadPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ArchHelper\\SaveState\\");
                        Load(loadPath, artefactListBox, artefactsAddedListBox, materialListBox, materialsAddedListBox, allArtefacts, allMaterials);

                        PopulateDebugConsole("Attempted to load from: " + loadPath);

                        knownCommand = true;
                        break;
                }

                if (!knownCommand)
                {
                    PopulateDebugConsole("\"" + UserInput.Text + "\" is not a valid command. Type \"Help\" for a list of commands.");
                }

                UserInput.Text = "";
            }
            else if (e.Key == Key.Up && inputHistory.Count > 0)
            {
                MoveThroughInputHistory(UserInput, inputHistory, ref inputHistoryTracker, true);
            }
            else if (e.Key == Key.Down && inputHistory.Count > 0)
            {
                MoveThroughInputHistory(UserInput, inputHistory, ref inputHistoryTracker, false);
            }
        }

        private void UserInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (inputHistory.Count > 0 && inputHistoryTracker != -2 && UserInput.Text != inputHistory[inputHistoryTracker])
            {
                inputHistoryTracker = -2;
            }
        }
    }
}