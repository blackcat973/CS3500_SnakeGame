/**
 * This code is from GameLab\Models\Player.cs, just for simple code
 */

using SnakeGame;

namespace GameWorld
{
    public class Snake
    {
        public int ID { get; private set; }
        public string Name { get; private set; }
        public List<Vector2D> Body;
        public Vector2D Dir;
        public int Score { get; private set; }
        public bool Died { get; private set; } = false;
        public bool Alive { get; private set; } = true;
        public bool Disconnected { get; private set; } = false;
        public bool Join { get; private set; } = false;


        public Snake(int id, string name, int x, int y, int score)
        {
            ID = id;
            Name = name;
            Body = new List<Vector2D>();
            Body.Add(new Vector2D(x, y)); Body.Add(new Vector2D(x, y - 5));
            Dir = new Vector2D(1, 0);
            Score = score;
        }
    }
}