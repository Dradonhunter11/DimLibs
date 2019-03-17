using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.World.Generation;

namespace Dimlibs.API
{
    class OverworldDimension : ModDimension
    {
        public override void ModifyGenerationPass(int seed, GenerationProgress customProgressObject)
        {
            WorldGen.generateWorld(Main.ActiveWorldFileData.Seed, customProgressObject);
        }

        public override void SetDefault()
        {
            return;
        }
    }
}
