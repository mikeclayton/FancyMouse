namespace FancyMouse.Internal
{

    public static class LayoutHelper
    {

        #region General Helpers

        /// <summary>
        /// Return the smallest rectangle that contains all the specified regions.
        /// </summary>
        /// <param name="regions"></param>
        /// <returns></returns>
        public static Rectangle Combine(IEnumerable<Rectangle> regions)
        {
            if (regions == null)
            {
                throw new ArgumentNullException(nameof(regions));
            }
            var regionList = regions.ToList();
            if (regionList.Count == 0)
            {
                return Rectangle.Empty;
            }
            var combined = regionList.Aggregate(
                seed: regionList[0],
                func: Rectangle.Union
            );
            return combined;
        }

        /// <summary>
        /// Scale an object to fit inside the specified bounds while maintaining aspect ratio.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="bounds"></param>
        /// <returns></returns>
        public static Size ScaleToFit(Size obj, Size bounds)
        {
            if (bounds.Width == 0 || bounds.Height == 0)
            {
                return Size.Empty;
            }
            var widthRatio = (double)obj.Width / bounds.Width;
            var heightRatio = (double)obj.Height / bounds.Height;
            var scaledSize = (widthRatio > heightRatio)
                ? bounds with
                {
                    Height = (int)(obj.Height / widthRatio)
                }
                : bounds with
                {
                    Width = (int)(obj.Width / heightRatio)
                };
            return scaledSize;
        }

        /// <summary>
        /// Maps a location within a reference region onto a new region
        /// so that it's proportionally in the same position in the new region.
        /// </summary>
        /// <param name="originalBounds"></param>
        /// <param name="originalLocation"></param>
        /// <param name="scaledBounds"></param>
        /// <returns></returns>
        public static Point MapLocation(Rectangle originalBounds, Point originalLocation, Rectangle scaledBounds)
        {
            return new Point(
               (int)(originalLocation.X / (double)originalBounds.Width * scaledBounds.Width) + scaledBounds.Left,
               (int)(originalLocation.Y / (double)originalBounds.Height * scaledBounds.Height) + scaledBounds.Top
           );
        }

        /// <summary>
        /// Calculate the new location for an object so that it's centered on the given midpoint.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="midpoint"></param>
        /// <returns></returns>
        public static Point Center(Size obj, Point midpoint)
        {
            return new Point(
                x: (int)(midpoint.X - (float)obj.Width / 2),
                y: (int)(midpoint.Y - (float)obj.Height / 2)
            );
        }

        /// <summary>
        /// Returns the midpoint of the given bounds.
        /// </summary>
        /// <param name="bounds"></param>
        /// <returns></returns>
        public static Point Midpoint(Rectangle bounds)
        {
            return new Point(
                (bounds.Left + bounds.Right) / 2,
                (bounds.Top + bounds.Bottom) / 2
            );
        }

        ///// <summary>
        ///// Return where to position a scaled object so the original location overlaps the 
        ///// same point on the orignal sized object.
        ///// </summary>
        ///// <param name="size"></param>
        ///// <param name="location"></param>
        ///// <returns></returns>
        //private static Point AlignScaled(Size originalSize, Point originalLocation, Size scaledSize)
        //{
        //    return new Point(
        //        (int)(originalLocation.X - (float)scaledSize.Width * originalLocation.X / originalSize.Width),
        //        (int)(originalLocation.Y - (float)scaledSize.Height * originalLocation.Y / originalSize.Height)
        //    );
        //}
        public static int Between(int min, int value, int max)
        {
            if (min > max)
            {
                throw new ArgumentException($"{nameof(min)} cannot be greater than {nameof(max)}");
            }
            return Math.Max(min, Math.Min(value, max));
        }

        /// <summary>
        /// Returns the smallest width and height from the given sizes.
        /// </summary>
        /// <param name="sizes"></param>
        /// <returns></returns>
        public static Size Minimum(IList<Size> sizes)
        {
            return new Size(
                sizes.Min(s => s.Width),
                sizes.Min(s => s.Height)
            );
        }

        /// <summary>
        /// Returns the location to move the inner rectangle so that it sits entirely inside
        /// the outer rectangle. Returns the inner rectangle's current position if it is
        /// already inside the outer rectangle.
        /// </summary>
        /// <param name="inner"></param>
        /// <param name="outer"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static Rectangle Inside(Rectangle inner, Rectangle outer)
        {
            if ((inner.Width > outer.Width) || (inner.Height > outer.Height))
            {
                throw new ArgumentException($"{nameof(inner)} cannot be larger than {nameof(outer)}.");
            }
            return inner with
            {
                X = LayoutHelper.Between(outer.X, inner.X, outer.Right - inner.Width),
                Y = LayoutHelper.Between(outer.Y, inner.Y, outer.Bottom - inner.Height)
            };
        }

        #endregion

        /// <summary>
        /// Calculates the position to show the preview form based on a number of factors.
        /// </summary>
        /// <param name="desktopBounds">
        /// The bounds of the entire desktop / virtual screen. Might start at a negative
        /// x, y if a non-primary screen is located left of or above the primary screen.
        /// </param>
        /// <param name="cursorPosition">
        /// The current position of the cursor on the virtual desktop.
        /// </param>
        /// <param name="currentMonitorBounds">
        /// The bounds of the screen the cursor is currently on. Might start at a negative
        /// x, y if a non-primary screen is located left of or above the primary screen.
        /// </param>
        /// <param name="maximumPreviewImageSize">
        /// The largest allowable size of the preview image. This is literally the just
        /// image itself, not including padding around the image.
        /// </param>
        /// <param name="previewImagePadding">
        /// The total width and height of padding around the preview image.
        /// </param>
        /// <returns>
        /// The size and location to use when showing the preview image form.
        /// </returns>
        public static Rectangle GetPreviewFormBounds(
            Rectangle desktopBounds,
            Point cursorPosition,
            Rectangle currentMonitorBounds,
            Size maximumPreviewImageSize,
            Size previewImagePadding
        )
        {

            // see https://learn.microsoft.com/en-gb/windows/win32/gdi/the-virtual-screen

            // calculate the maximum size the form is allowed to be
            var maxFormSize = LayoutHelper.Minimum(
                new List<Size> {
                    // can't be bigger than the current screen
                    currentMonitorBounds.Size,
                    // can't be bigger than the max preview image
                    // *plus* the padding around the preview image
                    // (max preview image size doesn't include the padding)
                    maximumPreviewImageSize + previewImagePadding
                }
            );

            // calculate the actual form size by scaling the entire
            // desktop bounds into the max form size while accounting
            // for the size of the padding around the preview
            var previewImageSize = LayoutHelper.ScaleToFit(
                obj: desktopBounds.Size,
                bounds: maxFormSize - previewImagePadding
            );
            var formSize = previewImageSize + previewImagePadding;

            // centre the form to the cursor's position, but nudge it back
            // inside the visible area of the screen if it falls outside
            var formBounds = LayoutHelper.Inside(
                inner: new Rectangle(
                    LayoutHelper.Center(formSize, cursorPosition),
                    formSize
                ),
                outer: currentMonitorBounds
            );

            //// the preview image and the desktop are aligned at current mouse position
            //var aligned = FancyMouseForm.ClipLocation(
            //    bounds: screenBounds,
            //    location: FancyMouseForm.AlignScaled(desktopBounds.Size, cursorPosition, pbxPreview.Size),
            //    size: this.Size
            //);

            return formBounds;

        }

    }

}