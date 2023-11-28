// ProcessCSV by Andreas Aakvik Gogstad, 2023
// https://github.com/snjo/processcsv
// Uses CsvHelper by Josh Close
// https://joshclose.github.io/CsvHelper/
using CsvHelper;
using CsvHelper.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace ProcessCsvLibrary
{
    public class ProcessTextFile
    {
        /// <summary>
        /// Input arguments based on command line arguments. Defines the actions of the processor
        /// </summary>
        public CsvArguments Arguments = new CsvArguments();

        // The message methods can be replaced if used outside of console application
        public delegate void MessageDelegate(string message, bool quiet);
        public delegate void ExitDelegate(ExitCode exitCode, string? message, bool quiet, bool pause, bool exit);
        public MessageDelegate Message = Messages.Message;
        public MessageDelegate Warning = Messages.Warning;
        public MessageDelegate Error = Messages.Error;
        public ExitDelegate Exit = Messages.ExitProgram;

        string[]? linesOut;
        List<Record> allRecords = new List<Record>(); // a record is equivalent to a row in the text file       
        int[] fieldIndexes = new int[0];
        //public bool recordsLoaded = false;
        private int fieldCount = 0;
        private int columnCount = 0;
        string[] linesAsArray = new string[0];

        public ProcessTextFile()
        {
        }

        public ProcessTextFile(CsvArguments arguments)
        {
            Arguments = arguments;
        }

        /// <summary>
        /// Loads a text file into a list of records, and an array of raw text lines. Exits the program if Arguments.ExitOnError is true and load fails
        /// </summary>
        /// <param name="file"></param>
        /// <param name="encoding"></param>
        /// <returns>True if the file loaded, false if not.</returns>
        public bool LoadFile(string file, Encoding encoding)
        {
            file = Environment.ExpandEnvironmentVariables(file);
            if (File.Exists(file))
            {
                Message("Loading CSV (" + file + ") with codepage " + encoding.EncodingName, Arguments.Quiet);
                linesAsArray = File.ReadAllLines(file);
                GetAllFields(file, encoding);
                return true;
            }
            else
            {
                Exit(exitCode: ExitCode.SourceFileNotFound, message: "File not found: " + file, Arguments.SupressErrors, Arguments.Pause, exit: Arguments.ExitOnError);
                return false;
            }
        }

        /// <summary>
        /// Allows using LoadFile with a string encoding name
        /// </summary>
        /// <param name="file">the source file</param>
        /// <param name="encoding">name of an encoding</param>
        public void LoadFile(string file, string encoding)
        {
            Encoding enc = GetEncoding(encoding);
            LoadFile(file, enc);
        }

        /// <summary>
        /// Get encoding from an encoding name or alias
        /// </summary>
        /// <param name="encodingName">name of an encoding</param>
        /// <returns>encoding matching the codepage, or UTF-8 if it can't be found<</returns>
        public Encoding GetEncoding(string encodingName)
        {
            Debug.WriteLine("Getting encoding from value:" + encodingName);
            switch (encodingName.ToLowerInvariant())
            {
                case "":
                    return Encoding.UTF8;
                case "default":
                    return Encoding.Default;
                case "utf-8":
                case "utf8":
                    return Encoding.UTF8;
                case "utf-8-bom":
                case "utf8bom":
                    Arguments.ByteOrderMark = true;
                    return Encoding.UTF8;
                case "latin1":
                    return Encoding.Latin1;
                case "ascii":
                    return Encoding.ASCII;
                default:
                    try
                    {
                        Debug.WriteLine("Custom encoding detected");
                        if (int.TryParse(encodingName, out int value))
                        {
                            Debug.WriteLine("Encoding, parsing codepage: " + value);
                            Debug.WriteLine("Encoding result:" + GetEncodingDOS(value).EncodingName);
                            return GetEncodingDOS(value);
                        }
                        else
                        {
                            Debug.WriteLine("Encoding, parsing name: " + encodingName);
                            Debug.WriteLine("Encoding result:" + Encoding.GetEncoding(encodingName));
                            return Encoding.GetEncoding(encodingName);
                        }
                    }
                    catch
                    {
                        Exit(ExitCode.InvalidEncoding, "Error parsing encoding " + encodingName, Arguments.SupressErrors, Arguments.Pause, exit: Arguments.ExitOnError);
                        return Encoding.UTF8; // should be unreachable
                    }   
            }
        }

        /// <summary>
        /// Gets the encoding corresponding with a numbered codepage
        /// </summary>
        /// <param name="codepage">a numbered codepage</param>
        /// <returns>encoding matching the codepage, or UTF-8 if it can't be found</returns>
        public static Encoding GetEncodingDOS(int codepage)
        {
            Encoding encoding;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            try
            {
                encoding = Encoding.GetEncoding(codepage);
            }
            catch
            {
                encoding = Encoding.UTF8;
            }
            return encoding;
        }

        /// <summary>
        /// Gets the contents of a single record as a single string.
        /// </summary>
        /// <param name="selectedLine"></param>
        /// <returns>a single record, with quotes around each field.</returns>
        public string GetResultRecordConcatenated(int selectedLine)
        {
            string result = string.Empty;
            //Debug.WriteLine("Concat: " + fieldIndexes.Length); // TEST
            for (int i = 0; i < fieldIndexes.Length; i++)
            {
                List<Field>? record = allRecords.SafeIndex((int)selectedLine, null);

                if (record == null) { Debug.WriteLine("record is null"); }
                else if (record.Count == 0) { Debug.WriteLine("record count is 0"); }

                if (record != null && record.Count > 0)
                {
                    //Debug.WriteLine("Concat 2: " + record.SafeIndex(fieldIndexes[i], "*")); // TEST
                    Field? field = record.SafeIndex(fieldIndexes[i]);
                    if (field != null)
                    {
                        result += FixQuotes(field.Text);
                    }
                    else
                    {
                        if (fieldIndexes[i] != -1) // ignore index errors when using -1, that's used for leaving a blank field on purpose. If someone used another out of index value, warn them.
                            Warning("Error reading field " + i + " on line " + selectedLine + ". Field select " + fieldIndexes[i] + " is out of range.", quiet: Arguments.SupressWarnings);
                    }
                    //result += record.SafeIndex(fieldIndexes[i], "*");
                    if (i < fieldIndexes.Length - 1)
                        result += Arguments.DelimiterWrite;
                }
                else
                {
                    Debug.WriteLine("getrecord error");
                    //result += Environment.NewLine;
                }
            }
            return result;
        }

        /// <summary>
        /// Gets the text used in Display examble record /ex
        /// </summary>
        /// <param name="limitNumberOfLines">If true, shows either 5 lines, or the number of lines set in maxLines</param>
        /// <param name="maxLines">The number of records to show as an example, starting from line 0. Only used if LimitNumberOfLines is true</param>
        /// <returns>The full text used in example output</returns>
        public string GetResultRecordsAsText(bool limitNumberOfLines = false, int maxLines=0)
        {
            string result = string.Empty;
            if (limitNumberOfLines == false)
                maxLines = allRecords.Count;
            for (int i = 0; i < maxLines; i++)
            {
                result += i.ToString().PadRight(3) + ":  ";
                result += GetResultRecordConcatenated(i);
                if (i < maxLines - 1)
                    result += Environment.NewLine;
            }
            return result;
        }

        /// <summary>
        /// outputs a single record as an array of fields
        /// </summary>
        /// <param name="selectedLine">the line number of the record</param>
        /// <returns>outputs the array of fields, or null if the index is not present</returns>
        public Field[]? GetRecordAsArray(int selectedLine)
        {
            List<Field>? result = allRecords.SafeIndex(selectedLine);
            if (result == null)
            {
                return null;
            }
            else
            {
                return result.ToArray();
            }
        }

        /// <summary>
        /// Save the processed records and fields to the target file
        /// </summary>
        /// <param name="filename">the target file</param>
        /// <param name="encoding">encoding used for the target file</param>
        public void SaveFile(string filename, Encoding encoding)
        {
            Message("Saving to file: " + filename + ", Encoding: " + encoding.EncodingName, Arguments.Quiet);
            linesOut = processLinesCSV().ToArray();
            try
            {
                Debug.WriteLine("Writing " + linesOut.Length + " lines to file");
                // A blank line is added to the end of the file because of the final line break CR LF. This is expected behavior from WriteAllLines, but unfortunate.
                if (encoding == Encoding.UTF8 && Arguments.ByteOrderMark == false)
                    File.WriteAllLines(filename, linesOut); // Don't add Byte order mark (0x EF BB BF) to the start of the file in UTF-8
                else
                    File.WriteAllLines(filename, linesOut, encoding); // set custom encoding, when UTF-8 encoding is specified, byte order mark gets added automatically
            }
            catch (Exception e)
            {
                ExitCode exitCode = ExitCode.TargetFileError;
                string errorMessage = string.Empty;
                if (IllegalCharactersFound(filename))
                {
                    // only warn of illegal characters if saving with them caused an exception, not preventing the attempt on assumed wrong names.
                    errorMessage = "Can't save file, illegal characters in file name: " + filename;
                }
                else if (e is DirectoryNotFoundException)
                {
                    errorMessage = "Directory not found: " + Path.GetDirectoryName(filename);
                    exitCode = ExitCode.TargetDirectoryNotFound;
                }
                else if (e is UnauthorizedAccessException)
                {
                    errorMessage = "Can't write to folder, Unauthorized: " + Path.GetDirectoryName(filename);
                    exitCode = ExitCode.TargetUnauthorized;
                }
                else
                {
                    errorMessage = "Unknown error, see details below:" + Environment.NewLine + e.ToString();
                }
                Error(Environment.NewLine + errorMessage, quiet: Arguments.SupressErrors);
                Exit(exitCode: exitCode, message: "Error saving to file: " + filename, Arguments.SupressErrors, Arguments.Pause, exit: Arguments.ExitOnError);
            }
        }

        /// <summary>
        /// Allows SaveFile with a string name for the encoding
        /// </summary>
        /// <param name="filename">target file</param>
        /// <param name="encoding">encoding used for the target file</param>
        public void SaveFile(string filename, string encoding)
        {
            Encoding enc = GetEncoding(encoding);
            SaveFile(filename, enc);
        }

        /// <summary>
        /// Detect if these are characters that should not appear in a file or directory name
        /// </summary>
        /// <param name="text">text to check for illegal characters</param>
        /// <returns>True if there are illegal characters present</returns>
        private bool IllegalCharactersFound(string text)
        {
            // these are characters that should not appear in a file or directory name
            string[] IllegalFileCharacters = { "/", "*", "?", "<", ">", "|" };
            bool illegalFound = false;
            foreach (string illegal in IllegalFileCharacters)
            {
                if (text.Contains(illegal))
                {
                    illegalFound = true;
                }
            }
            return illegalFound;
        }

        
        /// <summary>
        /// Prepares the lines to save to the target file
        /// </summary>
        /// <returns>all the lines that will be saved to file</returns>
        private List<string> processLinesCSV()
        {
            List<string> linesOut = new List<string>();

            for (int r = 0; r < allRecords.Count; r++)
            {
                List<Field> lineArray = allRecords[r].Fields;
                string fullLine = "";
                for (int f = 0; f < fieldIndexes.Length; f++)
                {
                    Field? field = lineArray.SafeIndex((int)fieldIndexes[f]);
                    string element = string.Empty;
                    if (field != null) {
                        element = field.Text;
                    }
                    element = FixQuotes(element);

                    // don't put a comma delimiter on the end of the line
                    if (f < fieldIndexes.Length - 1)
                        fullLine += element + Arguments.DelimiterWrite;
                    else
                        fullLine += element;
                }
                linesOut.Add(fullLine);
            }
            return linesOut;
        }

        /// <summary>
        /// Changes single quotes inside a field into double quotes, then adds quotes at the start and end of the text. Used before saving the file.
        /// </summary>
        /// <param name="element">a text field</param>
        /// <returns>text with updated quotes</returns>
        private static string FixQuotes(string element)
        {
            // Any double quotes in the source file will already have been converted to single quotes by CsvHelper.
            // Before saving this out again, they must be converted back to double quotes.
            if (element.Contains("\"")) 
            {
                //Debug.WriteLine("Replacing quote in: " + element);
                element = element.Replace("\"", "\"\"");
            }
            element = "\"" + element + "\"";
            return element;
        }

        /// <summary>
        /// Counts the delimiters in a text by splitting the line with the delimiter. Does not account for text enclosed in quotes.
        /// </summary>
        /// <param name="delimiter">the delimiter used, like comma, semicolon, tab, or other</param>
        /// <param name="text">the text to count delimiters in</param>
        /// <returns></returns>
        private int countDelimiter(string delimiter, string text)
        {
            return text.Split(delimiter).Length;
        }

        /// <summary>
        /// Reads through the file, adding lines as records and delimiter separated values as fields
        /// </summary>
        /// <param name="filename">the source csv file</param>
        /// <param name="encoding">the encoding of the source file, usually UTF-8 or Latin1</param>
        private void GetAllFields(string filename, Encoding encoding)
        {
            int linesLoadedFromCSV = 0;
            Debug.WriteLine("Get all fields in " + filename + " with encoding " + encoding.EncodingName);
            
            StreamReader reader;
            try
            {
                reader = new StreamReader(filename, encoding);
            }
            catch
            {
                Error("File load error", quiet: Arguments.SupressErrors);
                return;
            }

            // detect if the file uses , ; or tab as delimiter
            if (Arguments.DelimiterRead == "auto")
            {
                Arguments.DelimiterRead = AutoDetectDelimiter();
            }
            
            // Set up CsvHelper
            var config = new CsvConfiguration(CultureInfo.CurrentCulture);
            config.Delimiter = Arguments.DelimiterRead;
            config.Encoding = encoding;
            if (Arguments.IgnoreBadData) 
                { config.BadDataFound = null; }
            if (Arguments.IgnoreMissingField)
                { config.MissingFieldFound = null; }

            // Get time to see the time it took to process the file.
            DateTime time = DateTime.Now;

            // Set up the CSV reader
            using (var csv = new CsvReader(reader, config))
            {
                allRecords = new List<Record> { };

                int maxFields = 0;
                if (Arguments.FieldCount == 0)
                {
                    maxFields = GuessNumberOfColumns();
                }
                else
                {
                    maxFields = Arguments.FieldCount;
                }
                fieldCount = maxFields;
                bool forceFieldCount = maxFields != 0;

                // Read the CSV file
                linesLoadedFromCSV = ReadCSV(csv, maxFields);
            }
            TimeSpan span = DateTime.Now - time;


            Message("Loaded " + linesLoadedFromCSV + " lines from: " + Path.GetFileName(filename) + " in " + span.TotalSeconds + " seconds", Arguments.Quiet);
                
        }

        /// <summary>
        /// Reads all lines in CSV, adds them to allRecords
        /// </summary>
        /// <param name="csv"></param>
        /// <param name="maxFields"></param>
        /// <returns>The number of lines loaded</returns>
        private int ReadCSV(CsvReader csv, int maxFields)
        {
            int lineNumber = 0;
            while (csv.Read())
            {
                Record record = new Record(lineNumber);
                for (int fieldNumber = 0; fieldNumber < maxFields; fieldNumber++)
                {
                    try
                    {
                        //Debug.WriteLine("get field start");
                        string fieldText = csv.GetField(fieldNumber) + "";
                        //Debug.WriteLine("get field success");

                        fieldText = RemoveSpaces(fieldText);
                        record.AddField(new Field(fieldText, record, fieldNumber));
                        Debug.WriteLine("Added field " + fieldNumber + ": " + fieldText);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("get field error on field:" + fieldNumber + ", line:" + lineNumber);
                        if (ex is BadDataException)
                        {
                            if (Arguments.FixBadData)
                            {
                                FixBadData(record, maxFields, lineNumber, fieldNumber);
                            }
                            else
                            {
                                Exit(ExitCode.SourceFileParseError, "Error: Bad Data on line " + lineNumber + ", field " + fieldNumber +
                                    Environment.NewLine + "Line: " + (linesAsArray != null ? linesAsArray[lineNumber] : "???") +
                                    Environment.NewLine + "Please check if correct delimiters are set (, or ;) and all \" quotes are closed out.",
                                    Arguments.SupressErrors, Arguments.Pause, exit: Arguments.ExitOnError);
                            }
                        }
                        else if (ex is CsvHelper.MissingFieldException)
                        {
                            if (Arguments.FixBadData)
                            {
                                FixBadData(record, maxFields, lineNumber, fieldNumber);
                            }
                            else
                            {
                                Exit(ExitCode.SourceFileParseError, "Error: Missing Field on line " + lineNumber + ", field " + fieldNumber + ". Expected number of fields: " + maxFields, Arguments.SupressErrors, Arguments.Pause, exit: Arguments.ExitOnError);
                            }
                        }
                        else if (ex is CsvHelper.ParserException) //CsvHelper.ParserException)
                        {
                            Exit(ExitCode.SourceFileParseError, "Error: Can't parse file. Check delimiter type. Remove /id to use auto detect.", Arguments.SupressErrors, Arguments.Pause, exit: Arguments.ExitOnError);
                        }
                        else
                        {
                            Exit(ExitCode.UnkownError, "Error: An exception occured, see details: " + Environment.NewLine + ex.Message, Arguments.SupressErrors, Arguments.Pause, exit: Arguments.ExitOnError);
                        }


                        fieldCount = maxFields;
                        break;
                    }
                }

                allRecords.Add(record);
                lineNumber++;
            }

            
            
            if (Arguments.ReplaceHeaders)
            {
                ChangeColumnNames();
            }
            UpdateColumnNames();

            return lineNumber;
        }

        private void ChangeColumnNames()
        {
            if (allRecords.Count == 0)
            {
                Warning("Could not change column names, there are no records", quiet: Arguments.SupressWarnings);
                return;
            }

            if (Arguments.FileHasHeaders == false)
            {
                allRecords.Insert(0, new Record(0));
            }

            if (Arguments.NewHeaders == string.Empty)
            {
                GenerateGenericColumnNames();
            }
            else
            {
                SetColumnNamesFromNewHeadersArgument();
            }

            allRecords[0].Fields.Clear();
            for (int i = 0; i < ColumnNames.Count; i++)
            {
                allRecords[0].Fields.Add(new Field(ColumnNames[i], allRecords[0], i));
            }
        }

        private void GenerateGenericColumnNames()
        {
            List<string> newHeaders = new();
            for (int i = 0; i < fieldCount; i++)
            {
                newHeaders.Add("Column " + i);
            }
            ColumnNames = newHeaders;
        }

        private void UpdateColumnNames()
        {
            SetColumnNamesFromFirstRecord();
            foreach (Record r in allRecords)
            {
                r.ColumnNames = ColumnNames;
            }
        }

        private void SetColumnNamesFromNewHeadersArgument()
        {
            Message("Changing column names to: " + Arguments.NewHeaders, Arguments.Quiet);
            List<string> newHeaders = new List<string>(Arguments.NewHeaders.Split(Arguments.DelimiterRead));
            if (newHeaders.Count > fieldCount)
            {
                Warning("Too many headers in New Headers argument, expected " + fieldCount + ". The extra ones will be dropped.", quiet: Arguments.SupressWarnings);
            }
            else if (newHeaders.Count < fieldCount)
            {
                Warning("Too few headers in New Headers argument, expected " + fieldCount + ". Extra headers will be insterted with generic name.", quiet: Arguments.SupressWarnings);
                for (int i = newHeaders.Count-1; i < fieldCount; i++)
                {
                    newHeaders.Add("Column " + (i+1));
                }
            }
            for (int i = 0; i < newHeaders.Count; i++)
            {
                newHeaders[i] = newHeaders[i].Replace("\"", "");
            }
            ColumnNames = newHeaders;
        }

        List<string> ColumnNames = new List<string>();

        /// <summary>
        /// Uses the first record (row 0) as column names
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void SetColumnNamesFromFirstRecord()
        {
            ColumnNames.Clear();
            if (allRecords.Count > 0)
            {
                foreach (Field f in allRecords[0].Fields)
                {
                    ColumnNames.Add(f.Text);
                }
            }
        }

        /// <summary>
        /// Corrects a record containing bad data by checking the raw text from the source file, removing extra quotes and splitting it up..
        /// </summary>
        /// <param name="record">the record to replace the contents of</param>
        /// <param name="maxFields">the number of columns in the source file</param>
        /// <param name="lineNumber">the current line number being read</param>
        /// <param name="fieldNumber">the current field number being read</param>
        private void FixBadData(Record record, int maxFields, int lineNumber, int fieldNumber)
        {
            record.Fields.Clear();
            string lineText = linesAsArray != null ? linesAsArray.SafeIndex(lineNumber) : "";
            string[] splitText = lineText.Split(Arguments.DelimiterRead);
            int delimiterCount = splitText.Length;
            Warning("Fixing fields on line " + lineNumber + ": " + lineText + ". Also Removing quotes (\")", quiet: Arguments.SupressWarnings);
            if (delimiterCount > maxFields)
            {
                Warning("Line " + lineNumber + " has too many delimiters. Data may be misaligned in columns.", quiet: Arguments.SupressWarnings);
            }
            else if (delimiterCount < maxFields)
            {
                Warning("Line " + lineNumber + " has too few delimiters. Data may be misaligned in columns.", quiet: Arguments.SupressWarnings);
            }

            // fixing and replacing the entire line instead of just this one field, otherwise, there's a high chance of running into a missing field error later.
            foreach (string field in splitText)
            {
                if (fieldNumber < splitText.Length)
                {
                    string fixedText = RemoveSpaces(field);
                    fixedText = fixedText.Replace("\"", "");
                    record.AddField(new Field(fixedText, record, fieldNumber));
                }
                else
                {
                    Warning("Skipped field" + fieldNumber + " Too few delimiters on line " + lineNumber + ": " + lineText, quiet: Arguments.SupressWarnings);
                }
            }

        }

        /// <summary>
        /// Removes both zero width non breaking space and trims the field for leading and trailing spaces
        /// </summary>
        /// <param name="fieldText">Text to trim</param>
        /// <returns>Trimmed text</returns>
        private static string RemoveSpaces(string fieldText)
        {
            if (fieldText.Contains((char)65279))
            {
                fieldText = fieldText.Replace(((char)65279).ToString(), string.Empty);
            }
            fieldText = fieldText.Trim();
            return fieldText;
        }

        /// <summary>
        /// Checks line 0 and 1 for columns, if line 0 has fewer than 2, check if line 1 has more.
        /// </summary>
        /// <returns>The number of columns in the file</returns>
        private int GuessNumberOfColumns()
        {
            // Checks line 0 and 1 for columns, if line 0 has fewer than 2, check if line 1 has more.
            int columnCount0 = 0;
            int columnCount1 = 0;
            if (linesAsArray != null)
            {
                if (linesAsArray.Length > 0)
                {
                    columnCount0 = linesAsArray[0].Split(Arguments.DelimiterRead).Length;
                }
                if (linesAsArray.Length > 1)
                {
                    columnCount1 = linesAsArray[1].Split(Arguments.DelimiterRead).Length;
                }
                if (columnCount0 > 1)
                {
                    columnCount = columnCount0;
                }
                else
                {
                    if (columnCount0 >= columnCount1)
                    {
                        Warning("Autodetect field count: Found only 1 field on line 0 and 1, using count: " + columnCount0, quiet: Arguments.SupressWarnings);
                        columnCount = columnCount0;
                    }
                    else
                    {
                        Warning("Autodetect field count: Found only 1 field on line 0, using count from line 1: " + columnCount1, quiet: Arguments.SupressWarnings);
                        columnCount = columnCount1;
                    }
                }
            }
            Message("Guessed number of fields: " + columnCount + " using delimiter: " + Arguments.DelimiterRead, Arguments.Quiet);
            return columnCount;
        }

        /// <summary>
        /// Guesses the delimiter used in the file by what occurs more frequently on line 0 and 1. Checks for comma, semicolon and tab.
        /// </summary>
        /// <returns>The delimiter string used, or comma if it can't be determined</returns>
        private string AutoDetectDelimiter()
        {
            if (linesAsArray != null)
            {
                int comma, semicolon, tab;

                // Checks line 0 for delimiters, and if that fails, checks line 1
                CountAllDelimiterTypes(linesAsArray[0], out comma, out semicolon, out tab);

                if (comma < 1 && semicolon < 1 && tab < 1)
                {
                    Warning("Autodetect delimiter: Line 0 had no delimiters, trying line 1", quiet: Arguments.SupressWarnings);
                    if (linesAsArray.Length > 1)
                    {
                        CountAllDelimiterTypes(linesAsArray[1], out comma, out semicolon, out tab);
                    }
                }

                // determines the winning delimiter by majority count
                if (comma > semicolon)
                {
                    Message("Autodetect: Setting delimiter to comma", Arguments.Quiet);
                    return ",";
                }
                else if (semicolon > tab)
                {
                    Message("Autodetect: Setting delimiter to semicolon", Arguments.Quiet);
                    return ";";
                }
                else if (tab > 0)
                {
                    Message("Autodetect: Setting delimiter to tab", Arguments.Quiet);
                    return "\t";
                }
                else
                {
                    Warning("Autodetect: Couldn't count delimiters on line 0 or 1, defaulting to comma", quiet: Arguments.SupressWarnings);
                    return ",";
                }
            }

            Warning("Autodetect: Couldn't count delimiters, defaulting to comma", quiet: Arguments.SupressWarnings);
            return ",";
        }

        /// <summary>
        /// Check the number of delimiters used in a text.
        /// </summary>
        /// <param name="text">The text to check for delimiters</param>
        /// <param name="comma">the number of commas in the text</param>
        /// <param name="semicolon">the number of semicolons in the text</param>
        /// <param name="tab">the number of tabs in the text</param>
        private void CountAllDelimiterTypes(string text, out int comma, out int semicolon, out int tab)
        {
            comma = (countDelimiter(",", text)) -1 ;
            semicolon = (countDelimiter(";", text)) - 1;
            tab = (countDelimiter("\t", text)) - 1;
            Message("Delimiters detected: comma: " + comma + " semicolon:" + semicolon + " tab: " + tab, Arguments.Quiet);
        }

        /// <summary>
        /// Converts the argmunent field number pattern from string to an array of numbers.
        /// </summary>
        /// <param name="text"></param>
        public void SetPattern(string text)
        {
            Debug.WriteLine("Pattern: " + text);
            if (text.Length == 0)
            {
                Debug.WriteLine("Setting default pattern");
                CreateDefaultPattern();
                return;
            }

            string[] patternText = text.Split(',');
            List<int> pattern = new List<int>();
            foreach (string t in patternText)
            {
                try
                {
                    pattern.Add(int.Parse(t));
                }
                catch
                {
                    Exit(ExitCode.InvalidFields, "Error in pattern: " + text, Arguments.SupressErrors, Arguments.Pause, exit: Arguments.ExitOnError);
                }
            }
            fieldIndexes = pattern.ToArray();
        }

        /// <summary>
        /// Fills fieldIndexes with ints from 0 to n, where n is the number of fields in the file
        /// </summary>
        private void CreateDefaultPattern()
        {
            List<int> pattern = new List<int>();
            for (int i = 0; i < fieldCount; i++)
            {
                pattern.Add(i);
            }
            Debug.WriteLine("Default pattern created, with Count: " + pattern.Count);
            fieldIndexes = pattern.ToArray();
        }
    }
}
