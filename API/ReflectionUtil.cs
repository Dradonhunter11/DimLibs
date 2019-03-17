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
using log4net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.RuntimeDetour;
using MonoMod.RuntimeDetour.HookGen;
using Terraria;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.IO;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.Social;
using Terraria.Utilities;
using Terraria.World.Generation;

namespace Dimlibs.API
{
    public static class ReflectionUtil
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
            DimLibsHook.do_worldGenCallBack_Hook += do_worldGenCallBack;
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
            DimLibsHook.do_worldGenCallBack_Hook -= do_worldGenCallBack;
        }

        public static Object Clone(Object self)
        {
            return typeof(Object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(self, new object[] { });
        }

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
            Microsoft.Xna.Framework.Color[] mapColorCacheArray = (Color[]) typeof(Main).GetField("_mapColorCacheArray", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            int num = secX * 200;
            int num2 = num + 200;
            int num3 = secY * 150;
            int num4 = num3 + 150;
            int num5 = num / Main.textureMaxWidth;
            int num6 = num3 / Main.textureMaxHeight;
            int num7 = num % Main.textureMaxWidth;
            int num8 = num3 % Main.textureMaxHeight;

            bool flag = (bool) typeof(Main).GetMethod("checkMap", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(instance, new object[] {num5, num6});
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
                PlayerHooks.OnRespawn(instance);
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
                        if (Main.tile[i, j] != null)
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

        public static void do_worldGenCallBack(DimLibsHook.orig_do_worldGenCallBack orig, object threadContext)
        {
            Main.PlaySound(10, -1, -1, 1, 1f, 0f);
            foreach (ModDimension handler in Dimlibs.dimensionInstanceHandlers.Values)
            {
                WorldGen.clearWorld();
                handler.GenerateDimension(Main.ActiveWorldFileData.Seed, threadContext as GenerationProgress);
                handler.handler.Save();
            }
            WorldFile.saveWorld(Main.ActiveWorldFileData.IsCloudSave, true);
            if (Main.menuMode == 10 || Main.menuMode == 888)
            {
                Main.menuMode = 6;
            }
            Main.PlaySound(10, -1, -1, 1, 1f, 0f);
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
            orig.Invoke(useCloudSaving, resetTime);
            if (DimWorld.dimension == "OverworldDimension")
            {
                Dimlibs.Instance.GetDimension("OverworldDimension").handler.Save();
            }
            else
            {
                Dimlibs.dimensionInstanceHandlers[DimWorld.dimension].handler.Save();
            }
        }

        public static void LoadWorld(On.Terraria.IO.WorldFile.orig_loadWorld orig, bool loadFromCloud)
        {
            orig.Invoke(loadFromCloud);
            if (DimWorld.dimension == "OverworldDimension" || DimWorld.dimension == "Overworld")
            {
                Dimlibs.Instance.GetDimension("OverworldDimension").handler.LoadWorld();
            }
            else
            {
                Dimlibs.dimensionInstanceHandlers[DimWorld.dimension].handler.LoadWorld();
                //Dimlibs.Instance.GetDimension("OverworldDimension").handler.LoadWorld();
            }
        }
    }
}
