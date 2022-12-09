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
        private Dictionary<long, string> CommandStore;
        private List<int> respawnFrame;
        private List<int> respawnPowerup;

        private World? theWorld;
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

        static void Main(string[] args)
        {
            Server server = new Server();

            server.ReadAndSet();

            server.StartServer();

            Console.Read();
        }

        public Server()
        {
            CommandStore = new Dictionary<long, string>();
            respawnPowerup = new List<int>() { };
            respawnFrame = new List<int>() { };
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
                {
                    return;
                }

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
                return;
            }

            // The server must allow for additional clients to connect at any time, and add them to its list of clients.
            // Lock(state)가 필요한지 체크해야됨
            lock (theWorld)
            {
                string playerName = state.GetData();

                string replacement = Regex.Replace(playerName, @"\t|\n|\r", "");

                //state.RemoveData(0, playerName.Length);

                //Create the new Snake

                //Snake playerSnake = new Snake(state.ID, replacement, 0); // p = playerName

                //Vector2D vector = new Vector2D(-509.54809474945068, 705.3624391555786);
                //Vector2D vector2 = new Vector2D(-509.5480947494507, 585.3624391555786);

                //playerSnake.Body.Add(vector);
                //playerSnake.Body.Add(vector2);

                
                Snake playerSnake = SnakeRespawn(state.ID, replacement, 0);
                // Body를 생성해줘야함. Method따로? based on 
                theWorld.SnakePlayers.Add(state.ID, playerSnake);
                respawnFrame.Add(0);

                state.RemoveData(0, playerName.Length);

                // ??? - inside or outside of this lock branket?

                // Save clinet information into the clients list.
                CommandStore[state.ID] = playerSnake.currDIR;
                ////Send the client a unique player ID and the world size
                //Networking.Send(state.TheSocket, state.ID.ToString() + "\n" + _universeSize.ToString() + "\n");
                ////Send the JSON walls

                //foreach (PowerUp p in theWorld.PowerUps.Values)
                //    Networking.Send(state.TheSocket, JsonConvert.SerializeObject(p) + "\n");

                //foreach (Snake s in theWorld.SnakePlayers.Values)
                //    Networking.Send(state.TheSocket, JsonConvert.SerializeObject(s) + "\n");

                lock (state)
                {
                    Networking.Send(state.TheSocket!, state.ID + "\n" + _universeSize + "\n");
                            

                    foreach (Wall w in theWorld.Walls.Values)
                        Networking.Send(state.TheSocket, JsonConvert.SerializeObject(w) + "\n");
                }

                // ***********************************************************************************************
                // ***********************************************************************************************
                // ***After this point, the server begins sending the new client the world state on each frame.***
                // ***********************************************************************************************
                // ***********************************************************************************************
            }

            state.OnNetworkAction = ReceiveCommand;

            //Do we have to call?
            Run(state);
        }

        public void Run(SocketState state)
        {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            while (true)
            {
                if (state.ErrorOccurred)
                {
                    Console.WriteLine("Client " + state.ID + "are disconnected.");
                    return;
                }

                // wait until the next frame
                while (watch.ElapsedMilliseconds < _timePerFrame)
                {  }

                watch.Restart();

                //if(theWorld.PowerUps.Count< 20)
                //{
                //    _maxPowerupDelay
                //}

                //Update the world.
                lock (theWorld)
                {
                    Update(state);
                }
                //send the data to the network
                lock (theWorld)
                {
                    if (state.TheSocket.Connected)
                    {
                        foreach (Snake s in theWorld.SnakePlayers.Values)
                            Networking.Send(state.TheSocket, JsonConvert.SerializeObject(s) + "\n");
                        foreach (PowerUp p in theWorld.PowerUps.Values)
                            Networking.Send(state.TheSocket, JsonConvert.SerializeObject(p) + "\n");
                    }
                }
                Networking.GetData(state);
            }
        }

        private void ReceiveCommand(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                return;
            }

            lock (state)
            {
                string commandData = state.GetData();

                state.RemoveData(0, commandData.Length);

                try
                {
                    JObject obj = JObject.Parse(commandData);

                    JToken move = obj["moving"].ToString();

                    // 오류나는 부분
                    if (move is not null && theWorld is not null)
                    {
                        if (move.ToString().Equals("none") && !CommandStore[state.ID].Equals("none"))
                            return;
                        else
                        {
                            CommandStore[state.ID] = move.ToString();
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
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

                if (w.Point1.X > w.Point2.X)
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

            if (s.Body.Last().X >= xlowerVertex && s.Body.Last().X <= xUpperVertex)
                if (s.Body.Last().Y >= ylowerVertex && s.Body.Last().Y <= yUpperVertex)
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

        private bool checkSSCollision(Snake s, Snake s2)
        {
            for (int i = 0; i < s2.Body.Count - 1; i++)
            {
                double xUpperVertex = 0;
                double xlowerVertex = 0;
                double yUpperVertex = 0;
                double ylowerVertex = 0;


                if (s2.Body[i].X == s2.Body[i + 1].X)
                {
                    xUpperVertex = s2.Body[i].X + 5.0;
                    xlowerVertex = s2.Body[i].X - 5.0;

                    if (s2.Body[i].Y > s2.Body[i + 1].Y)
                    {
                        yUpperVertex = s2.Body[i].Y + 5.0;
                        ylowerVertex = s2.Body[i + 1].Y - 5.0;
                    }
                    else
                    {
                        yUpperVertex = s2.Body[i + 1].Y + 5.0;
                        ylowerVertex = s2.Body[i].Y - 5.0;
                    }
                }
                else if (s2.Body[i].Y == s2.Body[i + 1].Y)
                {
                    yUpperVertex = s2.Body[i].Y + 5.0;
                    ylowerVertex = s2.Body[i].Y - 5.0;

                    if (s2.Body[i].X > s2.Body[i + 1].X)
                    {
                        xUpperVertex = s2.Body[i].X + 5.0;
                        xlowerVertex = s2.Body[i + 1].X - 5.0;
                    }
                    else
                    {
                        xUpperVertex = s2.Body[i + 1].X + 5.0;
                        xlowerVertex = s2.Body[i].X - 5.0;
                    }
                }

                if (s.Body.Last().X >= xlowerVertex && s.Body.Last().X <= xUpperVertex)
                    if (s.Body.Last().Y >= ylowerVertex && s.Body.Last().Y <= yUpperVertex)
                        return true;
            }

            return false;
        }

        private bool checkSSSelfCollision(Snake s, Snake s2)
        {
            if (s.Body.Count >= 5)
            {
                for (int i = 0; i < s2.Body.Count - 3; i++)
                {
                    double xUpperVertex = 0;
                    double xlowerVertex = 0;
                    double yUpperVertex = 0;
                    double ylowerVertex = 0;


                    if (s2.Body[i].X == s2.Body[i + 1].X)
                    {
                        xUpperVertex = s2.Body[i].X + 5.0;
                        xlowerVertex = s2.Body[i].X - 5.0;

                        if (s2.Body[i].Y > s2.Body[i + 1].Y)
                        {
                            yUpperVertex = s2.Body[i].Y + 5.0;
                            ylowerVertex = s2.Body[i + 1].Y - 5.0;
                        }
                        else
                        {
                            yUpperVertex = s2.Body[i + 1].Y + 5.0;
                            ylowerVertex = s2.Body[i].Y - 5.0;
                        }
                    }
                    else if (s2.Body[i].Y == s2.Body[i + 1].Y)
                    {
                        yUpperVertex = s2.Body[i].Y + 5.0;
                        ylowerVertex = s2.Body[i].Y - 5.0;

                        if (s2.Body[i].X > s2.Body[i + 1].X)
                        {
                            xUpperVertex = s2.Body[i].X + 5.0;
                            xlowerVertex = s2.Body[i + 1].X - 5.0;
                        }
                        else
                        {
                            xUpperVertex = s2.Body[i + 1].X + 5.0;
                            xlowerVertex = s2.Body[i].X - 5.0;
                        }
                    }

                    if (s.Body.Last().X >= xlowerVertex && s.Body.Last().X <= xUpperVertex)
                        if (s.Body.Last().Y >= ylowerVertex && s.Body.Last().Y <= yUpperVertex)
                            return true;
                }
            }

            return false;
        }

        private bool checkPWCollision(Vector2D p, Wall w)
        {
            double xUpperVertex = 0;
            double xlowerVertex = 0;
            double yUpperVertex = 0;
            double ylowerVertex = 0;


            if (w.Point1.X == w.Point2.X)
            {
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

                if (w.Point1.X > w.Point2.X)
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

            if (p.X <= xlowerVertex  - 35 && p.X >= xUpperVertex + 35)
                if (p.Y >= ylowerVertex && p.Y <= yUpperVertex)
                    return true;

            return false;
        }

        private Snake SnakeRespawn(long id, string name, int score)
        {
            Snake s = new Snake(id, name, score);
            //int posOrNagX = rand.Next(2);
            //int posOrNagY = rand.Next(2);
            int dir = rand.Next(4);
            double HeadX = 0;
            double HeadY = 0;
            double TailX = 0;
            double TailY = 0;

            bool canRespawn = true;

            while (canRespawn)
            {

                if (rand.Next(2) % 2 == 0)
                    HeadX = rand.Next(950);
                else
                    HeadX = rand.Next(950) * (-1);

                if (rand.Next(2) % 2 == 0)
                    HeadY = rand.Next(950);
                else
                    HeadY = rand.Next(950) * (-1);

                Vector2D p1 = new Vector2D(HeadX, HeadY);

                if (dir % 4 == 0)
                {
                    s.Dir = new Vector2D(0,-1);
                    s.currDIR = "up";
                    TailX = HeadX;
                    TailY = HeadY + _startingLength;
                }
                else if (dir%4 == 1)
                {
                    s.Dir = new Vector2D(0, 1);
                    s.currDIR = "down";
                    TailX = HeadX;
                    TailY = HeadY - _startingLength;
                }
                else if(dir%4 == 2)
                {
                    s.Dir = new Vector2D(-1, 0);
                    s.currDIR = "left";
                    TailY = HeadY;
                    TailX = HeadX + _startingLength;
                }
                else
                {
                    s.Dir = new Vector2D(1, 0);
                    s.currDIR = "right";
                    TailY = HeadY;
                    TailX = HeadX - _startingLength;
                }

                Vector2D p2 = new Vector2D(TailX, TailY);

                s.Body.Add(p2); s.Body.Add(p1);

                foreach (PowerUp p in theWorld.PowerUps.Values)
                    canRespawn = checkSPCollision(s, p);
                foreach (Wall w in theWorld.Walls.Values)
                    canRespawn = checkSWCollision(s, w);
                foreach (Snake snake in theWorld.SnakePlayers.Values)
                    if(!s.Equals(snake))
                        canRespawn = checkSSCollision(s, snake);
            }

            return s;
        }

        private PowerUp powerUpRespawn(int id)
        {
            bool canRespawn = true;

            double HeadX = 0;
            double HeadY = 0;


            while (canRespawn)
            {

                if (rand.Next(2) % 2 == 0)
                    HeadX = rand.Next(950);
                else
                    HeadX = rand.Next(950) * (-1);

                if (rand.Next(2) % 2 == 0)
                    HeadY = rand.Next(950);
                else
                    HeadY = rand.Next(950) * (-1);

                Vector2D v = new Vector2D(HeadX, HeadY);

                foreach (Wall w in theWorld.Walls.Values)
                    canRespawn = checkPWCollision(v, w);
            }

            PowerUp p = new PowerUp(id, HeadX, HeadY);
            return p;
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

        public void Update(SocketState state)
        {

            // cleanup the deactivated objects
            IEnumerable<long> playersToRemove = theWorld.SnakePlayers.Values.Where(x => x.Disconnected).Select(x => x.UniqueID);
            //IEnumerable<int> powsToRemove = theWorld.PowerUps.Values.Where(x => x.Died).Select(x => x.Power);

            foreach (int i in playersToRemove)
                theWorld.SnakePlayers.Remove(i);
            //foreach (int i in powsToRemove)
            //    theWorld.PowerUps.Remove(i);

            //foreach (PowerUp p in theWorld.PowerUps.Values)
            //    p.Step();

            for (int i = 0; i < theWorld.SnakePlayers.Count; i++)
            {
                if (!theWorld.SnakePlayers[i].Alive)
                {
                    respawnFrame[i] += 1;
                    if (respawnFrame[i] == _respawnDelay)
                    {
                        respawnFrame[i] = 0;
                        theWorld.SnakePlayers[i].Alive = true;
                        theWorld.SnakePlayers[i] = SnakeRespawn(theWorld.SnakePlayers[i].UniqueID, theWorld.SnakePlayers[i].Name, 0);
                    }
                }
                theWorld.SnakePlayers[i].setDIR(CommandStore[i]);
            }

            for (int i = 0; i < 20; i++)
            {
                if (theWorld.PowerUps[i].Died)
                {
                    respawnPowerup[i] += 1;
                    if (respawnPowerup[i] == _maxPowerupDelay)
                    {
                        theWorld.PowerUps[i] = powerUpRespawn(i);
                    }
                }
            }

            for (int i = 0; i < theWorld.SnakePlayers.Count; i++)
            {
                theWorld.SnakePlayers[i].setDIR(CommandStore[i]);
            }

            // move/update the existing objects in the world
            theWorld.SnakePlayers[state.ID].Step(_snakeSpeed, _universeSize);

            foreach (Wall w in theWorld.Walls.Values)
            if (checkSWCollision(theWorld.SnakePlayers[state.ID], w))
            {
                theWorld.SnakePlayers[state.ID].Alive = false;
                theWorld.SnakePlayers[state.ID].Died = true;
            }

            foreach (Snake s2 in theWorld.SnakePlayers.Values)
            {
                if (checkSSSelfCollision(theWorld.SnakePlayers[state.ID], theWorld.SnakePlayers[state.ID]))
                {
                    theWorld.SnakePlayers[state.ID].Alive = false;
                    theWorld.SnakePlayers[state.ID].Died = true;
                }
                else
                {
                    if (!theWorld.SnakePlayers[state.ID].Equals(s2))
                    {
                        if (checkSSCollision(theWorld.SnakePlayers[state.ID], s2))
                        {
                            theWorld.SnakePlayers[state.ID].Alive = false;
                            theWorld.SnakePlayers[state.ID].Died = true;
                        }
                    }
                }
            }

            foreach (PowerUp p in theWorld.PowerUps.Values)
            {
                if (checkSPCollision(theWorld.SnakePlayers[state.ID], p) && !p.Died)
                {
                    theWorld.SnakePlayers[state.ID].GetScore();
                    p.Died = true;
                }
            }

            if (theWorld.SnakePlayers[state.ID].getScore)
                theWorld.SnakePlayers[state.ID].Count();
        }

        public void ReadAndSet()
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

            for (int i = 0; i < _maxPowerups; i++)
            {
                theWorld.PowerUps[i] = powerUpRespawn(i);
                respawnPowerup.Add(0);
            }
        }
    }
}