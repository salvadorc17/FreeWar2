using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Drawing;

namespace DEngine
{
    /// <summary>
    /// Contains an unsafe static method to load and convert an XNA image to a GDI image using a fully qualified path and a Game instance.
    /// </summary>
    public class XNAGDIImageLoader
    {
        /// <summary>
        /// Load and convert an XNA image file into a GDI image.
        /// </summary>
        /// <param name="game">Instance of your game.</param>
        /// <param name="imageName">Fully qualified name of the XNA image file (no extension).</param>
        /// <returns></returns>
        public unsafe static Image LoadImage(Game game, string imageName)
        {
            Texture2D tex = null;
            try
            {
                tex = game.Content.Load<Texture2D>(imageName);
            }
            catch (Exception ex)
            {
                Log.Message(ex.Message);
            }

            uint[] d = new uint[tex.Width * tex.Height];
            tex.GetData<uint>(d);

            Bitmap bmp = new Bitmap(tex.Width,
                            tex.Height,
                            System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            System.Drawing.Imaging.BitmapData bmpd =
                    bmp.LockBits(
                        new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                        System.Drawing.Imaging.ImageLockMode.WriteOnly,
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            uint* ptr = (uint*)bmpd.Scan0.ToPointer();

            for (int x = 0; x < tex.Width; x++)
                for (int y = 0; y < tex.Height; y++)
                {
                    ptr[x + y * tex.Width] = d[x + y * tex.Width];
                }

            bmp.UnlockBits(bmpd);
            return bmp;
        }


        /// <summary>
        /// Convert an already loaded texture into an image.
        /// </summary>
        /// <param name="tex"></param>
        /// <returns></returns>
        public unsafe static Image ImageFromTexture(Texture2D tex)
        {
            uint[] d = new uint[tex.Width * tex.Height];
            tex.GetData<uint>(d);

            Bitmap bmp = new Bitmap(tex.Width,
                            tex.Height,
                            System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            System.Drawing.Imaging.BitmapData bmpd =
                    bmp.LockBits(
                        new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                        System.Drawing.Imaging.ImageLockMode.WriteOnly,
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            uint* ptr = (uint*)bmpd.Scan0.ToPointer();

            for (int x = 0; x < tex.Width; x++)
                for (int y = 0; y < tex.Height; y++)
                {
                    ptr[x + y * tex.Width] = d[x + y * tex.Width];
                }

            bmp.UnlockBits(bmpd);

            return bmp;
        }



        /// <summary>
        /// Convert the other way: from image file (png etc.) to Texture2D
        /// </summary>
        /// <param name="game"></param>
        /// <param name="imageFile"></param>
        /// <returns></returns>
        public unsafe static Texture2D TextureFromImage(Game game, string imageFile)
        {
            Texture2D tex = null;
            if (System.IO.File.Exists(imageFile))
            {
                // Load the image file from disk
                Bitmap bmp = (Bitmap)Image.FromFile(imageFile);
                System.Drawing.Imaging.BitmapData bmpData = new System.Drawing.Imaging.BitmapData();
                bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Size.Width, bmp.Size.Height),
                    System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                // Make a new texture of the same dimensions
                tex = new Texture2D(game.GraphicsDevice, bmp.Width, bmp.Height);

                // Set the Texture2D color data
                Microsoft.Xna.Framework.Color[] colorArray = new Microsoft.Xna.Framework.Color[tex.Width * tex.Height];

                uint* ptr = (uint*)bmpData.Scan0.ToPointer();

                for (int x = 0; x < tex.Width; x++)
                    for (int y = 0; y < tex.Height; y++)
                    {
                        //ptr[x + y * tex.Width] = d[x + y * tex.Width];
                        uint intColor = ptr[x + y * tex.Width];
                        System.Drawing.Color gdiColor = ColorTranslator.FromWin32((int)intColor);
                        colorArray[x + y * tex.Width] = new Microsoft.Xna.Framework.Color(gdiColor.R, gdiColor.G, gdiColor.B, gdiColor.A);
                    }

                bmp.UnlockBits(bmpData);

                tex.SetData<Microsoft.Xna.Framework.Color>(colorArray);
            }
            return tex;
        }
    }

}
