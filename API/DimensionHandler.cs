using log4net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Dimlibs.UI;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.Map;
using ChatManager = Terraria.UI.Chat.ChatManager;

namespace Dimlibs.API
{
    internal class DimensionHandler
    {
        private Tile[,] dimensionTile;
        private readonly Projectile[] instanceProjectileArray;
        private Chest[] chest;
        private readonly WorldMap map;
        private readonly String _Path = "";
        internal DimGenerator generator;
        private String dimensionName;
        private int dimensionID;

        private readonly int maxTileX;
        private readonly int maxTileY;
        private readonly int spawnX;
        private readonly int spawnY;

        private readonly float bottomWorld;
        private readonly float topWorld;
        private readonly float leftWorld;
        private readonly float rightWorld;


        public int DimensionID
        {
            get { return dimensionID; }
            internal set { dimensionID = value; }
        }

        public String DimensionName
        {
            get { return dimensionName; }
            private set { dimensionName = value; }
        }

        public DimensionHandler(DimGenerator generator, String name)
        {
            this.generator = generator;
            DimensionName = name;

            DimWorld.AddDimension(this);
        }

        internal void setOverworldStats()
        {
            dimensionTile = Main.tile;
            chest = Main.chest;
        }


        public void Save()
        {
            if (!Directory.Exists(Path.Combine(Main.ActiveWorldFileData.Path.Replace(".wld", ""), dimensionName)))
            {
                Directory.CreateDirectory(
                    Path.Combine(Main.ActiveWorldFileData.Path.Replace(".wld", ""), dimensionName));
            }
            SaveFileFormatHeader();
            SaveFileHeader();
            SaveCurrentEntity();
            SaveCurrentTile();
            SaveChest();
            SaveModdedStuff();
        }

        private void SaveFileFormatHeader()
        {
            string headerPath = Path.Combine(Main.ActiveWorldFileData.Path.Replace(".wld", ""), dimensionName) + "/TrueHeader.data";
            if (File.Exists(headerPath))
            {
                File.Delete(headerPath);
            }

            using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(headerPath)))
            {
                short num = 470;
                short num2 = 10;
                writer.Write(194);
                Main.WorldFileMetadata.IncrementAndWrite(writer);
                writer.Write(num2);
                for (int i = 0; i < (int)num2; i++)
                {
                    writer.Write(0);
                }
                writer.Write(num);
                byte b = 0;
                byte b2 = 1;
                for (int i = 0; i < (int)num; i++)
                {
                    if (Main.tileFrameImportant[i])
                    {
                        b |= b2;
                    }
                    if (b2 == 128)
                    {
                        writer.Write(b);
                        b = 0;
                        b2 = 1;
                    }
                    else
                    {
                        b2 = (byte)(b2 << 1);
                    }
                }
                if (b2 != 1)
                {
                    writer.Write(b);
                }
            }
        }

        private void SaveFileHeader()
        {
            string headerPath = Path.Combine(Main.ActiveWorldFileData.Path.Replace(".wld", ""), dimensionName) + "/header.data";
            if (File.Exists(headerPath))
            {
                File.Delete(headerPath);
            }

            using (BinaryWriter headerWriter = new BinaryWriter(File.OpenWrite(headerPath)))
            {
                headerWriter.Write(Main.maxTilesX);
                headerWriter.Write(Main.maxTilesY);

                //Save the limit of the visible world, to avoid framing issue
                headerWriter.Write((float)(Main.maxTilesX * 16 - 16 * 7)); //Main.rightWorld
                headerWriter.Write((float)(Main.maxTilesY * 16 - 16 * 5)); //Main.bottomWorld
                headerWriter.Write(Main.leftWorld);
                headerWriter.Write(Main.topWorld);

                //Save the layer
                headerWriter.Write(Main.worldSurface);
                headerWriter.Write(Main.rockLayer);

            }
        }

        private void SaveCurrentEntity()
        {
            string path = Path.Combine(Main.ActiveWorldFileData.Path.Replace(".wld", ""), dimensionName) + "/NPC.data";
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(path)))
            {
                int activeNPCCount = Main.npc.Count(i => i != null && !i.townNPC);
                writer.Write(activeNPCCount);
                foreach (NPC npc in Main.npc)
                {
                    if (npc.townNPC)
                    {
                        continue;
                    }

                    //Save the type of NPC, will make it easier to load NPC later as a simple SetDefault is needed
                    writer.Write(npc.type);

                    //Save Position
                    writer.Write(npc.position.X);
                    writer.Write(npc.position.Y);

                    //Save velocity
                    writer.Write(npc.velocity.X);
                    writer.Write(npc.velocity.Y);

                    //Save the AI slot
                    for (int i = 0; i < npc.ai.Length; i++)
                    {
                        writer.Write(npc.ai[i]);
                    }

                    //Save alt texture, might be useful?
                    writer.Write(npc.altTexture);

                    //Save NPC health, because that is quite important
                    writer.Write(npc.life);

                    //Will make it load, if the amount of buff is the same
                    int buffCountAtThatTime = npc.buffTime.Length;
                    writer.Write(buffCountAtThatTime);
                    for (int i = 0; i < npc.buffTime.Length; i++)
                    {
                        writer.Write(npc.buffTime[i]);
                    }


                }

                writer.Flush();
                writer.Close();
            }
        }


        private void SaveCurrentTile()
        {
            string path = Path.Combine(Main.ActiveWorldFileData.Path.Replace(".wld", ""), dimensionName) + "/Tile.data";
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(path)))
            {
                byte[] array = new byte[13];
                for (int i = 0; i < Main.maxTilesX; i++)
                {
                    float num = (float)i / (float)Main.maxTilesX;
                    Main.statusText = string.Concat(new object[]
                    {
                        Lang.gen[49].Value,
                        " ",
                        (int) (num * 100f + 1f),
                        "%"
                    });
                    for (int j = 0; j < Main.maxTilesY; j++)
                    {
                        Tile tile = Main.tile[i, j];
                        int num2 = 3;
                        byte b3;
                        byte b2;
                        byte b = b2 = (b3 = 0);
                        bool flag = false;
                        if (tile.active() && tile.type < TileID.Count)
                        {
                            flag = true;
                            if (tile.type == 127)
                            {
                                WorldGen.KillTile(i, j, false, false, false);
                                if (!tile.active())
                                {
                                    flag = false;
                                    if (Main.netMode != 0)
                                    {
                                        NetMessage.SendData(17, -1, -1, null, 0, (float)i, (float)j, 0f, 0, 0, 0);
                                    }
                                }
                            }
                        }

                        if (flag)
                        {
                            b2 |= 2;
                            if (tile.type == 127)
                            {
                                WorldGen.KillTile(i, j, false, false, false);
                                if (!tile.active() && Main.netMode != 0)
                                {
                                    NetMessage.SendData(17, -1, -1, null, 0, (float)i, (float)j, 0f, 0, 0, 0);
                                }
                            }

                            array[num2] = (byte)tile.type;
                            num2++;
                            if (tile.type > 255)
                            {
                                array[num2] = (byte)(tile.type >> 8);
                                num2++;
                                b2 |= 32;
                            }

                            if (Main.tileFrameImportant[(int)tile.type])
                            {
                                short frameX = tile.frameX;
                                typeof(Main).Assembly.GetType("Terraria.ModLoader.IO.TileIO")
                                    .GetMethod("VanillaSaveFrames", BindingFlags.Static | BindingFlags.NonPublic)
                                    .Invoke(null, new object[] { tile, frameX });
                                array[num2] = (byte)(frameX & 255);
                                num2++;
                                array[num2] = (byte)(((int)frameX & 65280) >> 8);
                                num2++;
                                array[num2] = (byte)(tile.frameY & 255);
                                num2++;
                                array[num2] = (byte)(((int)tile.frameY & 65280) >> 8);
                                num2++;
                            }

                            if (tile.color() != 0)
                            {
                                b3 |= 8;
                                array[num2] = tile.color();
                                num2++;
                            }
                        }

                        if (tile.wall != 0 && tile.wall < WallID.Count)
                        {
                            b2 |= 4;
                            array[num2] = (byte)tile.wall;
                            num2++;
                            if (tile.wallColor() != 0)
                            {
                                b3 |= 16;
                                array[num2] = tile.wallColor();
                                num2++;
                            }
                        }

                        if (tile.liquid != 0)
                        {
                            if (tile.lava())
                            {
                                b2 |= 16;
                            }
                            else if (tile.honey())
                            {
                                b2 |= 24;
                            }
                            else
                            {
                                b2 |= 8;
                            }

                            array[num2] = tile.liquid;
                            num2++;
                        }

                        if (tile.wire())
                        {
                            b |= 2;
                        }

                        if (tile.wire2())
                        {
                            b |= 4;
                        }

                        if (tile.wire3())
                        {
                            b |= 8;
                        }

                        int num3;
                        if (tile.halfBrick())
                        {
                            num3 = 16;
                        }
                        else if (tile.slope() != 0)
                        {
                            num3 = (int)(tile.slope() + 1) << 4;
                        }
                        else
                        {
                            num3 = 0;
                        }

                        b |= (byte)num3;
                        if (tile.actuator())
                        {
                            b3 |= 2;
                        }

                        if (tile.inActive())
                        {
                            b3 |= 4;
                        }

                        if (tile.wire4())
                        {
                            b3 |= 32;
                        }

                        int num4 = 2;
                        if (b3 != 0)
                        {
                            b |= 1;
                            array[num4] = b3;
                            num4--;
                        }

                        if (b != 0)
                        {
                            b2 |= 1;
                            array[num4] = b;
                            num4--;
                        }

                        short num5 = 0;
                        int num6 = j + 1;
                        int num7 = Main.maxTilesY - j - 1;
                        while (num7 > 0 && tile.isTheSameAs(Main.tile[i, num6]))
                        {
                            num5 += 1;
                            num7--;
                            num6++;
                        }

                        j += (int)num5;
                        if (num5 > 0)
                        {
                            array[num2] = (byte)(num5 & 255);
                            num2++;
                            if (num5 > 255)
                            {
                                b2 |= 128;
                                array[num2] = (byte)(((int)num5 & 65280) >> 8);
                                num2++;
                            }
                            else
                            {
                                b2 |= 64;
                            }
                        }

                        array[num4] = b2;
                        writer.Write(array, num4, num2 - num4);
                    }
                }
            }
        }

        private void SaveModdedStuff()
        {
            string path = Path.Combine(Main.ActiveWorldFileData.Path.Replace(".wld", ""), dimensionName) + "/modded.data";

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            var tag = new Terraria.ModLoader.IO.TagCompound();
            tag["tiles"] = typeof(Main).Assembly.GetType("Terraria.ModLoader.IO.TileIO")
                .GetMethod("SaveTiles", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { });
            tag["containers"] = typeof(Main).Assembly.GetType("Terraria.ModLoader.IO.TileIO")
                .GetMethod("SaveContainers", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { });




            var stream = new MemoryStream();
            Terraria.ModLoader.IO.TagIO.ToStream(tag, stream);
            var data = stream.ToArray();
            using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(path)))
            {
                writer.Write(stream.GetBuffer().Length);
                writer.Write(stream.GetBuffer());
            }
        }

        public void SaveChest()
        {
            string headerPath = Path.Combine(Main.ActiveWorldFileData.Path.Replace(".wld", ""), dimensionName) + "/chest.data";

            if (File.Exists(headerPath))
            {
                File.Delete(headerPath);
            }

            using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(headerPath)))
            {
                short num = 0;
                for (int i = 0; i < 1000; i++)
                {
                    Chest chest = Main.chest[i];
                    if (chest != null)
                    {
                        bool flag = false;
                        for (int j = chest.x; j <= chest.x + 1; j++)
                        {
                            for (int k = chest.y; k <= chest.y + 1; k++)
                            {
                                if (j < 0 || k < 0 || j >= Main.maxTilesX || k >= Main.maxTilesY)
                                {
                                    flag = true;
                                    break;
                                }
                                Tile tile = Main.tile[j, k];
                                if (!tile.active() || !Main.tileContainer[(int)tile.type])
                                {
                                    flag = true;
                                    break;
                                }
                            }
                        }
                        if (flag)
                        {
                            Main.chest[i] = null;
                        }
                        else
                        {
                            num += 1;
                        }
                    }
                }
                writer.Write(num);
                writer.Write((short)40);
                for (int i = 0; i < 1000; i++)
                {
                    Chest chest = Main.chest[i];
                    if (chest != null)
                    {
                        writer.Write(chest.x);
                        writer.Write(chest.y);
                        writer.Write(chest.name);
                        for (int l = 0; l < 40; l++)
                        {
                            Item item = chest.item[l];
                            if (item == null || item.modItem != null)
                            {
                                writer.Write((short)0);
                            }
                            else
                            {
                                if (item.stack > item.maxStack)
                                {
                                    item.stack = item.maxStack;
                                }
                                if (item.stack < 0)
                                {
                                    item.stack = 1;
                                }
                                writer.Write((short)item.stack);
                                if (item.stack > 0)
                                {
                                    writer.Write(item.netID);
                                    writer.Write(item.prefix);
                                }
                            }
                        }
                    }
                }
            }


        }

        public void LoadHeader()
        {
            string headerPath = Path.Combine(Main.ActiveWorldFileData.Path.Replace(".wld", ""), dimensionName) + "/header.data";

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
            string path = Path.Combine(Main.ActiveWorldFileData.Path.Replace(".wld", ""), dimensionName) + "/NPC.data";

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
            string path = Path.Combine(Main.ActiveWorldFileData.Path.Replace(".wld", ""), dimensionName) + "/Tile.data";

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
            string path = Path.Combine(Main.ActiveWorldFileData.Path.Replace(".wld", ""), dimensionName) + "/chest.data";
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
            string path = Path.Combine(Main.ActiveWorldFileData.Path.Replace(".wld", ""), dimensionName) + "/modded.data";

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
            string path = Path.Combine(Main.ActiveWorldFileData.Path.Replace(".wld", ""), dimensionName) + "/Trueheader.data";
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

        public void LoadWorld()
        {
            String progress = "";

            Main.gameMenu = true;
            Main.menuMode = 888;
            Main.MenuUI.SetState(new UIDimensionLoading(this));
            ThreadPool.QueueUserWorkItem(do_LoadDimensionCallBack);
            
        }

        private static void DrawProgress()
        {
            Viewport dimension = Main.graphics.GraphicsDevice.Viewport;
            Texture2D texture = Dimlibs.Instance.GetTexture("Texture/LoadingScreen1");
            ILog log = LogManager.GetLogger("Dimension Loading");

            try
            {
                for (int i = 0; i < dimension.Width; i += texture.Width)
                {
                    for (int j = 0; j < dimension.Height; j += texture.Height)
                    {
                        Main.spriteBatch.Draw(texture, new Rectangle(i, j, texture.Width, texture.Height), null, Color.White, 0f,
                            Vector2.Zero, SpriteEffects.None, 0f);
                    }
                }

                ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, Main.fontDeathText, Main.statusText,
                    new Vector2(Main.screenWidth / 2, Main.screenHeight) -
                    Main.fontDeathText.MeasureString(Main.statusText) / 2, Color.White, 0f, Vector2.Zero, Vector2.One, 1f, 2f);
            }
            catch (Exception e)
            {
                log.Error("An exception happened while loading the dimension : " + e.Message, e);
            }

            

        }

        public void do_LoadDimensionCallBack(object ThreadContext)
        {
            try
            {
                Load(ThreadContext as string);
            }
            catch (Exception e)
            {
                ILog log = LogManager.GetLogger("Dimension Callback");
                log.Error(e.Message, e);
            }
        }

        public void Load(string statusText)
        {
            bool[] importance = null;
            int[] position = null;

            statusText = "Loading save file header...";
            Main.statusText = statusText;
            LoadFileFormatTrueHeader(out importance, out position);
            statusText = "Loading world size data...";
            Main.statusText = statusText;
            LoadHeader();
            statusText = "Attempting to load NPC data";
            Main.statusText = statusText;
            LoadNPC();
            statusText = "Loading the tile, really important";
            Main.statusText = statusText;
            LoadTile(importance);
            Main.statusText = statusText;
            LoadChests();
            Main.statusText = "Loading modded data";
            LoadModdedStuff();
            Main.statusText = "done";
            Main.LocalPlayer.Spawn();
        }
    }
}
