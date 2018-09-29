using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.IO;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Social;
using Terraria.Utilities;
using Terraria.World.Generation;

namespace Dimlibs.API
{
    internal class DimensionHandler
    {
        private NPC[] instanceHostileNPCArray;
        private Tile[,] dimensionTile;
        private Projectile[] instanceProjectileArray;
        private Chest[] chest;
        private WorldMap map;
        private String _Path = "";
        private DimGenerator generator;
        private String dimensionName;
        private int dimensionID;

        public int DimensionID {
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
            instanceProjectileArray = Main.projectile;
            instanceHostileNPCArray = Main.npc;
            chest = Main.chest;
            map = Main.Map;
            saveDimensionWorld(false);
        }

        public void load(String path)
        {
            this._Path = path;
            LoadWorld();
        }

        public void LoadDimension()
        {
            SwapTile();
        }

        private void SwapTile()
        {
            Main.tile = dimensionTile;
            Main.Map = map;
        }

        public void LoadWorld()
        {
            loadWorld(false);
        }

        public void saveDimensionWorld(bool useCloudSaving, bool resetTime = false)
        {
            Tile[,] tempTileArray = (Tile[,])Main.tile.Clone();
            if (dimensionTile != null)
            {
                Main.tile = dimensionTile;
            }
            else
            {
                dimensionTile = Main.tile;
            }

            FieldInfo padLockInfo = typeof(WorldFile).GetField("padlock", BindingFlags.Static | BindingFlags.NonPublic);
            Object padlock = padLockInfo.GetValue(null);

            FieldInfo HasCacheInfo = typeof(WorldFile).GetField("HasCache", BindingFlags.Static | BindingFlags.NonPublic);
            bool HasCache = (bool)HasCacheInfo.GetValue(null);

            MethodInfo Save = typeof(Main).Assembly.GetType("Terraria.ModLoader.IO.WorldIO").GetMethod("Save",
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);


            string savePath = Main.WorldPath + "/" + Main.worldName + "_dimension/";


            if (useCloudSaving && SocialAPI.Cloud == null)
            {
                return;
            }
            if (Main.worldName == "")
            {
                Main.worldName = "World";
            }
            string tempName = savePath + Main.worldName + "_" + dimensionName + ".wld";
            if (dimensionName == "Overworld")
            {
                tempName = Main.ActiveWorldFileData.Path;
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
                if (FileUtilities.Exists(tempName, useCloudSaving))
                {
                    array2 = FileUtilities.ReadAllBytes(tempName, useCloudSaving);
                }
                FileUtilities.Write(tempName, array, num, useCloudSaving);
                array = FileUtilities.ReadAllBytes(tempName, useCloudSaving);
                string text = null;
                using (MemoryStream memoryStream2 = new MemoryStream(array, 0, num, false))
                {
                    using (BinaryReader binaryReader = new BinaryReader(memoryStream2))
                    {
                        if (!Main.validateSaves || WorldFile.validateWorld(binaryReader))
                        {
                            if (array2 != null)
                            {
                                text = tempName + ".bak";
                                Main.statusText = Lang.gen[50].Value;
                            }
                        }
                        else
                        {
                            text = tempName;
                        }
                    }
                }
                if (text != null && array2 != null)
                {
                    FileUtilities.WriteAllBytes(text, array2, useCloudSaving);
                }
                Save.Invoke(null, new Object[] {tempName, useCloudSaving});
                WorldGen.saveLock = false;
            }
            Main.serverGenLock = false;
            Main.tile = tempTileArray;
        }

        public void loadWorld(bool loadFromCloud)
        {
            Tile[,] tempTileArray = (Tile[,])Main.tile.Clone();
            WorldMap tempMap = (WorldMap) ReflectionUtil.Clone(Main.Map);
            
            string savePath = Main.WorldPath + "/" + Main.ActiveWorldFileData.Name + "_dimension/";
            string tempName = Main.ActiveWorldFileData.Name + "_" + dimensionName + ".wld";
            string combinedPath = savePath + tempName;
            
            WorldFile.IsWorldOnCloud = loadFromCloud;
            Main.checkXMas();
            Main.checkHalloween();
            bool flag = loadFromCloud && SocialAPI.Cloud != null;
            //patch file: flag
            if (dimensionName == "Overworld" && !FileUtilities.Exists(combinedPath, flag))
            {
                //typeof(WorldFileData).GetField("_Path", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(Main.ActiveWorldFileData, combinedPath);
                combinedPath = Main.ActiveWorldFileData.Path;
            }


            if (!FileUtilities.Exists(combinedPath, flag) && dimensionName != "Overworld")
            {
                if (!flag)
                {
                    for (int i = combinedPath.Length - 1; i >= 0; i--)
                    {
                        if (combinedPath.Substring(i, 1) == string.Concat(Path.DirectorySeparatorChar))
                        {
                            string path = combinedPath.Substring(0, i);
                            Directory.CreateDirectory(path);
                            break;
                        }
                    }
                }
                WorldGen.clearWorld();
                //string name = (Main.worldName == "") ? "World" : Main.worldName;
                //Main.ActiveWorldFileData = WorldFile.CreateMetadata(name, flag, Main.expertMode);
                /*string text = (Main.AutogenSeedName ?? "").Trim();
                if (text.Length == 0)
                {
                    
                    Main.ActiveWorldFileData.SetSeedToRandom();
                }
                else
                {
                    Main.ActiveWorldFileData.SetSeed(text);
                }*/
                generator.GenerateDimension(Main.ActiveWorldFileData.Seed, new GenerationProgress());
                Save();
            }
            byte[] buffer = FileUtilities.ReadAllBytes(combinedPath, flag);
            using (MemoryStream memoryStream = new MemoryStream(buffer))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                {
                    try
                    {
                        WorldGen.loadFailed = false;
                        WorldGen.loadSuccess = false;
                        int num = binaryReader.ReadInt32();
                        WorldFile.versionNumber = num;
                        int num2;
                        if (num <= 87)
                        {
                            num2 = WorldFile.LoadWorld_Version1(binaryReader);
                        }
                        else
                        {
                            num2 = WorldFile.LoadWorld_Version2(binaryReader);
                        }
                        if (num < 141)
                        {
                            if (!loadFromCloud)
                            {
                                Main.ActiveWorldFileData.CreationTime = File.GetCreationTime(Main.worldPathName);
                            }
                            else
                            {
                                Main.ActiveWorldFileData.CreationTime = DateTime.Now;
                            }
                        }
                        binaryReader.Close();
                        memoryStream.Close();
                        //WorldHooks.SetupWorld();
                        typeof(WorldHooks).GetMethod("SetupWorld", BindingFlags.Static | BindingFlags.NonPublic)
                            .Invoke(null, new object[] { });
                        typeof(Main).Assembly.GetType("Terraria.ModLoader.IO.WorldIO").GetMethod("Load", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).Invoke(null, new object[] {combinedPath, flag});
                        if (num2 != 0)
                        {
                            WorldGen.loadFailed = true;
                        }
                        else
                        {
                            WorldGen.loadSuccess = true;
                        }
                        if (WorldGen.loadFailed || !WorldGen.loadSuccess)
                        {
                            return;
                        }
                        WorldGen.gen = true;
                        WorldGen.waterLine = Main.maxTilesY;
                        Liquid.QuickWater(2, -1, -1);
                        WorldGen.WaterCheck();
                        int num3 = 0;
                        Liquid.quickSettle = true;
                        int num4 = Liquid.numLiquid + LiquidBuffer.numLiquidBuffer;
                        float num5 = 0f;
                        while (Liquid.numLiquid > 0 && num3 < 100000)
                        {
                            num3++;
                            float num6 = (float)(num4 - (Liquid.numLiquid + LiquidBuffer.numLiquidBuffer)) / (float)num4;
                            if (Liquid.numLiquid + LiquidBuffer.numLiquidBuffer > num4)
                            {
                                num4 = Liquid.numLiquid + LiquidBuffer.numLiquidBuffer;
                            }
                            if (num6 > num5)
                            {
                                num5 = num6;
                            }
                            else
                            {
                                num6 = num5;
                            }
                            Main.statusText = string.Concat(new object[]
                                {
                                    Lang.gen[27].Value,
                                    " ",
                                    (int)(num6 * 100f / 2f + 50f),
                                    "%"
                                });
                            Liquid.UpdateLiquid();
                        }
                        Liquid.quickSettle = false;
                        Main.weatherCounter = WorldGen.genRand.Next(3600, 18000);
                        Cloud.resetClouds();
                        WorldGen.WaterCheck();
                        WorldGen.gen = false;
                        NPC.setFireFlyChance();
                        Main.InitLifeBytes();
                        if (Main.slimeRainTime > 0.0)
                        {
                            Main.StartSlimeRain(false);
                        }
                        NPC.setWorldMonsters();
                    }
                    catch
                    {
                        WorldGen.loadFailed = true;
                        WorldGen.loadSuccess = false;
                        try
                        {
                            binaryReader.Close();
                            memoryStream.Close();
                        }
                        catch
                        {
                        }
                        return;
                    }
                }
            }

            map = (WorldMap) ReflectionUtil.Clone(Main.Map);
            dimensionTile = (Tile[,])Main.tile.Clone();
            if (dimensionName != "Overworld")
            {
                Main.tile = tempTileArray;
                Main.Map = tempMap;
            }
            /*if (WorldFile.OnWorldLoad != null)
            {
                WorldFile.OnWorldLoad();
            }*/
        }
    }
}
