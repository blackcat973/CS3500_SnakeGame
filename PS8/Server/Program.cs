using System.Xml.Linq;
using System.Xml;
using System.Runtime.CompilerServices;
using GameWorld;

namespace Server
{
    public class Server
    {

        private static long msPerFrame { get; set; }
        private static long framePerShot { get; set; }
        private static long respawnRate { get; set; }
        private static long universeSize { get; set; }
        private static long size { get; set; }
        private World theWorld;

        public static void Main(string[] args)
        {
            ReadXml();   
        }

        public static void ReadXml()
        {

            XmlDocument doc = new XmlDocument();
            doc.Load(@"..\..\..\settings.xml");

            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                string text = node.InnerText; //or loop through its children as well
                Console.WriteLine(text);
            }



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
            //    }
            //    else if (node.Name.Equals("RespawnRate"))
            //    {
            //        respawnRate= Convert.ToInt32(node.InnerText);
            //    }else if (node.Name.Equals("UniverseSize"))
            //    {
            //        size= Convert.ToInt32(node.InnerText);
            //    }else if (node.Name.Equals("Walls"))
            //    {
            //        foreach(XmlNode node2 in node.ChildNodes)
            //        {
            //            foreach(XmlNode node3 in node2.ChildNodes)
            //            {
            //                Console.WriteLine(node3.InnerText);
            //            }
            //        }
            //    }
            //}
        }
    }
}