using Dimlibs.API;
using Terraria;
using Terraria.World.Generation;

namespace Dimlibs.Dimension
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
