using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Terraria;
using Terraria.Localization;
using Terraria.Utilities;

namespace Dimlibs.Network
{
	class DimensionServer
    {
        private string _hostName = Netplay.GetLocalIPAddress();
        private readonly int _port;
	    private readonly string _dimension;

	    private readonly Process _process = new Process();
	    private Socket _socket;

	    public bool isActive = false;

        public DimensionServer(string dimension, int port, bool SPHost)
        {
	        this._dimension = dimension;
            this._port = port;
	        string text = string.Concat(new object[]
	        {
		        "-password \"",
		        Netplay.ServerPassword,
		        "\" -lang ",
		        Language.ActiveCulture.LegacyId
	        });

	        text +=  " -world \"" + Main.worldPathName + "\"";
	        text += $" -dimension {dimension}";
	        text += $" -port {port}";
	        text += $" -originalPort 7777";
	        text += $" -chat true";
	        if (SPHost)
		        text += $" -FromHost true";


			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
		        _process.StartInfo.FileName = "tModLoaderServer.exe";
	        else
		        _process.StartInfo.FileName = "tModLoaderServer";

	        _process.StartInfo.Arguments = text;
	        _process.StartInfo.UseShellExecute = false;
	        _process.StartInfo.CreateNoWindow = false;

	        _process.Start();
		}

	    public void Connect()
	    {
			Netplay.Connection.Socket.Close();
		    try
		    {
			    Netplay.Connection.Socket.Close();
		    }
		    catch
		    {
		    }
		    if (!Main.gameMenu)
		    {
			    Main.SwitchNetMode(0);
			    Player.SavePlayer(Main.ActivePlayerFileData, false);
			    Main.ActivePlayerFileData.StopPlayTimer();
			    Main.gameMenu = true;
			    Main.StopTrackedSounds();
			    Main.menuMode = 14;
		    }

		    Main.statusText = "Changing server";
			if (Netplay.ServerIP == IPAddress.Parse("127.0.0.1"))
		    {
			    DirectConnection();
		    }
	    }

	    public void DirectConnection()
	    {
		    Netplay.ListenPort = _port;
		    Netplay.SetRemoteIP("127.0.0.1");
			Netplay.Connection.Socket.Close();
		    Netplay.StartTcpClient();
		}

	    public void DistantConnection()
	    {
		    Netplay.ListenPort = _port;
		    Netplay.Connection.Socket.Close();
		    Netplay.StartTcpClient();
		}

	    public static bool IsSocketOpen(int _port)
	    {
			Socket _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
		    try
		    {
			    _socket.Connect(Netplay.ServerIP, _port);
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



        public void Shutdown()
        {
			_process.Close();
        }
    }
}
