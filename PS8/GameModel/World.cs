using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameWorld
{
    public class World
    {
        public Dictionary<int, Snake> SnakePlayers;
        public Dictionary<int, PowerUp> PowerUps;
        public Dictionary<int, Wall> Walls;
        public int Size { get; private set; }

        public World(int _size)
        {
            SnakePlayers = new Dictionary<int, Snake>();
            PowerUps = new Dictionary<int, PowerUp>();
            Walls = new Dictionary<int, Wall>();
            Size = _size;
        }


    }
}