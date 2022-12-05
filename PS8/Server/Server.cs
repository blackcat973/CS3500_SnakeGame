
using GameWorld;
using System.Xml;
using System.Xml.Linq;

namespace GameServer
{
    public class Server
    {
        private World theWorld;

        public delegate void ServerUpdateHandler(IEnumerable<Snake> snakes, IEnumerable<Wall> walls, IEnumerable<PowerUp> powerups);
        public event ServerUpdateHandler ServerUpdate;

        private static long msPerFrame = 0;
        private static long framesPerShot = 0;
        private static int size;

        private int maxPlayers = 0;
        private int maxPowerups = 0;

        private int nextPlayerID = 0;
        private int nextWallID = 0;
        private int nextPowerupID = 0;

        private class GameSettings
        {
            private long framesPerShot { get; set; }
            private static long respawnRate { get; set; }
            private static long msPerFrame { get; set; }

            public GameSettings()
            {

            }
        }
    }
}