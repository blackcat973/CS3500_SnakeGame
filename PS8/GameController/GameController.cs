using GameWorld;
using NetworkUtil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Net.NetworkInformation;
using static System.Net.WebRequestMethods;
using System;


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
        private string UserName { get; set; }
        // we need to handle the JSON file in here.
        //  We will get Wall, Snake info from the server.

        private static System.Timers.Timer aTimer;


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

        public delegate void ConnectedHandler();
        public event ConnectedHandler? Connected;

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

        public bool IsConnected()
        {

            if (theServer.ErrorOccurred)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


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

            if (theServer is not null)
            {
                Networking.Send(theServer.TheSocket, UserName + "\n");
            }

            Connected?.Invoke();


            //2. Upon connection, send a single '\n' terminated string representing the player's name
            //send user name to the socket

            // Start an event loop to receive messages from the server
            theServer.OnNetworkAction = ReceiveData;

            //at this point getting wall info
            Networking.GetData(theServer);
        }

        public void CheckServerStatus()
        {

            if (theServer.ErrorOccurred)
            {
                Error?.Invoke("ERRRRR");
                return;
            }
        }

        private void ReceiveData(SocketState state)
        {
            if (state.ErrorOccurred || theServer.ErrorOccurred)
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
            string totalData = "";
            if (!state.ErrorOccurred)
            {

                totalData = state.GetData();
            }
            else
            {
                Error?.Invoke("getdata state");
            }
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
        }

        private void ParsingData(string s)
        {

            if (theServer.ErrorOccurred)
            {
                Error?.Invoke("error");
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
        }

        //
        public void InputKey(string s)
        {
            //https://stackoverflow.com/questions/13489139/how-to-write-json-string-value-in-code
            //The client shall not send any command requests to the server before
            //receiving its player ID, world size, and walls.

            if (theServer.ErrorOccurred)
            {
                Error?.Invoke("Lost connection.");
                return;
            }
            else if (World is not null && (World.SnakePlayers.Count > 0 || World.PowerUps.Count > 0) && s is not null)
            {
                Networking.Send(theServer.TheSocket, JsonConvert.SerializeObject(new { moving = s }) + "\n");
            }
        }
    }
}