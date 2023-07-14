using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindow(IntPtr hWnd);

        public bool CanMoveTo(int x, int y)
        {
            // Check if the new position is in the snake's body
            if (currentState.SnakeBody.Contains(new Point(x, y)))
            {
                // If it is, check if it's the tail (i.e., it will move in the next round)
                if (new Point(x, y) == currentState.SnakeBody[0])
                {
                    // The tail will move in the next round, so we can move to this position
                    return true;
                }
                else
                {
                    // It's not the tail, so we can't move to this position
                    return false;
                }
            }
            else
            {
                // The new position is not in the snake's body, so we can move to this position
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
                // Initialisierung des Q-Werts auf einen kleinen zufälligen Wert
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
            else if (state.HasEatenApple)
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
            if (CanMoveTo(currentState.SnakeHeadX + 1, currentState.SnakeHeadY))
            {
                PressKey(VK_RIGHT);
                currentState.IsMoving = true;
            }
            else
            {
                currentState.IsMoving = false;
            }
        }

        public void MoveLeft()
        {
            if (CanMoveTo(currentState.SnakeHeadX - 1, currentState.SnakeHeadY))
            {
                PressKey(VK_LEFT);
                currentState.IsMoving = true;
            }
            else
            {
                currentState.IsMoving = false;
            }
        }

        public void MoveUp()
        {
            if (CanMoveTo(currentState.SnakeHeadX, currentState.SnakeHeadY - 1))
            {
                PressKey(VK_UP);
                currentState.IsMoving = true;
            }
            else
            {
                currentState.IsMoving = false;
            }
        }

        public void MoveDown()
        {
            if (CanMoveTo(currentState.SnakeHeadX, currentState.SnakeHeadY + 1))
            {
                PressKey(VK_DOWN);
                currentState.IsMoving = true;
            }
            else
            {
                currentState.IsMoving = false;
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

            currentState.SnakeBody.Clear();

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color pixelColor = bitmap.GetPixel(x, y);

                    if (IsColorInRange(pixelColor, bodyColor, 10))
                    {
                        // This pixel is part of the snake body
                        currentState.SnakeBody.Add(new Point(x, y));
                    }
                    else if (IsColorInRange(pixelColor, appleColor, 10))
                    {
                        // This pixel is part of the apple
                        currentState.AppleX = x;
                        currentState.AppleY = y;
                    }
                    else if (IsColorInRange(pixelColor, eyeColor1, 10) || IsColorInRange(pixelColor, eyeColor2, 10))
                    {
                        // This pixel is part of the snake's eyes
                        currentState.SnakeHeadX = x;
                        currentState.SnakeHeadY = y;
                    }
                }
            }

            if (currentState.SnakeBody.Contains(new Point(currentState.SnakeHeadX, currentState.SnakeHeadY)) ||
               currentState.SnakeHeadX < 0 || currentState.SnakeHeadX >= bitmap.Width ||
               currentState.SnakeHeadY < 0 || currentState.SnakeHeadY >= bitmap.Height)
            {
                currentState.IsGameOver = true;
            }
            else
            {
                currentState.IsGameOver = false;
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
            var actions = Enum.GetValues(typeof(Action)).Cast<Action>().ToList();
            actions.RemoveAll(a => !CanMoveTo(state.SnakeHeadX + GetXOffset(a), state.SnakeHeadY + GetYOffset(a)));

            if (actions.Count == 0)
            {
                return Action.MoveUp;
            }

            // Epsilon-Greedy-Algorithmus
            if (random.NextDouble() < epsilon)
            {
                // Exploration: Wählen Sie eine zufällige Aktion
                return actions[random.Next(actions.Count)];
            }
            else
            {
                // Ausbeutung: Wählen Sie die Aktion mit dem höchsten Q-Wert
                return actions.OrderByDescending(a => Q.ContainsKey(state) && Q[state].ContainsKey(a) ? Q[state][a] : 0).First();
            }
        }


        public void Learn(IntPtr formHandle)
        {
            LoadQTable("C:\\Users\\jaeger04\\Desktop\\SnakeKi\\Ki.Txt");

            int counter = 0;

            while (true)
            {
                Bitmap bitmap = CaptureWindow(formHandle);
                currentState = new State(0, 0, 0, 0);
                AnalyzeGame(bitmap);
                if (currentState.IsGameOver)
                {
                    break;
                }
                currentState = GetState();
                currentAction = ChooseAction(currentState);
                PerformAction(currentAction);
                Bitmap newBitmap = CaptureWindow(formHandle);
                AnalyzeGame(newBitmap);
                State newState = GetState();
                double reward = GetReward(newState);
                epsilon = Math.Max(minEpsilon, epsilon * epsilonDecay);
                UpdateQTable(currentState, currentAction, newState, reward);
                Thread.Sleep(100);

                counter++;

                // Speichern der Q-Tabelle periodisch
                if (counter >= 100)
                {
                    SaveQTable("C:\\Users\\jaeger04\\Desktop\\SnakeKi\\Ki.Txt");
                    counter = 0;
                }
            }

            Stop();
        }

        public void SaveQTable(string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            {
                foreach (var entry in Q)
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
