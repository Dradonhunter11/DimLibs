using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Dimlibs
{
    public class DimPlayer : ModPlayer
    {
        internal static List<string> dimList = new List<string>();
        internal string serverCurrentDimension = "Overworld";
        internal string previousServerDimension = "Overworld";

        public String getServerDimension()
        {
            if (Main.netMode == 0)
            {
                return DimWorld.dimension;
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
                DimensionNetwork.ClientSendRequest(serverCurrentDimension);
            }
        }
    }
}
