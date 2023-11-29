// ProcessCSV by Andreas Aakvik Gogstad, 2023
// https://github.com/snjo/processcsv
// Uses CsvHelper by Josh Close
// https://joshclose.github.io/CsvHelper/

using ProcessCsv;
using ProcessCsvLibrary;
using System.Diagnostics;
using System.Text;

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
                Messages.Warning("Could not set console encoding to UTF8, some characters may display incorrectly", quiet: false);
            }

            ProcessTextFile processor = new ProcessTextFile();

            // if no argmunts are supplied, show help
            if (commandLineArgs.Length == 0)
            {
                // test input menu
                Menu menu = new Menu(processor.Arguments);
                menu.Start();
                if (menu.MenuResult == MenuResultValues.SaveFile)
                {
                    // proceed
                }
                else if (menu.MenuResult == MenuResultValues.Help)
                {
                    HelpOutput.helpText(pause: true, exit: true);
                    return (int)ExitCode.InfoShown; // unreachable
                }
                else
                {
                    return (int)ExitCode.Aborted;
                }
            }
            else
            {
                // convert command line arguments, must happen before processing starts
                processArgs(commandLineArgs, processor.Arguments);
            }

            //processArgs(commandLineArgs, processor.Arguments);
            CsvArguments arguments = processor.Arguments;

            // You can suppress error or warnings. Not implemented as arguments yet - TODO
            //arguments.SupressErrors = true;
            //arguments.SupressWarnings = true;

            // show help if /? or /help. Will exit the program, no file operations will be done.
            if (arguments.Help)
            {
                HelpOutput.helpText(arguments.Pause, exit: true);
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
                    //if (arguments.SelectedFields.Length > 0)
                    //{
                    //    processor.CreateDefaultPattern();
                    //}
                    processor.SetPattern(arguments.SelectedFields);
                    string display = Messages.GetResultRecordsAsText(processor.allRecords, processor.fieldIndexes, true, arguments.ExampleLines, Messages.Message, arguments.DelimiterWrite);
                    Messages.Message(display, quiet: false);
                }

                // output the contents of line 0 of the source file to console, with line number prefix
                if (arguments.DisplayHeaders)
                {
                    Messages.displayHeaders(processor, arguments, Messages.Message);
                }

                // if the target file is specified, save
                if (arguments.TargetFile.Length > 0)
                {
                    processor.SaveFile(arguments.TargetFile, arguments.TargetEncoding);
                }

                // all operations are complete, exit
                Messages.ExitProgram(exitCode: ExitCode.Success, "Operation completed successfully.", arguments.Quiet, arguments.Pause, exit: true);
                return (int)ExitCode.Success; // should be unreachable
            }
            else
            {
                Messages.ExitProgram(exitCode: ExitCode.SourceFileNotFound, message: "No valid source file specified. Use /? or /help for info.", arguments.SupressErrors, arguments.Pause, exit: true);
                return (int)ExitCode.SourceFileNotFound; // should be unreachable
            }
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
                                Messages.Warning("/load used, but no load file was specified. Example: /l source.csv", arguments.SupressWarnings);
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
                                Messages.Warning("/save used, but no save file was specified. Example: /s result.csv", arguments.SupressWarnings);
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
                                Messages.Warning("/inencoding used, but no encoding was specified. Examples: /ie Latin1  /ie UTF-8", arguments.SupressWarnings);
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
                                Messages.Warning("/outencoding used, but no encoding was specified. Examples: /oe Latin1  /oe UTF-8", arguments.SupressWarnings);
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
                                Messages.Warning("/fieldselect used, but no fields were specified. Example: /fs 0,-1,4,8", arguments.SupressWarnings);
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
                                arguments.DelimiterRead = Tools.GetDelimiter(commandValue);
                            }
                            else
                            {
                                Messages.Warning("/indelimiter used, but no delimiter was specified. Example: /id ;", arguments.SupressWarnings);
                            }
                            break;
                        case "outdelimiter":
                        case "od":
                            if (commandValue != null)
                            {
                                arguments.DelimiterWrite = Tools.GetDelimiter(commandValue);
                            }
                            else
                            {
                                Messages.Warning("/outdelimiter used, but no delimiter was specified. Examples: /od tab  /od comma  /od ;", arguments.SupressWarnings);
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
                                    Messages.Warning("/fc used, but the value was not a valid number. Examples: /fc 4", arguments.SupressWarnings);
                                }
                            }
                            else
                            {
                                Messages.Warning("/fc used, but no delimiter was specified. Examples: /fc 4", arguments.SupressWarnings);
                            }
                            break;
                        case "ignorebaddata":
                        case "ibd":
                            Messages.Warning("Ignoring exceptions from bad data (/ibd). This can cause fields to appear in the wrong column.", arguments.SupressWarnings);
                            arguments.IgnoreBadData = true;
                            break;
                        case "ignoremissing":
                        case "imf":
                            Messages.Warning("Ignoring exceptions from missing fields (/imf). Missing fields will be replaced by empty values.", arguments.SupressWarnings);
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
                                Messages.Warning("/hewheaders used, but no header names was specified. Generating generic column names", arguments.SupressWarnings);
                                arguments.ReplaceHeaders = true;
                            }
                            break;
                        default:
                            Messages.ExitProgram(exitCode: ExitCode.InvalidArgument, "Invalid argument passed: /" + commandType, arguments.SupressErrors, arguments.Pause, exit: true);
                            break;

                    }

                    //check conflicting arguments
                    if (arguments.FixBadData && arguments.IgnoreBadData)
                    {
                        Messages.Warning("Fix Bad Data requires Ignore Bad Data to be off. Disabling the ignore argument", arguments.SupressWarnings);
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
                            Debug.WriteLine("Assuming unspecified argument is file name: " + argument + " (This file exists)");
                            return argument;
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Assuming unspecified argument is file name: " + argument);
                        return argument;
                    }
                }
                Debug.WriteLine("Assuming unspecified argument is file name: " + argument + ", but that file can't be found.");
            }
            return string.Empty;
        }
    }
}



