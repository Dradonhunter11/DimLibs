using System;
using System.IO;
using System.Threading;
using Dimlibs.UI;
using log4net;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Dimlibs
{
    class DimensionNetwork
    {
        public static bool loading = false;

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
                case DimensionRequestID.ClientKick:
                    ClientKickExecute(reader.ReadString());
                    break;
                case DimensionRequestID.ClientReconnect:
                    ServerSendResponse(reader.ReadInt32());
                    break;
                case DimensionRequestID.ClientMessage:
                    Main.LocalPlayer.GetModPlayer<DimPlayer>().inTransit = true;
                    UINetworkConnection.Message = reader.ReadString();
                    break;
            }
        }

        public static void ClientKickRequest(string dimension)
        {
            LogManager.GetLogger("Client").Info("Kick request sent");
            ModPacket packet = Dimlibs.Instance.GetPacket();
            packet.Write((byte)DimensionRequestID.ClientKick);
            packet.Write(dimension);
            packet.Send();
        }

        public static void ClientKickExecute(string dimension)
        {
            Dimlibs.Instance.NewServerSocket();
            LogManager.GetLogger("Server").Info("Kick request received");
            if (Main.netMode == 2)
            {
                foreach (RemoteClient p in Netplay.Clients)
                {
                    if (!p.IsActive)
                    {
                        continue;
                    }
                    ModPacket packet = Dimlibs.Instance.GetPacket();
                    packet.Write((byte)DimensionRequestID.ClientMessage);
                    packet.Write("Server is currently changing to " + dimension);
                    packet.Send(p.Id);
                    NetMessage.SendChatMessageToClient(NetworkText.FromLiteral("Someone requested a dimension change, redirecting to the loading screen."), Color.White, p.Id);
                }
                ThreadPool.QueueUserWorkItem(ThreadKick);
            }
            DimWorld.SwapDimension();
        }

        public static void ThreadKick(object context)
        {
            Thread.Sleep(1500);
            foreach (RemoteClient p in Netplay.Clients)
            {
                if (!p.IsActive)
                {
                    continue;
                }
                p.PendingTermination = true;
            }
        }

        

        public static void ClientSendRequest()
        {         
            ModPacket packet = Dimlibs.Instance.GetPacket();
            packet.Write((byte)DimensionRequestID.ClientReconnect);
            packet.Write(Main.LocalPlayer.whoAmI);
            packet.Send(255);
        }

        public static void ServerSendResponse(int player)
        {
            LogManager.GetLogger("Dimlibs instance").Info("Is dimlibs alive : " + Dimlibs.Instance != null);
            ModPacket serverPacket = Dimlibs.Instance.GetPacket();
            serverPacket.Write((byte)DimensionRequestID.ServerReconnect);
            serverPacket.Write(Dimlibs.dimensionInstanceHandlers[DimWorld.dimension].handler.loading);
            serverPacket.Send(player);
            if (loading)
            {
                LogManager.GetLogger("Server").Info(Main.player[player] + " tried to connect but it's still loading");
                Netplay.Clients[player].PendingTermination = true;
            }
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
