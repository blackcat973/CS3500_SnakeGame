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
        public bool Died { get; set; } = false;
        [JsonProperty(PropertyName = "alive")]
        public bool Alive { get; set; } = true;
        [JsonProperty(PropertyName = "dc")]
        public bool Disconnected { get; private set; } = false;
        [JsonProperty(PropertyName = "join")]
        public bool Join { get; private set; } = false;

        public bool DirChange { get; set; } = false;
        public string currDir { get; set; }

        public Snake(long id, string name, int score)
        {
            this.UniqueID = id;
            this.Name = name;
            this.Body = new List<Vector2D>();
            this.Dir = new Vector2D(0, -1);
            this.Score = score;
            this.currDir = "up";
        }

        public void Step(float velocity, int worldsize)
        {
            if (Alive)
            {
                if (DirChange)
                {
                    Body.Add(Body.Last());
                    DirChange = false;
                }

                // Wraparound
                if (Body.Last().X >= worldsize / 2)
                {
                    Body.Add(Body.Last());
                    Body.Last().X = -worldsize / 2;
                    Body.Add(Body.Last());
                }
                else if (Body.Last().Y >= worldsize / 2)
                {
                    Body.Add(Body.Last());
                    Body.Last().Y = -worldsize / 2;
                    Body.Add(Body.Last());
                }
                else if (Body.Last().Y <= -worldsize / 2)
                {
                    Body.Add(Body.Last());
                    Body.Last().Y = worldsize / 2;
                    Body.Add(Body.Last());
                }
                else if (Body.Last().Y <= -worldsize / 2)
                {
                    Body.Add(Body.Last());
                    Body.Last().Y = worldsize / 2;
                    Body.Add(Body.Last());
                }

                int bodyCount = this.Body.Count;

                this.Body[bodyCount-1] += this.Dir * velocity;
                
                this.Body[0] += this.Dir * velocity;

                if(bodyCount != 2)
                {
                    if (Body[1] == Body[0])
                        Body.Remove(Body.First());
                }
            }
        }
    }
}