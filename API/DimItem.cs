using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Terraria;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Dimlibs.API
{
    
    public class DimItem : ModItem
    {
        public string dimensionName { get; set; }
        internal DimGenerator generator;
        public int itemUseCooldown = 0;
        public override bool Autoload(ref string name)
        {
            if (name == "DimItem") {
                return false;
            }
            return true;
        }

        public override void SetDefaults()
        {
            item.maxStack = 1;
        }

        public void setDimensionGenerator(DimGenerator generator) {
            this.generator = generator;
        }

        public override bool UseItem(Player player)
        {
            DimPlayer p = player.GetModPlayer<DimPlayer>();

            FieldInfo info = typeof(FileData).GetField("_path", BindingFlags.Instance | BindingFlags.NonPublic);
            string get = (string)info.GetValue(Main.ActiveWorldFileData);

            if (itemUseCooldown == 0)
            {
                WorldFile.saveWorld(false, true);
                if (p.getCurrentDimension() != dimensionName)
                {
                    p.setCurrentDimension(dimensionName);
                    Main.NewText("You are entering into a custom dimension...", Color.Orange);

                    if (!File.Exists(Main.SavePath + "/World/" + dimensionName + "/" + Main.worldName + ".wld"))
                    {
                        info.SetValue(Main.ActiveWorldFileData, Main.SavePath + "/World/" + dimensionName + "/" + Main.worldName + ".wld");
                        generateDimension();
                        p.player.Spawn();
                        return true;
                    }

                    info.SetValue(Main.ActiveWorldFileData, Main.SavePath + "/World/" + dimensionName + "/" + Main.worldName + ".wld");
                    itemUseCooldown = 500;
                    WorldGen.EveryTileFrame();
                    WorldGen.playWorld();
                    return true;
                }
                info.SetValue(Main.ActiveWorldFileData, Main.SavePath + "/World/" + Main.worldName + ".wld");
                p.setCurrentDimension("overworld");
                itemUseCooldown = 500;
                WorldGen.playWorld();
                return true;
            }
            return false;
        }

        internal void generateDimension() {
            WorldFile.saveWorld(false, true);
            WorldGen.clearWorld();

            generator.GenerateDimension(Main.rand.Next());
            itemUseCooldown = 500;

            WorldGen.EveryTileFrame();
        }

        public override void UpdateInventory(Player player)
        {
            itemUseCooldown--;
            if (itemUseCooldown < 0)
            {
                itemUseCooldown = 0;
            }
        }
    }
}