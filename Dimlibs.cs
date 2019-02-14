using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using Dimlibs.API;
using Dimlibs.Chunks;
using log4net;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.RuntimeDetour.HookGen;
using Terraria;
using Terraria.ModLoader;

namespace Dimlibs
{
    public class Dimlibs : Mod
    {
        private string previousWorldPath;
        internal static IList<ModCommand> commandsList = new List<ModCommand>();
        internal static Dimlibs Instance;

        public World tile = new World();

        public Dimlibs()
        {
            Properties = new ModProperties()
            {
                Autoload = true
            };
        }

        public override void Load()
        {
            Instance = this;
            ReflectionUtil.Load();
            GetDimLibsCommand();
            LoadModContent(mod =>
            {
                Autoload(mod);
            });
            //MassPatcher.StartPatching();
        }

        public override void Unload()
        {
            ReflectionUtil.Unload();
        }

        public override void PostSetupContent()
        {
            LoadModContent(Autoload);
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            DimensionNetwork.HandlePacket(reader, whoAmI);
        }

        public static string getPlayerDim()
        {
            if (Main.netMode == 0)
            {
                return DimWorld.dimension;
            }

            return Main.LocalPlayer.GetModPlayer<DimPlayer>().getServerDimension();

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
            foreach (var mod in ModLoader.Mods)
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

        private void GetDimLibsCommand()
        {
            FieldInfo commandListInfo =
                typeof(CommandManager).GetField("Commands", BindingFlags.Static | BindingFlags.NonPublic);
            Dictionary<String, List<ModCommand>> tempDictionary = (Dictionary<string, List<ModCommand>>) commandListInfo.GetValue(null);
            Dictionary<string, List<ModCommand>>.ValueCollection a = tempDictionary.Values;
            foreach (var modCommandList in a)
            {
                foreach (var modCommand in modCommandList)
                {
                    if (modCommand.mod.Name == Name)
                    {
                        commandsList.Add(modCommand);
                    }
                }
            }
        }

        private void AutoloadDimension(Type type)
        {
            DimGenerator dimension = (DimGenerator) Activator.CreateInstance(type);
            DimWorld.dimensionInstanceHandlers[dimension.dimensionName] = dimension.handler;
            ILog logger = LogManager.GetLogger("Kaboom");
            foreach (string str in DimWorld.dimensionInstanceHandlers.Keys)
            {
                logger.Info(str);
            }
        }

        internal static class ILPatching
        {
            public static void load()
            {
                On.Terraria.Main.ClampScreenPositionToWorld += RemoveCameraLimit;
                On.Terraria.Player.BordersMovement += RemoveBordersMovement;
            }

            private static void RemoveCameraLimit(On.Terraria.Main.orig_ClampScreenPositionToWorld orig)
            {
                return;
            }

            private static void RemoveBordersMovement(On.Terraria.Player.orig_BordersMovement orig, Player player)
            {
                return;
            }
        }

        internal static class MassPatcher
        {
            internal static Type[] GetAllTypeInCurrentAssembly(Assembly asm)
            {
                return asm.GetTypes();
            }

            internal static MethodInfo[] GetAllMethodInAType(Type type)
            {
                return type.GetMethods();
            }

            public static void StartPatching()
            {
                //ILog log = LogManager.GetLogger("Mass Patcher");
                var asm = Assembly.GetAssembly(typeof(Main));
                foreach (var typeInfo in GetAllTypeInCurrentAssembly(asm))
                {
                    if (typeInfo.Namespace == null || typeInfo.Namespace.Contains("ModLoader"))
                    {
                        continue;
                    }
                    foreach (var methodInfo in GetAllMethodInAType(typeInfo))
                    {
                        try
                        {
                            SetLoadingStatusText("Currently patching " + typeInfo.FullName);
                            MonoMod.RuntimeDetour.HookGen.HookEndpointManager.Modify(methodInfo, new ILManipulator(MassPatcher.ILEditing));
                        }
                        catch (Exception e)
                        {
                            //log.Error(e.Message, e);
                        }                      
                    }
                }
            }

            internal static void ILEditing(HookIL il)
            {
                ILog log = LogManager.GetLogger("Mass Patcher");
                PropertyInfo indexerInfo = typeof(Chunks.World).GetProperty("Item",
                    BindingFlags.Public | BindingFlags.Instance, null, typeof(Tile),
                    new Type[] { typeof(Int32), typeof(Int32) }, null);
                MethodReference get_ItemReference = il.Import(indexerInfo.GetGetMethod());
                MethodReference set_ItemReference = il.Import(indexerInfo.GetSetMethod());

                log.Info(get_ItemReference);
                log.Info(set_ItemReference);
                foreach (var instruction in il.Body.Instructions)
                {
                    Object operandType = instruction.Operand;
                    if (operandType != null && operandType is Mono.Cecil.FieldReference fieldRef)
                    {
                        FieldReference tileReference =
                            il.Module.ImportReference(typeof(Dimlibs).GetField("tile",
                                BindingFlags.Public | BindingFlags.Static));
                        if (fieldRef.FullName.Contains("Terraria.Tile[0...,0...] Terraria.Main::tile") && instruction.OpCode == OpCodes.Ldsfld)
                        {
                            instruction.Operand = tileReference;
                            instruction.OpCode = OpCodes.Ldsfld;

                        }
                    }

                    if (instruction != null && instruction.Operand is Mono.Cecil.MethodReference reference)
                    {
                        if (reference.FullName.Contains("Terraria.Tile[0..., 0...]::Get(int32, int32)") && instruction.OpCode == OpCodes.Call)
                        {
                            instruction.OpCode = OpCodes.Callvirt;
                            instruction.Operand = get_ItemReference;
                        }
                        if (reference.FullName.Contains("Terraria.Tile[0..., 0...]::Set(int32, int32, class Terraria.Tile)") && instruction.OpCode == OpCodes.Call)
                        {
                            instruction.OpCode = OpCodes.Callvirt;
                            instruction.Operand = set_ItemReference;
                        }
                    }
                }
            }

            public static void SetLoadingStatusText(string statusText)
            {
                Object uiModLoadInstance = typeof(Main).Assembly.GetType("Terraria.ModLoader.Interface")
                    .GetField("loadMods", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
                PropertyInfo subProgressText = typeof(Main).Assembly.GetType("Terraria.ModLoader.UI.UILoadMods")
                    .GetProperty("SubProgressText", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo setProgressText = subProgressText.GetSetMethod();
                setProgressText.Invoke(uiModLoadInstance, new object[] {statusText});
            }
        }
    }
}
