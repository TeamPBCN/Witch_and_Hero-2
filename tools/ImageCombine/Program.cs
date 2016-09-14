using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ImageMagick;

namespace ImageCombine
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: ImageCombine [Folder[...]]");
                Console.ReadKey(false);
                return;
            }
            foreach (string path in args)
            {
                if (Directory.Exists(path))
                {
                    CombineFolder(path);
                }
            }
        }
        static void CombineFolder(string dir)
        {
            var files = Directory.GetFiles(dir);
            List<ImageChild> children = new List<ImageChild>();
            foreach (var path in files)
            {
                if (Path.GetExtension(path) == ".png")
                {
                    children.Add(new ImageChild(path));
                }
            }
            ImageChild[] ichildren = children.ToArray();
            MagickImage mImage = new MagickImage(ImageCombiner.CombineImages(ref ichildren));
            mImage.Write(string.Format("{0}.tga", dir));

            using (StreamWriter writer = File.CreateText(string.Format("{0}.csv", dir)))
            {
                writer.WriteLine("FileName,{0}.tga,{0}.tga,{1},{2}", Path.GetFileName(dir), mImage.Width, mImage.Height);
                foreach (var ic in ichildren)
                {
                    writer.WriteLine(ic.ToCsvEntry());
                }
            }
        }
    }
}
