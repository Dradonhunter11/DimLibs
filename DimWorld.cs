using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dimlibs.API;
using Dimlibs.Commands;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Dimlibs
{
    internal class DimWorld : ModWorld
    {
        public static String previousDimension = "";
        public static String dimension = "Dimlibs:OverworldDimension";
        
        private DimensionHandler currentRunningHandler;

        internal static bool update = false;

        public override TagCompound Save()
        {
            TagCompound tag = new TagCompound();
            tag.Add("currentDimension", dimension);
            return tag;
        }

        public override void Load(TagCompound tag)
        {
            dimension = tag.GetString("currentDimension");
            if (dimension == "Overworld")
            {
                dimension = "Dimlibs:OverworldDimension";
            }
        }

        public override void NetSend(BinaryWriter writer)
        {
            TagCompound tag = new TagCompound();
            tag.Add("dimension", dimension);
            tag.Add("previousDimension", previousDimension);
            TagIO.Write(tag, writer);
        }

        public override void NetReceive(BinaryReader reader)
        {
            TagCompound tag = TagIO.Read(reader);
            dimension = tag.GetString("dimension");
            previousDimension = tag.GetString("previousDimension");
            if (dimension != previousDimension)
            {
                update = true;
            }
        }

        public static void CheckSection(int playerIndex, Vector2 position)
        {
            int sectionX = 0;
            int sectionY = 0;
            int num = 0;
            for (int i = sectionX; i < Main.maxSectionsX; i++)
            {
                for (int j = sectionY; j < Main.maxSectionsY; j++)
                {
                    if (i >= 0 && i < Main.maxSectionsX && j >= 0 && j < Main.maxSectionsY && !Netplay.Clients[playerIndex].TileSections[i, j])
                    {
                        num++;
                    }
                }
            }
            if (num > 0)
            {
                int num2 = num;
                NetMessage.SendData(9, playerIndex, -1, Lang.inter[44].ToNetworkText(), num2, 0f, 0f, 0f, 0, 0, 0);
                Netplay.Clients[playerIndex].StatusText2 = Language.GetTextValue("Net.IsReceivingTileData");
                Netplay.Clients[playerIndex].StatusMax += num2;
                for (int k = sectionX; k < Main.maxSectionsX; k++)
                {
                    for (int l = Main.maxSectionsY; l < Main.maxSectionsY; l++)
                    {
                        if (k >= 0 && k < Main.maxSectionsX && l >= 0 && l < Main.maxSectionsY && !Netplay.Clients[playerIndex].TileSections[k, l])
                        {
                            NetMessage.SendSection(playerIndex, k, l, false);
                            NetMessage.SendData(11, playerIndex, -1, null, k, (float)l, (float)k, (float)l, 0, 0, 0);
                        }
                    }
                }
            }
        }

        public void SyncWorld(int WhoAmI)
        {
            for (int i = 0; i < Main.maxTilesX; i++)
            {
                for (int j = 0; j < Main.maxTilesY; j++) {
                    WorldGen.SquareTileFrame(i, j, true);
                    NetMessage.SendTileRange(WhoAmI, i, j, 16, 16);
                }
            }
        }

        public override void PostUpdate()
        {
            base.PostUpdate();
        }
    }

    public enum MessageType : byte
    {
        PrepareDimensionSwapping,
        TransferringEveryBlock,
        TransferComplete
    }
}
