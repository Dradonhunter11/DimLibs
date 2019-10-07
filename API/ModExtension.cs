using System;
using System.IO;
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
			if(!Dimlibs.dimensionInstanceHandlers.ContainsKey(dimension.URN))
				Dimlibs.dimensionInstanceHandlers.Add(dimension.URN, dimension);
        }

        public static byte[] ReadAllBytes(this Stream instream)
        {
            if (instream is MemoryStream)
                return ((MemoryStream)instream).ToArray();

            using (var memoryStream = new MemoryStream())
            {
                instream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
