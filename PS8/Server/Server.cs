/**
 * Definitely modify this length of the snake
 * Random -> 벽이랑 안겹치는곳, Power안겹치게
 * PowerUP -> Respawn
 * 
 * 1. Snake body 좌표를 이용해서 p1, p2 가 주어질 경우 가운데 120 unit만큼의 List를 채워줘야함
 *    p1, p2 좌표를 우리가 찍어줘야함.
 *    PS8 디버그하면서 Snake의 Body List를 직접 보는게 좋을듯
 * 2. Snake 모델 쪽 수정해야할듯 ex) Join -> 들어왔을 때 True
 * 3. PowerUp 좌표는 랜덤. 랜덤발생 -> GameLab 랜덤하게 생성하게 만드는 함수
 * 4. Console.Read(); <- OnReceive
 * 
 * 12/08
 * Collision Snake <-> Wall
 * Collision Snake <-> Powerup
 */

using System.Xml.Linq;
using System.Xml;
using System.Runtime.CompilerServices;
using GameWorld;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using NetworkUtil;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using SnakeGame;
using System.Drawing;
using System.Numerics;

namespace Server
{
    public class Server
    {
        private Dictionary<long, SocketState> clients;

        private static World? theWorld;
        private Random rand = new();

        private float _snakeSpeed = 3;
        private int _startingLength = 120;
        private int _snakeGrowth = 12;
        private int _maxSnake = 50;
        private int _maxPowerups = 20;
        private int _maxPowerupDelay = 200;

        private static int _universeSize;
        private static long _timePerFrame;
        private static long _framesPerShot;
        private static long _respawnDelay;

        public static void Main(string[] args)
        {
            Server server = new Server();

            ReadAndSet();

            server.StartServer();

            Console.Read();
        }

        public Server()
        {
            clients = new Dictionary<long, SocketState>();
        }

        public void StartServer()
        {
            //we need to put here or in NewClientConnected.

            Networking.StartServer(NewClientConnected, 11000);



            Console.WriteLine("Server is running. Accepting clients");
        }

        private void NewClientConnected(SocketState state)
        {
            //https://stackoverflow.com/questions/363377/how-do-i-run-a-simple-bit-of-code-in-a-new-thread
            new Thread(() =>
            {
                if (state.ErrorOccurred)
                    return;

                // Client 0 connected console line 적어야 될듯
                //Accepted new connection.
                // New Thread creating

                state.OnNetworkAction = ReceiveName;

                Networking.GetData(state);
            }).Start();

        }


        private void ReceiveName(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                RemoveClient(state.ID);
                return;
            }

            // The server must allow for additional clients to connect at any time, and add them to its list of clients.
            // Lock(state)가 필요한지 체크해야됨
            lock (state)
            {
                string playerName = state.GetData();

                string replacement = Regex.Replace(playerName, @"\t|\n|\r", "");

                //Create the new Snake
                Snake playerSnake = new Snake(state.ID, replacement, 0); // p = playerName

                Vector2D vector = new Vector2D(-299.54809474945068, 705.3624391555786);
                Vector2D vector2 = new Vector2D(-299.5480947494507, 585.3624391555786);

                playerSnake.Body.Add(vector);
                playerSnake.Body.Add(vector2);
                // Body를 생성해줘야함. Method따로? based on 
                theWorld.SnakePlayers.Add(state.ID, playerSnake);

                state.RemoveData(0, playerName.Length);

                // ??? - inside or outside of this lock branket?

                // Save clinet information into the clients list.
                clients[state.ID] = state;
                ////Send the client a unique player ID and the world size
                //Networking.Send(state.TheSocket, state.ID.ToString() + "\n" + _universeSize.ToString() + "\n");
                ////Send the JSON walls

                //foreach (PowerUp p in theWorld.PowerUps.Values)
                //    Networking.Send(state.TheSocket, JsonConvert.SerializeObject(p) + "\n");

                //foreach (Snake s in theWorld.SnakePlayers.Values)
                //    Networking.Send(state.TheSocket, JsonConvert.SerializeObject(s) + "\n");

                HashSet<long> disconnectedClients = new HashSet<long>();
                lock (clients)
                {
                    foreach (SocketState client in clients.Values)
                    {
                        if (!Networking.Send(client.TheSocket!, state.ID + "\n" + _universeSize + "\n"))
                            disconnectedClients.Add(client.ID);
                    }

                    foreach (Wall w in theWorld.Walls.Values)
                        Networking.Send(state.TheSocket, JsonConvert.SerializeObject(w) + "\n");

                }


                foreach (long id in disconnectedClients)
                    RemoveClient(id);

                // ***********************************************************************************************
                // ***********************************************************************************************
                // ***After this point, the server begins sending the new client the world state on each frame.***
                // ***********************************************************************************************
                // ***********************************************************************************************
            }

            state.OnNetworkAction = ReceiveCommand;

            //Do we have to call?
            Run();

            Networking.GetData(state);
        }

        public void Run()
        {

            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();



            while (true)
            {
                // wait until the next frame
                while (watch.ElapsedMilliseconds < _timePerFrame)
                { }

                watch.Restart();
                //if(theWorld.PowerUps.Count< 20)
                //{
                //    _maxPowerupDelay
                //}

                //Update the world.
                Update();

                //send the data to the network
                lock (clients)
                {
                    foreach (SocketState c in clients.Values)
                    {
                        if (c.TheSocket.Connected)
                        {
                            foreach (Snake s in theWorld.SnakePlayers.Values)
                                Networking.Send(c.TheSocket, JsonConvert.SerializeObject(s) + "\n");
                            foreach (PowerUp p in theWorld.PowerUps.Values)
                                Networking.Send(c.TheSocket, JsonConvert.SerializeObject(p) + "\n");
                        }
                    }
                }
            }
        }

        private void ReceiveCommand(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                RemoveClient(state.ID);
                return;
            }
            string commandData = state.GetData();
            try
            {
                JObject obj = JObject.Parse(commandData);

                if (obj.ContainsKey("moving"))
                    if (obj.ContainsKey("moving"))
                    {

                        string? direction = JsonConvert.DeserializeObject<string>(obj.ToString());
                        if (direction is not null && theWorld is not null)
                            if (direction is not null && theWorld is not null)
                            {

                                //155 155-3
                                if (direction.Equals("up"))
                                    theWorld.SnakePlayers[state.ID].Dir = new SnakeGame.Vector2D(0f, 1f);
                                else if (direction.Equals("down"))
                                    theWorld.SnakePlayers[state.ID].Dir = new SnakeGame.Vector2D(0f, -1f);
                                else if (direction.Equals("down"))
                                    theWorld.SnakePlayers[state.ID].Dir = new SnakeGame.Vector2D(0f, 1f);
                                else if (direction.Equals("left"))
                                    theWorld.SnakePlayers[state.ID].Dir = new SnakeGame.Vector2D(-1f, 0f);
                                else if (direction.Equals("up"))
                                    theWorld.SnakePlayers[state.ID].Dir = new SnakeGame.Vector2D(1f, 0f);
                            }
                    }
            }
            catch (Exception e)
            {
            }
            Networking.GetData(state);
        }

        private bool checkSWCollision(Snake s, Wall w)
        {
            double xUpperVertex = 0;
            double xlowerVertex = 0;
            double yUpperVertex = 0;
            double ylowerVertex = 0;


            if (w.Point1.X == w.Point2.X)
            {
                xUpperVertex = w.Point1.X + 25.0;
                xlowerVertex = w.Point1.X - 25.0;

                if (w.Point1.Y > w.Point2.Y)
                {
                    yUpperVertex = w.Point1.Y + 25.0;
                    ylowerVertex = w.Point2.Y - 25.0;
                }
                else
                {
                    yUpperVertex = w.Point2.Y + 25.0;
                    ylowerVertex = w.Point1.Y - 25.0;
                }
            }
            else if (w.Point1.Y == w.Point2.Y)
            {
                yUpperVertex = w.Point1.Y + 25.0;
                ylowerVertex = w.Point1.Y - 25.0;

                if (w.Point1.Y > w.Point2.Y)
                {
                    xUpperVertex = w.Point1.X + 25.0;
                    xlowerVertex = w.Point2.X - 25.0;
                }
                else
                {
                    xUpperVertex = w.Point2.X + 25.0;
                    xlowerVertex = w.Point1.X - 25.0;
                }
            }

            if ( s.Body.Last().X >= xlowerVertex && s.Body.Last().X <= xUpperVertex)
                if ( s.Body.Last().Y >= ylowerVertex && s.Body.Last().Y <= yUpperVertex)
                    return true;

            return false;
        }

        private bool checkSPCollision(Snake s, PowerUp o)
        {
            double SnakePointLength = (o.Location - s.Body.Last()).Length();

            if (SnakePointLength < 15.0)
                return true;

            return false;
        }

        private bool checkSSCollision(Snake s, Wall o)
        {


            return false;
        }

        //private void ReceiveCommand(SocketState state)
        //{
        //    if (state.ErrorOccurred)
        //    {
        //        RemoveClient(state.ID);
        //        return;
        //    }
        //    string commandData = state.GetData();

        //    try
        //    {
        //        JObject obj = JObject.Parse(commandData);

        //        if(obj.ContainsKey("moving"))
        //        {

        //            string? direction = JsonConvert.DeserializeObject<string>(obj.ToString());
        //            if(direction is not null && theWorld is not null)
        //            {
        //                if (direction.Equals("up"))
        //                    theWorld.SnakePlayers[state.ID].Dir = new SnakeGame.Vector2D(0f, 1f);
        //                else if (direction.Equals("down"))
        //                    theWorld.SnakePlayers[state.ID].Dir = new SnakeGame.Vector2D(0f, -1f);
        //                else if (direction.Equals("left"))
        //                    theWorld.SnakePlayers[state.ID].Dir = new SnakeGame.Vector2D(-1f, 0f);
        //                else if (direction.Equals("up"))
        //                    theWorld.SnakePlayers[state.ID].Dir = new SnakeGame.Vector2D(1f, 0f);
        //            }
        //        }
        //    }
        //    catch (Exception e) 
        //    {

        //    }

        //    Networking.GetData(state);
        //}

        private void RemoveClient(long id)
        {
            Console.WriteLine("Client " + id + " disconnected");
            lock (clients)
            {
                clients.Remove(id);
            }
        }

        public void Update()
        {

            int nextPlayerID = 0;

            // cleanup the deactivated objects
            IEnumerable<long> playersToRemove = theWorld.SnakePlayers.Values.Where(x => x.Disconnected).Select(x => x.UniqueID);
            IEnumerable<int> powsToRemove = theWorld.PowerUps.Values.Where(x => x.Died).Select(x => x.Power);

            foreach (int i in playersToRemove)
                theWorld.SnakePlayers.Remove(i);
            foreach (int i in powsToRemove)
                theWorld.SnakePlayers.Remove(i);

            // move/update the existing objects in the world
            foreach (Snake s in theWorld.SnakePlayers.Values)
            {
                s.Step(_snakeSpeed, _universeSize);
                foreach (Wall w in theWorld.Walls.Values)
                    if (checkSWCollision(s, w))
                    {
                        s.Alive = false;
                        s.Died = true;
                    }                      

            }

            //foreach (PowerUp p in theWorld.PowerUps.Values)
            //    p.Step();
        }

        public static void ReadAndSet()
        {
            DataContractSerializer ser = new(typeof(GameSettings));

            XmlReader reader = XmlReader.Create(@"..\..\..\settings.xml");

            GameSettings gameSettings = (GameSettings)ser.ReadObject(reader);

            _universeSize = gameSettings.UniverseSize;
            _framesPerShot = gameSettings.FramesPerShot;
            _respawnDelay = gameSettings.RespawnRate;
            _timePerFrame = gameSettings.MSPerFrame;

            theWorld = new World(gameSettings.UniverseSize);

            foreach (Wall wall in gameSettings.Walls)
            {
                theWorld.Walls.Add(wall.WallID, wall);
            }
        }
    }
}