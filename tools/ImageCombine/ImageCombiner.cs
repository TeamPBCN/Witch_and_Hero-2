using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.IO;

namespace ImageCombine
{
    public class ImageCombiner
    {
        public static Bitmap CombineImages(ref ImageChild[] imageChildren, int resWidth = 0, int resHeight = 0)
        {
            List<ImageChild> ichildren = new List<ImageChild>(imageChildren);
            imageChildren = ichildren.OrderBy(ic => ic.Rect.Height).Reverse().ToArray();
            ichildren = new List<ImageChild>(imageChildren);

            if (resHeight == 0 && resWidth == 0)
            {
                resWidth = nlpo2(imageChildren.Max(ic => ic.Rect.Width));
                resHeight = resWidth;
            }
            Bitmap result = new Bitmap(resWidth, resHeight);
            Graphics gfx = Graphics.FromImage(result);

            int usedHeight = 0;
            Point curPoint = new Point(0, 0);
            ImageChild child;
            while (true)
            {
                child = GetFirstUnCombined(imageChildren, result.Width - curPoint.X);
                if (child == null)
                {
                    curPoint.X = 0; curPoint.Y += usedHeight;
                    usedHeight = 0;
                    continue;
                }
                if (curPoint.Y + child.Image.Height > result.Height)
                {
                    Bitmap temp = new Bitmap(result.Width, nlpo2(result.Height + 1));
                    var gfxtmp = Graphics.FromImage(temp);
                    gfxtmp.DrawImage(result, new Point(0, 0));
                    result = temp;
                    gfx = Graphics.FromImage(result);
                }
                gfx.DrawImage(child.Image, new Rectangle(curPoint, child.Image.Size));
                int index = ichildren.IndexOf(child);
                imageChildren[index].IsCombined = true;
                imageChildren[index].Rect = new Rectangle(curPoint, child.Image.Size);
                curPoint.X += child.Image.Width;
                usedHeight = Math.Max(usedHeight, child.Image.Height);

                if (GetFirstUnCombined(imageChildren) == null) { break; }
            }

            if (curPoint.Y + usedHeight < result.Height / 2)
            {
                result = result.Clone(new Rectangle(0, 0, result.Width, result.Height / 2), result.PixelFormat);
            }
            if (result.Height / result.Width > 2)
            {
                for (int i = 0; i < imageChildren.Length; i++)
                {
                    imageChildren[i].IsCombined = false;
                }
                result = CombineImages(ref imageChildren, result.Width * 2, result.Width * 2);
            }
            return result;
        }
        private static ImageChild GetFirstUnCombined(ImageChild[] imageChildren)
        {
            ImageChild imageChild;
            try
            {
                imageChild = imageChildren.First(ic => !ic.IsCombined);
            }
            catch (Exception)
            {
                return null;
            }
            return imageChild;
        }
        private static ImageChild GetFirstUnCombined(ImageChild[] imageChildren, int usableWidth)
        {
            ImageChild imageChild;
            try
            {
                imageChild = imageChildren.First(ic => !ic.IsCombined && (ic.Rect.Width < usableWidth));
            }
            catch (Exception)
            {
                return null;
            }
            return imageChild;
        }
        private static ImageChild GetFirstUnCombined(ImageChild[] imageChildren, Size usableSize)
        {
            ImageChild imageChild;
            try
            {
                imageChild = imageChildren.First(ic => !ic.IsCombined &&
                (ic.Rect.Width < usableSize.Width) &&
                (ic.Rect.Height < usableSize.Height));
            }
            catch (Exception)
            {
                return null;
            }
            return imageChild;
        }
        private static int nlpo2(int x)
        {
            x--;
            x |= (x >> 1);
            x |= (x >> 2);
            x |= (x >> 4);
            x |= (x >> 8);
            x |= (x >> 16);
            return (x + 1);
        }
    }
    public class ImageChild
    {
        public string Name { get; set; }
        public Rectangle Rect { get; set; }
        public Bitmap Image { get; set; }
        public bool IsCombined { get; set; }

        public ImageChild(string path)
        {
            //Console.WriteLine("Load: {0}", path);
            Image = new Bitmap(path);
            Name = Path.GetFileNameWithoutExtension(Path.GetFileName(path));
            Rect = new Rectangle(new Point(0, 0), Image.Size);
            IsCombined = false;
        }

        public string ToCsvEntry()
        {
            return string.Format("Image,{0},{1},{2},{3},{4}", Name, Rect.X, Rect.Y, Rect.Width, Rect.Height);
        }
    }
}
