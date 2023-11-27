// See https://aka.ms/new-console-template for more information

namespace ProcessCsvLibrary;

public enum ExitCode : int
{
    Success = 0,
    InfoShown = 1,
    SourceFileNotFound = 2,
    SourceFileParseError = 3,
    TargetFileError = 4,
    InvalidEncoding = 5,
    InvalidFields = 6,
    InvalidArgument = 7,
    TargetUnauthorized = 8,
    TargetDirectoryNotFound = 9,
    UnkownError = 10,
    END
}



