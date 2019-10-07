using System;
using System.Reflection;
using System.Threading;
using Dimlibs.Network;
using Terraria.ModLoader;

namespace Dimlibs.API.ReflectionUtils
{
	public static partial class ReflectionUtilities
	{

		public static void SetStaticValue(this FieldInfo info, object value)
		{
			info.SetValue(null, value);
		}

		public static object GetStaticValue(this FieldInfo info)
		{
			return info.GetValue(null);
		}

		public static FieldInfo GetStaticPrivateField(this Type type, string fieldName)
		{
			return type.GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic);
		}

		public static FieldInfo GetInstancePrivateField(this Type type, string fieldName)
		{
			return type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
		}

		public static object InvokeStaticMethod(this Type type, string methodName, object[] args)
		{
			return type.GetMethod(methodName,
					BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
				.Invoke(null, args);
		}

		public static void Load()
		{
			On.Terraria.IO.WorldFile.saveWorld_bool_bool += SaveWorld;
			On.Terraria.IO.WorldFile.loadWorld += LoadWorld;
			MonoModHooks.RequestNativeAccess();
			ProjectDimensionHook.do_worldGenCallBack_Hook += do_worldGenCallBack;
			On.Terraria.Player.Spawn += Spawn;
			On.Terraria.Main.DrawToMap_Section += DrawToMap_Section;
			On.Terraria.GameContent.UI.States.UIWorldLoad.DrawSelf += DrawSelfLoad;
		}

		public static void Unload()
		{
			On.Terraria.IO.WorldFile.saveWorld_bool_bool -= SaveWorld;
			On.Terraria.IO.WorldFile.loadWorld -= LoadWorld;
			On.Terraria.Player.Spawn -= Spawn;
			On.Terraria.Main.DrawToMap_Section -= DrawToMap_Section;
			On.Terraria.GameContent.UI.States.UIWorldLoad.DrawSelf -= DrawSelfLoad;
			ProjectDimensionHook.do_worldGenCallBack_Hook -= do_worldGenCallBack;
		}
		

		private static void StartServer(ref int startingPort, bool IsServer)
		{
			foreach (var stuff in Dimlibs.dimensionInstanceHandlers.Keys)
			{
				
				if(IsServer && stuff == "Dimlibs:OverworldDimension") {continue;}
				DimensionServer server = new DimensionServer(stuff, startingPort, !IsServer);
				Thread.Sleep(2000);
				Dimlibs.server.Add(server);
				startingPort++;
			}
		}
	}
}
