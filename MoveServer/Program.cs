using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Reflection;

namespace MoveServer
{
    class ClientState
    {
        public Socket socket;
        public byte[] readBuff = new byte[1024];
        // detailed information about the CtrlHuamn owned by the current client
        public int hp = -100;
        public float x = 0;
        public float y = 0;
        public float z = 0;
        public float eulY = 0;
        public string isDead = "NO";
        public string remoteClientIPAdr;
    }

    class MainClass
    {
        // listening socket
        static Socket listenfd;
        // the Socket and state of the client
        public static Dictionary<Socket, ClientState> clients =
            new Dictionary<Socket, ClientState>();

        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            // Socket 
            listenfd = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            // Bind 
            IPAddress ipAdr = IPAddress.Parse("127.0.0.1");
            IPEndPoint ipEp = new IPEndPoint(ipAdr, 8888);
            //IPHostEntry hostinfo = Dns.GetHostEntry("239o4651o8.wicp.vip");
            //IPAddress[] aryIP = hostinfo.AddressList;
            //IPAddress address = aryIP[0];
            //IPEndPoint ipEp = new IPEndPoint(address, 27300);

            listenfd.Bind(ipEp);
            // listen
            listenfd.Listen(10);
            Console.WriteLine("[ Server ] startup");
            // CheckRead list
            List<Socket> checkRead = new List<Socket>();

            // the Main Loop
            while (true)
            {
                // initialize the checkRead List
                checkRead.Clear();
                checkRead.Add(listenfd);
                foreach (ClientState s in clients.Values)
                {
                    checkRead.Add(s.socket);
                }
                // Select -- it's a static method in Socket class
                Socket.Select(checkRead, null, null, 1000);
                // check sockets that have data to be read 
                foreach (Socket s in checkRead)
                {
                    if (s == listenfd)
                    {
                        ReadListenfd(s);
                    }
                    else
                    {
                        ReadClientfd(s);
                    }
                }
            }
        }

        // Read listenfd
        public static void ReadListenfd(Socket listenfd)
        {
            Console.WriteLine("Accept");
            Socket clientfd = listenfd.Accept();
            ClientState state = new ClientState();
            state.socket = clientfd;
            clients.Add(clientfd, state);
        }

        // Read Clientfd
        public static bool ReadClientfd(Socket clientfd)
        {
            ClientState state = clients[clientfd];
            // Receive 
            int count = 0;
            try
            {
                count = clientfd.Receive(state.readBuff);
            }
            catch (SocketException ex)
            {
                MethodInfo mein = typeof(EventHandler).GetMethod("OnDisconnect",
                    BindingFlags.IgnoreCase | BindingFlags.NonPublic |
                    BindingFlags.Static);
                object[] ob = { state };
                mein.Invoke(null, ob);

                Console.WriteLine($"Receive SocketException {ex.ToString()}" +
                    $"\n[ Client: {clientfd.RemoteEndPoint.ToString()}]" +
                    $"[ Corresponding Remote Client IP Address: {state.remoteClientIPAdr} ]");
                clientfd.Close();
                clients.Remove(clientfd);
                return false;
            }

            // client is Closed
            if (count == 0)
            {
                MethodInfo mein = typeof(EventHandler).GetMethod("OnDisconnect",
                    BindingFlags.IgnoreCase | BindingFlags.NonPublic |
                    BindingFlags.Static);
                object[] ob = { state };
                mein.Invoke(null, ob);

                Console.WriteLine("Socket Closed " +
                    $"[ Client: {clientfd.RemoteEndPoint.ToString()} ]" +
                    $"[ Corresponding Remote Client IP Address: {state.remoteClientIPAdr} ]");
                clientfd.Close();
                clients.Remove(clientfd);
                return false;
            }

            // BroadCast
            string receiveStr
                = System.Text.Encoding.Default.GetString(state.readBuff, 0, count);
            if (receiveStr == null || receiveStr == "") return false;
            string[] protocols = receiveStr.Split('=');
            if (protocols == null) return false;

            foreach (string protocol in protocols)
            {
                if (protocol != null && protocol != "")
                {
                    string[] split = protocol.Split('|');
                    if (split.Length < 2) continue;

                    Console.WriteLine("Receive " + protocol);
                    string msgName = split[0];
                    string msgArgs = split[1];
                    string funcName = "Msg" + msgName;

                    if (funcName == "MsgMove" || funcName == "MsgAttack" ||
                        funcName == "MsgHit" || funcName == "MsgEnter" ||
                        funcName == "MsgList")
                    {
                        MethodInfo mi = typeof(MsgHandler).GetMethod(funcName,
                             BindingFlags.IgnoreCase | BindingFlags.NonPublic |
                             BindingFlags.Static);
                        object[] o = { state, msgArgs };
                        mi.Invoke(null, o);
                    }
                }
            }
            return true;
        }

        // Static Send method called by EventHandler and MsgHandler
        public static void Send(ClientState cs, string sendStr)
        {
            byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
            cs.socket.Send(sendBytes);
        }
    }
}
