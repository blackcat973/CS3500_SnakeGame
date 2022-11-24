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
            SnakePlayers= new Dictionary<int, Snake>();
            PowerUps = new Dictionary<int, PowerUp>();
            Walls = new Dictionary<int, Wall>();
            Size = _size;
            WallCreator();
        }

        private void WallCreator()
        {
            Walls.Add(0, new Wall(0, -975.0, -975.0, 975.0, -975.0));
            Walls.Add(1, new Wall(1, -975.0, -975.0, -975.0, 975.0));
            Walls.Add(2, new Wall(2, 975.0, 975.0, 975.0, -975.0));
            Walls.Add(3, new Wall(3, 975.0, 975.0, -975.0, 975.0));

            Walls.Add(4, new Wall(4, -425.0, -425.0, -275.0, -425.0));
            Walls.Add(5, new Wall(5, -425.0, -425.0, -425.0, -275.0));
            Walls.Add(6, new Wall(6, 425.0, -425.0, 275.0, -425.0));
            Walls.Add(7, new Wall(7, 425.0, -425.0, 425.0, -275.0));
            Walls.Add(8, new Wall(8, 425.0, 425.0, 275.0, 425.0));
            Walls.Add(9, new Wall(9, 425.0, 425.0, 425.0, 275.0));
            Walls.Add(10, new Wall(10, -425.0, 425.0, -275.0, 425.0));
            Walls.Add(11, new Wall(11, -425.0, 425.0, -425.0, 275.0));

            Walls.Add(12, new Wall(12, -50.0, -275.0, 50.0, -275.0));
            Walls.Add(13, new Wall(13, -50.0, 275.0, 50.0,275.0));

            Walls.Add(14, new Wall(14, -225.0, -50.0, -225.0, 50.0));
            Walls.Add(15, new Wall(15,225.0, -50.0,225.0, 50.0));

            Walls.Add(16, new Wall(16, -975.0, -200.0, -775.0, -200.0));
            Walls.Add(17, new Wall(17, -975.0, 200.0, -775.0, 200.0));

            Walls.Add(18, new Wall(18, -775.0, 200.0, -775.0, 75.0));
            Walls.Add(19, new Wall(19, -775.0, -200.0, -775.0, -75.0));

            Walls.Add(20, new Wall(20, 975.0, -200.0, 775.0, -200.0));
            Walls.Add(21, new Wall(21, 975.0, 200.0, 775.0, 200.0));

            Walls.Add(22, new Wall(22, 775.0, 200.0, 775.0, 75.0));
            Walls.Add(23, new Wall(23, 775.0, -200.0, 775.0, -75.0));
        }
    }
}
