/**
 * This code is from GameLab\Models\Player.cs, just for simple code
 */

using Newtonsoft.Json;
using SnakeGame;

namespace GameWorld
{
    public class Snake
    {
        [JsonProperty(PropertyName = "snake")]
        public int ID { get; private set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; private set; }
        [JsonProperty(PropertyName = "body")]
        public List<Vector2D> Body;
        [JsonProperty(PropertyName = "dir")]
        public Vector2D Dir;
        [JsonProperty(PropertyName = "score")]
        public int Score { get; private set; }
        [JsonProperty(PropertyName = "died")]
        public bool Died { get; private set; } = false;
        [JsonProperty(PropertyName = "alive")]
        public bool Alive { get; private set; } = true;
        [JsonProperty(PropertyName = "dc")]
        public bool Disconnected { get; private set; } = false;
        [JsonProperty(PropertyName = "join")]
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