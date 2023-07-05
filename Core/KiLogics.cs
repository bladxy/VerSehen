using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Threading;

namespace VerSehen.Core
{
    public class SnakeAI
    {
        private IntPtr gameWindowHandle;
        private int snakeHeadX;
        private int snakeHeadY;
        private int appleX;
        private int appleY;
        private List<Point> snakeBody = new List<Point>();
        public bool IsMoving { get; private set; }

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
            if (CanMoveTo(snakeHeadX + 1, snakeHeadY))
            {
                PressKey(VK_RIGHT);
                IsMoving = true;
            }
            else
            {
                IsMoving = false;
            }
        }

        public void MoveLeft()
        {
            if (CanMoveTo(snakeHeadX - 1, snakeHeadY))
            {
                PressKey(VK_LEFT);
                IsMoving = true;
            }
            else
            {
                IsMoving = false;
            }
        }

        public void MoveUp()
        {
            if (CanMoveTo(snakeHeadX, snakeHeadY - 1))
            {
                PressKey(VK_UP);
                IsMoving = true;
            }
            else
            {
                IsMoving = false;
            }
        }

        public void MoveDown()
        {
            if (CanMoveTo(snakeHeadX, snakeHeadY + 1))
            {
                PressKey(VK_DOWN);
                IsMoving = true;
            }
            else
            {
                IsMoving = false;
            }
        }

        public bool IsColorInRange(Color color, Color target, int range)
        {
            return Math.Abs(color.R - target.R) <= range &&
                   Math.Abs(color.G - target.G) <= range &&
                   Math.Abs(color.B - target.B) <= range;
        }

        public void AnalyzeGame(Bitmap bitmap)
        {

            Color bodyColor = ColorTranslator.FromHtml("#80FF80");
            Color appleColor = ColorTranslator.FromHtml("#FF6666");
            Color eyeColor1 = ColorTranslator.FromHtml("#F2F2F2");
            Color eyeColor2 = ColorTranslator.FromHtml("#1A1A1A");

            snakeBody.Clear();

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color pixelColor = bitmap.GetPixel(x, y);

                    if (IsColorInRange(pixelColor, bodyColor, 10))
                    {
                        // This pixel is part of the snake body
                        snakeBody.Add(new Point(x, y));
                    }
                    else if (IsColorInRange(pixelColor, appleColor, 10))
                    {
                        // This pixel is part of the apple
                        appleX = x;
                        appleY = y;
                    }
                    else if (IsColorInRange(pixelColor, eyeColor1, 10) || IsColorInRange(pixelColor, eyeColor2, 10))
                    {
                        // This pixel is part of the snake's eyes
                        snakeHeadX = x;
                        snakeHeadY = y;
                    }
                }
            }
        }


        public void ChooseAction()
        {
            // Calculate the direction from the snake head to the apple
            int dx = appleX - snakeHeadX;
            int dy = appleY - snakeHeadY;

            // Try to move in the direction of the apple
            if (Math.Abs(dx) > Math.Abs(dy))
            {
                // The apple is farther in the x direction
                if (dx > 0 && !snakeBody.Contains(new Point(snakeHeadX + 1, snakeHeadY)))
                {
                    MoveRight();
                }
                else if (!snakeBody.Contains(new Point(snakeHeadX - 1, snakeHeadY)))
                {
                    MoveLeft();
                }
            }
            else
            {
                // The apple is farther in the y direction
                if (dy > 0 && !snakeBody.Contains(new Point(snakeHeadX, snakeHeadY + 1)))
                {
                    MoveDown();
                }
                else if (!snakeBody.Contains(new Point(snakeHeadX, snakeHeadY - 1)))
                {
                    MoveUp();
                }
            }

            // If we couldn't move in the direction of the apple, try to move in a safe direction
            if (!IsMoving)
            {
                if (!snakeBody.Contains(new Point(snakeHeadX + 1, snakeHeadY)))
                {
                    MoveRight();
                }
                else if (!snakeBody.Contains(new Point(snakeHeadX - 1, snakeHeadY)))
                {
                    MoveLeft();
                }
                else if (!snakeBody.Contains(new Point(snakeHeadX, snakeHeadY + 1)))
                {
                    MoveDown();
                }
                else if (!snakeBody.Contains(new Point(snakeHeadX, snakeHeadY - 1)))
                {
                    MoveUp();
                }
            }
        }

        public bool CanMoveTo(int x, int y)
        {
            // Check if the given position is part of the snake's body
            return !snakeBody.Contains(new Point(x, y));
        }

        public void Start()
        {
            // Start a new thread to run the AI logic
            new Thread(() =>
            {
                while (true) // Loop forever, you might want to add a condition to stop the AI
                {
                    // Capture the game window
                    Bitmap bitmap = CaptureWindow(gameWindowHandle);

                    // Analyze the game state
                    AnalyzeGame(bitmap);

                    // Choose an action based on the current game state
                    ChooseAction();

                    // Wait a bit before the next iteration
                    Thread.Sleep(100);
                }
            }).Start();
        }

    }
}
