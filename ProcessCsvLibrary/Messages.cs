using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessCsvLibrary;

public class Messages
{ 
    public static void Message(string message, bool quiet = false)
    {
        if (quiet == false)
        {
            Console.WriteLine(message);
        }
        Debug.WriteLine("Message: " + message);
    }

    public static void Warning(string message, bool quiet = false)
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

    public static void Error(string message, bool quiet = false)
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

    public static void ExitProgram(ExitCode exitCode, string? message, bool quiet, bool pause)
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
        Environment.Exit((int)exitCode);
    }
}