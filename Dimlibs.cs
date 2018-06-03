using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.Localization;
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

        public static string getPlayerDim()
        {
            DimPlayer plr = Main.LocalPlayer.GetModPlayer<DimPlayer>();
            return plr.getCurrentDimension();
        }

        public override void UpdateMusic(ref int music, ref MusicPriority musicPriority)
        {
            if (Main.myPlayer != -1 && Main.gameMenu && Main.LocalPlayer.name != "")
            {
                DimPlayer p = Main.player[Main.myPlayer].GetModPlayer<DimPlayer>();
                FieldInfo info = typeof(LanguageManager).GetField("_localizedTexts", BindingFlags.Instance | BindingFlags.NonPublic);
                Dictionary<string, LocalizedText> dictionary = info.GetValue(LanguageManager.Instance) as Dictionary<string, LocalizedText>;

                FieldInfo textInfo = typeof(LocalizedText).GetField("value", BindingFlags.Instance | BindingFlags.NonPublic);
                setDimensionPath(p, dictionary, textInfo);


            }
        }

        private static void setDimensionPath(DimPlayer p, Dictionary<string, LocalizedText> dictionary, FieldInfo textInfo)
        {

            if (getPlayerDim() != "overworld")
            {
                Main.WorldPath = Main.SavePath + "/World/" + getPlayerDim().Replace(' ', '_');
            }
            else if (getPlayerDim() == "overworld")
            {
                Main.WorldPath = Main.SavePath + "/World";
            }

        }
    }
}
