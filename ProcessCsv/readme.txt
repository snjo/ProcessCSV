ProcessCSV by Andreas Aakvik Gogstad, 2023
https://github.com/snjo/processcsv
Uses CsvHelper by Josh Close
https://joshclose.github.io/CsvHelper/

Processes a CSV file to remove unwanted fields, change delimiter, encoding or inspect contents

PROCESSCSV /l file [/s file] [/f fields] [/ie encoding] [/oe encoding]
    [/id delimiter] [/od delimiter] [/ex [number]] [/hd] [/q] [/p]

PROCESSCSV sourcefile targetfile [other arguments]

ARGUMENT        ALIAS      FUNCTION

/help           /?         Displays this help page

/load           /l         File name to load from (Source)
/save           /s         File name to save to (Target)
                           Example: /l a.csv /s b.csv

/inencoding     /ie        Encoding or codepage of the source file
                           Skip to autodetect (Program counts , ; and TAB)
/outencoding    /oe        Encoding or codepage of the target file
                           Example encodings: UTF-8, UTF-8-BOM, LATIN1, ASCII, 865
                           Example: /ie Latin1 /oe UTF8

/fieldselect    /fs        Select fields (columns) in the CSV to keep in the target file.
                           If unspecified, all fields will be output.
                           Field set to -1 will be empty.
                           Example: /f 0,-1,-1,21,8

/fieldcount     /fc        Manually specify the number of field (columns) in the file.
                           Missing fields on a line will be added as empty values.
                           If not used, field count is guessed by the number of delimiters found on line 0 and 1
                           Example: /fc 4

/example        /ex        Lists a number of lines from the start of the file. (Default is 5 lines)
                           Example: /ex 10

/indelimiter    /id        The delimiter type used in the source file.
/outdelimiter   /od        The delimiter type used for the target file.
                           Tab, comma and semicolon can be referenced by name.
                           Example: /id ; /od ,
                           Example: /id tab /od comma

/headers        /hd        Lists all fields on the first line of the source file. These are often the headers.
                           Use this to find the field numbers to use for the fields argument.

/quiet          /q         No text output (except for Help, headers and example lines). Use exit codes to verify result.

/pause          /p         Wait for keypress before exiting

/ignorebaddata  /ibd       Proceed despite bad data, such as quotes that are not closed out or missing fields
/ignoremissing  /imf       Proceed despite missing fields. Missing fields on a line will be added as empty values

/fixbaddata     /fbd       Fix lines with bad data. Guesses based on number of delimiter characters present.
                           If delimiter count is wrong, columns may be misaligned. Also removes quotes from the fields.

/newheaders     /nwh       Replace or add headers (column names) with a custom set of names.
                           Enclose text in quotes if names contain spaces. If no names are listed, generic names are used.
                           Example: /nwh Name,Email,Phone
                           Example: /nwh "Name,Email Address,Phone Number"
                           Example: /nwh    >  This will generate Column 1, Column 2, Column 3 etc.

/noheaders      /noh       Used with /newheaders if the first line is not a list of column names, but data.
                           Inserts a new row at the start of the file with new header names from /newheader

EXIT CODES:

0 : Success
1 : InfoShown
2 : SourceFileNotFound
3 : SourceFileParseError
4 : TargetFileError
5 : InvalidEncoding
6 : InvalidFields
7 : InvalidArgument
8 : TargetUnauthorized
9 : TargetDirectoryNotFound
10 : UnkownError