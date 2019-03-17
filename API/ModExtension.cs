using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace Dimlibs.API
{
    public static class ModExtension
    {
        public static ModDimension GetDimension(this Mod self, string dimension) =>
            (Dimlibs.dimensionInstanceHandlers.ContainsKey(self.Name + ":" + dimension))
                ? Dimlibs.dimensionInstanceHandlers[self.Name + ":" + dimension]
                : null;

        public static ModDimension GetDimension<T>(this Mod self) => self.GetDimension(typeof(T).Name);

        internal static void AutoLoadDimension(this Mod self, Type type)
        {
            ModDimension dimension = (ModDimension) Activator.CreateInstance(type);
            dimension.mod = self;
            dimension.Name = dimension.GetType().Name;
			dimension.SetDefault();
            Dimlibs.dimensionInstanceHandlers.Add(dimension.URN, dimension);
        }
    }
}
