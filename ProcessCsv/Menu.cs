﻿using ProcessCsvLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO.IsolatedStorage;
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
        private List<MenuOption> errorMenu = new();
        private List<MenuOption> headerMenu = new();
        private CsvArguments Arguments;

        private string argSource = "Source File";
        private string argTarget = "Target File";
        private string argFixBadData = "Fix bad data";
        private string argIgnoreBadData = "Ignore bad data";
        private string argIgnoreMissingFields = "Ignore missing Fields";
        private string argSourceHasHeaders = "Source has headers";
        private string argNone = "";

        public MenuResultValues MenuResult = MenuResultValues.None;

        public Menu(CsvArguments arguments)
        {
            this.Arguments = arguments;
            mainMenu.Add(new MenuOption("Help", ActionShowHelp));

            // re-implement the ActionShowFileMenu and remove ActionInputFileName when there's more than one option there.
            //mainMenu.Add(new MenuOption("Select source file", ActionShowFileMenu, argSource, argNone));
            //mainMenu.Add(new MenuOption("Select target file", ActionShowFileMenu, argTarget, argNone));
            mainMenu.Add(new MenuOption("Select source file", ActionInputFileName, argSource, argNone));
            mainMenu.Add(new MenuOption("Select target file", ActionInputFileName, argTarget, argNone));
            
            mainMenu.Add(new MenuOption("> Select Delimiters", ActionShowDelimiterMenu, "DELIMITERS", argNone));
            mainMenu.Add(new MenuOption("> Select Encoding", ActionShowEncodingMenu, "ENCODING", argNone));

            mainMenu.Add(new MenuOption("Select Fields (Columns)", ActionSelectFields, "SELECT FIELDS", argNone));
            mainMenu.Add(new MenuOption("New header (column) names", ActionSetHeaderText, argNone, argNone));
            
            mainMenu.Add(new MenuOption("> Error handling", ActionShowErrorMenu, "ERROR HANDLIG", argNone));
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

            errorMenu.Add(new MenuOption(argFixBadData, ActionFlipBool, argFixBadData, argNone));
            errorMenu.Add(new MenuOption(argIgnoreBadData, ActionFlipBool, argIgnoreBadData, argNone));
            errorMenu.Add(new MenuOption(argIgnoreMissingFields, ActionFlipBool, argIgnoreMissingFields, argNone));
            errorMenu.Add(new MenuOption(argSourceHasHeaders, ActionFlipBool, argSourceHasHeaders, argNone));
            errorMenu.Add(new MenuOption("Set field count", ActionSetFieldCount, argNone, argNone));
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
                Console.WriteLine((i+1) + ": " + menu[i].Name);
            }

            if (argument == "Main menu")
            {
                Console.WriteLine("Q: Quit");
            }
            else
            {
                Console.WriteLine("Q: Back");
            }

            Console.WriteLine(Environment.NewLine + Environment.NewLine);
            ShowArguments();
            Console.SetCursorPosition(0, menu.Count+2);

            ConsoleKeyInfo pressedKey = Console.ReadKey(true);
            if (pressedKey.Key == ConsoleKey.Q || pressedKey.Key == ConsoleKey.Escape)
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
                pressedNumber -= 1;
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

        int padValue = 12;
        private void ArgumentFormatted(string name, string value, string comment, int padding, bool error = false)
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
            Console.Write(value.PadRight(padValue));
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(" " + comment);
            Console.ForegroundColor = previousColor;
        }

        private void ShowArguments()
        {
            int pad = 25;
            Console.WriteLine("ARGUMENTS ".PadRight(pad-1, '-') + " VALUE ".PadRight(padValue+1, '-') + " COMMENT ".PadRight(40,'-'));
            ArgumentFormatted("Source file", Arguments.SourceFile, comment: "", padding: pad, error: !File.Exists(Arguments.SourceFile));
            ArgumentFormatted("Source encoding", Arguments.SourceEncoding, comment: "Example: UTF-8, Latin1, codepage number", padding: pad, !CheckEncoding(Arguments.SourceEncoding));
            ArgumentFormatted("Source delimiter", GetDelimiterAlias(Arguments.DelimiterRead), comment: "Example: , ; comma semicolon tab (auto guesses based on start of file)", padding: pad);

            ArgumentFormatted("Target file", Arguments.TargetFile, comment: "", padding: pad, error: CheckTargetPathError());
            ArgumentFormatted("Target encoding", Arguments.TargetEncoding, comment: "Example: UTF-8, Latin1, codepage number", padding: pad, !CheckEncoding(Arguments.TargetEncoding));
            ArgumentFormatted("Target delimiter", GetDelimiterAlias(Arguments.DelimiterWrite), comment: "Example: , ; comma semicolon tab (auto guesses based on start of file)", padding: pad);
            Console.WriteLine();
            ArgumentFormatted("Selected Fields", Arguments.SelectedFields, comment: "Example: 0,1,4,8", padding: pad);
            ArgumentFormatted("New headers", Arguments.NewHeaders, comment: "Example: \"Name\",\"Phone\"", padding: pad);
            ArgumentFormatted("Source has headers", Arguments.FileHasHeaders.ToString(), comment: "False if first line has data instead of column names", padding: pad);
            ArgumentFormatted("Field Count", Arguments.FieldCount.ToString(), comment: "0 = Autodetect. Override if Autodetect guesses wrong", padding: pad);
            
            ArgumentFormatted("Fix bad data", Arguments.FixBadData.ToString(), comment: "Fixes errors due to missing quotes or fields", padding: pad);
            ArgumentFormatted("Ignore bad data", Arguments.IgnoreBadData.ToString(), comment: "Ignores incorrect quotes or delimiters", padding: pad);
            ArgumentFormatted("Ignore missing fields", Arguments.IgnoreMissingField.ToString(), comment: "Ignores missing fields, inserts blank fields", padding: pad);
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

        private string GetDelimiterAlias(string delimiter)
        {
            if (delimiter == "\t") return "tab";
            else return delimiter;
        }

        private bool CheckEncoding(string encoding)
        {
            if (encoding == null) return false;
            if (encoding.Length == 0) return false;
            try
            {
                Encoding enc = Encoding.GetEncoding(encoding);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool CheckTargetPathError()
        {
            string targetFile = Arguments.TargetFile;
            string? targetDirectory;
            bool validPath = false;
            if (targetFile.Length > 0)
            {
                if (targetFile.Contains("\\") == false)
                {
                    if (targetFile.Contains(":") == true)
                    {
                        // the path contains a :, like c:test.csv. That's not OK.
                        validPath = false;
                    }
                    else
                    {
                        // the path is the local directory, it's OK.
                        validPath = true;
                    }


                }
                else if (Directory.Exists(targetFile))
                {
                    // While the directory exists, we're missing a file name
                    validPath = false;
                }
                else
                {
                    // get the directory minus the file name
                    targetDirectory = Path.GetDirectoryName(targetFile);
                    if (Directory.Exists(targetDirectory))
                    {
                        // the directory exists
                        validPath = true;
                    }
                }
            }
            bool targetPathError = !validPath;
            return targetPathError;
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
            while (ShowMenuOptions(delimiterMenu, argument));
        }

        //ActionShowEncodingMenu
        private void ActionShowEncodingMenu(string argument = "", string subArgument = "")
        {
            while (ShowMenuOptions(encodingMenu, argument));
        }

        //ActionShowErrorMenu
        private void ActionShowErrorMenu(string argument = "", string subArgument = "")
        {
            while (ShowMenuOptions(errorMenu, argument));
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
            Console.Write("Enter selected Fields (example: 0,1,4,8): ");
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
                Arguments.DelimiterRead = Tools.GetDelimiter(text);
            }
            else if (argument == argTarget)
            {
                Arguments.DelimiterWrite = Tools.GetDelimiter(text);
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

        private void ActionFlipBool(string argument, string subArgument)
        {
            Debug.WriteLine("Flipping bool: " + argument);
            if (argument == argFixBadData)
            {
                Arguments.FixBadData = !Arguments.FixBadData;
            }
            else if (argument == argIgnoreBadData)
            {
                Arguments.IgnoreBadData = !Arguments.IgnoreBadData;
            }
            else if (argument == argIgnoreMissingFields)
            {
                Arguments.IgnoreMissingField = !Arguments.IgnoreMissingField;
            }
            else if (argument == argFixBadData)
            {
                Arguments.FixBadData = !Arguments.FixBadData;
            }
            else if (argument == argSourceHasHeaders)
            {
                Arguments.FileHasHeaders = !Arguments.FileHasHeaders;
            }
             /*
             New headers
             Field Count
             */
        }

        private void ActionSetFieldCount(string argument, string subArgument)
        {
            Console.Write("Set custom field count: ");
            int fieldCount = 0;
            string fieldString = Console.ReadLine()+"";
            int.TryParse(fieldString, out fieldCount);
            Arguments.FieldCount = fieldCount;
        }

        private void ActionSetHeaderText(string argument, string subArgument)
        {
            Console.Write("Enter custom column names: ");
            Arguments.NewHeaders = Console.ReadLine()+"";
            if (Arguments.NewHeaders.Length > 0)
            {
                Arguments.ReplaceHeaders = true;
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

    //enum argumentStrings
    //{
    //    SourceFile,
    //    TargetFile,
    //    FixBadData,
    //    IgnoreBadData,
    //    IgnoreMissingFields
    //}

    public enum MenuResultValues
    {
        None,
        Exit,
        Help,
        SaveFile
    }
}
