using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    [DataContract(Name = "GameSettings", Namespace ="")]
    public class GameSettings
    {
        [DataMember(Name = "FramesPerShot")]        
        public long FramesPerShot { get; private set; }

        [DataMember(Name = "MSPerFrame")]
        public long MSPerFrame { get; private set; }

        [DataMember(Name = "RespawnRate")]
        public long RespawnRate { get; private set; }

        [DataMember(Name = "UniverseSize")]
        public long UniverseSize { get; private set; }
    }
}
