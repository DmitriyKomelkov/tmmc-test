TMMC Programming Assignment - Vertical Black Line Counter
=========================================================

WHAT IT DOES
------------
A Windows console application that counts the number of vertical black lines
in a black-and-white JPEG image created with MS Paint.

It prints a single integer: the number of vertical lines found in the image.


REQUIREMENTS
------------
- Windows
- .NET 10 SDK (to build) / .NET 10 Runtime (to run)


HOW TO BUILD
------------
From the solution folder run:

    dotnet build Toyota.Test/Toyota.Test.csproj -c Release

The executable is produced at:

    Toyota.Test\bin\Release\net10.0-windows\Toyota.Test.exe


HOW TO USE
----------
The app takes EXACTLY ONE argument: the absolute path of a JPEG image.

    Toyota.Test.exe "C:\TMMC_interview_assignment\img_1.jpg"

Output: a single number, e.g.

    1

You can also run it through the SDK:

    dotnet run --project Toyota.Test -- "C:\TMMC_interview_assignment\img_1.jpg"


HOW TO RUN THE TESTS
--------------------
A small xUnit project (Toyota.Test.Tests) accompanies the app. It checks the
four provided images against their known answers and covers the edge cases
found during development (empty image, 1px-wide line, a line that only crosses
the middle, odd/even heights, missing file, non-image file).

    dotnet test


BEHAVIOR / ERROR HANDLING
-------------------------
- If the number of arguments is not exactly one, a usage message is printed.
- The application never crashes. Any error (e.g. missing file, unreadable
  image) is caught and reported to the console as "Error: <message>".


HOW IT WORKS (algorithm)
------------------------
The assignment guarantees that each line exists in BOTH the top half and the
bottom half of the image as one continuous straight line. A continuous shape
that is present in both halves must cross the boundary between them - the
middle row. So intersecting every line is as simple as scanning that single
row, and it assumes nothing about how far up or down a line reaches.

1. The image is loaded into memory.
2. The middle row (Height / 2) is scanned from left to right.
3. Each contiguous run of black pixels is one vertical line, so the answer is
   the number of white-to-black transitions on that row.

A brightness threshold (perceived luma < 128) is used instead of testing for
pure black, because JPEG compression turns sharp edges into shades of grey;
a single off-black pixel would otherwise split or drop a line.


ASSUMPTIONS
-----------
The task asks for the number of VERTICAL lines, but the stated image
properties do not, strictly speaking, force a line to be vertical: a slanted
straight line would also satisfy "a continuous straight line present in both
the top and bottom half". Taken literally, that would mean detecting each
line's orientation and counting only the vertical ones.

In this solution we instead assume every line is strictly vertical. This is
supported indirectly by the task wording ("vertical lines"), by the provided
sample images (all clean vertical bars), and by the absence of any criterion
for how much tilt would still count as "vertical" - so handling slanted lines
appears to be out of scope. The middle-row algorithm would in any case count a
mildly slanted line correctly as one line; only the strictly-vertical
assumption is documented here for completeness.
