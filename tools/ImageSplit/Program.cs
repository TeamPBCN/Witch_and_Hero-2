using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;

using ImageMagick;

namespace ImageSplit
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: ImageSplit [File[...]]");
                Console.ReadKey(false);
                return;
            }
            foreach (string path in args)
            {
                string ext = Path.GetExtension(path);
                if (ext == ".tga" || ext == ".png")
                {
                    SplitImage(path, Path.ChangeExtension(path, ".csv"));
                }
            }
        }

        public static void SplitImage(string imagePath, string csvPath)
        {
            if (!File.Exists(imagePath) || !File.Exists(csvPath))
            {
                return;
            }
            string[] csv = File.ReadAllLines(csvPath);
            List<ImageInfo> children = new List<ImageInfo>();
            for (int i = 1; i < csv.Length; i++)
            {
                if (string.IsNullOrEmpty(csv[i])) { continue; }
                children.Add(new ImageInfo(csv[i]));
            }

            MagickImage mImage = new MagickImage(imagePath);
            Bitmap bitmap = mImage.ToBitmap();
            string dir = Path.GetFileNameWithoutExtension(imagePath);
            Directory.CreateDirectory(dir);
            foreach (ImageInfo child in children)
            {
                SaveChild(bitmap, child, dir);
            }
        }
        public static void SaveChild(Bitmap bitmap, ImageInfo child, string dir)
        {
            SaveChild(bitmap, child.X, child.Y, child.Width, child.Height, string.Format("{0}\\{1}.png", dir, child.FileName));
        }
        public static void SaveChild(Bitmap bitmap, int x, int y, int width, int height, string path)
        {
            RectangleF rect = new RectangleF(x, y, width, height);
            Bitmap child = bitmap.Clone(rect, PixelFormat.Format32bppArgb);
            child.Save(path);
            Console.WriteLine("Save: {0}", path);
        }
        public class ImageInfo
        {
            public string Type;
            public string FileName;
            public int X;
            public int Y;
            public int Width;
            public int Height;

            public ImageInfo() { }
            public ImageInfo(string info)
            {
                string[] infos = info.Split(new char[] { ',' });
                if (infos.Length != 6) { return; }

                Type = infos[0];
                FileName = infos[1];
                X = int.Parse(infos[2]);
                Y = int.Parse(infos[3]);
                Width = int.Parse(infos[4]);
                Height = int.Parse(infos[5]);
            }
        }
    }
}
