using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Interop;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;
using System.Linq;

namespace VerSehen.Core
{
    public class SnakeAI
    {
        private int snakeHeadX;
        private int snakeHeadY;
        private int appleX;
        private int appleY;
        private List<Point> snakeBody = new List<Point>();
        public bool IsMoving { get; private set; }

        // Q-table
        private Dictionary<State, Dictionary<Action, double>> Q = new Dictionary<State, Dictionary<Action, double>>();

        // Learning rate
        private double alpha = 0.5;

        // Discount factor
        private double gamma = 0.9;

        // Exploration rate
        private double epsilon = 0.1;

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        public const int KEYEVENTF_EXTENDEDKEY = 0x0001; // Key down flag
        public const int KEYEVENTF_KEYUP = 0x0002; // Key up flag
        public const int VK_RIGHT = 0x27; // Right arrow key code
        public const int VK_LEFT = 0x25; // Left arrow key code
        public const int VK_UP = 0x26; // Up arrow key code
        public const int VK_DOWN = 0x28; // Down arrow key code

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindow(IntPtr hWnd);

        public void UpdateQTable(State oldState, Action action, State newState, double reward)
        {
            if (!Q.ContainsKey(oldState))
            {
                Q[oldState] = new Dictionary<Action, double>();
            }

            if (!Q[oldState].ContainsKey(action))
            {
                Q[oldState][action] = 0;
            }

            double oldQValue = Q[oldState][action];
            double maxNewStateQValue = Q.ContainsKey(newState) ? Q[newState].Values.Max() : 0;
            double newQValue = (1 - alpha) * oldQValue + alpha * (reward + gamma * maxNewStateQValue);

            Q[oldState][action] = newQValue;
        }

        private State GetState()
        {
            return new State(snakeHeadX, snakeHeadY, appleX, appleY, snakeBody);
        }


        private double GetReward(State state)
        {
            if (state.IsGameOver)
            {
                return -100.0;
            }
            else if (state.HasEatenApple)
            {
                return 100.0;
            }
            else
            {
                return 0.0;
            }
        }

        private void PerformAction(Action action)
        {
            switch (action)
            {
                case Action.MoveUp:
                    MoveUp();
                    break;
                case Action.MoveDown:
                    MoveDown();
                    break;
                case Action.MoveLeft:
                    MoveLeft();
                    break;
                case Action.MoveRight:
                    MoveRight();
                    break;
            }
        }

        public Bitmap CaptureWindow(IntPtr hWnd)
        {
            //System.Windows.Application.Current.Dispatcher.Invoke(() =>
            //{
            //    hWnd = new System.Windows.Interop.WindowInteropHelper(System.Windows.Application.Current.MainWindow).Handle;
            //});

            if (!IsWindow(hWnd))
            {
                // Handle is not valid, return null or throw an exception
                return null;
            }

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

        public void ShowBitmap(Bitmap bitmap)
        {
            // Create a new form
            Form form = new Form();

            // Create a new PictureBox control
            PictureBox pictureBox = new PictureBox();

            // Set the PictureBox to display the bitmap
            pictureBox.Image = bitmap;

            // Adjust the size of the PictureBox to fit the bitmap
            pictureBox.Width = bitmap.Width;
            pictureBox.Height = bitmap.Height;

            // Add the PictureBox to the form
            form.Controls.Add(pictureBox);

            // Display the form
            form.ShowDialog();
        }

        public Action ChooseAction(State state)
        {
            if (!Q.ContainsKey(state) || Random.NextDouble() < epsilon)
            {
                return (Action)Random.Next(Enum.GetNames(typeof(Action)).Length);
            }

            return Q[state].OrderByDescending(x => x.Value).First().Key;
        }

        public void Learn(IntPtr formHandle)
        {
            while (true)
            {
                Bitmap bitmap = CaptureWindow(formHandle);
                AnalyzeGame(bitmap);

                State currentState = GetState();
                Action action = ChooseAction(currentState);
                PerformAction(action);

                Bitmap newBitmap = CaptureWindow(formHandle);
                AnalyzeGame(newBitmap);

                State newState = GetState();
                double reward = GetReward(newState);

                UpdateQTable(currentState, action, newState, reward);

                Thread.Sleep(100);
            }
        }


        public bool CanMoveTo(int x, int y)
        {
            // Check if the given position is part of the snake's body
            return !snakeBody.Contains(new Point(x, y));
        }

        public void Start(IntPtr formHandle)
        {
            // Start a new thread to run the AI logic
            new Thread(() =>
            {
                while (true) // Loop forever, you might want to add a condition to stop the AI
                {
                    // Capture the game window
                    Bitmap bitmap = CaptureWindow(formHandle);

                    //ShowBitmap(bitmap);
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
