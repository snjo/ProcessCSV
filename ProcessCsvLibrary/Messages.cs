using System.Diagnostics;

namespace ProcessCsvLibrary;

public class Messages
{
    public static void Message(string message, bool quiet)
    {
        if (quiet == false)
        {
            Console.WriteLine(message);
        }
        Debug.WriteLine("Message: " + message);
    }

    public static void Warning(string message, bool quiet)
    {
        if (quiet == false)
        {
            ConsoleColor previousColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ForegroundColor = previousColor;
        }
        Debug.WriteLine("Warning: " + message);
    }

    public static void Error(string message, bool quiet)
    {
        if (quiet == false)
        {
            ConsoleColor previousColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = previousColor;
        }
        Debug.WriteLine("Error: " + message);
    }

    public static void ExitProgram(ExitCode exitCode, string? message, bool quiet, bool pause, bool exit)
    {
        ConsoleColor previousColor = Console.ForegroundColor;
        if ((int)exitCode > 1)
        {
            Console.ForegroundColor = ConsoleColor.Red;
        }
        Debug.WriteLine("ExitProgram, message: " + message);
        Debug.WriteLine("Exiting with code " + (int)exitCode + ": " + exitCode);
        if (quiet == false)
        {
            Console.WriteLine();
            if (message != null)
                Console.WriteLine(message);
            Console.WriteLine("Exiting with code " + (int)exitCode + ": " + exitCode);
        }
        if (pause)
        {
            Message("Press any key to exit", quiet);
            Console.ReadKey();
        }
        else
        {
            Debug.WriteLine("Exiting without pause");
        }
        Console.ForegroundColor = previousColor;
        if (exit)
        {
            Environment.Exit((int)exitCode);
        }
    }

    public delegate void MessageDelegate(string message, bool quiet);

    /// <summary>
    /// Shows a list of headers/column names based on the first line of the file.
    /// </summary>
    /// <param name="processor">ProcessTextFile</param>
    /// <param name="arguments">ProcessTextFile.Arguments</param>
    public static void displayHeaders(ProcessTextFile processor, CsvArguments arguments, MessageDelegate messenger)
    {
        messenger("\nDisplaying Headers from line 0 of the source file:\n", arguments.Quiet);
        int fieldNumber = 0;
        Field[]? fieldArray = processor.GetRecordAsArray(0);
        if (fieldArray != null)
        {
            for (int i = 0; i < fieldArray.Length; i++)
            {
                Field fieldName = fieldArray[i];
                messenger(fieldNumber.ToString().PadRight(3) + ": " + fieldName.Text, quiet: false);
                fieldNumber++;
            }
        }
        else
        {
            messenger("/headers: Could not get fields to display", quiet: false);
        }
        messenger("", quiet: false); // blank line
    }

    /// <summary>
    /// Gets the contents of a single record as a single string.
    /// </summary>
    /// <param name="selectedLine"></param>
    /// <returns>a single record, with quotes around each field.</returns>
    public static string GetResultRecordConcatenated(List<Record> records, int[] selectedFields, int selectedLine, Messages.MessageDelegate messenger, string delimiterWrite, bool supressWarnings = false)
    {
        string result = string.Empty;
        //Debug.WriteLine("Concat: " + fieldIndexes.Length); // TEST
        for (int i = 0; i < selectedFields.Length; i++)
        {
            List<Field>? record = records.SafeIndex((int)selectedLine, null);

            if (record == null)
            {
                Debug.WriteLine("getrecord error, null: " + (record == null) + ", selectedLine: " + selectedLine + ", field " + i);
            }
            else if (record.Count == 0)
            {
                Debug.WriteLine("record count is 0");
            }

            if (record != null && record.Count > 0)
            {
                //Debug.WriteLine("Concat 2: " + record.SafeIndex(fieldIndexes[i], "*")); // TEST
                Field? field = record.SafeIndex(selectedFields[i]);
                if (field != null)
                {
                    result += ProcessTextFile.FixQuotes(field.Text);
                }
                else
                {
                    if (selectedFields[i] != -1) // ignore index errors when using -1, that's used for leaving a blank field on purpose. If someone used another out of index value, warn them.
                        messenger("Error reading field " + i + " on line " + selectedLine + ". Field select " + selectedFields[i] + " is out of range.", quiet: supressWarnings);
                }
                //result += record.SafeIndex(fieldIndexes[i], "*");
                if (i < selectedFields.Length - 1)
                    result += delimiterWrite;
            }
            else
            {
                Debug.WriteLine("getrecord error, null: " + record == null + ", selectedLine: " + selectedLine + ", field " + i);
                //result += Environment.NewLine;
            }
        }
        return result;
    }

    /// <summary>
    /// Gets the text used in Display example record /ex
    /// </summary>
    /// <param name="limitNumberOfLines">If true, shows either 5 lines, or the number of lines set in maxLines</param>
    /// <param name="maxLines">The number of records to show as an example, starting from line 0. Only used if LimitNumberOfLines is true</param>
    /// <returns>The full text used in example output</returns>
    public static string GetResultRecordsAsText(List<Record> records, int[] selectedFields, bool limitNumberOfLines, int maxLines, Messages.MessageDelegate messenger, string delimiterWrite)
    {
        if (records.Count == 0)
        {
            return "There are no lines to display";
        }
        string result = string.Empty;
        if (limitNumberOfLines == false)
            maxLines = records.Count;
        for (int i = 0; i < Math.Min(maxLines, records.Count); i++)
        {
            result += i.ToString().PadRight(3) + ":  ";
            result += GetResultRecordConcatenated(records, selectedFields, i, messenger, delimiterWrite);
            if (i < maxLines - 1)
                result += Environment.NewLine;
        }
        return result;
    }
}