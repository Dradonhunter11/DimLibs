using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Dimlibs.Commands
{
    class DimensionList : ModCommand
    {
        public override void Action(CommandCaller caller, string input, string[] args)
        {
            Main.NewText("=== Current active dimension === ");
            foreach (var dimKey in DimWorld.dimensionInstanceHandlers.Keys)
            {
                Main.NewText("- " + dimKey);
            }

        }

        public override string Command
        {
            get { return "DimensionList"; }
        }
        public override CommandType Type
        {
            get { return CommandType.Chat; }
        }
    }
}
