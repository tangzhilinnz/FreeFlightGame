using System;
using System.Collections.Generic;
using System.Text;

namespace MoveServer
{
    class EventHandler
    {
        internal static void OnDisconnect(ClientState c)
        {
            string desc = c.remoteClientIPAdr;
            string sendStr = "Leave|" + desc + "=";
            foreach (ClientState cs in MainClass.clients.Values)
            {
                MainClass.Send(cs, sendStr);
            }
        }
    }
}
