using Toyota.Test;

// TMMC programming assignment
//
// Counts the number of vertical black lines in a black-and-white JPEG image
// created with MS Paint.
//
// Usage:
//     Toyota.Test.exe "C:\path\to\img.jpg"
//
// The program is required to:
//   * accept exactly one argument (the absolute path of the image);
//   * never crash – any error is reported to the console;
//   * print a single number: the count of vertical lines.

// --- 1. Argument validation -------------------------------------------------
// The app must take exactly one argument. Anything else is a usage error.
if (args.Length != 1)
{
    Console.WriteLine("Invalid number of arguments. Usage: Toyota.Test <absolute-path-to-jpg>");
    return;
}

// --- 2. Process the image ---------------------------------------------------
// All real work is wrapped in a try/catch so that no exception can ever crash
// the application; instead the message is written to the console.
try
{
    int lineCount = LineCounter.CountVerticalLines(args[0]);
    Console.WriteLine(lineCount);
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
