using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace server
{
    class Room
    {
        public Player player1;
        Player player2;
        Player watcher;
        public string Categ;
        public string Diffic;
        public string RoomName;
        public int Count;
        public string word;
        public string char_to_Watch="";

        public string answer1;
        public string answer2;
        public string score1;
        public string score2;
        public static bool RejectState;
        FileStream Scorefile;
        StreamWriter sw;

        public List<string> watch_R_list=new List<string>();
        db data = new db();
       
        public void Nullify(Player p)
        {
            p = null;
      
        }

        public Room(Socket soc, string Room_Name, string Category, string Difficulty,string ownerPlayer)
        {

            RoomName = Room_Name;
            Count = 1;
            player1 = new Player(soc);
            player1.Playername = ownerPlayer;
            Diffic = Difficulty;
            Categ = Category;
            word = data.select_word(Category, Difficulty).First();
            player1.bw.Write("New_"+ word);
            chat1();
            
            
        }
        ~Room(){
            sw.Close();
            Scorefile.Close();
        }

        public void Connect_player2(Socket s,string w,string p2)
        {
            
            player2 = new Player(s);
            player2.Playername = p2;
            player2.bw.Write("Join_"+w);
            chat2();
            Console.WriteLine("Player 2 connected To The Room");
        }
        public void Connect_watcher(Socket s, string w)
        {
            try
            {
                watcher = new Player(s);

                watcher.bw.Write("Watch_" + w + "_" + player1.Playername + "_" + player2.Playername);
                BinaryFormatter bf = new BinaryFormatter();

                bf.Serialize(watcher.nstream, watch_R_list);
                chat3();
                Console.WriteLine("a watcher connected To The Room");
            }
            catch (Exception)
            {

                Console.WriteLine("Client has disconnected"); ;
            }            
        }

        public void chat1()
        {
            Task tsk = new Task(new Action(delegate() {
                try
                {
                    while (true)
                    {
                        if (player1.nstream.DataAvailable)
                        {
                            string action = player1.br.ReadString();
                            string[] action_splitted = action.Split('_');

                            Console.WriteLine("player 1 said : " + action);
                            if (action_splitted.Length > 3)
                            {

                                watch_R_list.Add(action);
                            }
                            switch (action_splitted[0])
                            {
                                case "RequestYes":
                                    Count++;
                                    
                                    Console.WriteLine("count =" + Count);
                                    player2.bw.Write(action);
                                    break;

                                case "acceptNewGame":
                                    word = data.select_word(Categ, Diffic).First();
                                    answer1 = "yes";
                                    score1 = action_splitted[1];
                                while (answer2 == null);

                                    watch_R_list.Clear();

                                    player1.bw.Write("newWord_" + word);
                                    if (watcher != null)
                                    {
                                        watcher.bw.Write("newWord_" + word);
                                    }
                                    if(answer2=="No")
                                    {
                                        while (score2 == null) ;
                                        using (StreamWriter sw = File.AppendText("./score1.txt"))
                                        {
                                            sw.WriteLine(player1.Playername + "=" + score1 + " " + player2.Playername + "=" + score2);

                                        }
                                        score2 = null;
                                    }
                                    answer2 = null;

                                    break;

                                case "RejectNewGame":
                                    answer1 = "No";
                                    Count--;
                                    score1 = action_splitted[1];
                                    while (answer2 == null) ;
                                    player2.bw.Write("closeP2");
                                    word = data.select_word(Categ, Diffic).First();
                                    player1.bw.Write("newWord_" + word);
                                    while (score2 == null) ;
                                    using (StreamWriter sw = File.AppendText("score1.txt"))
                                    {
                                        sw.WriteLine(player1.Playername + "=" + score1 + " " + player2.Playername + "=" + score2);
                                        sw.Close();
                                    }
                                    score2 = null;
                                    answer2 = null;
                                    break;

                                case "CloseForm2":
                                    if (player2 != null)
                                    {
                                        score1= action_splitted[1];
                                        sw = new StreamWriter(Scorefile);
                                        while (score2 == null) ;
                                        sw.WriteLine(player1.Playername+"=" + score1 + " " + player2.Playername+"=" + score2);
                                        player1.bw.Write("closeP2");
                                        player1 = null;
                                        player1 = player2;
                                        Nullify(player2);
                                        Count--;
                                        word = data.select_word(Categ, Diffic).First();
                                        score2 = null;
                                        player1.bw.Write("newWord_" + word + "_" + "switch");
                                    }
                                    else
                                    {
                                        Count--;
                                        player1.bw.Write("closeP2");
                                    }
                                    break;

                                default:
                                    player2.bw.Write(action);
                                    if (watcher != null)
                                    {
                                        watcher.bw.Write(action);
                                    }
                                    Nullify(player2);


                                    break;
                            }

                            if (Count == 0)
                            {
                                string removedroom = RoomName + "," + Categ + "," + Diffic;
                                Program.Rooms.Remove(removedroom);
                            }
                        }
                    }
                }
                catch (Exception e)
                {

                    Console.WriteLine(e.Message) ;
                }

            }));

            if (tsk.Status != TaskStatus.Running)
            {
                tsk.Start();
            }
        }

        public void chat3()
        {
            Console.WriteLine("Chat 3 Running...");

            Task tsk2 = new Task(new Action(delegate () {
                while (true)
                {
                    if (watcher != null)
                    {
                        if (watcher.nstream.DataAvailable)
                        {
                            string action = watcher.br.ReadString();
                            
                            
                        }
                    }
                }
            }));
            if (tsk2.Status != TaskStatus.Running)
            {
                tsk2.Start();
            }
        }

        public void chat2()
        {
            Console.WriteLine("Chat 2 Running...");

            Task tsk2 = new Task(new Action(delegate() {
                try
                {
                    while (true)
                    {
                        if (player2 != null)
                        {

                            if (player2.nstream.DataAvailable)
                            {

                                string action = player2.br.ReadString();
                                Console.WriteLine("player 2 said : " + action);
                                string[] action_splitted = action.Split('_');
                                switch (action_splitted[0])
                                {
                                    case "acceptNewGame":
                                        answer2 = "Yes";
                                        while (answer1 == null) ;

                                        if (answer1 == "yes")
                                        {
                                            score2 = action_splitted[1];
                                            player2.bw.Write("newWord_" + word);

                                        }
                                        answer1 = null;

                                        break;
                                    case "RejectNewGame":
                                        answer2 = "No";
                                        while (answer1 == null);
                                        if (answer1 == "yes")
                                        {

                                            score2 = action_splitted[1];

                                            Count--;

                                            player2.bw.Write("closeP2");
                                           
                                        }
                                        answer1 = null;
                                        break;
                                    case "CloseForm2":
                                        Count--;
                                        score2 = action_splitted[1];
                                        while (score1 == null) ;
                                        using (StreamWriter sw = File.AppendText("score1.txt"))
                                        {
                                            sw.WriteLine(player1.Playername + "=" + score1 + " " + player2.Playername + "=" + score2);

                                        }
                                        score1 = null;
                                        word = data.select_word(Categ, Diffic).First();
                                        player1.bw.Write("newWord_" + word + "_" + "opponentLeft");
                                        player2.bw.Write("closeP2");
                                        answer1 = null;


                                        break;

                                    default:
                                        if (action_splitted.Length > 3)//means right
                                        {

                                            watch_R_list.Add(action);
                                        }

                                        player1.bw.Write(action);
                                        if (watcher != null)
                                        {
                                            watcher.bw.Write(action);
                                        }
                                        break;
                                }

                                if (Count == 0)
                                {
                                    string removedroom = RoomName + "," + Categ + "," + Diffic;
                                    Program.Rooms.Remove(removedroom);
                                    Program.Rooms[removedroom].Dispose();

                                }
                            }

                        }
                    }
                }
                catch
                {
                    Console.WriteLine("Client Disconnected");
                }
            }));
            if (tsk2.Status != TaskStatus.Running)
            {
                tsk2.Start();
            }
        }

        private void Dispose()
        {

            GC.SuppressFinalize(this);
        }

    }
}