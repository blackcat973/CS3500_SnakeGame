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

namespace Server
{
    public class Server
    {
        private Dictionary<long, SocketState> clients;

        private static World? theWorld;

        private float _snakeSpeed = 3;
        private int _startingLength = 120;
        private int _snakeGrowth = 12;
        private int _maxSnake = 50;
        private int _maxPowerups = 20;
        private int _maxPowerupDelay = 200;

        private static int _universeSize = -1;
        private int timePerFrame = -1;
        private int framesPerShot = -1;
        private int respawnDelay = -1;

        public static void Main(string[] args)
        {
            Server server = new Server();
            server.StartServer();

            ReadXml();

            Thread t = new Thread(server.Run);

            t.Start();

            Console.Read();
        }

        public Server()
        {   
            clients = new Dictionary<long, SocketState>();
        }

        public void Run()
        {
            // Start a new timer to control the frame rate
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            while (true)
            {
                // wait until the next frame
                while (watch.ElapsedMilliseconds < timePerFrame)
                { /* empty loop body */ }

                watch.Restart();

                // 여기가 맞는 위치인진 모르겟음

                //Update();

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

        public void StartServer()
        {
            Networking.StartServer(NewClientConnected, 11000);

            Console.WriteLine("Server is running. Accepting clients");
        }

        private void NewClientConnected(SocketState state)
        {
            if (state.ErrorOccurred)
                return;

            // Client 0 connected console line 적어야 될듯
            //Accepted new connection.
            // New Thread creating

            state.OnNetworkAction = ReceiveName;

            Networking.GetData(state);
        }

        private void ReceiveName(SocketState state)
        {
            if(state.ErrorOccurred)
            {
                RemoveClient(state.ID);
                return;
            }

            // The server must allow for additional clients to connect at any time, and add them to its list of clients.
            // Lock(state)가 필요한지 체크해야됨
            lock (state)
            {
                string playerName = state.GetData();
                string[] parts = Regex.Split(playerName, @"(?<=[\n])");

                foreach(string p in parts)
                {
                    if (p.Length == 0)
                        continue;
                    // The regex splitter will include the last string even
                    // if it doesn't end with a '\n',
                    // So we need to ignore it if this happens.
                    if (p[p.Length - 1] != '\n')
                        break;
                    // Then remove it from the SocketState's growable buffer

                    state.RemoveData(0, p.Length);

                    // Snake생성 - 고유ID에 맞는 자리에 ADD
                    Snake sp = new Snake(state.ID, p, 0); // p = playerName
                    // Body를 생성해줘야함. Method따로? based on 
                    theWorld.SnakePlayers.Add(state.ID, sp);
                }

                //ReceiveCommand(state);

                state.OnNetworkAction = ReceiveCommand;


                // ??? - inside or outside of this lock branket?
                lock (clients)
                {
                    // Save clinet information into the clients list.
                    clients[state.ID] = state;

                    Networking.Send(state.TheSocket, state.ID.ToString() + "\n" + _universeSize.ToString() + "\n");
                    foreach(Wall w in theWorld.Walls.Values)
                        Networking.Send(state.TheSocket, JsonConvert.SerializeObject(w) + "\n");
                    foreach (PowerUp p in theWorld.PowerUps.Values)
                        Networking.Send(state.TheSocket, JsonConvert.SerializeObject(p) + "\n");
                    foreach (Snake s in theWorld.SnakePlayers.Values)
                        Networking.Send(state.TheSocket, JsonConvert.SerializeObject(s) + "\n");

                    // ***********************************************************************************************
                    // ***********************************************************************************************
                    // ***After this point, the server begins sending the new client the world state on each frame.***
                    // ***********************************************************************************************
                    // ***********************************************************************************************
                }
            }

            Networking.GetData(state);
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

                if(obj.ContainsKey("moving"))
                {

                    string? direction = JsonConvert.DeserializeObject<string>(obj.ToString());
                    if(direction is not null && theWorld is not null)
                    {
                        if (direction.Equals("up"))
                            theWorld.SnakePlayers[state.ID].Dir = new SnakeGame.Vector2D(0f, 1f);
                        else if (direction.Equals("down"))
                            theWorld.SnakePlayers[state.ID].Dir = new SnakeGame.Vector2D(0f, -1f);
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

        private void RemoveClient(long id)
        {
            Console.WriteLine("Client " + id + " disconnected");
            lock (clients)
            {
                clients.Remove(id);
            }
        }

        public static void ReadXml()
        {
            DataContractSerializer ser = new(typeof(GameSettings));

            XmlReader reader = XmlReader.Create(@"..\..\..\settings.xml");

            GameSettings gs = (GameSettings)ser.ReadObject(reader);

            CreateWorld(gs.UniverseSize);

            //Create the wall.
            foreach (Wall wall in gs.Walls)
            {
                theWorld.Walls.Add(wall.WallID, wall);
            }

            //using (FileStream fileStream = new FileStream(@"..\..\..\settings.xml", FileMode.Open))
            //{
            //    GameSettings result = (GameSettings)serializer.Deserialize(fileStream);
            //}

            //XmlDocument doc = new XmlDocument();
            //doc.Load(@"..\..\..\settings.xml");

            //foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            //{
            //    string text = node.InnerText; //or loop through its children as well
            //    Console.WriteLine(text);
            //}
            ////https://stackoverflow.com/questions/642293/how-do-i-read-and-parse-an-xml-file-in-c
            //foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            //{
            //    if (node.Name.Equals("FramesPerShot"))
            //    {
            //        framePerShot = Convert.ToInt32(node.InnerText);
            //    }
            //    else if (node.Name.Equals("MSPerFrame"))
            //    {
            //        msPerFrame = Convert.ToInt32(node.InnerText);
            //    }z
            //    else if (node.Name.Equals("RespawnRate"))
            //    {
            //        respawnRate = Convert.ToInt32(node.InnerText);
            //    }
            //    else if (node.Name.Equals("UniverseSize"))
            //    {
            //        size = Convert.ToInt32(node.InnerText);
            //    }
            //    else if (node.Name.Equals("Walls"))
            //    {
            //        foreach (XmlNode node2 in node.ChildNodes)
            //        {
            //            foreach (XmlNode node3 in node2.ChildNodes)
            //            {
            //                Console.WriteLine(node3.InnerText);
            //            }
            //        }
            //    }
            //}
        }

        public static void CreateWorld(int worldSize)
        {
            _universeSize = worldSize;
            theWorld = new World(worldSize);
        }

        public void Update()
        {
            IEnumerable<long> playersToRemove = theWorld.SnakePlayers.Values.Where(x => x.Disconnected).Select(x => x.UniqueID);
            IEnumerable<int> powsToRemove = theWorld.PowerUps.Values.Where(x => x.Died).Select(x => x.Power);
            foreach( int i in playersToRemove)
            {
                theWorld.SnakePlayers.Remove(i);
            }
            foreach (int i in powsToRemove)
            {
                theWorld.PowerUps.Remove(i);
            }

            foreach( Snake s in theWorld.SnakePlayers.Values)
            {
                s.Step(_snakeSpeed);
            }
            foreach( PowerUp p in theWorld.PowerUps.Values)
            {
            }
        }

        //public void Connect()
        //{
        //    //start an event-loop that listens for TCP connections from clients. When a new client
        //    //connects, the server must implement the server-side of the handshake in the communication
        //    //protocol described in PS8
        //    //add them to its list of clients.

        //    //handshake.

        //    //Server Architecture

        //    /* 1. Client connects change Action, 
        //     *    ask for data.
        //        2. Client's name received
        //            Send startup info
        //            save SocketState
        //            Change Action, ask for data.
        //        3. Command received
        //    */

        //    // 38 : 00
        //    //new thread calls
        //    //change the Action and it asks the clients for data.
        //    //Save socket state for client.

        //    //multiple clients

        //    /*
        //     * while(true) -> updateworld() -> foreach client send world
        //     * one frame.
        //     */

        //    //When a new connection is made, ClientConnects will be called.
        //    Networking.StartServer(ClientConnects, 11000);



        //}

        //public void ClientConnects(SocketState socketState)
        //{
        //    socketState.OnNetworkAction =
        //}

        //public void UpdateWorld()
        //{

        //}
    }
}