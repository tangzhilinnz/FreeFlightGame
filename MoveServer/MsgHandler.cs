using System;
using System.Collections.Generic;
using System.Text;

namespace MoveServer
{
    class MsgHandler
    {
        static private Random random = new Random();
        internal static void MsgEnter(ClientState c, string msgArgs)
        {
            // Console.WriteLine("MsgEnter " + msgArgs);
            // parameter analysis
            if (msgArgs == null || msgArgs == "") return;
            string[] split = msgArgs.Split(',');
            if (split == null || split.Length < 5) return;

            string desc = split[0];
            float x = float.Parse(split[1]);
            float y = float.Parse(split[2]);
            float z = float.Parse(split[3]);
            float eulY = float.Parse(split[4]);
            // assign values to the client's server 
            c.hp = 100;
            c.x = x;
            c.y = y;
            c.z = z;
            c.eulY = eulY;
            c.remoteClientIPAdr = desc;
            // Broadcast
            // is used to separate each complete procotol within a sending bytes 
            string sendStr = "Enter|" + msgArgs + "=";
            Console.WriteLine(sendStr);
            foreach (ClientState cs in MainClass.clients.Values)
            {
                MainClass.Send(cs, sendStr);
            }
        }

        internal static void MsgList(ClientState c, string msgArgs)
        {
            string sendStr = "List|";
            foreach (ClientState cs in MainClass.clients.Values)
            {
                sendStr += cs.remoteClientIPAdr + ",";
                sendStr += cs.x.ToString() + ",";
                sendStr += cs.y.ToString() + ",";
                sendStr += cs.z.ToString() + ",";
                sendStr += cs.eulY.ToString() + ",";
                sendStr += cs.hp.ToString() + ",";
                sendStr += cs.isDead.ToString() + ",";
            }
            sendStr += "=";
            Console.WriteLine(sendStr);
            MainClass.Send(c, sendStr);
        }

        internal static void MsgMove(ClientState c, string msgArgs)
        {
            // parameter analysis
            if (msgArgs == null || msgArgs == "") return;
            string[] split = msgArgs.Split(',');
            if (split == null || split.Length < 5) return;
            string desc = split[0];
            float x = float.Parse(split[1]);
            float y = float.Parse(split[2]);
            float z = float.Parse(split[3]);
            float eulY = float.Parse(split[4]);
            // assign values to the client's server 
            c.x = x;
            c.y = y;
            c.z = z;
            c.eulY = eulY;
            // Broadcast
            string sendStr = "Move|" + msgArgs + "=";
            Console.WriteLine(sendStr);
            foreach (ClientState cs in MainClass.clients.Values)
            {
                MainClass.Send(cs, sendStr);
            }
        }

        internal static void MsgAttack(ClientState c, string msgArgs)
        {
            // parameter analysis
            if (msgArgs == null || msgArgs == "") return;
            string[] split = msgArgs.Split(',');
            if (split == null || split.Length < 2) return;

            string desc = split[0];
            float eulY = float.Parse(split[1]);
            c.eulY = eulY;
            // Broadcast
            string sendStr = "Attack|" + msgArgs + "=";
            Console.WriteLine(sendStr);
            foreach (ClientState cs in MainClass.clients.Values)
            {
                MainClass.Send(cs, sendStr);
            }
        }

        internal static void MsgHit(ClientState c, string msgArgs)
        {
            // parameter analysis
            if (msgArgs == null || msgArgs == "") return;
            string[] split = msgArgs.Split(',');
            if (split == null || split.Length < 3) return;

            //string attackDesc = split[0];
            string hitDesc = split[1];
            //float attackAngleY = float.Parse(split[2]);
            // find out the character being attacked
            ClientState hitCS = null;
            foreach (ClientState cs in MainClass.clients.Values)
            {
                if (cs.remoteClientIPAdr == hitDesc)
                    hitCS = cs;
            }

            if (hitCS == null)
                return;
            // Deduct hp
            int damage = random.Next(10, 16);
            hitCS.hp -= damage;

            // Send Hit protocol back to the client with this character
            if (hitCS.hp > 0)
            {
                string sendStr = "Hit|" + msgArgs + "," + hitCS.hp.ToString() + "=";
                Console.WriteLine(sendStr);
                foreach (ClientState cs in MainClass.clients.Values)
                {
                    MainClass.Send(cs, sendStr);
                }
            }
            // Send Die protocol back to the client with this character
            else if (hitCS.hp <= 0)
            {
                hitCS.isDead = "YES";

                string sendStr = "Die|" + msgArgs + "," + hitCS.hp.ToString() + "=";
                Console.WriteLine(sendStr);
                foreach (ClientState cs in MainClass.clients.Values)
                {
                    // send die protocol three times in a row to ensure no response 
                    // due to packet loss will not happen
                    MainClass.Send(cs, sendStr);
                    MainClass.Send(cs, sendStr);
                    MainClass.Send(cs, sendStr);
                }
            }
        }
    }
}
