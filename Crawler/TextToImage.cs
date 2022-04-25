using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace CielaCrawler
{
    public class TextToImage
    {
        public Image GeneratedImage { get; }
        public Image GeneratedImageMobile { get; }

        public TextToImage(string description)
        {
            this.GeneratedImage = this.DrawText(description, new Font("Times New Roman", 14f), Color.Black, Color.White, 580);
            this.GeneratedImageMobile = this.DrawText(description, new Font("Times New Roman", 41f), Color.Black, Color.White, 1000);
        }

        private Image DrawText(String text, Font font, Color textColor, Color backColor, int width)
        {
            //first, create a dummy bitmap just to get a graphics object
            Image img = new Bitmap(1, 1);
            Graphics drawing = Graphics.FromImage(img);

            //measure the string to see how big the image needs to be
            SizeF textSize = drawing.MeasureString(text, font, width);

            //free up the dummy image and old graphics object
            img.Dispose();
            drawing.Dispose();

            //create a new image of the right size

            img = new Bitmap(width, (int)textSize.Height);

            drawing = Graphics.FromImage(img);

            //paint the background
            drawing.Clear(backColor);

            //create a brush for the text
            Brush textBrush = new SolidBrush(textColor);

            drawing.TextRenderingHint = TextRenderingHint.AntiAlias;

            RectangleF rectF = new RectangleF(0, 0, width, textSize.Height);

            StringFormat sf = new StringFormat()
            {
                Alignment = StringAlignment.Near
            };

            drawing.DrawString(text, font, textBrush, rectF, sf);

            drawing.Save();

            textBrush.Dispose();
            drawing.Dispose();

            return img;
        }
    }
}
