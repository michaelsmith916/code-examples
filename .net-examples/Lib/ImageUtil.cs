using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Management;
using System.Collections.Specialized;
using System.Drawing.Imaging;
using System.IO;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace CodeExamples.Lib {
    /*
     This class provides methods for using high quality image rendering methods while resizing images to 
     * requestdd dimensions.
     */
    public static class ImageUtil {

        /**
         * get YMC componetnts of an RGB image and populate them in the img parameter
         */
        public static void getYMCfromRGB(Bitmap image, sbyte[,] img, int width, int height) {
            for (int i = 0; i < height; i++) {
                for (int j = 0; j < width; j++) {
                    Color pixel = Color.White;
                    if (i < image.Height && j < image.Width) {
                        pixel = image.GetPixel(j, i);
                    }

                    int index = i * width + j;
                    img[0, index] = (sbyte)(255 - pixel.B);//Y
                    img[1, index] = (sbyte)(255 - pixel.G);//M
                    img[2, index] = (sbyte)(255 - pixel.R);//C

                }
            }

        }

        /* get all of the pixels of the image, ignoring any colors above 50 decimal, so that only black images are included.
         * This method was designed to identify pixels to be printed with K Panel ink.
         */
        public static void getMonochromeFromRGB(Bitmap image, sbyte[,] img, int aryPosition, int width, int height) {
            int blackColorCount = 0;
            for (int i = 0; i < height; i++) {
                for (int j = 0; j < width; j++) {
                    Color pixel = Color.White;
                    if (i < image.Height && j < image.Width) {
                        pixel = image.GetPixel(j, i);
                    }
                    int index = i * image.Width + j;
                    // img[aryPosition,index] = (sbyte)((pixel.B == 255 && pixel.G ==255 && pixel.R ==255 ) ? 0 : 255);

                    img[aryPosition, index] = (sbyte)((pixel.B < 50 && pixel.G < 50 && pixel.R < 50) ? (255 - pixel.G) : 0);

                    if (img[aryPosition, index] < 0)
                        blackColorCount++;

                }

            }
            //Console.WriteLine("blackColorCount:" + blackColorCount + "  total count: " + width * height);
        }

        public static string getBase64FromImage(Bitmap bitImage) {            
            MemoryStream ms = new MemoryStream();
           
            bitImage.Save(ms, ImageFormat.Jpeg);

            string base64ImageData = Convert.ToBase64String(ms.ToArray());
            base64ImageData = base64ImageData.Replace('+', ' ').Replace('/', '_');
            return base64ImageData;
        }

        public static Bitmap resizeImage(Image image, int width, int height) {
            if (image.Width == width && image.Height == height)
                return new Bitmap(image);

            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage)) {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;                
                graphics.TextContrast = 4;
                graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
                

                using (var wrapMode = new ImageAttributes()) {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }

                graphics.Dispose();
            }
            

            return destImage;
        }

        /*
         get the image from the given base64 encoded string.
         */
        public static Image getImageFromBase64(string base64ImageData, Size size, bool isFargo) {
            string corrected = base64ImageData.Replace(' ', '+').Replace('_', '/');
            byte[] data = Convert.FromBase64String(corrected);

            System.IO.MemoryStream streamBitmap = new System.IO.MemoryStream(data);
            Bitmap bitImage = new Bitmap((Bitmap)Image.FromStream(streamBitmap));
            return ConvertImage(bitImage, size.Width, size.Height, 100, isFargo);
        }

        public static Image getImageFromBase64Scaled(string base64ImageData, int width, int height) {
            return resizeImage(getImageFromBase64Unscaled(base64ImageData), width, height);
        }

        public static Image getImageFromBase64Unscaled(string base64ImageData) {
            string corrected = base64ImageData.Replace(' ', '+').Replace('_', '/');
            byte[] data = Convert.FromBase64String(corrected);


            System.IO.MemoryStream streamBitmap = new System.IO.MemoryStream(data);
            Bitmap bitImage = new Bitmap((Bitmap)Image.FromStream(streamBitmap));
            return bitImage;
        }

        /**
         * return the image at imgFilePath file path, scaled to the given size dimensions.
         */
        public static Image getImage(string imgFilePath, Size size, bool isFargo) {
            return ConvertImage(new Bitmap(imgFilePath), size.Width, size.Height, 100, isFargo);
        }

        /// <summary>
        /// Method to resize, convert and save the image.
        /// </summary>
        /// <param name="image">Bitmap image.</param>
        /// <param name="maxWidth">resize width.</param>
        /// <param name="maxHeight">resize height.</param>
        /// <param name="quality">quality setting value.</param>
        /// <param name="filePath">file path.</param>      
        public static Image ConvertImage(Bitmap image, int maxWidth, int maxHeight, int quality, bool isFargo) {
            // Get the image's original width and height
            int originalWidth = image.Width;
            int originalHeight = image.Height;

            // To preserve the aspect ratio
            float ratioX = (float)maxWidth / (float)originalWidth;
            float ratioY = (float)maxHeight / (float)originalHeight;
            float ratio = Math.Min(ratioX, ratioY);

            // New width and height based on aspect ratio
            int newWidth = (int)(originalWidth * ratio);
            int newHeight = (int)(originalHeight * ratio);

            // Convert other formats (including CMYK) to RGB.
            Bitmap newImage = new Bitmap(newWidth, newHeight, PixelFormat.Format64bppPArgb);

            // Draws the image in the specified size with quality mode set to HighQuality
            using (Graphics graphics = Graphics.FromImage(newImage)) {
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                if (isFargo)
                    graphics.DrawImage(image, -7, -7, newWidth, newHeight);
                else
                    graphics.DrawImage(image, 0, 0, newWidth, newHeight);

                graphics.Dispose();
            }

            // Get an ImageCodecInfo object that represents the JPEG codec.
            ImageCodecInfo imageCodecInfo = GetEncoderInfo(ImageFormat.Jpeg);

            // Create an Encoder object for the Quality parameter.
            System.Drawing.Imaging.Encoder encoder = System.Drawing.Imaging.Encoder.Quality;

            // Create an EncoderParameters object. 
            EncoderParameters encoderParameters = new EncoderParameters(1);

            // Save the image as a JPEG file with quality level.
            EncoderParameter encoderParameter = new EncoderParameter(encoder, quality);
            encoderParameters.Param[0] = encoderParameter;

            MemoryStream ms = new MemoryStream();
            newImage.Save(ms, imageCodecInfo, encoderParameters);
            return Image.FromStream(ms);
        }

        /// <summary>
        /// Method to get encoder infor for given image format.                
         /// get the Image Codec for the given image format.  This is used to return 
         /// the proper image codec (ie jpg, png, gif, bmp, etc) that the system supports.         
        /// </summary>
        /// <param name="format">Image format</param>
        /// <returns>image codec info.</returns>
        private static ImageCodecInfo GetEncoderInfo(ImageFormat format) {
            return ImageCodecInfo.GetImageDecoders().SingleOrDefault(c => c.FormatID == format.Guid);
        }

        public static Bitmap ConvertTo24bpp(Image img) {
            var bmp = new Bitmap(img.Width, img.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (var gr = Graphics.FromImage(bmp))
                gr.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height));
            return bmp;
        }

        public static Image DrawFilledRectangle(Size size, bool unscaled) {
            Bitmap bmp = new Bitmap(size.Width,size.Height);
            using (Graphics graph = Graphics.FromImage(bmp)) {
                Rectangle ImageSize = new Rectangle(0, 0, size.Width,size.Height);
                graph.FillRectangle(Brushes.White, ImageSize);
            }
            if (unscaled)
                return bmp;
            else
                return ConvertImage(bmp, size.Width, size.Height, 100, false);            
        }
    }


}
