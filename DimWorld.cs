using System;
using System.IO;
using Dimlibs.API;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Dimlibs
{
    internal class DimWorld : ModWorld
    {
        public static string previousDimension = "";
        public static string dimension = "Dimlibs:OverworldDimension";
        

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

        public static void SwapDimension(string dimension = "Dimlibs:OverworldDimension")
        {
            Main.updateRate = 0;
            Dimlibs.dimensionInstanceHandlers[DimWorld.dimension].handler.Load();
            DimWorld.dimension = "Dimlibs:OverworldDimension";
            Dimlibs.dimensionInstanceHandlers["Dimlibs:OverworldDimension"].handler.Load();
            Main.updateRate = 1;
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
}
