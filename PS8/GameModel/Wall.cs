/**
 * This code is from GameLab\Models\Wall.cs, just for simple code
 */
using Newtonsoft.Json;
using SnakeGame;
using System.Numerics;
using System.Runtime.Serialization;

namespace GameWorld
{
    [DataContract(Namespace ="")]
    [JsonObject(MemberSerialization.OptIn)]
    public class Wall
    {
        [DataMember(Name = "ID")]
        [JsonProperty(PropertyName = "wall")]
        public int WallID { get; private set; }
        [DataMember(Name = "p1")]
        [JsonProperty(PropertyName = "p1")]
        public Vector2D Point1 { get; private set; }
        [DataMember(Name = "p2")]
        [JsonProperty(PropertyName = "p2")]
        public Vector2D Point2 { get; private set; }

        public Wall(int id, int x, int y)
        {
            WallID = id;
            Point1 = new Vector2D(x, y);
            Point2 = new Vector2D(x, y);
        }
    }
}