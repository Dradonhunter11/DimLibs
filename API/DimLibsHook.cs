using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MonoMod.RuntimeDetour.HookGen;
using Terraria;

namespace Dimlibs.API
{
    public class DimLibsHook
    {
        public delegate void orig_do_worldGenCallBack(object threadContext);
        public delegate void hook_do_worldGenCallBack(orig_do_worldGenCallBack orig, object threadContext);
    }
}
