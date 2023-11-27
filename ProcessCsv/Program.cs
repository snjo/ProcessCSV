// ProcessCSV by Andreas Aakvik Gogstad, 2023
// https://github.com/snjo/processcsv
// Uses CsvHelper by Josh Close
// https://joshclose.github.io/CsvHelper/

using System.Diagnostics;
using System.Text;
using ProcessCsvLibrary;

namespace ProcessCSV
{

    internal class Program
    {

        static int Main(string[] commandLineArgs)
        {
            // Set the encoding of the console in order to display norwegian characters (æøå) or others correctly
            try
            {
                Console.OutputEncoding = Encoding.UTF8;
            }
            catch
            {
                Messages.Warning("Could not set console encoding to UTF8, some characters may display incorrectly");
            }

            // if no argmunts are supplied, show help
            if (commandLineArgs.Length == 0)
            {
                HelpOutput.helpText(pause: false);
                return (int)ExitCode.InfoShown;
            }

            ProcessTextFile processor = new ProcessTextFile();
            // convert command line arguments, must happen before processing starts
            processArgs(commandLineArgs, processor.Arguments);
            CsvArguments arguments = processor.Arguments;

            // show help if /? or /help. Will exit the program, no file operations will be done.
            if (arguments.Help)
            {
                HelpOutput.helpText(arguments.Pause);
            }
            
            // if a source file is specified, load
            if (arguments.SourceFile.Length > 0)
            {
                processor.LoadFile(arguments.SourceFile, arguments.SourceEncoding);
                processor.SetPattern(arguments.SelectedFields);

                // output a number of lines from the result to console, with line number prefix
                if (arguments.DisplayResult)
                {
                    Messages.Message(Environment.NewLine + "Displaying " + arguments.ExampleLines + " example lines from the result:" + Environment.NewLine, arguments.Quiet);
                    string display = processor.GetResultRecordsAsText(true, arguments.ExampleLines);
                    Messages.Message(display, quiet: false);
                }

                // output the contents of line 0 of the source file to console, with line number prefix
                if (arguments.DisplayHeaders)
                {
                    displayHeaders(processor, arguments);
                }

                // if the target file is specified, save
                if (arguments.TargetFile.Length > 0)
                {
                    processor.SaveFile(arguments.TargetFile, arguments.TargetEncoding);
                }

                // all operations are complete, exit
                Messages.ExitProgram(exitCode: ExitCode.Success, "Operation completed successfully.", arguments.Quiet, arguments.Pause);
                return (int)ExitCode.Success; // should be unreachable
            }
            else
            {
                Messages.ExitProgram(exitCode: ExitCode.SourceFileNotFound, message: "No source file specified. Use /? for /help info.", arguments.Quiet, arguments.Pause);
                return (int)ExitCode.SourceFileNotFound; // should be unreachable
            }
        }

        /// <summary>
        /// Shows a list of headers/column names based on the first line of the file.
        /// </summary>
        /// <param name="processor">ProcessTextFile</param>
        /// <param name="arguments">ProcessTextFile.Arguments</param>
        private static void displayHeaders(ProcessTextFile processor, CsvArguments arguments)
        {
            Messages.Message("\nDisplaying Headers from line 0 of the source file:\n", arguments.Quiet);
            int fieldNumber = 0;
            Field[]? fieldArray = processor.GetRecordAsArray(0);
            if (fieldArray != null)
            {
                for (int i = 0; i < fieldArray.Length; i++)
                {
                    Field fieldName = fieldArray[i];
                    Messages.Message(fieldNumber.ToString().PadRight(3) + ": " + fieldName.Text, quiet: false);
                    fieldNumber++;
                }
            }
            else
            {
                Messages.Warning("/headers: Could not get fields to display");
            }
            Messages.Message("", quiet: false); // blank line
        }

        /// <summary>
        /// Check the command line arguments and updates the arguments in the processor
        /// </summary>
        /// <param name="commandLineArgs">An array of arguments from the command line</param>
        /// <param name="arguments">the Arguments object of ProcessTextFile</param>
        private static void processArgs(string[] commandLineArgs, CsvArguments arguments)
        {
            
            if (commandLineArgs.Length > 1) // check if the first and second argument are file names
            {
                arguments.SourceFile = AssumeFileNameFromArgument(commandLineArgs[0], checkFileExists: true);
                if (arguments.SourceFile != string.Empty) // only proceed to assume target if source was also assumed
                {
                    arguments.TargetFile = AssumeFileNameFromArgument(commandLineArgs[1], checkFileExists: false);
                }
            }
            // check the arguments passed in the command line, starting with / or -
            for (int i = 0; i < commandLineArgs.Length; i++)
            {
                string firstChar = commandLineArgs[i].Substring(0, 1);
                if ((firstChar == "-") || (firstChar == "/"))
                {
                    string commandType = commandLineArgs[i].Substring(1);
                    string? commandValue = null;

                    if (i + 1 < commandLineArgs.Length)
                    {
                        // if the next argument exists, and starts with a / or -, it's a new argument, not the value for the current argument
                        commandValue = commandLineArgs[i + 1];
                        string firstCharValue = commandValue.Substring(0, 1);
                        if ((firstCharValue == "-") || (firstCharValue == "/"))
                        {
                            commandValue = null;
                        }
                    }

                    commandType = commandType.ToLower();

                    switch (commandType)
                    {
                        case "help":
                        case "?":
                            arguments.Help = true;
                            // Will show help screen
                            break;
                        case "q":
                        case "quiet":
                            Debug.WriteLine("Quiet mode on");
                            arguments.Quiet = true;
                            break;
                        case "load":
                        case "l":
                            if (commandValue != null)
                            {
                                arguments.SourceFile = commandValue;
                            }
                            else
                            {
                                Messages.Warning("/load used, but no load file was specified. Example: /l source.csv");
                            }
                            break;
                        case "save":
                        case "s":
                            if (commandValue != null)
                            {
                                arguments.TargetFile = commandValue;
                            }
                            else
                            {
                                Messages.Warning("/save used, but no save file was specified. Example: /s result.csv");
                            }
                            break;
                        case "inencoding":
                        case "ie":
                            if (commandValue != null)
                            {
                                arguments.SourceEncoding = commandValue;
                            }
                            else
                            {
                                Messages.Warning("/inencoding used, but no encoding was specified. Examples: /ie Latin1  /ie UTF-8");
                            }
                            break;
                        case "outencoding":
                        case "oe":
                            if (commandValue != null)
                            { 
                                arguments.TargetEncoding = commandValue;
                            }
                            else
                            {
                                Messages.Warning("/outencoding used, but no encoding was specified. Examples: /oe Latin1  /oe UTF-8");
                            }
                            break;
                        case "fieldselect":
                        case "fs":
                            if (commandValue != null)
                            { 
                                arguments.SelectedFields = commandValue;
                            }
                            else
                            {
                                Messages.Warning("/fieldselect used, but no fields were specified. Example: /fs 0,-1,4,8");
                            }
                            break;
                        case "example":
                        case "ex":
                            arguments.DisplayResult = true;
                            if (commandValue != null)
                            {
                                int.TryParse(commandValue, out arguments.ExampleLines);
                            }
                            break;
                        case "indelimiter":
                        case "id":
                            if (commandValue != null)
                            {
                                GetDelimiter(ref arguments.DelimiterRead, commandValue);
                            }
                            else
                            {
                                Messages.Warning("/indelimiter used, but no delimiter was specified. Example: /id ;");
                            }
                            break;
                        case "outdelimiter":
                        case "od":
                            if (commandValue != null)
                            {
                                GetDelimiter(ref arguments.DelimiterWrite, commandValue);
                            }
                            else
                            {
                                Messages.Warning("/outdelimiter used, but no delimiter was specified. Examples: /od tab  /od comma  /od ;");
                            }
                            break;
                        case "headers":
                        case "hd":
                            arguments.DisplayHeaders = true;
                            break;
                        case "pause":
                        case "p":
                            arguments.Pause = true;
                            Debug.WriteLine("Pause at end: " + arguments.Pause);
                            break;
                        case "fieldcount":
                        case "fc":
                            if (commandValue != null)
                            {
                                if (int.TryParse(commandValue, out arguments.FieldCount) == false)
                                {
                                    Messages.Warning("/fc used, but the value was not a valid number. Examples: /fc 4");
                                }
                            }
                            else
                            {
                                Messages.Warning("/fc used, but no delimiter was specified. Examples: /fc 4");
                            }
                            break;
                        case "ignorebaddata":
                        case "ibd":
                            Messages.Warning("Ignoring exceptions from bad data (/ibd). This can cause fields to appear in the wrong column.");
                            arguments.IgnoreBadData = true;
                            break;
                        case "ignoremissing":
                        case "imf":
                            Messages.Warning("Ignoring exceptions from missing fields (/imf). Missing fields will be replaced by empty values.");
                            arguments.IgnoreMissingField = true;
                            break;
                        case "fixbaddata":
                        case "fbd":
                            arguments.FixBadData = true;
                            break;
                        case "noheaders":
                        case "noh":
                            arguments.FileHasHeaders = false;
                            break;
                        case "newheaders":
                        case "nwh":
                            if (commandValue != null)
                            {
                                arguments.ReplaceHeaders = true;
                                arguments.NewHeaders = commandValue;
                            }
                            else
                            {
                                Messages.Warning("/hewheaders used, but no header names was specified. Generating generic column names");
                                arguments.ReplaceHeaders = true;
                            }
                            break;
                        default:
                            Messages.ExitProgram(exitCode: ExitCode.InvalidArgument, "Invalid argument passed: /" + commandType, arguments.Quiet, arguments.Pause);
                            break;
                            
                    }

                    //check conflicting arguments
                    if (arguments.FixBadData && arguments.IgnoreBadData)
                    {
                        Messages.Warning("Fix Bad Data requires Ignore Bad Data to be off. Disabling the ignore argument");
                        arguments.IgnoreBadData = false;
                    }
                }
            }
        }

        private static string AssumeFileNameFromArgument(string argument, bool checkFileExists)
        {
            string firstChar = argument.Substring(0, 1);
            if ((firstChar != "-") && (firstChar != "/"))
            {
                if (argument.Contains("."))
                {
                    // set first argument as the source file. Can be overridden in the following loop.
                    if (checkFileExists)
                    {
                        if (File.Exists(argument))
                        {
                            Messages.Message("Assuming unspecified argument is file name: " + argument + " (This file exists)");
                            return argument;
                        }
                    }
                    else
                    {
                        Messages.Message("Assuming unspecified argument is file name: " + argument);
                        return argument;
                    }
                }
                Messages.Message("Assuming unspecified argument is file name: " + argument + ", but that file can't be found.");
            }
            return string.Empty;
        }

        private static void GetDelimiter(ref string delim, string? commandValue)
        {
            if (commandValue != null)
            {
                switch (commandValue)
                {
                    case "tab":
                        delim = "\t";
                        break;
                    case "comma":
                        delim = ",";
                        break;
                    case "semicolon":
                        delim = ";";
                        break;
                    default:
                        delim = commandValue;
                        break;
                }
            }
        }
    }
}



