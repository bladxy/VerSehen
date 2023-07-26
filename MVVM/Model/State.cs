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
        public Point SnakeHeadPosition { get; set; }

        // Position of the apple
        public Point ApplePosition { get; set; }

        // Whether the game is over
        public bool IsGameOver { get; set; }

        public List<Point> SnakeBodyPoints = new List<Point>();
    }

}
