using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dimlibs.API;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Dimlibs
{
    public class Dimlibs : Mod
    {
        private string previousWorldPath;

        public Dimlibs()
        {
            Properties = new ModProperties()
            {
                Autoload = true
            };
        }

        public override void Load()
        {
            ReflectionUtil.MassSwap();
        }

        public override void Unload()
        {
            // TerrariaHooks undoes our swaps for us.
        }

        public override void PostSetupContent()
        {
            /*FieldInfo uiModLoadInfo = typeof(Main).Assembly.GetType("Terraria.ModLoader.Interface")
                .GetField("loadMods", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo field = typeof(Main).Assembly.GetType("Terraria.ModLoader.UI.UILoadMods")
                .GetField("SubProgressText", BindingFlags.Instance | BindingFlags.NonPublic);
            typeof(Main).Assembly.GetType("Terraria.ModLoader.UI.UILoadMods")
                .GetProperty("SubProgressText", BindingFlags.Instance | BindingFlags.Public).SetMethod.Invoke(uiModLoadInfo.GetValue(null), new object[] {"Autoload dimension"});*/
            //Interface.loadMods.SubProgressText = Language.GetTextValue("tModLoader.MSFinishingResourceLoading");
            LoadModContent(mod =>
            {
                //typeof(TmodFile).GetMethod("Read", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(mod, new object[] { TmodFile.LoadedState.Streaming, ReflectionUtil.GetEventDelegate() });
                //mod.File?.Read(TmodFile.LoadedState.Streaming, mod.LoadResourceFromStream);
                Autoload(mod);
            });
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

            if (previousWorldPath != Main.worldPathName)
            {
                Console.Write(Main.worldPathName);
            }

            Object obj = DimWorld.dimensionInstanceHandlers;
            previousWorldPath = Main.worldPathName;
            
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

        internal void Autoload(Mod mod)
        {

            if (mod.Code == null)
                return;

            foreach (Type type in mod.Code.GetTypes().OrderBy(type => type.FullName, StringComparer.InvariantCulture))
            {
                /*if (type.IsAbstract || type.GetConstructor(new Type[0]) == null)//don't autoload things with no default constructor
                {
                    continue;
                }*/
                if (type.IsSubclassOf(typeof(DimGenerator)))
                {
                    AutoloadDimension(type);
                }
                
            }
        }

        private static void LoadModContent(Action<Mod> loadAction)
        {
            //Object o = new OverworldHandler();
            int num = 0;
            foreach (var mod in ModLoader.LoadedMods)
            {
                try
                {
                    loadAction(mod);
                }
                catch (Exception e)
                {
                }
            }
        }

        private void AutoloadDimension(Type type)
        {
            DimGenerator dimension = (DimGenerator) Activator.CreateInstance(type);
        }
    }
}
