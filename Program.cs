using System;              // Importing the System namespace
using System.Diagnostics;  // Importing the System.Diagnostics namespace
using System.IO;           // Importing the System.IO namespace
using System.Linq;         // Importing the System.Linq namespace

// A class named "Program" is defined
class Program
{
    // A method named "Main" is defined, which is the entry point of the program
    static void Main(string[] args)
    {
        // Storing the first parameter from command line into the "source" variable
        string source = args.ElementAtOrDefault(0);
        // Storing the second parameter from command line into the "destination" variable
        string destination = args.ElementAtOrDefault(1);

        // Creating the destination directory if it does not exist
        if (!Directory.Exists(destination))
        {
            Directory.CreateDirectory(destination);
        }

        // Checking if source and destination paths are provided, else printing an error message and exit
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(destination))
        {
            Console.WriteLine("Please provide source and destination paths as parameters.");
            return;
        }

        // If the source is a file, copy it to destination with progress
        else if (File.Exists(source))
        {
            CopyWithProgress(source, Path.Combine(destination, Path.GetFileName(source)));
        }

        // If the source is a directory, copy all its files and directories to destination with progress
        else if (Directory.Exists(source))
        {
            CopyAllFiles(source, destination);
        }

        // If the source does not exist, print an error message and exit
        else
        {
            Console.WriteLine($"File or directory does not exist: {source}");
        }
    }

    // A method named "CopyWithProgress" is defined, which copies the source file to destination file with progress
    static void CopyWithProgress(string src, string dst)
    {
        // Creating the file info object for the source file
        FileInfo fileInfo = new FileInfo(src);
        // Storing the name and the size of the file into variables
        string fileName = fileInfo.Name;
        long fileSize = fileInfo.Length;

        // A boolean variable named "overwrite" is initialized to false
        bool overwrite = false;

        // Checking if the user has not chosen to overwrite existing files
        if (!QuestionStorage.overwriteNothing)
        {
            // Checking if the destination file already exists
            if (File.Exists(dst))
            {
                // Prompting the user to overwrite the file
                if (!QuestionStorage.overwriteAll)
                {
                    Console.Write($"File already exists: {dst}. overwrite it? [Y/N/[A]ll/N[O]thing] ");

                    ConsoleKeyInfo key;

                    // Reading user input and waiting for either one of the four keys (Y, A, O, N)
                    do
                    {
                        key = Console.ReadKey();
                    } while (key.Key != ConsoleKey.Y
                             && key.Key != ConsoleKey.A
                             && key.Key != ConsoleKey.O
                             && key.Key != ConsoleKey.N
                    );

                    // If user presses Y, then overwrite the destination file
                    if (key.Key == ConsoleKey.Y)
                    {
                        overwrite = true;
                        Console.Write($"\r({FileUtilities.FormatBytes(fileSize)}), {FileUtilities.FormatBytes((long)0),-9}/s [{new string('#', 0)}{new string('-', 20 - 0)}] {0,6:0.00}% {fileName}");
                    }
                    // If user presses A, overwrite all
                    else if (key.Key == ConsoleKey.A)
                    {
                        QuestionStorage.overwriteAll = true;
                        QuestionStorage.overwriteNothing = false;
                        Console.WriteLine();
                    }
                    // If user presses O, overwrite nothing
                    else if (key.Key == ConsoleKey.O)
                    {
                        QuestionStorage.overwriteAll = false;
                        QuestionStorage.overwriteNothing = true;
                        Console.WriteLine();
                    }
                    // If user presses N, do nothing
                    else if (key.Key == ConsoleKey.N)
                    {
                        Console.WriteLine();
                    }
                }
            }

            // If the destination file does not exist or user chooses to overwrite it, create the source and destination streams and copy the file
            if (!File.Exists(dst) || overwrite || QuestionStorage.overwriteAll)
            {
                FileStream sourceStream = new FileStream(src, FileMode.Open, FileAccess.Read);
                FileStream destinationStream = new FileStream(dst, FileMode.Create, FileAccess.Write);

                byte[] buffer = new byte[64 * 1024];  // 64KB buffer size
                long bytesTotal = fileSize;
                long bytesWritten = 0;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                // Copying source file to destination file chunk by chunk
                while (true)
                {
                    int bytesRead = sourceStream.Read(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        stopwatch.Stop();
                        break;
                    }

                    destinationStream.Write(buffer, 0, bytesRead);
                    bytesWritten += bytesRead;

                    // Formatting and printing the progress
                    double percentageComplete = (double)bytesWritten / bytesTotal * 100;
                    int progress = (int)Math.Round(percentageComplete / 5);
                    double speed = bytesWritten / stopwatch.Elapsed.TotalSeconds;

                    Console.Write($"\r({FileUtilities.FormatBytes(fileSize)}), {FileUtilities.FormatBytes((long)speed),-9}/s [{new string('#', progress)}{new string('-', 20 - progress)}] {percentageComplete,6:0.00}% {fileName}");
                }
                // Printing a new line after copying is done
                Console.WriteLine();

                // Closing the streams
                sourceStream.Close();
                destinationStream.Close();
            }
        }
    }

    // A method named "CopyAllFiles" is defined, which copies all files and directories inside the specified directory
    static void CopyAllFiles(string srcDir, string dstDir)
    {
        // Getting all files inside the source directory
        string[] files = Directory.GetFiles(srcDir);

        int totalFiles = files.Length;
        int filesCopied = 0;

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        // Copying all files inside source directory to destination directory
        foreach (string file in files)
        {
            FileInfo fileInfo = new FileInfo(file);
            string fileName = fileInfo.Name;
            long fileSize = fileInfo.Length;
            string dstPath = Path.Combine(dstDir, fileName);

            // If file then copy with progress, if directory then copy recursively
            if (File.Exists(file))
            {
                CopyWithProgress(file, dstPath);
            }
            else if (Directory.Exists(file))
            {
                Console.WriteLine($"\rCopying directory: {fileName}");
                Directory.CreateDirectory(dstPath);
                CopyAllFiles(file, dstPath);
            }

            filesCopied++;

            // Formatting and printing the progress
            double percentageComplete = (double)filesCopied / totalFiles * 100;
            int progress = (int)Math.Round(percentageComplete / 5);
            double speed = filesCopied / stopwatch.Elapsed.TotalSeconds;
        }

        stopwatch.Stop();

        // Recursively copying all directories inside source directory to destination directory
        string[] directories = Directory.GetDirectories(srcDir);
        foreach (string directory in directories)
        {
            string dirName = Path.GetDirectoryName(directory);
            string dstPath = Path.Combine(dstDir, dirName);
            Directory.CreateDirectory(dstPath);
            CopyAllFiles(directory, dstPath);
        }
    }
}

// A public static class named "QuestionStorage" is defined which stores the overwrite settings
public static class QuestionStorage
{
    // A public static boolean variable named "overwriteAll" is initialized to false
    public static bool overwriteAll = false;
    // A public static boolean variable named "overwriteNothing" is initialized to false
    public static bool overwriteNothing = false;
}

// A public static class named "FileUtilities" is defined which has a method for formatting bytes
public static class FileUtilities
{
    // A public static method named "FormatBytes" is defined which formats the given bytes into human readable format
    public static string FormatBytes(long bytes)
    {
        string[] suffix = { "B", "KB", "MB", "GB", "TB" };
        int index = 0;
        double dblBytes = bytes;

        while (dblBytes >= 1024 && index < suffix.Length - 1)
        {
            dblBytes /= 1024;
            index++;
        }

        return $"{dblBytes,6:0.##} {suffix[index]}";
    }
}