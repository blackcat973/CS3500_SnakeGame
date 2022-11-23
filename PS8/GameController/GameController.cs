using NetworkUtil;
using GameWorld;
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
        private string UserName { get; set; }


        public delegate void ConnectedHandler();
        public event ConnectedHandler? Connected;

        public delegate void ErrorHandler(string error);
        public event ErrorHandler? Error;

        public delegate void GameUpdateHandler();
        public event GameUpdateHandler? GameUpdate;

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

            Connected?.Invoke();

            theServer = state;

            // Start an event loop to receive messages from the server
            state.OnNetworkAction = ReceiveData;

            //2. Upon connection, send a single '\n' terminated string representing the player's name
            //The name should be no longer than 16 charachters.
            if (UserName.Length > 16)
            {

            }
            else
            {
                Networking.Send(theServer.TheSocket, this.UserName + "\n");
            }

            Networking.GetData(state);
        }

        private void ReceiveData(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                Error?.Invoke("Lost connection to server");
                return;
            }

            ProcessData(state);

            Networking.GetData(state);
        }

        private void ProcessData(SocketState state)
        {
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
                // The regex splitter will include the last string even if it doesn't end with a '\n',
                // So we need to ignore it if this happens.
                if (p[p.Length - 1] != '\n')
                    break;
                // Then remove it from the SocketState's growable buffer
                state.RemoveData(0, p.Length);
            }

            //First is the player's unique ID.
            int uniqueID = Int32.Parse(parts[0]);
            //Second is the size of the world.
            int worldSize = Int32.Parse(parts[1]);

            //4. The server will then send all of the walls as JSON objects, each separated by a "\n"





        }



    }
}