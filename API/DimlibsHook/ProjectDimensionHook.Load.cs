using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoMod.RuntimeDetour.HookGen;

namespace Dimlibs.API.DimlibsHook
{
	public sealed partial class ProjectDimensionHook
	{
		public delegate void orig_Load(DimensionHandler.DimensionHandler self);
		public delegate void hook_Load(orig_Load orig, DimensionHandler.DimensionHandler self);

		public static event hook_Load Load_hook
		{
			add
			{
				HookEndpointManager.Add(typeof(DimensionHandler.DimensionHandler).GetMethod("Load"), value);
			}
			remove
			{
				HookEndpointManager.Remove(typeof(DimensionHandler.DimensionHandler).GetMethod("Load"), value);
			}
		}

		public delegate void orig_NewLoadChest(DimensionHandler.DimensionHandler self);
		public delegate void hook_NewLoadChest(orig_NewLoadChest orig, DimensionHandler.DimensionHandler self);

		public static event hook_NewLoadChest NewLoadChest_Hook
		{
			add
			{
				HookEndpointManager.Add(typeof(DimensionHandler.DimensionHandler).GetMethod("LoadChest"), value);
			}
			remove
			{
				HookEndpointManager.Remove(typeof(DimensionHandler.DimensionHandler).GetMethod("LoadChest"), value);
			}
		}

		public delegate void orig_LoadHeader(DimensionHandler.DimensionHandler self);
		public delegate void hook_LoadHeader(orig_LoadHeader orig, DimensionHandler.DimensionHandler self);

		public static event hook_LoadHeader LoadHeader_hook
		{
			add
			{
				HookEndpointManager.Add(typeof(DimensionHandler.DimensionHandler).GetMethod("LoadHeader"), value);
			}
			remove
			{
				HookEndpointManager.Remove(typeof(DimensionHandler.DimensionHandler).GetMethod("LoadHeader"), value);
			}
		}


		public delegate void orig_LoadNPC(DimensionHandler.DimensionHandler self);
		public delegate void hook_LoadNPC(orig_LoadNPC orig, DimensionHandler.DimensionHandler self);

		public static event hook_LoadNPC LoadNPC_hook
		{
			add
			{
				HookEndpointManager.Add(typeof(DimensionHandler.DimensionHandler).GetMethod("LoadNPC"), value);
			}
			remove
			{
				HookEndpointManager.Remove(typeof(DimensionHandler.DimensionHandler).GetMethod("LoadNPC"), value);
			}
		}


		public delegate void orig_LoadTile(DimensionHandler.DimensionHandler self, Boolean[] importance);
		public delegate void hook_LoadTile(orig_LoadTile orig, DimensionHandler.DimensionHandler self, Boolean[] importance);

		public static event hook_LoadTile LoadTile_hook
		{
			add
			{
				HookEndpointManager.Add(typeof(DimensionHandler.DimensionHandler).GetMethod("LoadTile"), value);
			}
			remove
			{
				HookEndpointManager.Remove(typeof(DimensionHandler.DimensionHandler).GetMethod("LoadTile"), value);
			}
		}
	}
}

