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

        public Wall(int id, double x, double y)
        {
            WallID = id;
            Point1 = new Vector2D(x, y);
            Point2 = new Vector2D(x, y);
        }

        //public bool Collsion(double x, double y)
        //{
        //    double xUpperVertex = 0;
        //    double xlowerVertex = 0;
        //    double yUpperVertex = 0;
        //    double ylowerVertex = 0;


        //    if (Point1.X == Point2.X)
        //    {
        //        xUpperVertex = Point1.X + 25.0;
        //        xlowerVertex = Point1.X - 25.0;

        //        if (Point1.Y > Point2.Y)
        //        {
        //            yUpperVertex = Point1.Y + 25.0;
        //            ylowerVertex = Point2.Y - 25.0;
        //        }
        //        else
        //        {
        //            yUpperVertex = Point2.Y + 25.0;
        //            ylowerVertex = Point1.Y - 25.0;
        //        }
        //    }

        //    if (Point1.Y == Point2.Y)
        //    {
        //        yUpperVertex = Point1.Y + 25.0;
        //        ylowerVertex = Point1.Y - 25.0;

        //        if (Point1.Y > Point2.Y)
        //        {
        //            xUpperVertex = Point1.X + 25.0;
        //            xlowerVertex = Point2.X - 25.0;
        //        }
        //        else
        //        {
        //            xUpperVertex = Point2.X + 25.0;
        //            xlowerVertex = Point1.X - 25.0;
        //        }
        //    }

        //    if (x >= xlowerVertex && x <= xUpperVertex)
        //        return true;
        //    if (y >= ylowerVertex && y <= yUpperVertex)
        //        return true;

        //    return false;
        //}
    }
}