using Dimlibs.API;
using Dimlibs.Chunks;
using Dimlibs.UI;
using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using Dimlibs.API.ReflectionUtils;
using Dimlibs.Network;
using Mono.Cecil;
using Mono.Cecil.Cil;
using ReLogic.OS;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Net;
using Terraria.Net.Sockets;
using Terraria.Utilities;

namespace Dimlibs
{
    public class Dimlibs : Mod
    {
        private readonly string previousWorldPath;
        internal static IList<ModCommand> commandsList = new List<ModCommand>();
        internal static Dimlibs Instance;
        internal static readonly IDictionary<String, ModDimension> dimensionInstanceHandlers = new Dictionary<String, ModDimension>();
		internal static readonly List<DimensionServer> server = new List<DimensionServer>();

	    public static string dimension = "Dimlibs:OverworldDimension";
	    public static bool isActive;
	    public static bool isLan = true;

        public override void Load()
        {
			/*if (!Environment.Is64BitProcess && Main.netMode != 2)
	        {
		        throw new Exception("Dimension library might or might not take a huge amount of RAM, it is recommended to download the 64bit version the game to run it\n" +
		                            "The 64bit version got some special patch to allow multiple instance of static thing without having problem\n" +
									"To download it, visit the 64bit discord https://discord.gg/DY8cx5T");
	        }*/

			if (Program.LaunchParameters.TryGetValue("-dimension", out string dim))
	        {
		        dimension = dim;
	        }

	        if (Main.netMode == 2)
		        Console.Title = "Terraria Project Dimension : " + Dimlibs.dimension + " dimension";

			foreach (string launchParametersKey in Program.LaunchParameters.Keys)
	        {
		        Logger.Info($"{launchParametersKey} : {Program.LaunchParameters[launchParametersKey]}");
	        }

	        Instance = this;
            ReflectionUtilities.Load();
	        
			GetDimLibsCommand();
            for (int i = 0; i < ModLoader.Mods.Length; i++)
            {
                Mod mod = ModLoader.Mods[i];
                try
                {
                    Autoload(mod);
                }
                catch (Exception e)
                {

                }
            }


	        if (Main.netMode == 2)
	        {
		        On.Terraria.WorldGen.UpdateWorld += WorldUpdate;
	        }
        }

        public override void Unload()
        {
            ReflectionUtilities.Unload();
			ChatServer.instance.Unload();
            On.Terraria.WorldGen.UpdateWorld -= WorldUpdate;
        }

        public override void PostSetupContent()
        {
            // LoadModContent(Autoload);
            for (int i = 0; i < ModLoader.Mods.Length; i++)
            {
                Mod mod = ModLoader.Mods[i];
                try
                {
                    Autoload(mod);
                }
                catch
                {
                    mod.Logger.InfoFormat("Failure to autoload dimensions for mod {0}", mod.DisplayName);
                }
            }
            
        }


        internal void Autoload(Mod mod)
        {
            if (mod.Code == null)
            {
                return;
            }

            TypeInfo[] array = mod.Code.DefinedTypes.OrderBy(type => type.FullName)
                .ToArray();
            foreach (Type type in mod.Code.GetTypes().OrderBy(type => type.FullName, StringComparer.InvariantCulture))
            {
                if (type.IsSubclassOf(typeof(ModDimension)))
                {
                    mod.AutoLoadDimension(type);
                }
            }
        }

	    public override bool HijackGetData(ref byte messageType, ref BinaryReader reader, int playerNumber)
	    {
		    if (Main.netMode == 2)
		    {
			    if (messageType == 1)
			    {
				    if (Main.netMode != 2)
				    {
					    return false;
				    }
				    if (Netplay.Clients[playerNumber].State != 0)
				    {
					    return false;
				    }

					RemoteAddress address = Netplay.Clients[playerNumber].Socket.GetRemoteAddress();


					if (isLan && !address.IsLocalHost())
				    {
					    NetMessage.SendData(2, playerNumber, -1, NetworkText.FromLiteral("The server you are trying to join is a LAN world."));
					    return true;
				    }
				} 
		    }
		    return base.HijackGetData(ref messageType, ref reader, playerNumber);
	    }

	    private void GetDimLibsCommand()
        {
            FieldInfo commandListInfo =
                typeof(CommandManager).GetField("Commands", BindingFlags.Static | BindingFlags.NonPublic);
            Dictionary<String, List<ModCommand>> tempDictionary = (Dictionary<string, List<ModCommand>>)commandListInfo.GetValue(null);
            Dictionary<string, List<ModCommand>>.ValueCollection a = tempDictionary.Values;
            foreach (var modCommandList in a)
            {
                foreach (var modCommand in modCommandList)
                {
                    if (modCommand.mod.Name == Name)
                    {
                        commandsList.Add(modCommand);
                    }
                }
            }
        }


        private void ServerLoop(object context)
        {
            TcpListener listener = context as TcpListener;
            TcpClient client = listener.AcceptTcpClient();
            TcpSocket terrariaSocket = new TcpSocket(client);
            ISocket socket = terrariaSocket as ISocket;

            Console.WriteLine("server loop started");

            RemoteServer server = new RemoteServer();


            server.Socket = terrariaSocket;
            while (false)
            {
                try
                {
                    if (socket.IsConnected())
                    {
                        if (socket.IsDataAvailable())
                        {
                            client.GetStream().Flush();
                            byte[] data = new byte[ushort.MaxValue];
                            using (MemoryStream stream = new MemoryStream(data))
                            {
                                BinaryWriter writer = new BinaryWriter(stream);
                                socket.AsyncSend(writer.BaseStream.ReadAllBytes(), 0, 256, new SocketSendCallback(Netplay.Connection.ClientWriteCallBack), null);
                            }
                            
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            listener.Stop();
            Console.WriteLine("server loop stopped");
        }


        #region Client


        public void ClientThread(object context)
        {
            Main.gameMenu = true;
            Main.menuMode = 888;
            Main.MenuUI.SetState(new UINetworkConnection());
            object[] parameter = (object[])context;
            bool exitThread = false;
            DimPlayer player = (DimPlayer)parameter[0];
            int numberOfAttempt = 0;

            RemoteAddress adress = new TcpAddress(Netplay.ServerIP, 7776);
            ClientLoopSetup(adress);
            ISocket secondarySocket = new TcpSocket();
            secondarySocket.Connect(new TcpAddress(Netplay.ServerIP, 7776));

            while (!exitThread)
            {
                try
                {
                   
                    Thread.Sleep(2500);
                    if (secondarySocket.IsDataAvailable())
                    {
                        
                        byte[] data = new byte[ushort.MaxValue];
                        secondarySocket.AsyncReceive(data, 0, ushort.MaxValue, new SocketReceiveCallback(Netplay.Connection.ClientReadCallBack), null);
                        using (MemoryStream stream = new MemoryStream(data))
                        {
                            BinaryReader reader = new BinaryReader(new MemoryStream(data));
                        }
                        numberOfAttempt++;
                    }
                    else
                    {
                        byte[] data = new byte[ushort.MaxValue];
                        using (MemoryStream stream = new MemoryStream(data))
                        {
                            BinaryWriter writer = new BinaryWriter(stream);
                            writer.Write("hey");
                            secondarySocket.AsyncSend(writer.BaseStream.ReadAllBytes(), 0, ushort.MaxValue, new SocketSendCallback(Netplay.Connection.ClientWriteCallBack), null);
                        }
                    }
                }
                catch (Exception e)
                {
                    LogManager.GetLogger("Second thread").Error(e.Message, e);
                }

            }
            Netplay.Connection.Socket.Close();
            Netplay.StartTcpClient();
            player.inTransit = false;
        }

        private static void ClientLoopSetup(RemoteAddress address)
        {
            LogManager.GetLogger("Dimension Library Server").InfoFormat("Connecting to {0}", address.GetFriendlyName());
            Netplay.ResetNetDiag();
            Main.ServerSideCharacter = false;
            if (Main.rand == null)
            {
                Main.rand = new UnifiedRandom((int)DateTime.Now.Ticks);
            }
            Main.player[Main.myPlayer].hostile = false;
            Main.clientPlayer = (Player)Main.player[Main.myPlayer].clientClone();
            for (int i = 0; i < 255; i++)
            {
                if (i != Main.myPlayer)
                {
                    Main.player[i] = new Player();
                }
            }
            Main.netMode = 1;
            Netplay.disconnect = false;
            Netplay.Connection = new RemoteServer();
            Netplay.Connection.ReadBuffer = new byte[ushort.MaxValue];
        }
        #endregion

	    public override void MidUpdateTimeWorld()
	    {
		    if (Main.netMode == 2)
			    Console.Title = "Terraria Project Dimension : " + Dimlibs.dimension + " dimension";
	    }

	    public static void WorldUpdate(On.Terraria.WorldGen.orig_UpdateWorld orig)
        {
	        if (Main.netMode == 2)
		        Console.Title = "Terraria Project Dimension : " + Dimlibs.dimension + " dimension";
			isActive = Netplay.anyClients;
            if (isActive)
            {
                orig.Invoke();
            }
        }
    }
}
