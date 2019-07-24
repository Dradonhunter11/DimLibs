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
using Mono.Cecil;
using Mono.Cecil.Cil;
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

        public World tile = new World();



        public override void Load()
        {
            Instance = this;
            ReflectionUtil.Load();
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

            if (!Main.dedServ)
            {
                Main.OnTick += ClientTickUpdateTransit;
            }
            //MassPatcher.StartPatching();
            //On.Terraria.WorldGen.UpdateWorld += WorldUpdate;
        }

        public override void Unload()
        {
            ReflectionUtil.Unload();
            if (!Main.dedServ)
            {
                Main.OnTick -= ClientTickUpdateTransit;
            }
            //On.Terraria.WorldGen.UpdateWorld -= WorldUpdate;
        }

        private static DefaultAssemblyResolver resolver = new DefaultAssemblyResolver();

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



        internal static Type[] GetAllTypeInCurrentAssembly(Assembly asm)
        {
            return asm.GetTypes();
        }

        internal static MethodInfo[] GetAllMethodInAType(Type type)
        {
            return type.GetMethods();
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI) => DimensionNetwork.HandlePacket(reader, whoAmI);

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

        private bool threadLaunched = false;

        internal void NewServerSocket(int port = 7776)
        {
            
            TcpListener listener = TcpListener.Create(port);
            listener.Start();
            ThreadPool.QueueUserWorkItem(ServerLoop, listener);
            
        }


        private void ServerLoop(object context)
        {
            TcpListener listener = context as TcpListener;
            TcpClient client = listener.AcceptTcpClient();
            TcpSocket terrariaSocket = new TcpSocket(client);
            ISocket socket = terrariaSocket as ISocket;

            Console.WriteLine("server loop started");

            RemoteServer server = new RemoteServer();

            DimensionNetwork.loading = true;

            server.Socket = terrariaSocket;
            while (DimensionNetwork.loading)
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
                                writer.Write(DimensionNetwork.loading);
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

        private void ClientTickUpdateTransit()
        {
            if (Main.LocalPlayer == null || Main.LocalPlayer.name == "")
            {
                return;
            }

            DimPlayer dimPlayer = Main.LocalPlayer.GetModPlayer<DimPlayer>();
            if (!dimPlayer.inTransit || threadLaunched)
            {
                return;
            }


            threadLaunched = true;

            ThreadPool.QueueUserWorkItem(ClientThread, new object[] { dimPlayer });

        }

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
                            DimensionNetwork.loading = reader.ReadBoolean();
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
                    exitThread = DimensionNetwork.loading;
                }
                catch (Exception e)
                {
                    LogManager.GetLogger("Second thread").Error(e.Message, e);
                }

            }
            Netplay.Connection.Socket.Close();
            Netplay.StartTcpClient();
            player.inTransit = false;
            threadLaunched = false;
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


        /*public static void WorldUpdate(On.Terraria.WorldGen.orig_UpdateWorld orig)
        {
            if (!DimensionHandler.FreezeWorldUpdate)
            {
                orig.Invoke();
            }
        }*/
    }
}
