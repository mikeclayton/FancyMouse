using System.Drawing;

namespace FancyMouse.Lib
{

    public static class LayoutHelper
    {

        #region General Helpers

        /// <summary>
        /// Return the smallest rectangle that contains all the specified regions.
        /// </summary>
        /// <param name="regions"></param>
        /// <returns></returns>
        public static Rectangle CombineBounds(IEnumerable<Rectangle> regions)
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
                ? bounds with {
                    Height = (int)(obj.Height / widthRatio)
                }
                : bounds with {
                    Width = (int)(obj.Width / heightRatio)
                };
            return scaledSize;
        }

        public static Point ScaleLocation(Rectangle originalBounds, Point originalLocation, Rectangle scaledBounds)
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
        /// Returns the location to move the inner rectangle so that it sits entirely inside
        /// the outer rectangle. Returns the inner rectangle's current position if it is
        /// already inside the outer rectangle.
        /// </summary>
        /// <param name="inner"></param>
        /// <param name="outer"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static Rectangle MoveInside(Rectangle inner, Rectangle outer)
        {
            if ((inner.Width > outer.Width) || (inner.Height > outer.Height))
            {
                throw new ArgumentException($"{nameof(inner)} cannot be larger than {nameof(outer)}.");
            }
            return inner with {
                X = LayoutHelper.Between(outer.X, inner.X, outer.Right - inner.Width),
                Y = LayoutHelper.Between(outer.Y, inner.Y, outer.Bottom - inner.Height)
            };
        }

        #endregion

        public static Rectangle GetPreviewBounds(
            Rectangle screenBounds, Point cursorPosition, Size desiredSize
        )
        {

            // scale the preview form down if it's bigger than the screen
            var scaledSize = (desiredSize.Width > screenBounds.Width) || (desiredSize.Height > screenBounds.Height)
                ? LayoutHelper.ScaleToFit(desiredSize, screenBounds.Size)
                : desiredSize;

            // centre the preview on the cursor position
            return LayoutHelper.MoveInside(
                inner: new Rectangle(
                    LayoutHelper.Center(scaledSize, cursorPosition),
                    scaledSize
                ),
                outer: screenBounds
            );

            //// the preview image and the desktop are aligned at current mouse position
            //var desktopBounds = ScreenHelper.GetDesktopBounds();
            //var aligned = FancyMouseForm.ClipLocation(
            //    bounds: screenBounds,
            //    location: FancyMouseForm.AlignScaled(desktopBounds.Size, cursorPosition, pbxPreview.Size),
            //    size: this.Size
            //);

        }

    }

}