/**
 * This code is from GameLab\Models\Player.cs, just for simple code
 */

using Newtonsoft.Json;
using SnakeGame;

namespace GameWorld
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Snake
    {
        //snake's unique ID
        [JsonProperty(PropertyName = "snake")]
        public long UniqueID { get; private set; }
        //player's name
        [JsonProperty(PropertyName = "name")]
        public string Name { get; private set; }
        [JsonProperty(PropertyName = "body")]
        public List<Vector2D> Body { get; private set; }
        [JsonProperty(PropertyName = "dir")]
        public Vector2D Dir { get;  set; }
        [JsonProperty(PropertyName = "score")]
        public int Score { get; set; }
        [JsonProperty(PropertyName = "died")]
        public bool Died { get; set; } = false;
        [JsonProperty(PropertyName = "alive")]
        public bool Alive { get; set; } = true;
        [JsonProperty(PropertyName = "dc")]
        public bool Disconnected { get; private set; } = false;
        [JsonProperty(PropertyName = "join")]
        public bool Join { get; private set; } = false;

        public bool DirChange { get; set; } = false;
        public string currDIR { get; set; }

        private int frameCount = -1;

        public bool getScore { get; set; } = false;

        public Snake(long id, string name, int score)
        {
            this.UniqueID = id;
            this.Name = name;
            this.Body = new List<Vector2D>();
            this.Dir = new Vector2D(0, -1);
            this.Score = score;
            this.currDIR = "up";
        }

        public void Count()
        {
            frameCount += 1;
            if (frameCount == 12)
            {
                getScore = false;
                frameCount = -1;
            }
        }

        public void setDIR(string dir)
        {
            if (dir.Equals("up") && !currDIR.Equals("down") && !currDIR.Equals("up"))
            {
                Dir = new Vector2D(0, -1);
                currDIR = "up";
                DirChange = true;
            }
            else if (dir.Equals("down") && !currDIR.Equals("up") && !currDIR.Equals("down"))
            {
                Dir = new Vector2D(0, 1);
                currDIR = "down";
                DirChange = true;
            }
            else if (dir.Equals("left") && !currDIR.Equals("right") && !currDIR.Equals("left"))
            {
                Dir = new Vector2D(-1, 0);
                currDIR = "left";
                DirChange = true;
            }
            else if (dir.Equals("right") && !currDIR.Equals("left") && !currDIR.Equals("right"))
            {
                Dir = new Vector2D(1, 0);
                currDIR = "right";
                DirChange = true;
            }
            else
                return;
        }

        public bool GetScore()
        {
            this.Score += 1;
            return getScore = true;
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
                else if (Body.Last().X <= -worldsize / 2)
                {
                    Body.Add(Body.Last());
                    Body.Last().X = worldsize / 2;
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

                Vector2D tailDir = (Body[1] - Body[0]);

                tailDir.Normalize();

                if(!getScore)
                    this.Body[0] += tailDir * velocity;

                if(bodyCount != 2)
                {
                    if (Body[1].Equals(Body[0]))
                        Body.Remove(Body.First());
                }
            }
        }
    }
}