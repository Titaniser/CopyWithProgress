"# CopyWithProgress" 

# usage
open the Windows-CMD
copyWithProgress <source> <dest>

# explaination
This is a program that copies files from one location (source) to another location (destination) on a computer.
It will show a progressbar as Text for each File

First, the program checks if the destination directory exists and creates it if it does not. Then, it checks if the source and destination paths are given as parameters. If they are not, the program asks the user to provide them.

If the source location is a file, the program copies the file to the destination location. If the source location is a directory, it copies all the files and subdirectories in that directory to the destination location.

The program also has a feature that shows the progress of the file transfer, including the file name, the size of the file, the speed at which it is being copied, and the overall progress.

If a file already exists in the destination location, the program prompts the user to decide whether to overwrite it, overwrite all files without prompting in the future, skip the file, or skip all files without prompting in the future. The user's answer decides what the program does.

The program contains two helper classes: QuestionStorage and FileUtilities.

QuestionStorage keeps track of whether the user decides to overwrite all files or skip all files during the file transfer process.

FileUtilities is a class with a helper method that formats the file size in bytes, kilobytes, megabytes, gigabytes, or terabytes.