using ProcessCsvLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessCsv
{
    public class Menu
    {
        private List<MenuOption> mainMenu = new();
        private List<MenuOption> selectSourceFileMenu = new();
        private List<MenuOption> selectTargetFileMenu = new();
        private List<MenuOption> delimiterMenu = new();
        private List<MenuOption> encodingMenu = new();
        private CsvArguments Arguments;

        private string argSource = "Source File";
        private string argTarget = "Target File";
        private string argNone = "";

        public MenuResultValues MenuResult = MenuResultValues.None;

        public Menu(CsvArguments arguments)
        {
            this.Arguments = arguments;
            mainMenu.Add(new MenuOption("Help", ActionShowHelp));

            // re-implement the ActionShowFileMenu and remove ActionInputFileName when there's more than one option there.
            //mainMenu.Add(new MenuOption("Select source file", ActionShowFileMenu, argSource, argNone));
            mainMenu.Add(new MenuOption("Select source file", ActionInputFileName, argSource, argNone));
            //mainMenu.Add(new MenuOption("Select target file", ActionShowFileMenu, argTarget, argNone));
            mainMenu.Add(new MenuOption("Select target file", ActionInputFileName, argTarget, argNone));

            mainMenu.Add(new MenuOption("Select Fields (Columns)", ActionSelectFields, "SELECT FIELDS", argNone));
            mainMenu.Add(new MenuOption("Select Delimiters", ActionShowDelimiterMenu, "DELIMITERS", argNone));
            mainMenu.Add(new MenuOption("Select Encoding", ActionShowEncodingMenu, "ENCODING", argNone));
            mainMenu.Add(new MenuOption("Save file and Exit", ActionSaveFile, argNone, argNone));



            selectSourceFileMenu.Add(new MenuOption("Enter source file name manually", ActionInputFileName, argSource, argNone));
            selectTargetFileMenu.Add(new MenuOption("Enter target file name manually", ActionInputFileName, argTarget, argNone));

            delimiterMenu.Add(new MenuOption("Select Source delimiter", ActionSelectDelimiter, argSource, argNone));
            delimiterMenu.Add(new MenuOption("Select Target delimiter", ActionSelectDelimiter, argTarget, argNone));

            encodingMenu.Add(new MenuOption("Source: Set Custom encoding name", ActionSetCustomEncoding, argSource, argNone));
            encodingMenu.Add(new MenuOption("Source: UTF-8", ActionSetEncodingPreset, argSource, "UTF-8"));
            encodingMenu.Add(new MenuOption("Source: Latin1", ActionSetEncodingPreset, argSource, "Latin1"));
            encodingMenu.Add(new MenuOption("Target: Set Custom encoding name", ActionSetCustomEncoding, argTarget, argNone));
            encodingMenu.Add(new MenuOption("Target: UTF-8", ActionSetEncodingPreset, argTarget, "UTF-8"));
            encodingMenu.Add(new MenuOption("Target: Latin1", ActionSetEncodingPreset, argTarget, "Latin1"));
        }

        public void Start()
        {   
            while (ShowMenuOptions(mainMenu, "Main menu"));
            Console.Clear();
        }

        private bool ShowMenuOptions(List<MenuOption> menu, string argument = "")
        {
            if (MenuResult != MenuResultValues.None)
            {
                // user has selected to save and exit or quit.
                return false;
            }

            Console.Clear();
            Console.WriteLine("MENU: " + argument);
            for (int i = 0; i < menu.Count; i++)
            {
                Console.WriteLine(i + ": " + menu[i].Name);
            }

            if (argument == "Main menu")
            {
                Console.WriteLine("Q: Quit");
            }
            else
            {
                Console.WriteLine("Q: Back");
            }

            Console.WriteLine(Environment.NewLine + Environment.NewLine + Environment.NewLine);
            Console.WriteLine("--------- ARGUMENTS ----------------");
            ShowArguments();
            Console.SetCursorPosition(0, menu.Count+2);

            ConsoleKeyInfo pressedKey = Console.ReadKey(true);
            if (pressedKey.KeyChar == 'Q' || pressedKey.KeyChar == 'q')
            {
                if (argument == "Main Menu")
                {
                    MenuResult = MenuResultValues.Exit;
                }
                return false; // exit the menu
            }


            

            Console.WriteLine();
            int pressedNumber = 0;
            if (int.TryParse(pressedKey.KeyChar.ToString(), out pressedNumber) == false)
            {
                pressedNumber = -1;
                //return to start
            }
            else
            {
                //Console.WriteLine("You pressed " + pressedNumber); //.key will show Numpad 1 as D1, use KeyChar
                
                if (pressedNumber >= 0 && pressedNumber < menu.Count) 
                {
                    menu[pressedNumber].Action(menu[pressedNumber].Argument, menu[pressedNumber].SubArgument);
                }
                //else
                //{
                //    Console.WriteLine("This menu option does not exist"); // can be removed later, just return to start
                //}
                //Console.WriteLine("Click any key to continue.");
                //Console.ReadKey(); // remove later
            }
            
            return true; // continue showing the menu
        }

        private void ArgumentFormatted(string name, string value, int padding, bool error = false)
        {
            ConsoleColor previousColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(name.PadRight(padding, ' '));
            if (error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
            Console.WriteLine(value);
            Console.ForegroundColor = previousColor;
        }

        private void ShowArguments()
        {
            int padding = 25;
            ArgumentFormatted("Source file", Arguments.SourceFile, padding, error: !File.Exists(Arguments.SourceFile));
            ArgumentFormatted("Source encoding", Arguments.SourceEncoding, padding);
            ArgumentFormatted("Source delimiter", Arguments.DelimiterRead, padding);

            string targetFile = Arguments.TargetFile;
            string targetDirectory = "";
            bool directoryExists = false;
            if (targetFile.Length > 0)
            {
                Debug.WriteLine("1: targetFile length is > 0");
                if (targetFile.Contains("\\") == false)
                {
                    Debug.WriteLine("File name without a directory, therefore local folder. OK");
                    directoryExists = true;
                }
                else if (Directory.Exists(targetFile))
                {
                    Debug.WriteLine("2: Dir exists, but missing a file name");
                    directoryExists = false;
                }
                else
                {
                    Debug.WriteLine("3: Dir does not exist with full path, getting dir");
                    targetDirectory = Path.GetDirectoryName(targetFile);
                    if (Directory.Exists(targetDirectory))
                    {
                        Debug.WriteLine("4: dir exists");
                        directoryExists = true;
                    }
                }
                
            }
            Debug.WriteLine(Arguments.TargetFile + "; dir is: " + targetFile + "; exists: " + directoryExists);

            ArgumentFormatted("Target file", Arguments.TargetFile, padding, !directoryExists);
            ArgumentFormatted("Target encoding", Arguments.TargetEncoding, padding);
            ArgumentFormatted("Target delimiter", Arguments.DelimiterWrite, padding);
            Console.WriteLine();
            ArgumentFormatted("New headers", Arguments.NewHeaders, padding);
            ArgumentFormatted("Source has headers", Arguments.FileHasHeaders.ToString(), padding);
            ArgumentFormatted("Field Count", Arguments.FieldCount.ToString(), padding);
            ArgumentFormatted("Columns Selected", Arguments.SelectedFields, padding);

            ArgumentFormatted("Ignore bad data", Arguments.IgnoreBadData.ToString(), padding);
            ArgumentFormatted("Ignore missing fields", Arguments.IgnoreMissingField.ToString(), padding);
            ArgumentFormatted("Fix bad data", Arguments.FixBadData.ToString(), padding);
            /*
                    public bool Help = false;
                    public string SourceFile = string.Empty;
                    public string TargetFile = string.Empty;
                    public string SourceEncoding = "";
                    public string TargetEncoding = "UTF8";
                    public string SelectedFields = "";
                    public bool DisplayResult = false;
                    public bool DisplayHeaders = false;
                    public int ExampleLines = 5;
                    public string DelimiterRead = "auto";
                    public string DelimiterWrite = ",";
                    public bool Quiet = false;
                    public bool Pause = false;
                    public bool ByteOrderMark = false;
                    public bool IgnoreBadData = false;
                    public bool IgnoreMissingField = false;
                    public bool FixBadData = false;
                    public int FieldCount = 0; // if 0, autodetect
                    public bool ReplaceHeaders = false;
                    public string NewHeaders = string.Empty;
                    public bool FileHasHeaders = true;
                    public bool ExitOnError = true;
                    public bool SupressWarnings = false;
                    public bool SupressErrors = false; 
            */
        }

        private void ActionShowHelp(string argument = "", string subArgument = "")
        {
            MenuResult = MenuResultValues.Help;
        }

        private void ActionShowFileMenu(string argument, string subArgument = "")
        {
            if (argument == argSource)
            {
                ShowMenuOptions(selectSourceFileMenu, argument);
            }
            else if (argument == argTarget)
            {
                ShowMenuOptions(selectTargetFileMenu, argument);
            }
        }

        private void ActionShowDelimiterMenu(string argument = "", string subArgument = "")
        {
            while (ShowMenuOptions(delimiterMenu, argument)) ;
        }

        //ActionShowEncodingMenu
        private void ActionShowEncodingMenu(string argument = "", string subArgument = "")
        {
            while (ShowMenuOptions(encodingMenu, argument)) ;
        }

        private void ActionSaveFile(string argument = "", string subArgument = "")
        {
            MenuResult = MenuResultValues.SaveFile;
        }

        private void ActionInputFileName(string argument, string subArgument = "")
        {
            Debug.WriteLine("aifn: " + argument);
            if (argument == argSource)
            {
                Arguments.SourceFile = EnterFileName(argument);
            }
            else if (argument == argTarget)
            {
                Arguments.TargetFile = EnterFileName(argument);
            }
        }

        private string EnterFileName(string argument, string subArgument = "")
        {
            Console.Write(argument + " name: ");
            string? filename = Console.ReadLine();
            if (filename != null)
            {
                return filename;
            }
            else
            {
                return string.Empty;
            }
        }

        private void ActionSelectFields(string argument = "", string subArgument = "")
        {
            Console.Clear();
            Console.Write("Enter selected Fields (example: 0,1,4,8):");
            string text = Console.ReadLine()+"";
            Arguments.SelectedFields = text;
        }

        private void ActionSelectDelimiter(string argument, string subArgument = "")
        {
            //Console.Clear();
            Console.WriteLine("Valid delimiter aliases: tab, comma, semicolon");
            Console.Write("Enter " + argument + " delimiter: ");
            string text = Console.ReadLine() + "";
            if (argument == argSource)
            {
                Arguments.DelimiterRead = text;
            }
            else if (argument == argTarget)
            {
                Arguments.DelimiterWrite = text;
            }
        }

        private void ActionSetEncodingPreset(string argument, string subArgument)
        {
            Debug.WriteLine("Setting custom encoding: " + argument + " / " + subArgument);
            if (argument == argSource)
            {
                Arguments.SourceEncoding = subArgument;
            }
            else if (argument == argTarget)
            {
                Arguments.TargetEncoding = subArgument;
            }
        }

        private void ActionSetCustomEncoding(string argument, string subArgument)
        {
            Console.Write("Enter custom encoding value: ");
            string custom = Console.ReadLine()+"";
            if (argument == argSource)
            {
                Arguments.SourceEncoding = custom;
            }
            else if (argument == argTarget)
            {
                Arguments.TargetEncoding = custom;
            }
        }

    }

    class MenuOption
    {
        public string Name;
        public delegate void ActionDelegate(string argument = "", string subArgument = "");
        public ActionDelegate Action;
        public string Argument;
        public string SubArgument;

        public MenuOption(string name, ActionDelegate action, string argument = "", string subArgument = "")
        {
            Name = name;
            Action = action;
            Argument = argument;
            SubArgument = subArgument;
        }
    }

    enum argumentStrings
    {
        SourceFile,
        TargetFile,
    }

    public enum MenuResultValues
    {
        None,
        Exit,
        Help,
        SaveFile
    }
}
