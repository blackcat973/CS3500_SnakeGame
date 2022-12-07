/**
 * This code is from GameLab\Models\World.cs, just for simple code
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameWorld
{
    public class World
    {
        public Dictionary<long, Snake> SnakePlayers;
        public Dictionary<long, PowerUp> PowerUps;
        public Dictionary<long, Wall> Walls;
        public int Size { get; private set; }

        public World(int _size)
        {
            SnakePlayers = new Dictionary<long, Snake>();
            PowerUps = new Dictionary<long, PowerUp>();
            Walls = new Dictionary<long, Wall>();
            Size = _size;
        }
    }
}