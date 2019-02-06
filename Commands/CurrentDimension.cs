using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Dimlibs.Commands
{
    class CurrentDimension : ModCommand
    {
        public override void Action(CommandCaller caller, string input, string[] args)
        {
            Console.WriteLine("Current dimension: " + DimWorld.dimension);

        }

        public override string Command
        {
            get { return "CurrentDimension"; }
        }

        public override string Description
        {
            get { return "Show the current dimension you are in"; }
        }

        public override CommandType Type
        {
            get { return CommandType.Console; }
        }
    }
}
