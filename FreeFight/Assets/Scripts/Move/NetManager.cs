using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using UnityEngine.UI;
using System;
using System.Net;

public static class NetManager
{
    // Define socket
    internal static Socket socket;
    // Receive buff
    static byte[] readBuff = new byte[1024];
    // Delegate type
    public delegate void Msglistener(string str);
    // Listener list
    private static Dictionary<string, Msglistener> listeners =
        new Dictionary<string, Msglistener>();
    // Message list
    static List<string> msgList = new List<string>();

    // Add Listener
    public static void AddListener(string msgName, Msglistener listener)
    {
        listeners[msgName] = listener;
    }

    // Obtain description
    public static string GetDesc()
    {
        if (socket == null) return "";
        if (!socket.Connected) return "";
        return socket.LocalEndPoint.ToString();
    }

    // Connect
    public static void Connect(string DNS, int port)
    {
        IPHostEntry hostinfo = Dns.GetHostEntry(DNS);
        IPAddress[] aryIP = hostinfo.AddressList;
        IPAddress address = aryIP[0];
        // Socket
        socket = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);
        // synchronous Connect
        socket.Connect(address, port);

        Debug.Log("Socket Connection is Successful!");
        Debug.Log($"[ Server: {socket.RemoteEndPoint.ToString()} ]");
        socket.BeginReceive(readBuff, 0, 1024, 0, ReceiveCallback, socket);
    }

    // Receive Callback
    private static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            int count = socket.EndReceive(ar);
            string receiveStr =
                System.Text.Encoding.Default.GetString(readBuff, 0, count);
            string[] protocols = receiveStr.Split('=');
            foreach (string protocol in protocols)
            {
                if (protocol != "")
                    msgList.Add(protocol);
            }

            socket.BeginReceive(readBuff, 0, 1024, 0, ReceiveCallback, socket);
        }
        catch(SocketException ex)
        {
            Debug.Log("Socket Receive Fail: " + ex.ToString());
            Main.isNetworkPromtInfo = true;
        }
    }

    public static void Send(string sendStr)
    {
        if (socket == null) return;
        if (!socket.Connected) return;

        byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
        socket.Send(sendBytes);
        // socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, socket);
    }

    // Send Callback
    //private static void SendCallback(IAsyncResult ar)
    //{
    //    try
    //    {
    //        Socket socket = (Socket)ar.AsyncState;
    //        int count = socket.EndSend(ar);
    //        Debug.Log("Socket Sending is successful: " + count);
    //    }
    //    catch (SocketException ex)
    //    {
    //        Debug.Log("Socket Sending Failed " + ex.ToString());
    //    }
    //}

    // Update
    public static void Update()
    {
        if (msgList.Count <= 0)
            return;
        string msgStr = msgList[0];
        msgList.RemoveAt(0);

        if (msgStr == null && msgStr == "") return;
        string[] split = msgStr.Split('|');
        if (split.Length < 2) return;

        string msgName = split[0];
        string msgArgs = split[1];
        // listener Callback
        if (listeners.ContainsKey(msgName))
        {
            listeners[msgName](msgArgs);
        }
    }
}
