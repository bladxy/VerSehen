using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace VerSehen.Core
{
    public class SnakeAI
    {
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        public const int KEYEVENTF_EXTENDEDKEY = 0x0001; // Key down flag
        public const int KEYEVENTF_KEYUP = 0x0002; // Key up flag
        public const int VK_RIGHT = 0x27; // Right arrow key code
        public const int VK_LEFT = 0x25; // Left arrow key code
        public const int VK_UP = 0x26; // Up arrow key code
        public const int VK_DOWN = 0x28; // Down arrow key code

        public class RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);

        public Bitmap CaptureWindow(IntPtr hWnd)
        {
            RECT rect = new RECT();
            GetWindowRect(hWnd, ref rect);

            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;

            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.CopyFromScreen(rect.Left, rect.Top, 0, 0, bitmap.Size, CopyPixelOperation.SourceCopy);

            return bitmap;
        }

        public void PressKey(int keyCode)
        {
            // Press the key
            keybd_event((byte)keyCode, 0, KEYEVENTF_EXTENDEDKEY, 0);
            // Release the key
            keybd_event((byte)keyCode, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
        }

        public void MoveRight()
        {
            PressKey(VK_RIGHT);
        }

        public void MoveLeft()
        {
            PressKey(VK_LEFT);
        }

        public void MoveUp()
        {
            PressKey(VK_UP);
        }

        public void MoveDown()
        {
            PressKey(VK_DOWN);
        }

        public void AnalyzeGame(Bitmap bitmap)
        {
            Color bodyColor = ColorTranslator.FromHtml("#80FF80");
            Color appleColor = ColorTranslator.FromHtml("#FF6666");
            Color eyeColor1 = ColorTranslator.FromHtml("#F2F2F2");
            Color eyeColor2 = ColorTranslator.FromHtml("#1A1A1A");

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color pixelColor = bitmap.GetPixel(x, y);

                    if (pixelColor == bodyColor)
                    {
                        // This pixel is part of the snake's body
                    }
                    else if (pixelColor == appleColor)
                    {
                        // This pixel is part of the apple
                    }
                    else if (pixelColor == eyeColor1 || pixelColor == eyeColor2)
                    {
                        // This pixel is part of the snake's eyes
                    }
                }
            }
        }
    }
}
