using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Terraria;
using Terraria.Chat;
using Terraria.GameContent.NetModules;
using Terraria.GameContent.UI.Chat;
using Terraria.Localization;
using Terraria.Net;
using Terraria.Net.Sockets;
using Terraria.UI.Chat;
using NetManager = On.Terraria.Net.NetManager;
using NetPacket = Terraria.Net.NetPacket;
using RemoteAddress = Terraria.Net.RemoteAddress;

namespace Dimlibs.Network
{
	internal class ChatServer
	{
		private readonly int _port = 7776;

		private static ISocket _socket;
		public static RemoteServer connection;
		public static RemoteClient client;

		public static ChatServer instance = new ChatServer();

		public static List<TcpClient> localServer;
		public static List<TcpClient> otherPlayer;

		private NetModule module = new NetTextModule();

		private ChatServer()
		{

		}

		public void Load()
		{


			//Dimlibs.Instance.Logger.Info("Is chat socket open? " + IsChatOpen());

			Dimlibs.Instance.Logger.Info("Chat is initializing");

			try
			{
				if (!Program.LaunchParameters.ContainsKey("-chat"))
				{
					TcpListener listener = new TcpListener(IPAddress.Any, _port);
					listener.Start();
					ThreadPool.QueueUserWorkItem(InitializeChatRelay, listener);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}

			while (true)
			{
				if (_socket != null && _socket.IsConnected())
				{
					break;
				}
			}



			if (Main.netMode == 2)
			{
				On.Terraria.Net.NetManager.SendToServer += NetManagerOnSendToServer;
				_socket = new TcpSocket();
				Netplay.SetRemoteIP("127.0.0.1");
				_socket.Connect(new Terraria.Net.TcpAddress(Netplay.ServerIP, _port));
				connection = new RemoteServer();
				connection.Socket = _socket;
				connection.ReadBuffer = new byte[ushort.MaxValue];
			}
			else
			{
				On.Terraria.Net.NetManager.SendToClient += NetManagerOnSendToClient;
				_socket = new TcpSocket();
				Netplay.SetRemoteIP("127.0.0.1");
				_socket.Connect(new Terraria.Net.TcpAddress(Netplay.ServerIP, _port));
				connection = new RemoteServer();
				connection.Socket = _socket;
				connection.ReadBuffer = new byte[ushort.MaxValue];
			}

			Dimlibs.Instance.Logger.Info("Chat is initialized and ready to go");
		}


		/// <summary>
		/// Initalize Server chat relay, possible message:
		/// First Byte : Send/Receive chat message
		/// Second Byte : From server/client
		/// Rest : The text from the chat
		/// </summary>
		/// <param name="server"></param>
		internal void InitializeChatRelay(object server)
		{
			TcpListener listener = server as TcpListener;
			TcpClient client = listener.AcceptTcpClient();
			TcpSocket terrariaSocket = new TcpSocket(client);
			ISocket chatRelay = terrariaSocket as ISocket;
			
			Dimlibs.Instance.Logger.Info("Chat initializing");

			while (true)
			{
				if (chatRelay.IsConnected())
				{
					if (listener.Pending())
					{
						TcpClient newClient = listener.AcceptTcpClient();
						IPEndPoint endpoint = newClient.Client.RemoteEndPoint as IPEndPoint;
						if (endpoint.Address.Equals(IPAddress.Parse("127.0.0.1")))
						{
							localServer.Add(newClient);
						}
						else
						{
							otherPlayer.Add(newClient);
						}
					}

					if (chatRelay.IsDataAvailable())
					{
						byte[] data = new byte[ushort.MaxValue];
						_socket.AsyncReceive(data, 0, ushort.MaxValue, new SocketReceiveCallback(Netplay.Connection.ClientReadCallBack));
						using (MemoryStream stream = new MemoryStream(data))
						{
							using (BinaryReader reader = new BinaryReader(stream))
							{
								byte playerID = reader.ReadByte();
							}
						}

					}
				}
			}
		}

		private bool DeserializeAsClient(BinaryReader reader, int senderPlayerId)
		{
			byte b = reader.ReadByte();
			string text = NetworkText.Deserialize(reader).ToString();
			Color c = reader.ReadRGB();
			if (b < 255)
			{
				Main.player[(int)b].chatOverhead.NewMessage(text, Main.chatLength / 2);
				text = NameTagHandler.GenerateTag(Main.player[(int)b].name) + " " + text;
			}
			Main.NewTextMultiline(text, false, c, -1);
			return true;
		}

		private bool DeserializeAsServer(BinaryReader reader, int senderPlayerId)
		{
			ChatMessage message = ChatMessage.Deserialize(reader);
			ChatManager.Commands.ProcessReceivedMessage(message, senderPlayerId);
			return true;
		}

		public void Unload()
		{
			if (Main.netMode == 2)
			{
				On.Terraria.Net.NetManager.SendToServer -= NetManagerOnSendToServer;
				connection = null;
			}
			else
			{
				On.Terraria.Net.NetManager.SendToClient -= NetManagerOnSendToClient;
			}
			_socket = null;
		}

		private void NetManagerOnSendToClient(NetManager.orig_SendToClient orig, Terraria.Net.NetManager self, NetPacket packet, int playerid)
		{
			//if (IsChatOpen())
			{
				packet.ShrinkToFit();
				_socket.AsyncSend(packet.Buffer.Data, 0, packet.Length, new SocketSendCallback(SendCallback), packet);
				//typeof(NetManager).GetMethod("SendData", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(Terraria.Net.NetManager.Instance, new object[] { connection.Socket, packet });
			}
		}

		private void NetManagerOnSendToServer(NetManager.orig_SendToServer orig, Terraria.Net.NetManager self, NetPacket packet)
		{
			//if (IsChatOpen())
			foreach (TcpClient tcpClient in localServer)
			{
				Console.WriteLine("Sending to server");
				if (tcpClient.Client.Connected)
				{
					byte[] data = packet.Buffer.Data;
					using (MemoryStream stream = new MemoryStream())
					{
						using (BinaryWriter writer = new BinaryWriter(stream))
						{
							writer.Write(0);
							using (MemoryStream stream2 = new MemoryStream(data))
							{
								stream2.WriteTo(stream);
							}
							_socket.AsyncSend(stream.GetBuffer(), 0, packet.Length, new SocketSendCallback(SendCallback), packet);
						}
					}
					
					
				}
			}
			//typeof(NetManager).GetMethod("SendData", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(Terraria.Net.NetManager.Instance, new object[] {connection.Socket, packet });
		}

		public static bool IsChatOpen()
		{
			Socket _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
			try
			{
				_socket.Connect(Netplay.ServerIP, 7776);
				if (_socket.Connected)
				{
					_socket.Disconnect(true);
					return true;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
			return false;
		}

		public static void SendCallback(object state)
		{
			((NetPacket)state).Recycle();
		}
	}
}
