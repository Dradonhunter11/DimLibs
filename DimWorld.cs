using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dimlibs.API;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Dimlibs
{
    internal class DimWorld : ModWorld
    {
        public static String previousDimension = "";
        public static String dimension = "Overworld";
        internal static readonly IDictionary<String, DimensionHandler> dimensionInstanceHandlers = new Dictionary<String, DimensionHandler>();
        private DimensionHandler currentRunningHandler;

        public DimensionHandler GetDimensionHandler(String dimensionName)
        {
            return (dimensionInstanceHandlers.ContainsKey(dimensionName))
                ? dimensionInstanceHandlers[dimensionName]
                : null;
        }

        public override TagCompound Save()
        {
            TagCompound tag = new TagCompound();
            tag.Add("currentDimension", dimension);
            return tag;
        }

        public override void Load(TagCompound tag)
        {
            
        }

        private void massLoading()
        {

        }

        public override void PostWorldGen()
        {
            base.PostWorldGen();
        }

        internal static void AddDimension(DimensionHandler handler)
        {
            if (!dimensionInstanceHandlers.ContainsKey(handler.DimensionName))
            {
                dimensionInstanceHandlers.Add(handler.DimensionName, handler);
            }
        }

        public override void PreUpdate()
        {
            if (previousDimension != dimension && previousDimension != "")
            {
                dimensionInstanceHandlers[previousDimension].Save();
                dimensionInstanceHandlers[dimension].LoadWorld();
                WorldGen.EveryTileFrame();
            }

            previousDimension = dimension;
        }
    }
}
