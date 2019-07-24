using System;
using System.Reflection;
using System.Text;
using log4net;
using log4net.Core;
using MonoMod.Cil;
using MonoMod.RuntimeDetour.HookGen;
using Terraria;

namespace Dimlibs.API
{
    public class DimLibsHook
    {
        public delegate void orig_do_worldGenCallBack(object threadContext);
        public delegate void hook_do_worldGenCallBack(orig_do_worldGenCallBack orig, object threadContext);

        public static event hook_do_worldGenCallBack do_worldGenCallBack_Hook
        {
            add
            {
                HookEndpointManager.Add(typeof(WorldGen).GetMethod("do_worldGenCallBack"), value);
            }
            remove
            {
                HookEndpointManager.Remove(typeof(WorldGen).GetMethod("do_worldGenCallBack"), value);
            }
        }

        public delegate void orig_NewLoadChest(DimensionHandler self);
        public delegate void hook_NewLoadChest(orig_NewLoadChest orig, DimensionHandler self);

        public static event hook_NewLoadChest NewLoadChest_Hook
        {
            add
            {
                HookEndpointManager.Add(typeof(DimensionHandler).GetMethod("LoadChest"), value);
            }
            remove
            {
                HookEndpointManager.Remove(typeof(DimensionHandler).GetMethod("LoadChest"), value);
            }
        }

        public delegate void orig_NewSaveChest(DimensionHandler self);
        public delegate void hook_NewSaveChest(orig_NewSaveChest orig, DimensionHandler self);

        public static event hook_NewSaveChest NewSaveChest_hook
        {
            add
            {
                HookEndpointManager.Add(typeof(DimensionHandler).GetMethod("SaveChest"), value);
            }
            remove
            {
                HookEndpointManager.Remove(typeof(DimensionHandler).GetMethod("SaveChest"), value);
            }
        }

        public delegate void orig_NewSaveCurrentTile(DimensionHandler self);
        public delegate void hook_NewSaveCurrentTile(orig_NewSaveCurrentTile orig, DimensionHandler handler);

        public static event hook_NewSaveCurrentTile NewSaveTile_hook
        {
            add
            {
                HookEndpointManager.Add(typeof(DimensionHandler).GetMethod("SaveCurrentTile"), value);
            }
            remove
            {
                HookEndpointManager.Remove(typeof(DimensionHandler).GetMethod("SaveCurrentTile"), value);
            }
        }

        public delegate void orig_Save(DimensionHandler self);
        public delegate void hook_Save(orig_Save orig, DimensionHandler self);

        public static event hook_Save Save_hook
        {
            add
            {
                HookEndpointManager.Add(typeof(DimensionHandler).GetMethod("Save"), value);
            }
            remove
            {
                HookEndpointManager.Remove(typeof(DimensionHandler).GetMethod("Save"), value);
            }
        }

        public delegate void orig_LoadHeader(DimensionHandler self);
        public delegate void hook_LoadHeader(orig_LoadHeader orig, DimensionHandler self);

        public static event hook_LoadHeader LoadHeader_hook
        {
            add
            {
                HookEndpointManager.Add(typeof(DimensionHandler).GetMethod("LoadHeader"), value);
            }
            remove
            {
                HookEndpointManager.Remove(typeof(DimensionHandler).GetMethod("LoadHeader"), value);
            }
        }


        public delegate void orig_LoadNPC(DimensionHandler self);
        public delegate void hook_LoadNPC(orig_LoadNPC orig, DimensionHandler self);

        public static event hook_LoadNPC LoadNPC_hook
        {
            add
            {
                HookEndpointManager.Add(typeof(DimensionHandler).GetMethod("LoadNPC"), value);
            }
            remove
            {
                HookEndpointManager.Remove(typeof(DimensionHandler).GetMethod("LoadNPC"), value);
            }
        }


        public delegate void orig_LoadTile(DimensionHandler self, Boolean[] importance);
        public delegate void hook_LoadTile(orig_LoadTile orig, DimensionHandler self, Boolean[] importance);

        public static event hook_LoadTile LoadTile_hook
        {
            add
            {
                HookEndpointManager.Add(typeof(DimensionHandler).GetMethod("LoadTile"), value);
            }
            remove
            {
                HookEndpointManager.Remove(typeof(DimensionHandler).GetMethod("LoadTile"), value);
            }
        }

        public delegate void orig_LoadWorld(DimensionHandler self);
        public delegate void hook_LoadWorld(orig_LoadWorld orig, DimensionHandler self);

        public static event hook_LoadWorld LoadWorld_hook
        {
            add
            {
                HookEndpointManager.Add(typeof(DimensionHandler).GetMethod("LoadWorld"), value);
            }
            remove
            {
                HookEndpointManager.Remove(typeof(DimensionHandler).GetMethod("LoadWorld"), value);
            }
        }


        public delegate void orig_do_LoadDimensionCallBack(DimensionHandler self, Object ThreadContext);
        public delegate void hook_do_LoadDimensionCallBack(orig_do_LoadDimensionCallBack orig, DimensionHandler self, Object ThreadContext);

        public static event hook_do_LoadDimensionCallBack do_LoadDimensionCallBack_hook
        {
            add
            {
                HookEndpointManager.Add(typeof(DimensionHandler).GetMethod("do_LoadDimensionCallBack"), value);
            }
            remove
            {
                HookEndpointManager.Remove(typeof(DimensionHandler).GetMethod("do_LoadDimensionCallBack"), value);
            }
        }

        public delegate void orig_Load(DimensionHandler self);
        public delegate void hook_Load(orig_Load orig, DimensionHandler self);

        public static event hook_Load Load_hook
        {
            add
            {
                HookEndpointManager.Add(typeof(DimensionHandler).GetMethod("Load"), value);
            }
            remove
            {
                HookEndpointManager.Remove(typeof(DimensionHandler).GetMethod("Load"), value);
            }
        }

        public delegate void orig_TileGet(Tile[,] self, int x, int y);
        public delegate void hook_TileGet(orig_TileGet orig, Tile[,] self, int x, int y);

        public static event hook_Load TileGet_Hook
        {
            add
            {
                HookEndpointManager.Add(typeof(Tile[,]).GetMethod("Get"), value);
            }
            remove
            {
                HookEndpointManager.Remove(typeof(Tile[,]).GetMethod("Get"), value);
            }
        }


        public static event ILContext.Manipulator IL_Terraria_Tile_Get
        {
            add
            {
                HookEndpointManager.Modify<hook_TileGet>(typeof(Tile[,]).GetMethod("Get"), value);
            }
            remove
            {
                HookEndpointManager.Unmodify<hook_TileGet>(typeof(Tile[,]).GetMethod("Get"), value);
            }
        }


        public static void ReadILCode(ILContext il)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var instruction in il.Instrs)
            {
                builder.AppendLine(instruction.ToString());
            }
            LogManager.GetLogger("ILCode").Info(builder.ToString());
        }
    }
}
