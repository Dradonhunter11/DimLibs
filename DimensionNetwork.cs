using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dimlibs.API;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Dimlibs
{
    class DimensionNetwork
    {
        public static void PrepareServerMP(string dimension, DimPlayer p)
        {
            //Netplay.Clients[NetMessage.buffer[p.player.whoAmI].whoAmI].ResetSections();
            //NetMessage.SendData(25, -1, -1, NetworkText.FromLiteral("Server is loading new dimension: " + dimension), 255, 255f, 127f, 0f, 0, 0, 0);
            

            if (Main.netMode != 1)
            {
                //Dimlibs.dimensionInstanceHandlers[p.previousServerDimension].Save();
                //Dimlibs.dimensionInstanceHandlers[dimension].LoadWorld();
                DimWorld.dimension = dimension;
                Netplay.Clients[p.player.whoAmI].ResetSections();
                CheckSection(0, p.player.position);
                NetMessage.SendData(MessageID.RequestTileData);
                Console.WriteLine("Hi, I finished loading!");
            }
            else
            {
                WorldGen.gen = true;              
                NetMessage.SendData(MessageID.RequestTileData);
                WorldGen.gen = false;
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

        public static void HandlePacket(BinaryReader reader, int whoAmI)
        {
            //Handle based on first byte, standard
            byte type = reader.ReadByte();
            switch ((DimensionRequestID)type)
            {
                case DimensionRequestID.ClientRequest:
                    ReceiveRequest(reader);
                    break;
            }
        }

        public static void ClientSendRequest(String dimension)
        {
            /*if (!DimWorld.dimensionInstanceHandlers.ContainsKey(dimension))
            {
                return;
            }*/

            ModPacket packet = Dimlibs.Instance.GetPacket();
            packet.Write((byte)DimensionRequestID.ClientRequest);
            packet.Write(dimension);
            packet.Send();
            
        }

        public static void ReceiveRequest(BinaryReader reader)
        {
            DimPlayer p = Main.LocalPlayer.GetModPlayer<DimPlayer>();
            p.serverCurrentDimension = reader.ReadString();
            Console.WriteLine("Receiving data");
            PrepareServerMP(p.serverCurrentDimension, p);
            p.previousServerDimension = p.serverCurrentDimension;
        }

        public static void SyncWorld(int WhoAmI)
        {
            for (int i = 0; i < Main.maxTilesX; i++)
            {
                for (int j = 0; j < Main.maxTilesY; j++)
                {
                    WorldGen.SquareTileFrame(i, j, true);
                    NetMessage.SendTileRange(WhoAmI, i, j, 16, 16);
                }
            }
        }
    }
}
