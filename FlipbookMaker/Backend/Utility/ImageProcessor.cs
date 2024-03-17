using FlipbookMaker.Data;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;

namespace FlipbookMaker.Backend
{
    public sealed class ImageProcessor
    {
        public void CreateFlipbookImage(IList<FlipbookFrame> frames, int columns, int frameSize, string resultFilePath)
        {
            int numOfFrames = frames.Count;
            int rowNumber = (int)(frames.Count / ((float)columns) + 1);
            using Image result = new Image<Rgba32>(frameSize * columns, frameSize * rowNumber);
            result.Mutate(o => o.Clear(new SolidBrush(Color.Transparent)));

            int x = 0, y = 0;
            foreach (var frame in frames)
            {
                if (x >= columns)
                {
                    x = 0;
                    y++;
                }

                Point position = new Point(x * frameSize, y * frameSize);
                byte[] b = frame.Image;

                using Image frameImage = Image.Load<Rgba32>(b);
                result.Mutate(o => o.DrawImage(frameImage, position, 1f));
                x++;
            }

            result.Save(resultFilePath);
        }
    }
}
