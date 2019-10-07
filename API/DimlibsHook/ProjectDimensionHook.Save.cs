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
		public delegate void orig_Save(DimensionHandler.DimensionHandler self);
		public delegate void hook_Save(orig_Save orig, DimensionHandler.DimensionHandler self);

		public static event hook_Save Save_hook
		{
			add
			{
				HookEndpointManager.Add(typeof(DimensionHandler.DimensionHandler).GetMethod("Save"), value);
			}
			remove
			{
				HookEndpointManager.Remove(typeof(DimensionHandler.DimensionHandler).GetMethod("Save"), value);
			}
		}

		public delegate void orig_NewSaveChest(DimensionHandler.DimensionHandler self);
		public delegate void hook_NewSaveChest(orig_NewSaveChest orig, DimensionHandler.DimensionHandler self);

		public static event hook_NewSaveChest NewSaveChest_hook
		{
			add
			{
				HookEndpointManager.Add(typeof(DimensionHandler.DimensionHandler).GetMethod("SaveChest"), value);
			}
			remove
			{
				HookEndpointManager.Remove(typeof(DimensionHandler.DimensionHandler).GetMethod("SaveChest"), value);
			}
		}

		public delegate void orig_NewSaveCurrentTile(DimensionHandler.DimensionHandler self);
		public delegate void hook_NewSaveCurrentTile(orig_NewSaveCurrentTile orig, DimensionHandler.DimensionHandler handler);

		public static event hook_NewSaveCurrentTile NewSaveTile_hook
		{
			add
			{
				HookEndpointManager.Add(typeof(DimensionHandler.DimensionHandler).GetMethod("SaveCurrentTile"), value);
			}
			remove
			{
				HookEndpointManager.Remove(typeof(DimensionHandler.DimensionHandler).GetMethod("SaveCurrentTile"), value);
			}
		}

		
	}
}
