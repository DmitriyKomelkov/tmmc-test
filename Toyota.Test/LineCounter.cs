using System.Drawing;

namespace Toyota.Test;

/// <summary>
/// Detects and counts vertical black lines in a black-and-white image.
/// </summary>
public static class LineCounter
{
    // A pixel is treated as "black" when its perceived brightness is below this
    // threshold. JPEG compression blurs sharp edges into shades of grey, so we
    // use a mid-grey cut-off rather than testing for pure black (0,0,0). The
    // images are bi-modal (near-black vs near-white) with nothing in between,
    // so any value in the wide empty middle works; 128 is the safe midpoint.
    private const int BrightnessThreshold = 128;

    /// <summary>
    /// Loads the image at <paramref name="path"/> and returns the number of
    /// vertical black lines it contains.
    /// </summary>
    public static int CountVerticalLines(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Image not found: {path}");

        // Bitmap is disposable (it holds an unmanaged GDI+ handle).
        using var bitmap = LoadImage(path);

        // The assignment guarantees each line exists in BOTH the top and bottom
        // half as one continuous straight line. A continuous shape present in
        // both halves must cross the boundary between them, i.e. the middle
        // row. So scanning that single row is enough to intersect every line -
        // and it makes no assumption about how far up or down a line reaches.
        int middleRow = bitmap.Height / 2;

        int lineCount = 0;
        bool insideLine = false; // are we currently scanning across a black line?

        // Scan left to right. Each contiguous run of black pixels is one line,
        // so we count the rising edges (a white-to-black transition).
        for (int x = 0; x < bitmap.Width; x++)
        {
            bool isBlack = IsBlack(bitmap.GetPixel(x, middleRow));

            if (isBlack && !insideLine)
                lineCount++; // rising edge: a new line begins

            insideLine = isBlack;
        }

        return lineCount;
    }

    /// <summary>
    /// Loads the image as a <see cref="Bitmap"/>, translating GDI+'s cryptic
    /// failures into a clear message. When the data cannot be decoded as an
    /// image, GDI+ throws "Parameter is not valid." (ArgumentException) - and
    /// for some corrupt files an OutOfMemoryException - which says nothing
    /// useful to the user.
    /// </summary>
    private static Bitmap LoadImage(string path)
    {
        try
        {
            return new Bitmap(path);
        }
        catch (Exception ex) when (ex is ArgumentException or OutOfMemoryException)
        {
            throw new InvalidOperationException($"File is not a valid image: {path}", ex);
        }
    }

    /// <summary>
    /// Returns true when the colour is dark enough to be considered black,
    /// using the standard Rec. 601 luma weighting for perceived brightness.
    /// </summary>
    private static bool IsBlack(Color c)
    {
        double brightness = 0.299 * c.R + 0.587 * c.G + 0.114 * c.B;
        return brightness < BrightnessThreshold;
    }
}
