using GameWorld;
using NetworkUtil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace GameSystem
{
    public class GameController
    {
        /// <summary>
        /// This should contain logic for parsing the data received by the server, updating
        /// the model accordingly, and anything elsoe you think belongs here.
        /// Key press handlers in your View should be "landing points"only,
        /// and should invoke controller method that contain the heavy logic.
        /// </summary>

        //Input user name.
        private string? UserName { get; set; }
        // we need to handle the JSON file in here.
        //  We will get Wall, Snake info from the server.

        private bool FirstSend = true;

        private int UniqueID { get; set; } 

        public int getUniqueID()
        {
            return this.UniqueID;
        }

        private int WorldSize = -1;

        public World World { get; set; }

        public delegate void DataHandler();
        public event DataHandler? DatasArrived;

        public delegate void ErrorHandler(string error);
        public event ErrorHandler? Error;

        public delegate void GameUpdateHandler();
        public event GameUpdateHandler? GameUpdate;

        public delegate void WorldCreated();
        public event WorldCreated? WorldCreate;

        public delegate void InputAccess(object t);
        public event InputAccess? InputAvailiable;

        //public delegate void WallCreated();
        //public event WallCreated? WallCreate;

        SocketState? theServer = null;

        public void Connect(string address, string userName)
        {
            // 1. Establish a socket connection to the server on port 11000.
            Networking.ConnectToServer(OnConnect, address, 11000);

            this.UserName = userName;
        }

        private void OnConnect(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                Error?.Invoke("Error connecting to server");
                return;
            }

            theServer = state;

            if(theServer is not null)
            {
                Networking.Send(theServer.TheSocket, UserName + "\n");
            }

            //2. Upon connection, send a single '\n' terminated string representing the player's name
            //send user name to the socket

            // Start an event loop to receive messages from the server
            state.OnNetworkAction = ReceiveData;

            //at this point getting wall info
            Networking.GetData(state);
        }

        private void ReceiveData(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                Error?.Invoke("Lost connection to server");
                return;
            }
            lock (state)
            {
                ProcessData(state);
            }
            Networking.GetData(state);
        }

        private void ProcessData(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                Error?.Invoke("Lost connection to server");
                return;
            }

            string totalData = state.GetData();
            //  3. The server will then send two strings representing integer numbers, and each
            //  terminated by a "\n". The first number is the player's unique ID.
            //  The second is the size of the world, representing both the width
            //  the width and height. All game worlds are square.

            string[] parts = Regex.Split(totalData, @"(?<=[\n])");

            foreach (string p in parts)
            {
                // Ignore empty strings added by the regex splitter
                if (p.Length == 0)
                    continue;
                // The regex splitter will include the last string even
                // if it doesn't end with a '\n',
                // So we need to ignore it if this happens.
                if (p[p.Length - 1] != '\n')
                    break;
                // Then remove it from the SocketState's growable buffer
                state.RemoveData(0, p.Length);

                ParsingData(p);

            }
            DatasArrived?.Invoke();
            //inform to view that datas are updated.

            //state.OnNetworkAction = ProcessData;

            //Networking.GetData(state);
        }

        private void ParsingData(string s)
        {
            if (theServer.ErrorOccurred)
            {
                Error?.Invoke("Lost connection to server");
                return;
            }

            try
            {
                //Convert String to JObject 
                JObject obj = JObject.Parse(s);

                //If objects' key is wall
                if (obj.ContainsKey("wall"))
                {
                    Wall? DeseriWall = JsonConvert.DeserializeObject<Wall>(obj.ToString());
                    //add to world.

                    if (World.Walls.ContainsKey(DeseriWall.WallID))
                    {
                        World.Walls[DeseriWall.WallID] = DeseriWall;
                    }
                    else
                    {
                        World.Walls.Add(DeseriWall.WallID, DeseriWall);
                    }

                }
                //if objects' key is snake
                else if (obj.ContainsKey("snake"))
                {
                    //WallCreate.Invoke();

                    Snake? DeseriSnake = JsonConvert.DeserializeObject<Snake>(obj.ToString());
                    //add snake to world

                    if (DeseriSnake.Disconnected)
                    {
                        World.SnakePlayers.Remove(DeseriSnake.UniqueID);
                    }
                    else
                    {
                        if (World.SnakePlayers.ContainsKey(DeseriSnake.UniqueID))
                        {
                            World.SnakePlayers[DeseriSnake.UniqueID] = DeseriSnake;
                        }
                        else
                        {
                            World.SnakePlayers.Add(DeseriSnake.UniqueID, DeseriSnake);
                        }
                    }

                    //InputAvailiable?.Invoke();
                }
                else if (obj.ContainsKey("power"))
                {
                    //WallCreate.Invoke();

                    PowerUp? DeseriPower = JsonConvert.DeserializeObject<PowerUp>(obj.ToString());
                    //add power to world

                    if (DeseriPower.Died)
                    {
                        World.PowerUps.Remove(DeseriPower.Power);
                    }
                    else
                    {
                        if (World.PowerUps.ContainsKey(DeseriPower.Power))
                        {
                            World.PowerUps[DeseriPower.Power] = DeseriPower;
                        }
                        else
                        {
                            World.PowerUps.Add(DeseriPower.Power, DeseriPower);
                        }
                    }
                    //InputAvailiable?.Invoke();
                }
            }
            //when string s is not JSON type, it means first send. 
            //Maybe, need to find another way to do.
            catch (Exception)
            {
                if (FirstSend)
                {
                    this.UniqueID = Convert.ToInt32(s);

                    FirstSend = false;
                }
                else
                {
                    this.WorldSize = Convert.ToInt32(s);
                    World = new World(WorldSize);
                    WorldCreate?.Invoke();
                }
            }

            //lock (this)
            //{
            //    if (FirstSend)
            //    {
            //        First is the player's unique ID.
            //        this.UniqueID = Int32.Parse(parts[0]);
            //        Second is the size of the world.

            //        this.World = new World(this.WorldSize = Int32.Parse(parts[1]));



            //        infrom the view
            //        FirstSend = false;
            //        change the eventloop starting point.
            //        state.OnNetworkAction = ProcessData;
            //    }
            //    else if (!FirstSend)
            //    {
            //        At any time after receiving its ID < the world size, and the walls, the client can send a command
            //        Request to the server.
            //        The client shall not send any command requests to the server before receving its player ID<world, size, and walls,
            //        Well-behaved client should not send more than one command request object per frame.


            //        JObject obj = new();

            //        foreach (string p in parts)
            //        {
            //            parsing.
            //            obj = JObject.Parse(p);

            //            if (obj.ContainsKey("wall"))
            //            {
            //                Walls? walls = JsonConvert.DeserializeObject<Walls>(obj.ToString());
            //                now client can begin sending control commands,
            //            }
            //            else if (obj.ContainsKey("snake"))
            //            {
            //                Snake? walls = JsonConvert.DeserializeObject<Snake>(obj.ToString());
            //            }
            //            else if (obj.ContainsKey("power"))
            //            {
            //                PowerUp? walls = JsonConvert.DeserializeObject<PowerUp>(obj.ToString());
            //            }
            //        }
            //    }
            //}

        }

        //
        public void InputKey(string s)
        {
            //https://stackoverflow.com/questions/13489139/how-to-write-json-string-value-in-code
    
            //The client shall not send any command requests to the server before
            //receiving its player ID, world size, and walls.

            if(World is not null && (World.SnakePlayers.Count>0 || World.PowerUps.Count>0) || s is not null)
            {
                 Networking.Send(theServer.TheSocket, JsonConvert.SerializeObject(new { moving = s }) + "\n");
            }
        }
    }
}