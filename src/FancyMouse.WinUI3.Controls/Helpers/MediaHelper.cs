using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;

using Microsoft.UI.Xaml.Media.Imaging;

using SkiaSharp;

namespace FancyMouse.WinUI3.Controls.Helpers;

internal static class MediaHelper
{
    /// <summary>
    /// Copies <paramref name="bitmap"/>'s pixels directly into a <see cref="WriteableBitmap"/>,
    /// rather than round-tripping through a PNG encode + decode. <see cref="WriteableBitmap"/>'s
    /// own pixel buffer is tightly-packed BGRA8, premultiplied, so the pixels are read out in
    /// exactly that layout - a straight copy for the bitmaps the drawing code produces (e.g.
    /// <see cref="DrawingHelper.RenderBorder"/>), which are already
    /// <see cref="SKColorType.Bgra8888"/>/<see cref="SKAlphaType.Premul"/>, and a conversion for
    /// anything else.
    /// </summary>
    internal static WriteableBitmap ToBitmapImage(SKBitmap bitmap)
    {
        var writeableBitmap = new WriteableBitmap(bitmap.Width, bitmap.Height);

        var targetInfo = new SKImageInfo(bitmap.Width, bitmap.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
        var buffer = new byte[targetInfo.BytesSize];
        var bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        try
        {
            using var pixmap = bitmap.PeekPixels();
            if (!pixmap.ReadPixels(targetInfo, bufferHandle.AddrOfPinnedObject(), targetInfo.RowBytes))
            {
                throw new InvalidOperationException("Failed to read the bitmap's pixels.");
            }
        }
        finally
        {
            bufferHandle.Free();
        }

        using (var pixelStream = writeableBitmap.PixelBuffer.AsStream())
        {
            pixelStream.Write(buffer, 0, buffer.Length);
        }

        writeableBitmap.Invalidate();
        return writeableBitmap;
    }
}
