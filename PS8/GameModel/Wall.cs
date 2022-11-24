/**
 * This code is from GameLab\Models\World.cs, just for simple code
 */
using Newtonsoft.Json;
using SnakeGame;
using System.Numerics;

namespace GameWorld
{
    public class Wall
    {

        [JsonProperty(PropertyName = "wall")]
        public int WallID { get; private set; }
        [JsonProperty(PropertyName = "p1")]
        public Vector2D Point1;
        [JsonProperty(PropertyName = "p2")]
        public Vector2D Point2;

        public Wall(int id, int x, int y)
        {
            WallID = id;
            Point1 = new Vector2D(x, y);
            Point2 = new Vector2D(x, y);
        }


    }
}