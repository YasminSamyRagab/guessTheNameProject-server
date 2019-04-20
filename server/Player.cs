using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace server
{
    class Player
    {
        public string Playername;
        public NetworkStream nstream;
        public BinaryReader br;
        public BinaryWriter bw;
        public Socket soc;
        public Player(Socket s)
        {
            soc = s;
            nstream = new NetworkStream(soc);
            br = new BinaryReader(nstream);
            bw = new BinaryWriter(nstream);
        } 
    }
}
