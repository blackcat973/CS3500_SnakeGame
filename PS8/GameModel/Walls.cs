/**
 * This code is from GameLab\Models\World.cs, just for simple code
 */
using Newtonsoft.Json;
using SnakeGame;

namespace GameWorld
{
    public class Walls
    {

        [JsonProperty(PropertyName = "wall")]
        public int Wall { get; private set; }
        [JsonProperty(PropertyName = "p1")]
        public Vector2D Point1;
        [JsonProperty(PropertyName = "p2")]
        public Vector2D Point2;

        public Walls(int id, int x, int y)
        {
            Wall = id;
            Point1 = new Vector2D(x, y);
            Point2 = new Vector2D(x, y);
        }
    }
}