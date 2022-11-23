/**
 * This code is from GameLab\Models\Powerup.cs, just for simple code
 */
using SnakeGame;

namespace GameWorld
{
    public class PowerUp
    {
        public int Power;
        public Vector2D Location;
        public bool Died { get; private set; } = false;
        public PowerUp(int id, int x, int y)
        {
            Power = id;
            Location = new Vector2D(x, y);
        }
    }
}