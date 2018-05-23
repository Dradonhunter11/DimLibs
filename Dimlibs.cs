using Terraria;
using Terraria.ModLoader;

namespace Dimlibs
{
	public class Dimlibs : Mod
	{
		public Dimlibs()
		{
            Properties = new ModProperties()
            {
                Autoload = true
            };
        }

        public override void Load()
        {
            base.Load();
        }

        public static string getPlayerDim() {
            DimPlayer plr = Main.LocalPlayer.GetModPlayer<DimPlayer>();
            return plr.getCurrentDimension();
        }
	}
}
