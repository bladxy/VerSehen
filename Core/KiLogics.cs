using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using VerSehen.MVVM.Model;
using Action = VerSehen.MVVM.Model.Action;

namespace VerSehen.Core
{
    public class SnakeAI
    {
        private Random random = new Random();
        private Dictionary<State, Dictionary<Action, double>> Q = new Dictionary<State, Dictionary<Action, double>>();
        private State currentState;
        private Action currentAction;
        private double alpha = 0.5;
        private double gamma = 0.9;
        private double epsilon = 1.0;  // Startwert für Epsilon
        private double minEpsilon = 0.01;  // Minimaler Wert für Epsilon
        private double epsilonDecay = 0.995;  // Faktor, mit dem Epsilon in jedem Schritt reduziert wird
        private CancellationTokenSource cancellationTokenSource;

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
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindow(IntPtr hWnd);

        public bool CanMoveTo(int x, int y)
        {
            if (currentState.SnakeHeadPositions.Contains(new Point(x, y)))
            {
                return false;
            }
            else
            {
                return true;
            }
        }


        private int GetXOffset(Action action)
        {
            switch (action)
            {
                case Action.MoveRight:
                    return 1;
                case Action.MoveLeft:
                    return -1;
                default:
                    return 0;
            }
        }

        private int GetYOffset(Action action)
        {
            switch (action)
            {
                case Action.MoveUp:
                    return -1;
                case Action.MoveDown:
                    return 1;
                default:
                    return 0;
            }
        }

        public void UpdateQTable(State oldState, Action action, State newState, double reward)
        {
            if (!Q.ContainsKey(oldState))
            {
                Q[oldState] = new Dictionary<Action, double>();
            }
            if (!Q[oldState].ContainsKey(action))
            {
                Q[oldState][action] = random.NextDouble() * 0.1;
            }
            double oldQValue = Q[oldState][action];
            double maxNewStateQValue = Q.ContainsKey(newState) ? Q[newState].Values.Max() : 0;
            double newQValue = (1 - alpha) * oldQValue + alpha * (reward + gamma * maxNewStateQValue);
            Q[oldState][action] = newQValue;
        }

        private State GetState()
        {
            return currentState;
        }


        private double GetReward(State state)
        {
            if (state.IsGameOver)
            {
                return -100.0;
            }
            else if (state.SnakeHeadPositions.Contains(state.ApplePosition))
            {
                return 100.0;
            }
            else
            {
                return -0.1;
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
            if (CanMoveTo(currentState.SnakeHeadPositions[0].X + 1, currentState.SnakeHeadPositions[0].Y))
            {
                PressKey(VK_RIGHT);
            }
        }

        public void MoveLeft()
        {
            if (CanMoveTo(currentState.SnakeHeadPositions[0].X - 1, currentState.SnakeHeadPositions[0].Y))
            {
                PressKey(VK_LEFT);
            }
        }

        public void MoveUp()
        {
            if (CanMoveTo(currentState.SnakeHeadPositions[0].X, currentState.SnakeHeadPositions[0].Y - 1))
            {
                PressKey(VK_UP);
            }
        }

        public void MoveDown()
        {
            if (CanMoveTo(currentState.SnakeHeadPositions[0].X, currentState.SnakeHeadPositions[0].Y + 1))
            {
                PressKey(VK_DOWN);
            }
        }



        public bool IsColorInRange(Color color, Color target, int range)
        {
            // Print the RGB values of the color and the target color
            Debug.WriteLine($"Color: R={color.R}, G={color.G}, B={color.B}");
            Debug.WriteLine($"Target: R={target.R}, G={target.G}, B={target.B}");

            bool isInRange = Math.Abs(color.R - target.R) <= range &&
                             Math.Abs(color.G - target.G) <= range &&
                             Math.Abs(color.B - target.B) <= range;

            // Print whether the color is in range of the target color
            Debug.WriteLine($"Is in range: {isInRange}");

            return isInRange;
        }


        public State AnalyzeGame(Bitmap bitmap)
        {
            // Define the color ranges for the head of the snake
            Color bodyColor = ColorTranslator.FromHtml("#80FF80"); // Green
            int bodyRange = 50; // Adjust this value as needed

            Color whiteColor = ColorTranslator.FromHtml("#F1F1F1"); // White
            int whiteRange = 50; // Adjust this value as needed

            Color blackColor = ColorTranslator.FromHtml("#1A1A1A"); // Black
            int blackRange = 50; // Adjust this value as needed

            // Define the color for the apple
            Color appleColor = ColorTranslator.FromHtml("#FF6666"); // Red
            int appleRange = 50; // Adjust this value as needed

            // Initialize a new state
            State state = new State();

            // Analyze each pixel in the bitmap
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    // Get the color of the pixel
                    Color pixelColor = bitmap.GetPixel(x, y);

                    // Check if the color of the pixel is within any of the ranges for the head of the snake
                    if (IsColorInRange(pixelColor, bodyColor, bodyRange) ||
                        IsColorInRange(pixelColor, whiteColor, whiteRange) ||
                        IsColorInRange(pixelColor, blackColor, blackRange))
                    {
                        // The pixel is part of the head of the snake
                        state.SnakeHeadPositions.Add(new Point(x, y));
                    }

                    // Check if the color of the pixel is within the range for the apple
                    if (IsColorInRange(pixelColor, appleColor, appleRange))
                    {
                        // The pixel is part of the apple
                        state.ApplePosition = new Point(x, y);
                    }
                }
            }

            // Check if the game is over
            if (state.SnakeHeadPositions.Count == 0)
            {
                state.IsGameOver = true;
            }

            // Return the analyzed state
            return state;
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
            var actions = Enum.GetValues(typeof(Action)).Cast<Action>().ToList();
            actions.RemoveAll(a => !CanMoveTo(state.SnakeHeadPositions[0].X + GetXOffset(a), state.SnakeHeadPositions[0].Y + GetYOffset(a)));
            if (actions.Count == 0)
            {
                return Action.MoveUp;
            }
            if (random.NextDouble() < epsilon)
            {
                return actions[random.Next(actions.Count)];
            }
            else
            {
                return actions.OrderByDescending(a => Q.ContainsKey(state) && Q[state].ContainsKey(a) ? Q[state][a] : 0).First();
            }
        }




        public void Learn(IntPtr formHandle)
        {
            LoadQTable("C:\\Users\\jaeger04\\Desktop\\SnakeKi\\Ki.Txt");
            int counter = 0;
            StartGame();
            Thread.Sleep(3000);
            while (true)
            {
                if (GetForegroundWindow() != formHandle)
                {
                    break;
                }
                Bitmap bitmap = CaptureWindow(formHandle);
                currentState = AnalyzeGame(bitmap);
                if (currentState.IsGameOver)
                {
                    StartGame();
                    Thread.Sleep(3000);
                    currentState = AnalyzeGame(bitmap);
                }
                currentAction = ChooseAction(currentState);
                PerformAction(currentAction);
                Bitmap newBitmap = CaptureWindow(formHandle);
                State newState = AnalyzeGame(newBitmap);
                double reward = GetReward(newState);
                epsilon = Math.Max(minEpsilon, epsilon * epsilonDecay);
                UpdateQTable(currentState, currentAction, newState, reward);
                Thread.Sleep(100);
                Debug.WriteLine($"Reward: {reward}");
                counter++;
                if (counter >= 100)
                {
                    SaveQTable("C:\\Users\\jaeger04\\Desktop\\SnakeKi\\Ki.Txt");
                    counter = 0;
                }
            }
            SaveQTable("C:\\Users\\jaeger04\\Desktop\\SnakeKi\\Ki.Txt");
            Stop();
        }


        public void SaveQTable(string filePath)
        {
            var qTableCopy = Q.ToDictionary(entry => entry.Key, entry => entry.Value);

            using (var writer = new StreamWriter(filePath))
            {
                foreach (var entry in qTableCopy)
                {
                    var stateString = JsonConvert.SerializeObject(entry.Key);
                    var actionValuesString = JsonConvert.SerializeObject(entry.Value);
                    writer.WriteLine($"{stateString}: {actionValuesString}");
                }
            }
        }

        public void LoadQTable(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            using (var reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var parts = line.Split(':');
                    var state = JsonConvert.DeserializeObject<State>(parts[0]);
                    var actionValues = JsonConvert.DeserializeObject<Dictionary<Action, double>>(parts[1]);

                    Q[state] = actionValues;
                }
            }
        }

        public void StartGame()
        {
            // Simulate pressing the 'Enter' key
            const int VK_ENTER = 0x0D;
            PressKey(VK_ENTER);
        }

        public void Start(IntPtr formHandle)
        {
            cancellationTokenSource = new CancellationTokenSource();
            new Thread(() =>
            {
                Learn(formHandle);
            }).Start();
        }


        public void Stop()
        {
            cancellationTokenSource.Cancel();
        }

    }
}
