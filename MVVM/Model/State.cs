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
        // List of positions of the snake's head
        public List<Point> SnakeHeadPositions { get; set; } = new List<Point>();

        // Position of the apple
        public Point ApplePosition { get; set; }

        // Whether the game is over
        public bool IsGameOver { get; set; }
    }

}
