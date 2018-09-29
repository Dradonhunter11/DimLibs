using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.World.Generation;

namespace Dimlibs.API
{
    class OverworldHandler : DimGenerator
    {
        public OverworldHandler() : base("Overworld")
        {
            string a = "a";
        }

        public override void ModifyGenerationPass(int seed, GenerationProgress customProgressObject)
        {
            WorldGen.generateWorld(Main.ActiveWorldFileData.Seed, customProgressObject);
        }
    }
}
