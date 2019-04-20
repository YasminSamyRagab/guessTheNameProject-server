using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace server
{
    class Program
    {
        public static Dictionary<string, Room> Rooms = new Dictionary<string, Room>();
        
        static void Main(string[] args)
        {
            Socket s;
            NetworkStream nStream;
            BinaryWriter bw;
            BinaryReader br;
            IPAddress ServerAddr = new IPAddress(new byte[] { 127, 0, 0, 1 });
            TcpListener Listener;

            Listener = new TcpListener(ServerAddr, 14015);
            Listener.Start();

            string avaliable_rooms = "avaliable_";

            Task task = new Task(new Action(delegate() {
                while (true)
                {
                    Console.WriteLine("Waiting For a Socket.....");
                    s = Listener.AcceptSocket();
                    Console.WriteLine("Socket Connected...");
                    nStream = new NetworkStream(s);
                    bw = new BinaryWriter(nStream);
                    br = new BinaryReader(nStream);

                    Console.WriteLine("in if");
                    string came_action = br.ReadString();

                    Console.WriteLine(came_action);
                    string action = came_action.Split('_')[0];
                    string created_Room = came_action.Split('_')[1];//roomName,cat,diff the key in dictionary
                    switch (action)
                    {
                        case "New":
                            Room room = new Room(s, created_Room.Split(',')[0], created_Room.Split(',')[1], created_Room.Split(',')[2], came_action.Split('_')[2]);
                            Rooms.Add(created_Room, room);
                            

                            break;
                        case "Show":
                            avaliable_rooms = "avaliable_";
                            foreach (var r in Rooms)
                            {

                                avaliable_rooms += r.Key + "," + r.Value.Count + "_";
                            }
                           
                            bw.Write(avaliable_rooms);
                            break;

                        case "Join":
                            
                            Rooms[created_Room].Connect_player2(s, Rooms[created_Room].word,came_action.Split('_')[2]);
                            break;

                        case "Watch":
                            
                            Rooms[created_Room].Connect_watcher(s, Rooms[created_Room].word);
                            break;

                        default:
                            bw.Write(action);
                            break;
                    }
                }
            }));

           
            task.Start();

            Console.ReadLine();
        }
        
    }
}
