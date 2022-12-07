/**
 * This code is from GameLab\Models\Player.cs, just for simple code
 */

using Newtonsoft.Json;
using SnakeGame;

namespace GameWorld
{
    public class Snake
    {
        //snake's unique ID
        [JsonProperty(PropertyName = "snake")]
        public long UniqueID { get; private set; }
        //player's name
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

        public Snake(long id, string name, int score)
        {
            this.UniqueID = id;
            this.Name = name;
            this.Body = new List<Vector2D>();
            this.Dir = new Vector2D(1, 0);
            this.Score = score;
        }

        public void Step(float velocity)
        {
            if (Alive)
            {
                Vector2D previousBody = this.Body[0];

                this.Body[0] += this.Dir * velocity;

                for (int i = 1; i < Body.Count; i++)
                {
                    this.Body[i] = previousBody;
                    previousBody = this.Body[i];
                }
            }
        }

        public bool Collsion(object o, double x, double y)
        {
            return false;
        }
    }
}