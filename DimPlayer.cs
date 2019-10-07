using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Net;

namespace Dimlibs
{
    public class DimPlayer : ModPlayer
    {
        internal static List<string> dimList = new List<string>();
        internal string serverCurrentDimension = "Overworld";
        internal string previousServerDimension = "Overworld";

        internal RemoteAddress currentConnectedAdress;
        internal string hostName;
        internal int port;
        internal bool inTransit = false;


        public String getServerDimension()
        {
            if (Main.netMode == 0)
            {
                return Dimlibs.dimension;
            }

            return serverCurrentDimension;
        }

        public override TagCompound Save()
        {
            TagCompound tag = new TagCompound();
            tag.Add("dimension", serverCurrentDimension);
            return tag;
        }

        public override void Load(TagCompound tag)
        {
            serverCurrentDimension = tag.GetString("dimension");
        }

        public override void OnEnterWorld(Player player)
        {
            if (Main.netMode == 1)
            {
                currentConnectedAdress = Netplay.Connection.Socket.GetRemoteAddress();
                port = Netplay.ListenPort;
                hostName = Netplay.GetLocalIPAddress();
            }
        }
    }
}
