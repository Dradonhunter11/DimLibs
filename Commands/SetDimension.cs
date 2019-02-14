using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Dimlibs.Commands
{
    class SetDimension : ModCommand
    {
        public override void Action(CommandCaller caller, string input, string[] args)
        {
            
            if (DimWorld.dimensionInstanceHandlers.ContainsKey(args[0]))
            {
                if (Main.netMode == 1)
                {
                    DimensionNetwork.ClientSendRequest(args[0]);
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
                    DimWorld.dimensionInstanceHandlers[args[0]].LoadWorld();
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
            get { return CommandType.World; }
        }
    }
}
