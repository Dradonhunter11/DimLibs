using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.IO;
using Terraria.ModLoader;

namespace Dimlibs.API
{
    class DimTile : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            return false;
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
        }

        public override void RightClick(int i, int j)
        {
            DimPlayer p = Main.LocalPlayer.GetModPlayer<DimPlayer>();

            FieldInfo info = typeof(FileData).GetField("_path", BindingFlags.Instance | BindingFlags.NonPublic);
            string get = (string) info.GetValue(Main.ActiveWorldFileData);

        }

        /*
        if (itemUseCooldown == 0)
        {
            WorldFile.saveWorld(false, true);
            if (p.getCurrentDimension() != dimensionName)
            {
                p.setCurrentDimension(dimensionName);
                if (dimensionMessage != null)
                {
                    Main.NewText(dimensionMessage, Color.Orange);
                }
                else
                {
                    Main.NewText("You are entering into a custom dimension...", Color.Orange);
                }
                if (!File.Exists(Main.SavePath + "/World/" + dimensionName + "/" + Main.worldName + ".wld"))
                {

                    info.SetValue(Main.ActiveWorldFileData, Main.SavePath + "/World/" + dimensionName + "/" + Main.worldName + ".wld");
                    startGen();
                    p.player.Spawn();
                    //WorldFile.saveWorld(false, true);
                }

                info.SetValue(Main.ActiveWorldFileData, Main.SavePath + "/World/" + dimensionName + "/" + Main.worldName + ".wld");
                itemUseCooldown = 500;
                //WorldGen.EveryTileFrame();
                WorldGen.playWorld();
            }
            info.SetValue(Main.ActiveWorldFileData, Main.SavePath + "/World/" + Main.worldName + ".wld");
            p.setCurrentDimension("overworld");
            itemUseCooldown = 500;
            WorldGen.playWorld();
        }
    }*/
    }
}