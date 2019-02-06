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
            List<float> position = new List<float>(){player.position.X, player.position.Y};
            tag.Add(Main.ActiveWorldFileData.Name, position);
            return tag;
        }

        public override void Load(TagCompound tag)
        {
            serverCurrentDimension = tag.GetString("dimension");

            if (tag.ContainsKey(Main.ActiveWorldFileData.Name))
            {
                List<float> position = (List<float>)tag.GetList<float>(Main.ActiveWorldFileData.Name);
                player.position = new Vector2(position[0], position[1]);
            }
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
