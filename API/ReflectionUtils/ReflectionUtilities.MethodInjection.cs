using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dimlibs.Network;
using log4net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.States;
using Terraria.IO;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.World.Generation;

namespace Dimlibs.API.ReflectionUtils
{
	public static partial class ReflectionUtilities
	{
		public static void DrawSelfLoad(On.Terraria.GameContent.UI.States.UIWorldLoad.orig_DrawSelf orig,
			UIWorldLoad instance, SpriteBatch sb)
		{
			Viewport dimension = Main.graphics.GraphicsDevice.Viewport;
			Texture2D texture = Dimlibs.Instance.GetTexture("Texture/LoadingScreen3");
			for (int i = 0; i < dimension.Width; i += texture.Width)
			{
				for (int j = 0; j < dimension.Height; j += texture.Height)
				{
					sb.Draw(texture, new Rectangle(i, j, texture.Width, texture.Height), null, Color.White, 0f,
						Vector2.Zero, SpriteEffects.None, 0f);
				}
			}
			Texture2D logo = Dimlibs.Instance.GetTexture("Texture/TerrariaLogo");
			Vector2 TerrariaLogo = new Vector2(Main.screenWidth / 2 - logo.Width / 2, 40);
			sb.Draw(logo, TerrariaLogo, Color.White);
			orig.Invoke(instance, sb);
		}

		public static void DrawToMap_Section(On.Terraria.Main.orig_DrawToMap_Section org, Main instance, int secX, int secY)
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			Microsoft.Xna.Framework.Color[] mapColorCacheArray = (Color[])typeof(Main).GetField("_mapColorCacheArray", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
			int num = secX * 200;
			int num2 = num + 200;
			int num3 = secY * 150;
			int num4 = num3 + 150;
			int num5 = num / Main.textureMaxWidth;
			int num6 = num3 / Main.textureMaxHeight;
			int num7 = num % Main.textureMaxWidth;
			int num8 = num3 % Main.textureMaxHeight;

			bool flag = (bool)typeof(Main).GetMethod("checkMap", BindingFlags.Instance | BindingFlags.NonPublic)
				.Invoke(instance, new object[] { num5, num6 });
			if (!flag)
			{
				return;
			}
			int num9 = 0;
			Microsoft.Xna.Framework.Color arg_6A_0 = Microsoft.Xna.Framework.Color.Transparent;
			for (int i = num3; i < num4; i++)
			{
				for (int j = num; j < num2; j++)
				{
					MapTile mapTile = Main.Map[j, i];
					mapColorCacheArray[num9] = MapHelper.GetMapTileXnaColor(ref mapTile);
					num9++;
				}
			}
			try
			{
				instance.GraphicsDevice.SetRenderTarget(instance.mapTarget[num5, num6]);
			}
			catch (ObjectDisposedException)
			{
				Main.initMap[num5, num6] = false;
				return;
			}
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
			double totalMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
			instance.mapSectionTexture.SetData<Microsoft.Xna.Framework.Color>(mapColorCacheArray, 0, mapColorCacheArray.Length);
			double arg_128_0 = stopwatch.Elapsed.TotalMilliseconds;
			totalMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
			Main.spriteBatch.Draw(instance.mapSectionTexture, new Vector2((float)num7, (float)num8), Microsoft.Xna.Framework.Color.White);
			Main.spriteBatch.End();
			instance.GraphicsDevice.SetRenderTarget(null);
			double arg_17F_0 = stopwatch.Elapsed.TotalMilliseconds;
			stopwatch.Stop();
		}

		public static void Spawn(On.Terraria.Player.orig_Spawn orig, Player instance)
		{
			Main.InitLifeBytes();
			if (instance.whoAmI == Main.myPlayer)
			{
				if (Main.mapTime < 5)
				{
					Main.mapTime = 5;
				}
				Main.quickBG = 10;
				instance.FindSpawn();
				if (!Player.CheckSpawn(instance.SpawnX, instance.SpawnY))
				{
					instance.SpawnX = -1;
					instance.SpawnY = -1;
				}
				Main.maxQ = true;
			}
			if (Main.netMode == 1 && instance.whoAmI == Main.myPlayer)
			{
				NetMessage.SendData(12, -1, -1, null, Main.myPlayer, 0f, 0f, 0f, 0, 0, 0);
				Main.gameMenu = false;
			}
			instance.headPosition = Vector2.Zero;
			instance.bodyPosition = Vector2.Zero;
			instance.legPosition = Vector2.Zero;
			instance.headRotation = 0f;
			instance.bodyRotation = 0f;
			instance.legRotation = 0f;
			instance.lavaTime = instance.lavaMax;
			if (instance.statLife <= 0)
			{
				int num = instance.statLifeMax2 / 2;
				instance.statLife = 100;
				if (num > instance.statLife)
				{
					instance.statLife = num;
				}
				instance.breath = instance.breathMax;
				if (instance.spawnMax)
				{
					instance.statLife = instance.statLifeMax2;
					instance.statMana = instance.statManaMax2;
				}
			}
			instance.immune = true;
			if (instance.dead)
			{
				PlayerHooks.OnRespawn(instance);
			}

			instance.dead = false;
			instance.immuneTime = 0;
			instance.active = true;
			if (instance.SpawnX >= 0 && instance.SpawnY >= 0)
			{
				instance.position.X = (float)(instance.SpawnX * 16 + 8 - instance.width / 2);
				instance.position.Y = (float)(instance.SpawnY * 16 - instance.height);
			}
			else
			{
				instance.position.X = (float)(Main.spawnTileX * 16 + 8 - instance.width / 2);
				instance.position.Y = (float)(Main.spawnTileY * 16 - instance.height);
				for (int i = Main.spawnTileX - 1; i < Main.spawnTileX + 2; i++)
				{
					for (int j = Main.spawnTileY - 3; j < Main.spawnTileY; j++)
					{
						ILog log = LogManager.GetLogger("Temp dim logger");
						log.Info($"Spawn tile : {i} , {j}");
						log.Info($"World size : {Main.tile.Length}");
						if (i < Main.tile.GetLength(0) && j < Main.tile.GetLength(1) && Main.tile[i, j] != null)
						{
							if (Main.tileSolid[(int)Main.tile[i, j].type] && !Main.tileSolidTop[(int)Main.tile[i, j].type])
							{
								WorldGen.KillTile(i, j, false, false, false);
							}
							if (Main.tile[i, j].liquid > 0)
							{
								Main.tile[i, j].lava(false);
								Main.tile[i, j].liquid = 0;
								WorldGen.SquareTileFrame(i, j, true);
							}
						}
					}
				}
			}
			instance.wet = false;
			instance.wetCount = 0;
			instance.lavaWet = false;
			instance.fallStart = (int)(instance.position.Y / 16f);
			instance.fallStart2 = instance.fallStart;
			instance.velocity.X = 0f;
			instance.velocity.Y = 0f;
			for (int k = 0; k < 3; k++)
			{
				instance.UpdateSocialShadow();
			}
			instance.oldPosition = instance.position + instance.BlehOldPositionFixer;
			instance.talkNPC = -1;
			if (instance.whoAmI == Main.myPlayer)
			{
				Main.npcChatCornerItem = 0;
			}
			if (instance.pvpDeath)
			{
				instance.pvpDeath = false;
				instance.immuneTime = 300;
				instance.statLife = instance.statLifeMax;
			}
			else
			{
				instance.immuneTime = 60;
			}
			if (instance.whoAmI == Main.myPlayer)
			{
				Main.BlackFadeIn = 255;
				Main.renderNow = true;
				if (Main.netMode == 1)
				{
					Netplay.newRecent();
				}
				Main.screenPosition.X = instance.position.X + (float)(instance.width / 2) - (float)(Main.screenWidth / 2);
				Main.screenPosition.Y = instance.position.Y + (float)(instance.height / 2) - (float)(Main.screenHeight / 2);
			}
		}

		internal static void do_worldGenCallBack(ProjectDimensionHook.orig_do_worldGenCallBack orig, object threadContext)
		{
			Main.PlaySound(10, -1, -1, 1, 1f, 0f);
			foreach (ModDimension handler in Dimlibs.dimensionInstanceHandlers.Values)
			{
				Dimlibs.dimension = handler.URN;
				WorldGen.clearWorld();
				handler.GenerateDimension(Main.ActiveWorldFileData.Seed, threadContext as GenerationProgress);
				handler.handler.Save();
				WorldFile.saveWorld(Main.ActiveWorldFileData.IsCloudSave, true);
			}
			if (Main.menuMode == 10 || Main.menuMode == 888)
			{
				Main.menuMode = 6;
			}
			Main.PlaySound(10, -1, -1, 1, 1f, 0f);
		}

		public static void SaveWorld(On.Terraria.IO.WorldFile.orig_saveWorld_bool_bool orig, bool useCloudSaving, bool resetTime = false)
		{
			if (Dimlibs.dimension == "Dimlibs: OverworldDimension")
				orig.Invoke(useCloudSaving, resetTime);
			Dimlibs.dimensionInstanceHandlers[Dimlibs.dimension].handler.Save();

		}

		public static void LoadWorld(On.Terraria.IO.WorldFile.orig_loadWorld orig, bool loadFromCloud)
		{
			int startingPort;
			ChatServer.instance.Load();
			if (Main.netMode == 0)
			{
				startingPort = 7777;
				StartServer(ref startingPort, false);
				Main.netMode = 1;
				Netplay.SetRemoteIP("127.0.0.1");
				Netplay.ListenPort = 7777;
				Netplay.Connection.Socket.Close();
				Netplay.StartTcpClient();
			}
			else if (Main.netMode == 2)
			{
				orig.Invoke(loadFromCloud);
				if (Netplay.ListenPort == 7777 && Program.LaunchParameters.ContainsKey("FromHost"))
				{
					startingPort = 7778;
					StartServer(ref startingPort, true);
				}

				if (Program.LaunchParameters.ContainsKey("-port"))
				{
					Netplay.ListenPort = Int32.Parse(Program.LaunchParameters["-port"]);
				}
				Dimlibs.Instance.Logger.Info(Netplay.ListenPort);
				Dimlibs.dimensionInstanceHandlers[Dimlibs.dimension].handler.Load();
			}
		}

	}
}
