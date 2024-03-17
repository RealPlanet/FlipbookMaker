using System.Windows.Media.Imaging;

namespace FlipbookMaker.Backend
{
    public static class Utility
    {
        public static bool IsPowerOfTwo(int x)
        {
            return (x != 0) && ((x & (x - 1)) == 0);
        }

        public static BitmapImage CreateBitmapFromBytes(byte[] bytes)
        {
            using (var memoryStream = new System.IO.MemoryStream(bytes))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = memoryStream;
                image.DecodePixelHeight = 256;
                image.DecodePixelWidth = 256;
                image.EndInit();
                image.Freeze();
                return image;
            }
        }
    }
}
