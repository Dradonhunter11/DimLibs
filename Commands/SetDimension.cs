using Terraria;
using Terraria.ModLoader;

namespace Dimlibs.Commands
{
    /*class SetDimension : ModCommand
    {
        public override void Action(CommandCaller caller, string input, string[] args)
        {

            if (Dimlibs.dimensionInstanceHandlers.ContainsKey("Dimlibs:OverworldDimension"))
            {
                if (Main.netMode == 1)
                {
                    DimensionNetwork.ClientKickRequest("Dimlibs:OverworldDimension");
                }

                if (Main.netMode == 0)
                {
                    DimWorld.SwapDimension();
                }
            }
            
            
            return;
            
            if (Dimlibs.dimensionInstanceHandlers.ContainsKey(args[0]))
            {
                if (Main.netMode == 1)
                {
                    DimensionNetwork.ClientKickRequest("Dimension");
                    return;
                }

                if (Main.netMode == 2)
                {
                    Main.player[Main.LocalPlayer.whoAmI].GetModPlayer<DimPlayer>().serverCurrentDimension = args[0];
                    DimWorld.dimension = args[0];
                    DimWorld.update = true;
                }

                if (Main.netMode == 0)
                {
                    DimWorld.dimension = args[0];
                    Dimlibs.dimensionInstanceHandlers[args[0]].handler.LoadWorld();
                }

            }
        }

        public override string Command {
            get { return "SetDimension"; }
        }

        public override string Description
        {
            get { return "Change the current dimension you are in"; }
        }

        public override CommandType Type {
            get { return CommandType.Chat; }
        }
    }*/
}
