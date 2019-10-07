using log4net;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.IO;
using Terraria.Localization;
using Terraria.World.Generation;

namespace Dimlibs.API.DimensionHandler
{
	public sealed partial class DimensionHandler
	{
		public void Load()
		{

			Dimlibs.Instance.Logger.Info(Path.Combine(Main.ActiveWorldFileData.Path.Replace(".wld", ""), ActiveDimensionName.Split(':')[1]) + "\\Trueheader.data");
			if (!File.Exists(Path.Combine(Main.ActiveWorldFileData.Path.Replace(".wld", ""), ActiveDimensionName.Split(':')[1]) + "\\Trueheader.data")
				|| !Directory.Exists(Path.Combine(Main.ActiveWorldFileData.Path.Replace(".wld", ""), ActiveDimensionName.Split(':')[1])))
			{
				Console.WriteLine("I'm a bitch");
				Directory.CreateDirectory(Path.Combine(Main.ActiveWorldFileData.Path.Replace(".wld", ""), ActiveDimensionName.Split(':')[1]));
				Dimlibs.dimensionInstanceHandlers[ActiveDimensionName].GenerateDimension(Main.ActiveWorldFileData.Seed, new GenerationProgress());
			}


			bool[] importance = null;
			int[] position = null;

			loading = true;

			ILog log = LogManager.GetLogger("logger");
			Console.Write("Loading dimension...");
			log.Info("Loading save file header...");
			LoadFileFormatTrueHeader(out importance, out position);
			log.Info("Loading world size data...");
			LoadHeader();
			log.Info("Attempting to load NPC data");
			LoadNPC();
			log.Info("Loading the tile, really important");
			LoadTile(importance);
			log.Info("Loading chest data");
			LoadChests();
			log.Info("Loading modded data");
			LoadModdedStuff();
			//Still need to do TileEntity
			log.Info("done");

			if (Main.netMode == 0)
			{
				WorldGen.EveryTileFrame();
				Main.LocalPlayer.Spawn();
			}

			Console.Write("Done");
			loading = false;
		}


		public void LoadHeader()
		{
			string headerPath = Path.Combine(Main.ActiveWorldFileData.Path.Replace(".wld", ""), ActiveDimensionName.Split(':')[1]) + "\\header.data";

			using (BinaryReader headerReader = new BinaryReader(File.OpenRead(headerPath)))
			{

				Main.maxTilesX = headerReader.ReadInt32();
				Main.maxTilesY = headerReader.ReadInt32();

				Main.rightWorld = headerReader.ReadSingle();
				Main.bottomWorld = headerReader.ReadSingle();
				Main.leftWorld = headerReader.ReadSingle();
				Main.topWorld = headerReader.ReadSingle();

				Main.worldSurface = headerReader.ReadDouble();
				Main.rockLayer = headerReader.ReadDouble();
			}
		}

		public void LoadNPC()
		{
			string path = Path.Combine(Main.ActiveWorldFileData.Path.Replace(".wld", ""), ActiveDimensionName.Split(':')[1]) + "\\NPC.data";

			if (!File.Exists(path))
			{
				return;
			}

			using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))
			{
				int activeNPCCount = reader.ReadInt32();
				for (int i = 0; i < activeNPCCount; i++)
				{
					NPC npc = new NPC();
					npc.SetDefaults(reader.ReadInt32());

					Vector2 position = new Vector2(reader.ReadSingle(), reader.ReadSingle());
					Vector2 velocity = new Vector2(reader.ReadSingle(), reader.ReadSingle());

					npc.position = position;
					npc.velocity = velocity;

					for (int j = 0; j < 4; j++)
					{
						npc.ai[j] = reader.ReadSingle();
					}

					npc.altTexture = reader.ReadInt32();

					npc.life = reader.ReadInt32();

					int buffCount = reader.ReadInt32();
					//Don't load if the number of buff isn't the same
					for (int j = 0; j < buffCount; j++)
					{
						reader.ReadInt32();
					}
				}
			}
		}

		public void LoadTile(bool[] importance)
		{
			string path = Path.Combine(Main.ActiveWorldFileData.Path.Replace(".wld", ""), ActiveDimensionName.Split(':')[1]) + "\\Tile.data";

			Main.tile = new Tile[Main.maxTilesX, Main.maxTilesY];

			for (int i = 0; i < Main.maxTilesX; i++)
			{
				for (int j = 0; j < Main.maxTilesY; j++)
				{
					Main.tile[i, j] = new Tile();
				}

			}

			using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))
			{
				for (int i = 0; i < Main.maxTilesX; i++)
				{
					float num = (float)i / (float)Main.maxTilesX;
					Main.statusText = string.Concat(new object[]
						{
						Lang.gen[51].Value,
						" ",
						(int)((double)num * 100.0 + 1.0),
						"%"
						});
					for (int j = 0; j < Main.maxTilesY; j++)
					{
						int num2 = -1;
						byte b2;
						byte b = b2 = 0;
						Tile tile = Main.tile[i, j];
						byte b3 = reader.ReadByte();
						if ((b3 & 1) == 1)
						{
							b2 = reader.ReadByte();
							if ((b2 & 1) == 1)
							{
								b = reader.ReadByte();
							}
						}
						byte b4;
						if ((b3 & 2) == 2)
						{
							tile.active(true);
							if ((b3 & 32) == 32)
							{
								b4 = reader.ReadByte();
								num2 = (int)reader.ReadByte();
								num2 = (num2 << 8 | (int)b4);
							}
							else
							{
								num2 = (int)reader.ReadByte();
							}
							tile.type = (ushort)num2;
							if (num2 < importance.Length && importance[num2])
							{
								tile.frameX = reader.ReadInt16();
								tile.frameY = reader.ReadInt16();
								if (tile.type == 144)
								{
									tile.frameY = 0;
								}
							}
							else
							{
								tile.frameX = -1;
								tile.frameY = -1;
							}
							if ((b & 8) == 8)
							{
								tile.color(reader.ReadByte());
							}
						}
						if ((b3 & 4) == 4)
						{
							tile.wall = reader.ReadByte();
							if ((b & 16) == 16)
							{
								tile.wallColor(reader.ReadByte());
							}
						}
						b4 = (byte)((b3 & 24) >> 3);
						if (b4 != 0)
						{
							tile.liquid = reader.ReadByte();
							if (b4 > 1)
							{
								if (b4 == 2)
								{
									tile.lava(true);
								}
								else
								{
									tile.honey(true);
								}
							}
						}
						if (b2 > 1)
						{
							if ((b2 & 2) == 2)
							{
								tile.wire(true);
							}
							if ((b2 & 4) == 4)
							{
								tile.wire2(true);
							}
							if ((b2 & 8) == 8)
							{
								tile.wire3(true);
							}
							b4 = (byte)((b2 & 112) >> 4);
							if (b4 != 0 && Main.tileSolid[(int)tile.type])
							{
								if (b4 == 1)
								{
									tile.halfBrick(true);
								}
								else
								{
									tile.slope((byte)(b4 - 1));
								}
							}
						}
						if (b > 0)
						{
							if ((b & 2) == 2)
							{
								tile.actuator(true);
							}
							if ((b & 4) == 4)
							{
								tile.inActive(true);
							}
							if ((b & 32) == 32)
							{
								tile.wire4(true);
							}
						}
						b4 = (byte)((b3 & 192) >> 6);
						int k;
						if (b4 == 0)
						{
							k = 0;
						}
						else if (b4 == 1)
						{
							k = (int)reader.ReadByte();
						}
						else
						{
							k = (int)reader.ReadInt16();
						}
						if (num2 != -1)
						{
							if ((double)j <= Main.worldSurface)
							{
								if ((double)(j + k) <= Main.worldSurface)
								{
									WorldGen.tileCounts[num2] += (k + 1) * 5;
								}
								else
								{
									int num3 = (int)(Main.worldSurface - (double)j + 1.0);
									int num4 = k + 1 - num3;
									WorldGen.tileCounts[num2] += num3 * 5 + num4;
								}
							}
							else
							{
								WorldGen.tileCounts[num2] += k + 1;
							}
						}
						while (k > 0)
						{
							j++;
							Main.tile[i, j].CopyFrom(tile);
							k--;
						}
					}
				}
				WorldGen.AddUpAlignmentCounts(true);
				if (WorldFile.versionNumber < 105)
				{
					WorldGen.FixHearts();
				}
			}
		}

		public void LoadChests()
		{
			string path = Path.Combine(Main.ActiveWorldFileData.Path.Replace(".wld", ""), ActiveDimensionName.Split(':')[1]) + "\\chest.data";
			using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))
			{
				int num = (int)reader.ReadInt16();
				int num2 = (int)reader.ReadInt16();
				int num3;
				int num4;
				if (num2 < 40)
				{
					num3 = num2;
					num4 = 0;
				}
				else
				{
					num3 = 40;
					num4 = num2 - 40;
				}
				int i;
				for (i = 0; i < num; i++)
				{
					Chest chest = new Chest(false);
					chest.x = reader.ReadInt32();
					chest.y = reader.ReadInt32();
					chest.name = reader.ReadString();
					for (int j = 0; j < num3; j++)
					{
						short num5 = reader.ReadInt16();
						Item item = new Item();
						if (num5 > 0)
						{
							item.netDefaults(reader.ReadInt32());
							item.stack = (int)num5;
							item.Prefix((int)reader.ReadByte());
						}
						else if (num5 < 0)
						{
							item.netDefaults(reader.ReadInt32());
							item.Prefix((int)reader.ReadByte());
							item.stack = 1;
						}
						chest.item[j] = item;
					}
					for (int j = 0; j < num4; j++)
					{
						short num5 = reader.ReadInt16();
						if (num5 > 0)
						{
							reader.ReadInt32();
							reader.ReadByte();
						}
					}
					Main.chest[i] = chest;
				}
				List<Point16> list = new List<Point16>();
				for (int k = 0; k < i; k++)
				{
					if (Main.chest[k] != null)
					{
						Point16 item2 = new Point16(Main.chest[k].x, Main.chest[k].y);
						if (list.Contains(item2))
						{
							Main.chest[k] = null;
						}
						else
						{
							list.Add(item2);
						}
					}
				}
				while (i < 1000)
				{
					Main.chest[i] = null;
					i++;
				}
				if (WorldFile.versionNumber < 115)
				{
					WorldFile.FixDresserChests();
				}
			}
		}

		internal void LoadModdedStuff()
		{
			string path = Path.Combine(Main.ActiveWorldFileData.Path.Replace(".wld", ""), ActiveDimensionName.Split(':')[1]) + "\\modded.data";

			using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))
			{
				int bufferSize = reader.ReadInt32();
				Terraria.ModLoader.IO.TagCompound tag = Terraria.ModLoader.IO.TagIO.FromStream(new MemoryStream(reader.ReadBytes(bufferSize)));

				typeof(Main).Assembly.GetType("Terraria.ModLoader.IO.TileIO")
					.GetMethod("LoadTiles", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { tag.GetCompound("tiles") });
				typeof(Main).Assembly.GetType("Terraria.ModLoader.IO.TileIO")
					.GetMethod("LoadContainers", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { tag.GetCompound("containers") });
			}
		}

		private void LoadFileFormatTrueHeader(out bool[] importance, out int[] positions)
		{
			string path = Path.Combine(Main.ActiveWorldFileData.Path.Replace(".wld", ""), ActiveDimensionName.Split(':')[1]) + "\\Trueheader.data";
			using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))
			{
				importance = null;
				positions = null;
				int num = reader.ReadInt32();
				WorldFile.versionNumber = num;
				if (num >= 135)
				{
					try
					{
						Main.WorldFileMetadata = FileMetadata.Read(reader, FileType.World);
						goto IL_54;
					}
					catch (FileFormatException value)
					{
						Console.WriteLine(Language.GetTextValue("Error.UnableToLoadWorld"));
						Console.WriteLine(value);
					}
				}
				Main.WorldFileMetadata = FileMetadata.FromCurrentSettings(FileType.World);
			IL_54:
				short num2 = reader.ReadInt16();
				positions = new int[(int)num2];
				for (int i = 0; i < (int)num2; i++)
				{
					positions[i] = reader.ReadInt32();
				}
				short num3 = reader.ReadInt16();
				importance = new bool[(int)num3];
				byte b = 0;
				byte b2 = 128;
				for (int i = 0; i < (int)num3; i++)
				{
					if (b2 == 128)
					{
						b = reader.ReadByte();
						b2 = 1;
					}
					else
					{
						b2 = (byte)(b2 << 1);
					}
					if ((b & b2) == b2)
					{
						importance[i] = true;
					}
				}
			}
		}


		public void do_LoadDimensionCallBack(object ThreadContext)
		{
			try
			{
				Load();
			}
			catch (Exception e)
			{
				ILog log = LogManager.GetLogger("Dimension Callback");
				log.Error(e.Message, e);
			}
		}
	}
}
