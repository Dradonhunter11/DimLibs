using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria.World.Generation;

namespace Dimlibs.API
{
    public abstract class ModDimension
	{

		public string Name;

		public string URN => mod.Name + ":" + Name;

		internal DimensionHandler.DimensionHandler handler;

        public Mod mod
        {
            get;
            internal set;
        }

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

        public int dimensionMaxTileX;
        public int dimensionMaxTileY;


        public abstract void ModifyGenerationPass(int seed, GenerationProgress customProgressObject);

        public List<GenPass> GetPasses(WorldGenerator generator) => (List<GenPass>) typeof(WorldGenerator)
            .GetField("_passes", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(generator);

        public float GetTotalLoadWeight(WorldGenerator generator) => (float)typeof(WorldGenerator)
            .GetField("_totalLoadWeight", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(generator);


        public void SetTotalLoadWeight(WorldGenerator generator, float weight) => typeof(WorldGenerator)
            .GetField("_totalLoadWeight", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(generator, weight);

        public WorldGenerator _generator
        {
            get => (WorldGenerator)typeof(WorldGen).GetField("_generator", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            set => typeof(WorldGen).GetField("_generator", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, value);
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
            PostGeneration();
        }

        public void reset(int seed)
        {
            if (dimensionMaxTileX > 500)
            {
                Main.maxTilesX = dimensionMaxTileX;
            }

            if (dimensionMaxTileY > 500)
            {
                Main.maxTilesY = dimensionMaxTileY;
            }

            
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
        }

        public virtual void PostGeneration()
        {

        }

        private void final(GenerationProgress customProgressObject)
        {
            float weight = GetTotalLoadWeight(_generator);
            SetTotalLoadWeight(_generator, weight);
            Main.menuMode = 888;
            if(this.GetType().Name != "OverworldDimension") //To avoid generation of the overworld twice
                gen(customProgressObject);
            Main.WorldFileMetadata = FileMetadata.FromCurrentSettings(FileType.World);
        }

        private async void gen(GenerationProgress customProgressObject)
        {
            g(customProgressObject);
        }

        public void g(GenerationProgress customProgressObject)
        {
            _generator.GenerateWorld(customProgressObject);
        }

        /// <summary>
        /// Allow you to draw your own custom loading background
        /// Return false if you want to make the default one draw
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <returns></returns>
        public virtual bool DrawCustomBackground(SpriteBatch spriteBatch)
        {
            return false;
        }

        /// <summary>
        /// Set the world size there I guess?
        /// </summary>
        public virtual void SetDefault()
        {
            dimensionMaxTileX = Main.maxTilesX;
            dimensionMaxTileY = Main.maxTilesY;
        }

        public DimensionHandler.DimensionHandler GetHandler()
        {
            return handler;
        }

        public ModDimension()
        {
            handler = new DimensionHandler.DimensionHandler(this.GetType().Name);
        }
    }
}
