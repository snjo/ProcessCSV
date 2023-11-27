// See https://aka.ms/new-console-template for more information

namespace ProcessCsvLibrary
{
    public class CsvArguments
    {
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
    }
}



