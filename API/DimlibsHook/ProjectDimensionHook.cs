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
	public sealed partial class ProjectDimensionHook
	{
	    internal delegate void orig_do_worldGenCallBack(object threadContext);
	    internal delegate void hook_do_worldGenCallBack(orig_do_worldGenCallBack orig, object threadContext);

	    internal static event hook_do_worldGenCallBack do_worldGenCallBack_Hook
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

        public delegate void orig_TileGet(Tile[,] self, int x, int y);
        public delegate void hook_TileGet(orig_TileGet orig, Tile[,] self, int x, int y);

        public static event hook_TileGet TileGet_Hook
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
    }
}
