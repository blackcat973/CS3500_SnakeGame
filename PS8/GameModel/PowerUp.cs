/**
 * This code is from GameLab\Models\Powerup.cs, just for simple code
 */
using Newtonsoft.Json;
using SnakeGame;

namespace GameWorld
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PowerUp
    {
        [JsonProperty(PropertyName = "power")]
        public int Power;

        [JsonProperty(PropertyName = "loc")]
        public Vector2D Location;

        [JsonProperty(PropertyName = "died")]
        public bool Died { get; private set; } = false;

        public PowerUp(int id, int x, int y)
        {
            Power = id;
            Location = new Vector2D(x, y);
        }
        //public void Step(double x, double y)
        //{
            
        //}

        //public bool Collision(double x, double y)
        //{
        //    if (Math.Sqrt(Location.X * x + Location.Y * y) <= 5)
        //        return true;

        //    return false;
        //}
    }
}