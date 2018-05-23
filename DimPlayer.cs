using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Dimlibs
{
    public class DimPlayer : ModPlayer
    {
        internal static List<string> dimList = new List<string>();
        private string currentDimension = "overworld";

        public string getCurrentDimension()
        {
            return currentDimension;
        }

        internal void setCurrentDimension(String currentDimension)
        {
            this.currentDimension = currentDimension;
        }

        public override TagCompound Save()
        {
            TagCompound tag = new TagCompound();
            tag.Add("dimension", currentDimension);
            return tag;
        }

        public override void Load(TagCompound tag)
        {
            currentDimension = tag.GetString("dimension");
        }
    }
}
