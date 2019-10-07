using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dimlibs.API;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.World.Generation;

namespace Dimlibs.Test
{
	class DungeonWorld : ModDimension
	{
		public override void ModifyGenerationPass(int seed, GenerationProgress customProgressObject)
		{
			AddGenerationPass("Init", delegate(GenerationProgress progress)
			{
				progress.Message = "init";

				int maxtilesX  = 1000;
				int maxtilesY = 750;

				FieldInfo info = typeof(WorldFileData).GetField("WorldSizeX", BindingFlags.Instance | BindingFlags.Public);
				int get = (int)info.GetValue(Main.ActiveWorldFileData);
				info.SetValue(Main.ActiveWorldFileData, maxtilesX);

				info = typeof(WorldFileData).GetField("WorldSizeY", BindingFlags.Instance | BindingFlags.Public);
				get = (int)info.GetValue(Main.ActiveWorldFileData);
				info.SetValue(Main.ActiveWorldFileData, maxtilesY);

				info = typeof(WorldGen).GetField("lastMaxTilesX",
					BindingFlags.Static | BindingFlags.NonPublic);
				get = (int)info.GetValue(null);
				info.SetValue(null, maxtilesX);

				info = typeof(WorldGen).GetField("lastMaxTilesY",
					BindingFlags.Static | BindingFlags.NonPublic);
				get = (int)info.GetValue(null);
				info.SetValue(null, maxtilesY);

				Main.maxTilesX = maxtilesX;
				Main.maxTilesY = maxtilesY;

				Main.rightWorld = maxtilesX * 16;
				Main.bottomWorld = maxtilesY * 16;
				Main.maxSectionsX = Main.maxTilesX / 200;
				Main.maxSectionsY = Main.maxTilesY / 150;

				Main.Map = new Terraria.Map.WorldMap(maxtilesX, maxtilesY);

				int mapSizeX = maxtilesX / Main.textureMaxWidth + 1;
				int mapSizeY = maxtilesY / Main.textureMaxHeight + 1;

				Main.mapTargetX = mapSizeX;
				Main.mapTargetY = mapSizeY;

				Main.instance.mapTarget = new Microsoft.Xna.Framework.Graphics.RenderTarget2D[mapSizeX, mapSizeY];
				Main.initMap = new bool[mapSizeX, mapSizeY];
				Main.mapWasContentLost = new bool[mapSizeX, mapSizeY];

				WorldGen.clearWorld();
			});

			AddGenerationPass("Wall", delegate(GenerationProgress progress)
			{
				for (int i = 0; i < Main.maxTilesX; i++)
				{
					for (int j = 0; j < Main.maxTilesY; j++)
					{
						Main.tile[i, j].wall = WallID.BlueDungeon;
					}
				}
			});

			AddGenerationPass("SpawnPoint", delegate(GenerationProgress progress)
			{
				Point spawnpoint = new Point(Main.maxTilesX / 2, Main.maxTilesY / 2);
				Main.spawnTileX = spawnpoint.X;
				Main.spawnTileY = spawnpoint.Y - 3;
				PutBlock(spawnpoint.X - 1, spawnpoint.Y, TileID.CobaltBrick);
				PutBlock(spawnpoint.X, spawnpoint.Y, TileID.CobaltBrick);
				PutBlock(spawnpoint.X + 1, spawnpoint.Y, TileID.CobaltBrick);
			});
		}

		private void PutBlock(int x, int y, ushort tileID)
		{
			Main.tile[x, y].active(true);
			Main.tile[x, y].type = tileID;
		}
	}

}
