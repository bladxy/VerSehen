using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerSehen.MVVM.Model
{
    public class State
    {
        public int SnakeHeadX { get; set; }
        public int SnakeHeadY { get; set; }
        public int AppleX { get; set; }
        public int AppleY { get; set; }
        public List<Point> SnakeBody { get; set; }
        public bool IsGameOver { get; set; }
        public bool HasEatenApple { get; set; }
        public bool IsMoving { get; set; }

        public State(int snakeHeadX, int snakeHeadY, int appleX, int appleY)
        {
            SnakeHeadX = snakeHeadX;
            SnakeHeadY = snakeHeadY;
            AppleX = appleX;
            AppleY = appleY;
            SnakeBody = new List<Point>();  // Initialisierung von SnakeBody
        }
    }
}
