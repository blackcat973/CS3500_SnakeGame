using System.Xml.Linq;
using System.Xml;
using System.Runtime.CompilerServices;
using GameWorld;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using NetworkUtil;

namespace Server
{
    public class Server
    {

        private static World theWorld { get; set; }

        public static void Main(string[] args)
        {
            ReadXml();   


        }

        public static void ReadXml()
        {

            
            DataContractSerializer ser = new(typeof(GameSettings));

            XmlReader reader = XmlReader.Create(@"..\..\..\settings.xml");

            GameSettings gs = (GameSettings)ser.ReadObject(reader);

            CreateWorld(gs.UniverseSize);

            //Create the wall.
            foreach(Wall wall in gs.Walls)
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
            theWorld = new World(worldSize);
        }

        public void Connect()
        {
            //start an event-loop that listens for TCP connections from clients. When a new client
            //connects, the server must implement the server-side of the handshake in the communication
            //protocol described in PS8
            //add them to its list of clients.

            //Server Architecture

            /* 1. Client connects change Action, ask for data.
                2. Client's name received
                    Send startup info
                    save SocketState
                    Change Action, ask for data.
                3. Command received


            */
           
            Networking.StartServer( ,11000)

        }

    }
}