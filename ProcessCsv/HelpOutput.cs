using ProcessCsvLibrary;

namespace ProcessCSV
{
    public static class HelpOutput
    {
        private static string NL = Environment.NewLine;
        public static void helpText(bool pause, bool exit)
        {
            Console.WriteLine(NL + "Processes a CSV file to remove unwanted columns, change delimiter, encoding or inspect contents" + NL);

            Console.WriteLine("PROCESSCSV /l file [/s file] [/cs columns] [/ie encoding] [/oe encoding]" + NL +
                "    [/id delimiter] [/od delimiter] [/ex [number]] [/hd] [/q] [/p]" + NL);

            Console.WriteLine("PROCESSCSV sourcefile targetfile [other arguments]");
            Console.WriteLine("PROCESSCSV [No arguments] : Starts the program in menu based mode" + NL);

            Console.WriteLine(
                "ARGUMENT        ALIAS      FUNCTION" + NL +
                "" + NL +
                "/help           /?         Displays this help page" + NL +
                "" + NL +
                "/load           /l         File name to load from (Source)" + NL +
                "/save           /s         File name to save to (Target)" + NL +
                "                           Example: /l a.csv /s b.csv" + NL +
                "" + NL +
                "/inencoding     /ie        Encoding or codepage of the source file" + NL +
                "                           Skip to autodetect (Program counts , ; and TAB)" + NL +
                "/outencoding    /oe        Encoding or codepage of the target file" + NL +
                "                           Example encodings: UTF-8, UTF-8-BOM, LATIN1, ASCII, 865" + NL +
                "                           Example: /ie Latin1 /oe UTF8" + NL +
                "" + NL +
                "/columnselect   /cs        Select columns in the CSV to keep in the target file." + NL +
                "                           If unspecified, all columns will be output." + NL +
                "                           Columns set to -1 will be empty." + NL +
                "                           Example: /cs 0,-1,-1,21,8" + NL +
                NL +
                "/columncount    /cc        Manually specify the number of columns (fields) in the file." + NL +
                "                           Missing columns (fields) on a line will be added as empty values." + NL +
                "                           If not used, column count is guessed by the number of delimiters found on line 0 and 1" + NL +
                "                           Example: /cc 4" + NL +
                NL +
                "/example        /ex        Lists a number of lines from the start of the file. (Default is 5 lines)" + NL +
                "                           Example: /ex 10" + NL +
                "" + NL +
                "/indelimiter    /id        The delimiter type used in the source file." + NL +
                "/outdelimiter   /od        The delimiter type used for the target file." + NL +
                "                           Tab, comma and semicolon can be referenced by name." + NL +
                "                           Example: /id ; /od ," + NL +
                "                           Example: /id tab /od comma" + NL +
                "" + NL +
                "/showheaders    /hd        Lists all headers (column names) on the first line of the source file." + NL +
                "                           Use this to find the column numbers to use for the columnselect argument." + NL +
                NL +
                "/quiet          /q         No text output (except for Help, headers and example lines). Use exit codes to verify result." + NL +
                NL +
                "/pause          /p         Wait for keypress before exiting" + NL +
                NL +
                "/ignorebaddata  /ibd       Proceed despite bad data, such as quotes that are not closed out or missing fields" + NL +
                "/ignoremissing  /imf       Proceed despite missing fields. Missing fields on a line will be added as empty values" + NL +
                NL +
                "/fixbaddata     /fbd       Fix lines with bad data. Guesses based on number of delimiter characters present." + NL +
                "                           If delimiter count is wrong, columns may be misaligned. Also removes quotes from the fields." + NL +
                NL +
                "/newheaders     /nwh       Replace or add headers (column names) with a custom set of names." + NL +
                "                           Enclose text in quotes if names contain spaces. " + NL +
                "                           If no names are listed, generic names are used." + NL +
                "                           Example: /nwh Name,Email,Phone" + NL +
                "                           Example: /nwh \"Name,Email Address,Phone Number\"" + NL +
                "                           Example: /nwh    >  This will generate Column 1, Column 2, Column 3 etc." + NL +
                NL +
                "/noheaders      /noh       Used with /newheaders if the first line is not a list of column names, but data." + NL +
                "                           Inserts a new row at the start of the file with new header names from /newheader" + NL
                );

            // nwh
            // noh


            Console.WriteLine("EXIT CODES:" + NL);
            for (int i = 0; i < (int)ExitCode.END; i++)
            {
                Console.WriteLine(i + " : " + ((ExitCode)i).ToString());
            }

            Messages.ExitProgram(exitCode: ExitCode.InfoShown, message: null, quiet: false, pause, exit);
        }
    }
}
