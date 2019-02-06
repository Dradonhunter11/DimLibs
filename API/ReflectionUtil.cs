using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.Social;
using Terraria.Utilities;

namespace Dimlibs.API
{
    public static class ReflectionUtil
    {
        public static void Load()
        {
            On.Terraria.IO.WorldFile.SaveWorldTiles += SaveWorldTiles;
            On.Terraria.IO.WorldFile.saveWorld_bool_bool += SaveWorld;
            On.Terraria.IO.WorldFile.loadWorld += LoadWorld;
        }

        public static void Unload()
        {
            On.Terraria.IO.WorldFile.SaveWorldTiles -= SaveWorldTiles;
            On.Terraria.IO.WorldFile.saveWorld_bool_bool -= SaveWorld;
            On.Terraria.IO.WorldFile.loadWorld -= LoadWorld;
        }

        public static Object Clone(Object self)
        {
            return typeof(Object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(self, new object[] { });
        }

        private static int SaveWorldTiles(On.Terraria.IO.WorldFile.orig_SaveWorldTiles orig, BinaryWriter writer)
        {
            byte[] array = new byte[13];
            for (int i = 0; i < Main.maxTilesX; i++)
            {
                float num = (float)i / (float)Main.maxTilesX;
                /*Main.statusText = string.Concat(new object[]
                    {
                        Lang.gen[49].Value,
                        " ",
                        (int)(num * 100f + 1f),
                        "%"
                    });*/
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
                            typeof(Main).Assembly.GetType("Terraria.ModLoader.IO.TileIO").GetMethod("VanillaSaveFrames",
                                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).Invoke(null, new object[] { tile, frameX});
                            //TileIO.VanillaSaveFrames(tile, ref frameX);
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
            return (int)writer.BaseStream.Position;
        }



        public static void SaveWorld(On.Terraria.IO.WorldFile.orig_saveWorld_bool_bool orig, bool useCloudSaving, bool resetTime = false)
        {
            FieldInfo padLockInfo = typeof(WorldFile).GetField("padlock", BindingFlags.Static | BindingFlags.NonPublic);
            Object padlock = padLockInfo.GetValue(null);

            FieldInfo HasCacheInfo = typeof(WorldFile).GetField("HasCache", BindingFlags.Static | BindingFlags.NonPublic);
            bool HasCache = (bool)HasCacheInfo.GetValue(null);

            MethodInfo Save = typeof(Main).Assembly.GetType("Terraria.ModLoader.IO.WorldIO").GetMethod("Save",
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (DimensionHandler dim in DimWorld.dimensionInstanceHandlers.Values)
            {
                dim.Save();
            }
            /*if (useCloudSaving && SocialAPI.Cloud == null)
            {
                return;
            }
            if (Main.worldName == "")
            {
                Main.worldName = "World";
            }
            if (WorldGen.saveLock)
            {
                return;
            }
            WorldGen.saveLock = true;
            while (WorldGen.IsGeneratingHardMode)
            {
                Main.statusText = Lang.gen[48].Value;
            }
            lock (padlock)
            {
                try
                {
                    Directory.CreateDirectory(Main.WorldPath);
                }
                catch
                {
                }
                if (Main.skipMenu)
                {
                    return;
                }
                if (HasCache)
                {
                    typeof(WorldFile).GetMethod("SetTempToCache", BindingFlags.Static | BindingFlags.NonPublic)
                        .Invoke(null, new object[] { });
                }
                else
                {
                    typeof(WorldFile).GetMethod("SetTempToOngoing", BindingFlags.Static | BindingFlags.NonPublic)
                        .Invoke(null, new object[] { });
                }
                if (resetTime)
                {
                    typeof(WorldFile).GetMethod("ResetTempsToDayTime", BindingFlags.Static | BindingFlags.NonPublic)
                        .Invoke(null, new object[] { });
                }
                if (Main.worldPathName == null)
                {
                    return;
                }
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                byte[] array = null;
                int num = 0;
                using (MemoryStream memoryStream = new MemoryStream(7000000))
                {
                    using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
                    {
                        WorldFile.SaveWorld_Version2(binaryWriter);
                    }
                    array = memoryStream.ToArray();
                    num = array.Length;
                }
                if (array == null)
                {
                    return;
                }
                byte[] array2 = null;
                if (FileUtilities.Exists(Main.worldPathName, useCloudSaving))
                {
                    array2 = FileUtilities.ReadAllBytes(Main.worldPathName, useCloudSaving);
                }
                FileUtilities.Write(Main.worldPathName, array, num, useCloudSaving);
                array = FileUtilities.ReadAllBytes(Main.worldPathName, useCloudSaving);
                string text = null;
                using (MemoryStream memoryStream2 = new MemoryStream(array, 0, num, false))
                {
                    using (BinaryReader binaryReader = new BinaryReader(memoryStream2))
                    {
                        if (!Main.validateSaves || WorldFile.validateWorld(binaryReader))
                        {
                            if (array2 != null)
                            {
                                text = Main.worldPathName + ".bak";
                                Main.statusText = Lang.gen[50].Value;
                            }
                        }
                        else
                        {
                            text = Main.worldPathName;
                        }
                    }
                }
                if (text != null && array2 != null)
                {
                    FileUtilities.WriteAllBytes(text, array2, useCloudSaving);
                }
                Save.Invoke(null, new Object[] { Main.ActiveWorldFileData.Path, useCloudSaving });
                WorldGen.saveLock = false;
            }
            Main.serverGenLock = false;*/
        }

        public static void LoadWorld(On.Terraria.IO.WorldFile.orig_loadWorld orig, bool loadFromCloud)
        {
            foreach (DimensionHandler dim in DimWorld.dimensionInstanceHandlers.Values)
            {
                dim.LoadWorld();   
            }
        }
    }
}
