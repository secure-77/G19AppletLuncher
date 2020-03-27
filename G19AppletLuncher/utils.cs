using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.InteropServices;


    class utils
    {
        // getPixelforBackgroundImage
        static public byte[] GetPixel(Bitmap bitmap)
        {
            PixelFormat format = PixelFormat.Format32bppArgb;

            Bitmap bm = new Bitmap(bitmap);

            byte[] pixels = new byte[LogitechGSDK.LOGI_LCD_COLOR_WIDTH * LogitechGSDK.LOGI_LCD_COLOR_HEIGHT * 4];

            BitmapData bitmapData = bm.LockBits(
                new Rectangle(0, 0, LogitechGSDK.LOGI_LCD_COLOR_WIDTH, LogitechGSDK.LOGI_LCD_COLOR_HEIGHT),
                ImageLockMode.ReadOnly,
                format);

            Marshal.Copy(bitmapData.Scan0, pixels, 0, pixels.Length);

            bm.UnlockBits(bitmapData);
            return pixels;

        }

        public static Color HexToColor(string hexString)
        {
            //replace # occurences
            if (hexString.IndexOf('#') != -1)
                hexString = hexString.Replace("#", "");

            int r, g, b = 0;

            r = int.Parse(hexString.Substring(0, 2), NumberStyles.AllowHexSpecifier);
            g = int.Parse(hexString.Substring(2, 2), NumberStyles.AllowHexSpecifier);
            b = int.Parse(hexString.Substring(4, 2), NumberStyles.AllowHexSpecifier);

            return Color.FromArgb(r, g, b);
        }



}

