using System.Drawing;
using System.Threading.Tasks.Dataflow;

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
                func: (total, next) => Rectangle.Union(total, next)
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
                ? new Size(
                    width: bounds.Width,
                    height: (int)(obj.Height / widthRatio)
                )
                : new Size(
                    width: (int)(obj.Width / heightRatio),
                    height: bounds.Height
                );
            return scaledSize;
        }

        /// <summary>
        /// Calculate the new region of an object so that it's centered on the given midpoint.
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
        /// <param name="obj"></param>
        /// <param name="bounds"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static Rectangle MoveInside(Rectangle obj, Rectangle bounds)
        {
            if ((obj.Width > bounds.Width) || (obj.Height > bounds.Height))
            {
                throw new ArgumentException($"{nameof(obj)} cannot be larger than {nameof(bounds)}.");
            }
            return new Rectangle(
                LayoutHelper.Between(bounds.X, obj.X, bounds.Right - obj.Width),
                LayoutHelper.Between(bounds.Y, obj.Y, bounds.Bottom - obj.Height),
                obj.Width,
                obj.Height
            );
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
                obj: new Rectangle(
                    LayoutHelper.Center(scaledSize, cursorPosition),
                    scaledSize
                ),
                bounds: screenBounds
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