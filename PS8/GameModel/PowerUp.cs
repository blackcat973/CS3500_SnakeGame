/**
 * This code is from GameLab\Models\Powerup.cs, just for simple code
 */
using Newtonsoft.Json;
using SnakeGame;

namespace GameWorld
{
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
    }
}