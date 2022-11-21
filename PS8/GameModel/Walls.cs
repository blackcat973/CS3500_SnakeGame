/**
 * This code is from GameLab\Models\World.cs, just for simple code
 */
using SnakeGame;
using Vector2D;

namespace GameWorld
{
    public class Walls
    {
        private Random rand = new();
        public int Wall { get; private set; }
        public Vector2D.Vector2D Point1;
        public Vector2D.Vector2D Point2;

        public Walls(int id, int x, int y)
        {
            Wall = id;
            Point1 = new Vector2D.Vector2D(x, y);
            Point2 = new Vector2D.Vector2D(x, y);
        }
    }
}