using System;
using System.Runtime.InteropServices;

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
    }
}
