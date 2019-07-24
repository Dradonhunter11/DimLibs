using System;
using Terraria.ModLoader;

namespace Dimlibs.Commands
{
    class DimHelp : ModCommand
    {
        public override void Action(CommandCaller caller, string input, string[] args)
        {
            Console.WriteLine("[c/FFFF00:=== Dimlibs command libs ===]");
            foreach (var modCommand in Dimlibs.commandsList)
            {
                Console.WriteLine("- /" + modCommand.Command + " (" + modCommand.Description + ")");
            }
        }

        public override string Command
        {
            get { return "DimHelp"; }
        }

        public override string Description
        {
            get { return "Show this list"; }
        }

        public override CommandType Type
        {
            get { return CommandType.Console; }
        }
    }
}
