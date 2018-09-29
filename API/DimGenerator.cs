using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria.World.Generation;

namespace Dimlibs.API
{
    public abstract class DimGenerator
    {
        internal bool isGenerating = false;
        internal static string dimensionName;
        internal static DimensionHandler handler;

        protected int copper;
        protected int iron;
        protected int silver;
        protected int gold;
        protected int dungeonSide;
        protected ushort jungleHut;
        protected int howFar;
        protected int[] PyrX;
        protected int[] PyrY;
        protected int numPyr;
        protected int[] snowMinX;
        protected int[] snowMaxX;
        protected int snowTop;
        protected int snowBottom;
        protected float dub2;

        protected double worldSurface;
        protected double rockLayer;
        protected double worldSurfaceHigh;
        protected double rockLayerLow;
        protected double rockLayerHigh;

        protected int _DimensionMaxTileX = 2400;
        protected int _DimensionMaxTileY = 1200;


        public abstract void ModifyGenerationPass(int seed, GenerationProgress customProgressObject);

        public List<GenPass> GetPasses(WorldGenerator generator)
        {
            FieldInfo info = typeof(WorldGenerator).GetField("_passes", BindingFlags.Instance | BindingFlags.NonPublic);
            return (List<GenPass>)info.GetValue(generator);
        }

        public float GetTotalLoadWeight(WorldGenerator generator)
        {
            FieldInfo info = typeof(WorldGenerator).GetField("_totalLoadWeight", BindingFlags.Instance | BindingFlags.NonPublic);
            return (float)info.GetValue(generator);
        }

        public void SetTotalLoadWeight(WorldGenerator generator, float weight)
        {
            FieldInfo info = typeof(WorldGenerator).GetField("_totalLoadWeight", BindingFlags.Instance | BindingFlags.NonPublic);
            info.SetValue(generator, weight);
        }

        public WorldGenerator _generator
        {
            get
            {
                FieldInfo info = typeof(WorldGen).GetField("_generator", BindingFlags.NonPublic | BindingFlags.Static);
                return (WorldGenerator)info.GetValue(null);
            }
            set
            {
                FieldInfo info = typeof(WorldGen).GetField("_generator", BindingFlags.NonPublic | BindingFlags.Static);
                info.SetValue(null, value);
            }
        }

        protected void AddGenerationPass(string name, WorldGenLegacyMethod method)
        {
            _generator.Append(new PassLegacy(name, method));
        }

        protected void AddGenerationPass(string name, float weight, WorldGenLegacyMethod method)
        {
            _generator.Append(new PassLegacy(name, method, weight));
        }

        internal void GenerateDimension(int seed, GenerationProgress customProgressObject = null)
        {
            reset(seed);
            ModifyGenerationPass(seed, customProgressObject);
            final(customProgressObject);
            finish();
        }

        

        public void reset(int seed)
        {
            
            WorldGen._lastSeed = seed;
            _generator = new WorldGenerator(seed);
            Main.rand = new UnifiedRandom(seed);
            MicroBiome.ResetAll();
            WorldGen.worldSurfaceLow = 0.0;
            copper = 7;
            iron = 6;
            silver = 9;
            gold = 8;
            dungeonSide = 0;
            jungleHut = (ushort)WorldGen.genRand.Next(5);
            howFar = 0;
            PyrX = null;
            PyrY = null;
            numPyr = 0;
            snowMinX = new int[Main.maxTilesY];
            snowMaxX = new int[Main.maxTilesY];
            snowTop = 0;
            snowBottom = 0;
            dub2 = 0f;

            worldSurface = 0.0;
            rockLayer = 0.0;
            worldSurfaceHigh = 0.0;
            rockLayerLow = 0.0;
            rockLayerHigh = 0.0;

            WorldHooks.PreWorldGen();
        }

        public void finish()
        {
            isGenerating = false;
        }

        private void final(GenerationProgress customProgressObject)
        {
            float weight = GetTotalLoadWeight(_generator);
            //WorldHooks.ModifyWorldGenTasks(GetPasses(_generator), ref weight);
            SetTotalLoadWeight(_generator, weight);
            Main.menuMode = 888;
            gen(customProgressObject);
            Main.WorldFileMetadata = FileMetadata.FromCurrentSettings(FileType.World);
        }

        private async void gen(GenerationProgress customProgressObject)
        {
            g(customProgressObject);
        }

        async Task g(GenerationProgress customProgressObject)
        {
            _generator.GenerateWorld(customProgressObject);
            //WorldHooks.PostWorldGen();
        }

        public DimGenerator(String dimensionName)
        {
            handler = new DimensionHandler(this, dimensionName);
        }
    }
}
