using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dimlibs.API;
using Terraria;

namespace Dimlibs.Network
{
    class TransitServer
    {
        private string hostName = Netplay.GetLocalIPAddress();
        private readonly int port;

        [ThreadStatic] internal Main main;
        [ThreadStatic] internal Tile[,] tile;

        [ThreadStatic] internal NPC[] npc;
        [ThreadStatic] internal Projectile[] projectile;
        [ThreadStatic] internal Player[] player;
        [ThreadStatic] internal int tileMaxX, tileMaxY;

        [ThreadStatic] internal Process server;



        public TransitServer(int port)
        {
            this.port = port;
            
            
        }


        public void Open()
        {
            ProcessStartInfo info = new ProcessStartInfo(Environment.CurrentDirectory + "tModLoaderServer.exe");
            info.Arguments = "";

            server = Process.Start();
        }

        public static unsafe void SwapVariable()
        {
            
        }

        unsafe public static void PointerSwap<T>(object[] objectA, object[] objectB)
        {

        }
    }
}
