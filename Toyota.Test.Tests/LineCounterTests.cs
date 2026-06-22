using System.Drawing;
using System.Drawing.Imaging;
using Toyota.Test;
using Xunit;

namespace Toyota.Test.Tests;

/// <summary>
/// Tests for <see cref="LineCounter"/>. They fall into two groups:
///   * the four reference images provided with the assignment (known answers);
///   * synthetic images generated on the fly to pin down edge cases we hit
///     while developing (empty image, 1px-wide line, a line that only crosses
///     the middle, bad input, ...).
/// </summary>
public class LineCounterTests
{
    // --- The provided reference images: the assignment's own oracle ---------

    [Theory]
    [InlineData("img_1.jpg", 1)]
    [InlineData("img_2.jpg", 3)]
    [InlineData("img_3.jpg", 1)] // a fully black image is one wide line
    [InlineData("img_4.jpg", 7)]
    public void Counts_reference_images(string fileName, int expected)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "TestImages", fileName);

        int actual = LineCounter.CountVerticalLines(path);

        Assert.Equal(expected, actual);
    }

    // --- Synthetic edge cases ----------------------------------------------

    [Fact]
    public void Empty_white_image_has_no_lines()
    {
        using var img = new TempImage(200, 200); // no black at all

        Assert.Equal(0, LineCounter.CountVerticalLines(img.Path));
    }

    [Fact]
    public void Single_full_height_line_counts_as_one()
    {
        using var img = new TempImage(200, 200, new Rectangle(100, 0, 6, 200));

        Assert.Equal(1, LineCounter.CountVerticalLines(img.Path));
    }

    [Fact]
    public void One_pixel_wide_line_is_still_detected()
    {
        // The thinnest possible line - JPEG must not blur it away.
        using var img = new TempImage(200, 200, new Rectangle(100, 0, 1, 200));

        Assert.Equal(1, LineCounter.CountVerticalLines(img.Path));
    }

    [Fact]
    public void Line_only_crossing_the_middle_is_detected()
    {
        // Spec guarantees a line crosses the middle row but not that it reaches
        // the top/bottom. A short central line must still be counted.
        using var img = new TempImage(200, 200, new Rectangle(100, 95, 4, 11));

        Assert.Equal(1, LineCounter.CountVerticalLines(img.Path));
    }

    [Fact]
    public void Separated_lines_are_counted_individually()
    {
        using var img = new TempImage(200, 200,
            new Rectangle(40, 0, 6, 200),
            new Rectangle(100, 0, 6, 200),
            new Rectangle(160, 0, 6, 200));

        Assert.Equal(3, LineCounter.CountVerticalLines(img.Path));
    }

    [Fact]
    public void Odd_and_even_heights_both_work()
    {
        using var even = new TempImage(120, 200, new Rectangle(60, 0, 5, 200));
        using var odd = new TempImage(120, 201, new Rectangle(60, 0, 5, 201));

        Assert.Equal(1, LineCounter.CountVerticalLines(even.Path));
        Assert.Equal(1, LineCounter.CountVerticalLines(odd.Path));
    }

    // --- Error handling -----------------------------------------------------

    [Fact]
    public void Missing_file_throws_FileNotFound()
    {
        string path = Path.Combine(Path.GetTempPath(), "tmmc_does_not_exist.jpg");

        Assert.Throws<FileNotFoundException>(() => LineCounter.CountVerticalLines(path));
    }

    [Fact]
    public void Non_image_file_throws_a_clear_message()
    {
        // A text file renamed to .jpg: GDI+ would throw a cryptic
        // "Parameter is not valid."; our wrapper must explain the real problem.
        string path = Path.Combine(Path.GetTempPath(), $"tmmc_{Guid.NewGuid():N}.jpg");
        File.WriteAllText(path, "this is not an image");
        try
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => LineCounter.CountVerticalLines(path));
            Assert.Contains("not a valid image", ex.Message);
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>
    /// Creates a temporary white JPEG with the given black rectangles painted
    /// on it, and deletes the file on dispose. Saving as JPEG (not PNG) keeps
    /// the test honest: the same lossy compression the real app must cope with.
    /// </summary>
    private sealed class TempImage : IDisposable
    {
        public string Path { get; }

        public TempImage(int width, int height, params Rectangle[] blackRects)
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(), $"tmmc_{Guid.NewGuid():N}.jpg");

            using var bitmap = new Bitmap(width, height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);
                foreach (var rect in blackRects)
                    g.FillRectangle(Brushes.Black, rect);
            }
            bitmap.Save(Path, ImageFormat.Jpeg);
        }

        public void Dispose()
        {
            try { File.Delete(Path); } catch { /* best-effort cleanup */ }
        }
    }
}
