using ProcessCsvLibrary;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;

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
        private List<MenuOption> outputMenu = new();
        private List<MenuOption> columnMenu = new();
        private List<MenuOption> manualEntryMenu = new();
        private CsvArguments Arguments;

        private string argSource = "Source File";
        private string argTarget = "Target File";
        private string argFixBadData = "Fix bad data";
        private string argIgnoreBadData = "Ignore bad data";
        private string argIgnoreMissingFields = "Ignore missing Fields";
        private string argSourceHasHeaders = "Source has headers";
        private string argNone = "";

        ProcessTextFile processor;
        public MenuResultValues MenuResult = MenuResultValues.None;

        public Menu(CsvArguments arguments)
        {
            this.Arguments = arguments;
            processor = new ProcessTextFile(Arguments);
            processor.Message = messageOverride;
            processor.Warning = warningOverride;
            processor.Error = errorOverride;
            processor.Exit = exitOverride;
            mainMenu.Add(new MenuOption("Help - Command line arguments", ActionShowHelp, argNone, argNone, TextColor.Normal));

            // re-implement the ActionShowFileMenu and remove ActionInputFileName when there's more than one option there.
            //mainMenu.Add(new MenuOption("Select source file", ActionShowFileMenu, argSource, argNone));
            //mainMenu.Add(new MenuOption("Select target file", ActionShowFileMenu, argTarget, argNone));
            mainMenu.Add(new MenuOption("Select source file", ActionInputFileName, argSource, argNone));
            mainMenu.Add(new MenuOption("Select target file", ActionInputFileName, argTarget, argNone));

            mainMenu.Add(new MenuOption("> Select Delimiters", ActionShowDelimiterMenu, "Delimiters", argNone, TextColor.Style1));
            mainMenu.Add(new MenuOption("> Select Encoding", ActionShowEncodingMenu, "Encoding", argNone, TextColor.Style2));

            //mainMenu.Add(new MenuOption("Select columns", ActionSelectFields, argNone, argNone));
            //mainMenu.Add(new MenuOption("New headers (column names)", ActionSetHeaderText, argNone, argNone));
            mainMenu.Add(new MenuOption("> Select and rename columns", ActionShowColumnMenu, "Toggle and rename columns", argNone));
            mainMenu.Add(new MenuOption("> Manual entry", ActionShowManualEntry, "Manual entry", argNone));

            mainMenu.Add(new MenuOption("> Error handling", ActionShowErrorMenu, "Error handling", argNone, TextColor.ErrorHandling));
            mainMenu.Add(new MenuOption("> Output (Save or display result)", ActionShowOutputMenu, "Output", argNone));


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

            errorMenu.Add(new MenuOption("Set columnt count", ActionSetFieldCount, argNone, argNone));
            errorMenu.Add(new MenuOption(argSourceHasHeaders, ActionFlipBool, argSourceHasHeaders, argNone));
            errorMenu.Add(new MenuOption(argFixBadData, ActionFlipBool, argFixBadData, argNone));
            errorMenu.Add(new MenuOption(argIgnoreBadData, ActionFlipBool, argIgnoreBadData, argNone));
            errorMenu.Add(new MenuOption(argIgnoreMissingFields, ActionFlipBool, argIgnoreMissingFields, argNone));

            outputMenu.Add(new MenuOption("Display file headers", ActionDisplayHeaders, argNone, argNone));
            outputMenu.Add(new MenuOption("Display example lines", ActionDisplayExample, argNone, argNone));
            outputMenu.Add(new MenuOption("Save file and Exit", ActionSaveFile, argNone, argNone));

            manualEntryMenu.Add(new MenuOption("Set columns", ActionSelectFields, argNone, argNone));
            manualEntryMenu.Add(new MenuOption("Set header names", ActionSetHeaderText, argNone, argNone));
        }

        private void messageOverride(string message, bool quiet)
        {
            Console.WriteLine(message);
            Debug.WriteLine(message);
        }
        private void warningOverride(string message, bool quiet)
        {
            Debug.WriteLine(message);
        }

        private string delayedErrorMessage = string.Empty;
        private void errorOverride(string message, bool quiet)
        {
            delayedErrorMessage = message;
            Debug.WriteLine(message);
        }
        private void displayError()
        {
            if (delayedErrorMessage != string.Empty)
            {
                int x = Console.CursorLeft;
                int y = Console.CursorTop;
                ConsoleColor previousColor = Console.ForegroundColor;
                Console.SetCursorPosition(0, Console.BufferHeight - 2);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(delayedErrorMessage);
                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = previousColor;
            }
            delayedErrorMessage = string.Empty;
        }
        private void exitOverride(ExitCode exitCode, string? message, bool quiet, bool pause, bool exit)
        {
            if (message != null)
            {
                delayedErrorMessage = message;
                Debug.WriteLine(message);
            }
        }

        public void Start()
        {
            bool resume = true;
            while (resume)
            {
                Debug.WriteLine("Resuming Main menu");
                resume = ShowMenuOptions(mainMenu, "Main menu");
            }
            Console.Clear();
        }

        private bool ShowMenuOptions(List<MenuOption> menu, string argument = "")
        {
            ConsoleColor previousColor = Console.ForegroundColor;
            if (MenuResult != MenuResultValues.None)
            {
                // user has selected to save and exit or quit.
                return false;
            }

            Console.Clear();

            Console.WriteLine("--- " + argument + " ---");// + " " + DateTime.Now.ToLongTimeString());

            // Output the menu options, with color
            for (int i = 0; i < menu.Count; i++)
            {
                Console.ForegroundColor = menu[i].Color;
                Console.WriteLine((i + 1) + ": " + menu[i].Name);
                Console.ForegroundColor = previousColor;
            }

            // Exit or go back
            if (argument == "Main menu")
            {
                Console.WriteLine("Q: Quit");
            }
            else
            {
                Console.WriteLine("Q: Back");
            }

            // If there's room, output the Arguments to the bottom of the screen
            Console.WriteLine(Environment.NewLine);
            if (menu.Count < 15)
            {
                ShowArguments();
            }

            // display any error messages from the previous actions performed, placed before Read so you can see it.
            displayError();

            // Get user input
            Console.SetCursorPosition(0, Math.Min(menu.Count + 2, Console.BufferHeight-1));
            string userInput = string.Empty;

            // If it's possible to use single keypress input to select menu items, use ReadKey, otherwise, Read and confirm with Enter.
            if (menu.Count < 10)
            {
                ConsoleKeyInfo pressedKey = Console.ReadKey(true);
                if (pressedKey.Key == ConsoleKey.Q || pressedKey.Key == ConsoleKey.Escape)
                {
                    if (argument == "Main Menu")
                    {
                        MenuResult = MenuResultValues.Exit;
                    }
                    return false; // exit the menu
                }
                userInput = pressedKey.KeyChar.ToString();
            }
            else // Allow numbers greater than 9
            {
                userInput = Console.ReadLine()+"";
                if (userInput.ToLower() == "q")
                {
                    if (argument == "Main Menu")
                    {
                        MenuResult = MenuResultValues.Exit;
                    }
                    return false; // exit the menu
                }
            }
            
            // Only accept numbered menu selection inputs
            Console.WriteLine();
            int pressedNumber = 0;
            if (int.TryParse(userInput, out pressedNumber) == false)
            {
                pressedNumber = -1;
                //return to start
            }
            else
            {
                pressedNumber -= 1;
                // Run the delegate methods assigned to the menu option
                if (pressedNumber >= 0 && pressedNumber < menu.Count)
                {
                    menu[pressedNumber].Action(menu[pressedNumber].Argument, menu[pressedNumber].SubArgument);
                }
            }
            
            return true; // continue showing the menu
        }

        int padValue = 12;
        private void ArgumentFormatted(string name, string value, string comment, int padding, ErrorType error = ErrorType.Normal, ConsoleColor nameColor = ConsoleColor.Cyan)
        {
            ConsoleColor previousColor = Console.ForegroundColor;
            Console.ForegroundColor = nameColor;
            Console.Write(name.PadRight(padding, ' '));
            switch (error)
            {
                case ErrorType.Normal:
                    Console.ForegroundColor = TextColor.Normal;
                    break;
                case ErrorType.Warning:
                    Console.ForegroundColor = TextColor.Warning;
                    break;
                case ErrorType.Error:
                    Console.ForegroundColor = TextColor.Error;
                    break;
                case ErrorType.Information:
                    Console.ForegroundColor = TextColor.Information;
                    break;
                default:
                    Console.ForegroundColor = previousColor;
                    break;
            }

            Console.Write(value.PadRight(padValue));
            Console.ForegroundColor = TextColor.Comment;
            if (value.Length < padValue)
            {
                Console.WriteLine(" " + comment);
            }
            else
            {
                Console.WriteLine();
            }
            Console.ForegroundColor = previousColor;
        }

        private enum ErrorType
        {
            Normal,
            Error,
            Warning,
            Information,
        }

        private void ShowArguments()
        {
            int pad = 25;
            //if (Console.GetCursorPosition().Top < 10)
            Console.SetCursorPosition(0, 14);
            Console.WriteLine("ARGUMENTS ".PadRight(pad - 1, '-') + " VALUE ".PadRight(padValue + 1, '-') + " COMMENT ".PadRight(40, '-'));

            ArgumentFormatted("Source file", Arguments.SourceFile, comment: "Full or relative path of the file to load", padding: pad, error: File.Exists(Arguments.SourceFile) ? ErrorType.Normal : ErrorType.Error);
            ArgumentFormatted("Source delimiter", GetDelimiterAlias(Arguments.DelimiterRead), comment: "Example: , ; comma semicolon tab (auto guesses based on start of file)", padding: pad, ErrorType.Normal, TextColor.Style1);
            ArgumentFormatted("Source encoding", Arguments.SourceEncoding, comment: "Example: UTF-8, Latin1, codepage number", padding: pad, GetEncodingType(Arguments.SourceEncoding), TextColor.Style2);


            ArgumentFormatted("Target file", Arguments.TargetFile, comment: "Full or relative path of the file to save to", padding: pad, error: CheckTargetPathError());
            ArgumentFormatted("Target delimiter", GetDelimiterAlias(Arguments.DelimiterWrite), comment: "Example: , ; comma semicolon tab", padding: pad, ErrorType.Normal, TextColor.Style1);
            ArgumentFormatted("Target encoding", Arguments.TargetEncoding, comment: "Example: UTF-8, Latin1, codepage number", padding: pad, GetEncodingType(Arguments.TargetEncoding), TextColor.Style2);


            ArgumentFormatted("Selected columns", Arguments.SelectedColumns, comment: "Blank = Use all columns. Example: 0,1,4,8.", padding: pad);
            ArgumentFormatted("New headers", Arguments.NewHeaders, comment: "Replace headers (column names) on first line. Example: Name,Phone,Address", padding: pad);

            ArgumentFormatted("Columnt Count", Arguments.ColumnCount.ToString(), comment: "0 = Autodetect. Override if Autodetect guesses wrong", padding: pad, ErrorType.Normal, TextColor.ErrorHandling);
            ArgumentFormatted("Source has headers", Arguments.FileHasHeaders.ToString(), comment: "False if first line has data instead of header (column) names", padding: pad, ErrorType.Normal, TextColor.ErrorHandling);
            ArgumentFormatted("Fix bad data", Arguments.FixBadData.ToString(), comment: "Fixes errors due to missing quotes or fields/columns", padding: pad, ErrorType.Normal, TextColor.ErrorHandling);
            ArgumentFormatted("Ignore bad data", Arguments.IgnoreBadData.ToString(), comment: "Ignores incorrect quotes or delimiters", padding: pad, ErrorType.Normal, TextColor.ErrorHandling);
            ArgumentFormatted("Ignore missing fields", Arguments.IgnoreMissingField.ToString(), comment: "Ignores missing fields/columns, inserts blank fields", padding: pad, ErrorType.Normal, TextColor.ErrorHandling);
            /*
                    Arguments:
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

        private ErrorType GetEncodingType(string encoding)
        {
            if (int.TryParse(encoding, out int x))
                return ErrorType.Information;
            else
                return CheckEncoding(encoding) ? ErrorType.Normal : ErrorType.Error;
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

        private ErrorType CheckTargetPathError()
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
            if (validPath) return ErrorType.Normal;
            else return ErrorType.Error;
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

        //ActionShowErrorMenu
        private void ActionShowErrorMenu(string argument = "", string subArgument = "")
        {
            while (ShowMenuOptions(errorMenu, argument)) ;
        }

        private void ActionShowOutputMenu(string argument = "", string subArgument = "")
        {
            while (ShowMenuOptions(outputMenu, argument)) ;
        }

        private enum ColumnAction
        {
            ChangeName,
            Toggle
        }

        

        private void ActionShowColumnMenu(string argument = "", string subArgument = "")
        {
            newColumnNames.Clear();
            columnEnabled.Clear();
            processor.LoadFile(Arguments.SourceFile, Arguments.SourceEncoding);
            //processor.SetPattern(Arguments.SelectedColumns);
            if (processor.allRecords.Count == 0)
            {
                errorOverride("Records are empty, couldn't show columns. Enter a valid file name.", true);
                return;
            }
            columnMenu = new List<MenuOption>();
            
            for (int i = 0; i < processor.allRecords[0].Fields.Count; i++)
            {
                Field f = processor.allRecords[0].Fields[i];
                newColumnNames.Add(processor.allRecords[0].Fields[i].Text);
                columnEnabled.Add(true);
                
            }
            AddColumnMenuOptions();
            while (ShowMenuOptions(columnMenu, argument))
            {
                AddColumnMenuOptions();
                Arguments.NewHeaders = ComposeNewHeaderString(newColumnNames);
                Arguments.SelectedColumns = UpdateSelectedColumns(columnEnabled);
            }
        }

        private void AddColumnMenuOptions()
        {
            columnMenu.Clear();
            columnMenu.Add(new MenuOption(modeChange, ActionChangeColumnEditMode));
            for (int i = 0; i < newColumnNames.Count; i++)
            {
                if (columnAction == ColumnAction.ChangeName)
                {
                    columnMenu.Add(new MenuOption(newColumnNames[i], ActionSetColumnName, i.ToString(), argNone, GetColorFromBool(i)));
                }
                else if (columnAction == ColumnAction.Toggle)
                {
                    columnMenu.Add(new MenuOption(newColumnNames[i], ActionToggleColumn, i.ToString(), argNone, GetColorFromBool(i)));
                }
            }
        }

        private ConsoleColor GetColorFromBool(int col)
        {
            if (columnEnabled[col])
                return TextColor.Normal;
            else
                return TextColor.Disabled;
        }

        private string modeChange = "Switch to Toggle Mode";
        private string newColumnName = string.Empty;
        private ColumnAction columnAction = ColumnAction.ChangeName;
        List<string> newColumnNames = new List<string>();
        List<bool> columnEnabled = new List<bool>();
        private void ActionSetColumnName(string colNum, string subArgument = "")
        {
            int col = int.Parse(colNum);
            Console.Write("Enter new name for column" + col + " : ");
            newColumnName = Console.ReadLine()+"";
            newColumnNames[col] = newColumnName;
            Debug.WriteLine("Set col " + col + " to " + newColumnName);
            Arguments.ReplaceHeaders = true;
        }

        private void ActionToggleColumn(string colNum, string subArgument)
        {
            int col = int.Parse(colNum);
            Debug.WriteLine($"ActionToggleColumn: {colNum}: {col}");
            columnEnabled[col] = !columnEnabled[col];
        }

        private void ActionChangeColumnEditMode(string argument, string subArgument)
        {
            if (columnAction == ColumnAction.ChangeName)
            {
                columnAction = ColumnAction.Toggle;
                modeChange = "Switch to Change name mode";
            }
            else
            {
                columnAction = ColumnAction.ChangeName;
                modeChange = "Switch to Toggle mode";
            }
        }

        private string ComposeNewHeaderString(List<string> headers)
        {
            string result = string.Empty;
            for (int i = 0; i < headers.Count; i++ )
            {
                result += headers[i];
                if (i < headers.Count - 1)
                {
                    result += ",";
                }
            }
            Debug.WriteLine("Composed new header string: " + result);
            return result;
        }

        private string UpdateSelectedColumns(List<bool> columns)
        {
            List<int> enabledColumns = new List<int>();
            string result = string.Empty;
            for (int i = 0; i < columns.Count; i++)
            {
                if (columns[i] == true)
                {
                    enabledColumns.Add(i);
                }
            }
            for (int i = 0; i < enabledColumns.Count; i++)
            {
                result += enabledColumns[i];
                if (i < enabledColumns.Count - 1)
                {
                    result += ",";
                }
            }
            Debug.WriteLine("Composed new header string: " + result);
            return result;
        }

        private void ActionSaveFile(string argument = "", string subArgument = "")
        {
            Console.Clear();
            if (processor.LoadFile(Arguments.SourceFile, Arguments.SourceEncoding))
            {
                int linesWritten = 0;
                bool success = false;

                (success, linesWritten) = processor.SaveFile(Arguments.TargetFile, Arguments.TargetEncoding);
                if (success)
                {
                    Console.WriteLine(linesWritten + " lines written to \"" + Arguments.TargetFile + "\"");
                }
                else
                {
                    Console.WriteLine("Could not write to file \"" + Arguments.TargetFile + "\"");
                }
            }
            else
            {
                Console.WriteLine("Could not load file \"" + Arguments.SourceFile + "\"");
            }
            Console.ReadKey();
            //MenuResult = MenuResultValues.SaveFile;
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

        private void ActionShowManualEntry(string argument = "", string subArgument = "")
        {
            while (ShowMenuOptions(manualEntryMenu, argument))
            {
            }
        }

        private void ActionSelectFields(string argument = "", string subArgument = "")
        {
            Console.Write("Enter selected columns (example: 0,1,4,8): ");
            string text = Console.ReadLine() + "";
            Arguments.SelectedColumns = text;
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
            string custom = Console.ReadLine() + "";
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
            Console.Write("Set custom column count: ");
            int fieldCount = 0;
            string fieldString = Console.ReadLine() + "";
            int.TryParse(fieldString, out fieldCount);
            Arguments.ColumnCount = fieldCount;
        }

        private void ActionSetHeaderText(string argument, string subArgument)
        {
            Console.Write("Enter custom headers/column names (Example: Name,Phone,Address): ");
            Arguments.NewHeaders = Console.ReadLine() + "";
            if (Arguments.NewHeaders.Length > 0)
            {
                Arguments.ReplaceHeaders = true;
            }
            else
            {
                Arguments.ReplaceHeaders = false;
            }
        }

        private void ActionDisplayExample(string argument, string subArgument)
        {
            Console.Clear();

            processor.LoadFile(Arguments.SourceFile, Arguments.SourceEncoding);
            processor.SetPattern(Arguments.SelectedColumns);
            Console.WriteLine(Environment.NewLine + "Displaying " + Arguments.ExampleLines + " example lines from the result:" + Environment.NewLine, Arguments.Quiet);
            string display = Messages.GetResultRecordsAsText(processor.allRecords, processor.fieldIndexes, true, Arguments.ExampleLines, Messages.Message, Arguments.DelimiterWrite);
            Console.WriteLine(display);

            Console.ReadKey();
        }

        private void ActionDisplayHeaders(string argument, string subArgument)
        {
            Console.Clear();
            processor.LoadFile(Arguments.SourceFile, Arguments.SourceEncoding);
            Messages.displayHeaders(processor, Arguments, messageOverride);
            Console.ReadKey();
        }

    }

    class MenuOption
    {
        public string Name;
        public delegate void ActionDelegate(string argument = "", string subArgument = "");
        public ActionDelegate Action;
        public string Argument;
        public string SubArgument;
        public ConsoleColor Color;

        public MenuOption(string name, ActionDelegate action, string argument = "", string subArgument = "", ConsoleColor color = ConsoleColor.Cyan)
        {
            Name = name;
            Action = action;
            Argument = argument;
            SubArgument = subArgument;
            Color = color;
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
