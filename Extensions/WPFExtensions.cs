#region Copyright (C) 2017-2020  Starflash Studios
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html
#endregion

#region Using Directives

using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

#endregion

namespace Osu_BackgroundPurge {
    public static class WPFExtensions {

        #region Bitmap

        #region Resize

        /// <summary>Returns a resized form of the specified <paramref name="Bmp"/> to the requested <paramref name="FinalWidth"/> and <paramref name="FinalHeight"/>, using the specified parameters.</summary>
        /// <param name="Bmp">The BMP.</param>
        /// <param name="FinalWidth">The final width.</param>
        /// <param name="FinalHeight">The final height.</param>
        /// <param name="OffsetX">The offset x.</param>
        /// <param name="OffsetY">The offset y.</param>
        /// <param name="CompositingMode">The compositing mode.</param>
        /// <param name="CompositingQuality">The compositing quality.</param>
        /// <param name="InterpolationMode">The interpolation mode.</param>
        /// <param name="SmoothingMode">The smoothing mode.</param>
        /// <param name="PixelOffsetMode">The pixel offset mode.</param>
        /// <param name="WrapMode">The wrap mode.</param>
        /// <returns></returns>
        public static Bitmap Resize(this Bitmap Bmp, int FinalWidth, int FinalHeight, int OffsetX = 0, int OffsetY = 0, CompositingMode CompositingMode = CompositingMode.SourceCopy, CompositingQuality CompositingQuality = CompositingQuality.HighQuality, InterpolationMode InterpolationMode = InterpolationMode.HighQualityBicubic, SmoothingMode SmoothingMode = SmoothingMode.HighQuality, PixelOffsetMode PixelOffsetMode = PixelOffsetMode.HighQuality, WrapMode WrapMode = WrapMode.TileFlipXY) {
            Rectangle DestRect = new Rectangle(OffsetX, OffsetY, FinalWidth, FinalHeight);
            Bitmap DestImage = new Bitmap(FinalWidth, FinalHeight);

            DestImage.SetResolution(Bmp.HorizontalResolution, Bmp.VerticalResolution);

            using (Graphics Graphics = Graphics.FromImage(DestImage)) {
                Graphics.CompositingMode = CompositingMode;
                Graphics.CompositingQuality = CompositingQuality;
                Graphics.InterpolationMode = InterpolationMode;
                Graphics.SmoothingMode = SmoothingMode;
                Graphics.PixelOffsetMode = PixelOffsetMode;

                using (ImageAttributes WrapAttrb = new ImageAttributes()) {
                    WrapAttrb.SetWrapMode(WrapMode);
                    Graphics.DrawImage(Bmp, DestRect, 0, 0, Bmp.Width, Bmp.Height, GraphicsUnit.Pixel, WrapAttrb);
                }
            }

            return DestImage;
        }

        /// <summary><inheritdoc cref="Resize(Bitmap, int, int, int, int, CompositingMode, CompositingQuality, InterpolationMode, SmoothingMode, PixelOffsetMode, WrapMode)"/> Final result represents the <see cref="Bitmap"/> stretched to the given dimensions. </summary>
        /// <param name="Bmp">The BMP.</param>
        /// <param name="MinWidth">The minimum width.</param>
        /// <param name="MinHeight">The minimum height.</param>
        /// <param name="CompositingMode">The compositing mode.</param>
        /// <param name="CompositingQuality">The compositing quality.</param>
        /// <param name="InterpolationMode">The interpolation mode.</param>
        /// <param name="SmoothingMode">The smoothing mode.</param>
        /// <param name="PixelOffsetMode">The pixel offset mode.</param>
        /// <param name="WrapMode">The wrap mode.</param>
        /// <returns><see cref="Bitmap"/></returns>
        public static Bitmap ResizeToStretch(this Bitmap Bmp, int MinWidth, int MinHeight, CompositingMode CompositingMode = CompositingMode.SourceCopy, CompositingQuality CompositingQuality = CompositingQuality.HighQuality, InterpolationMode InterpolationMode = InterpolationMode.HighQualityBicubic, SmoothingMode SmoothingMode = SmoothingMode.HighQuality, PixelOffsetMode PixelOffsetMode = PixelOffsetMode.HighQuality, WrapMode WrapMode = WrapMode.TileFlipXY) {
            int FinalHeight = (int)(Bmp.Width * ((double)MinHeight / MinWidth)).Ceil();
            return Bmp.Resize(Bmp.Width, FinalHeight, 0, 0, CompositingMode, CompositingQuality, InterpolationMode, SmoothingMode, PixelOffsetMode, WrapMode);
        }

        /// <summary><inheritdoc cref="Resize(Bitmap, int, int, int, int, CompositingMode, CompositingQuality, InterpolationMode, SmoothingMode, PixelOffsetMode, WrapMode)"/> Final result represents the <see cref="Bitmap"/> upscaled to the largest relative resolution, cropped to fit. </summary>
        /// <param name="Bmp">The BMP.</param>
        /// <param name="RequestedWidth">The requested width.</param>
        /// <param name="RequestedHeight">The requested height.</param>
        /// <param name="Format">The resultant image format</param>
        /// <param name="CompositingMode">The compositing mode.</param>
        /// <param name="CompositingQuality">The compositing quality.</param>
        /// <param name="InterpolationMode">The interpolation mode.</param>
        /// <param name="SmoothingMode">The smoothing mode.</param>
        /// <param name="PixelOffsetMode">The pixel offset mode.</param>
        /// <param name="WrapMode">The wrap mode.</param>
        /// <returns><see cref="Bitmap"/></returns>
        public static Bitmap ResizeToFill(this Bitmap Bmp, int RequestedWidth, int RequestedHeight, PixelFormat Format = PixelFormat.Format24bppRgb, CompositingMode CompositingMode = CompositingMode.SourceCopy, CompositingQuality CompositingQuality = CompositingQuality.HighQuality, InterpolationMode InterpolationMode = InterpolationMode.HighQualityBicubic, SmoothingMode SmoothingMode = SmoothingMode.HighQuality, PixelOffsetMode PixelOffsetMode = PixelOffsetMode.HighQuality, WrapMode WrapMode = WrapMode.TileFlipXY) {
            //Store original sizes to prevent multiple getter calls.
            int BmpWidth = Bmp.Width;
            int BmpHeight = Bmp.Height;

            //Find the (up/down)scaling ratios and pick the largest one.
            //We do this since we want to scale to the largest fit size and crop the excess,
            //  instead of scaling to possibly the lower size and needing stretch out one dimension.
            float RatioA = (float)RequestedWidth / BmpWidth;
            float RatioB = (float)RequestedHeight / BmpHeight;
            float Ratio = RatioA > RatioB ? RatioA : RatioB;

            //Find the final (up/down)scaled size by multiplying the original size by the largest ratio.
            int FinalWidth = (BmpWidth * Ratio).CeilToWhole();
            int FinalHeight = (BmpHeight * Ratio).CeilToWhole();

            //Find the center offsets
            int OffsetX = ((FinalWidth - RequestedWidth) / 2.0f).CeilToWhole();
            int OffsetY = ((FinalHeight - RequestedHeight) / 2.0f).CeilToWhole();

            //Resize the original bitmap to the largest possible fitting size.
            Bitmap Resized = Bmp.Resize(FinalWidth, FinalHeight, 0, 0, CompositingMode, CompositingQuality, InterpolationMode, SmoothingMode, PixelOffsetMode, WrapMode);

            //Determine the final dimensions and crop the resized bitmap to them. Return it.
            Rectangle FinalDimensions = new Rectangle(OffsetX, OffsetY, RequestedWidth, RequestedHeight);
            return Resized.Clone(FinalDimensions, Format);
        }

        #endregion

        #region Save

        /// <summary>Saves the given <paramref name="Bitmap"/> to PNG.</summary>
        /// <param name="Bitmap">The bitmap.</param>
        /// <param name="Destination">The destination.</param>
        public static void SaveToPNG(this Bitmap Bitmap, FileInfo Destination) => Bitmap.Save(Destination.FullName, ImageFormat.Png);

        /// <summary>Saves the given <paramref name="Bitmap"/> to JPG.</summary>
        /// <param name="Bitmap">The bitmap.</param>
        /// <param name="Destination">The destination.</param>
        public static void SaveToJPG(this Bitmap Bitmap, FileInfo Destination) => Bitmap.Save(Destination.FullName, ImageFormat.Jpeg);

        /// <summary>Saves the given <paramref name="Bitmap"/> to BMP.</summary>
        /// <param name="Bitmap">The bitmap.</param>
        /// <param name="Destination">The destination.</param>
        public static void SaveToBMP(this Bitmap Bitmap, FileInfo Destination) => Bitmap.Save(Destination.FullName, ImageFormat.Bmp);

        /// <summary>Saves the given <paramref name="Visual"/> with the specified <paramref name="Encoder"/> format.</summary>
        /// <param name="Visual">The visual.</param>
        /// <param name="Destination">The destination.</param>
        /// <param name="Encoder">The encoder.</param>
        public static void SaveUsingEncoder(this FrameworkElement Visual, FileInfo Destination, BitmapEncoder Encoder) {
            RenderTargetBitmap Bitmap = new RenderTargetBitmap((int)Visual.ActualWidth, (int)Visual.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            Bitmap.Render(Visual);
            BitmapFrame Frame = BitmapFrame.Create(Bitmap);
            Encoder.Frames.Add(Frame);

            using (FileStream Stream = Destination.Create()) {
                Encoder.Save(Stream);
            }
        }

        #endregion

        #endregion

        /// <summary>Sets the visibility of the given <see cref="Element"/>.</summary>
        /// <param name="Element">The element.</param>
        /// <param name="Visibility">The visibility.</param>
        public static void SetVisibility(this UIElement Element, Visibility Visibility) {
            if (Element != null) { Element.Visibility = Visibility; }
        }

        public static bool TryGetWindow<T>(this Application App, out T Found) where T : Window {
            // ReSharper disable once LoopCanBePartlyConvertedToQuery
            foreach(Window OpenWindow in App.Windows) {
                if (OpenWindow.GetType() == typeof(T)) {
                    Found = (T)OpenWindow;
                    return true;
                }
            }

            Found = null;
            return false;
        }
    }
}
