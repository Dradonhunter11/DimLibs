using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace Dimlibs.Commands
{
    class SetDimension : ModCommand
    {
        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (DimWorld.dimensionInstanceHandlers.ContainsKey(args[0]))
            {
                DimWorld.dimension = args[0];
            }
        }

        public override string Command {
            get { return "SetDimension"; }
        }
        public override CommandType Type {
            get { return CommandType.Chat; }
        }
    }
}
