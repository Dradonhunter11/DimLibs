using Dimlibs.API;
using Dimlibs.Chunks;
using log4net;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.RuntimeDetour.HookGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Terraria;
using Terraria.ModLoader;

namespace Dimlibs
{
    public class Dimlibs : Mod
    {
        private readonly string previousWorldPath;
        internal static IList<ModCommand> commandsList = new List<ModCommand>();
        internal static Dimlibs Instance;

        public World tile = new World();

        public override void Load()
        {
            Instance = this;
            ReflectionUtil.Load();
            GetDimLibsCommand();
            for (int i = 0; i < ModLoader.Mods.Length; i++)
            {
                Mod mod = ModLoader.Mods[i];
                try
                {
                    Autoload(mod);
                }
                catch { }
            }
            //MassPatcher.StartPatching();

            ILog log = LogManager.GetLogger("HookGenerator");
            log.Info("==============================\n" + GenHook.ConvertToHook(typeof(DimensionHandler)));
            
        }

        public override void Unload()
        {
            ReflectionUtil.Unload();
        }

        public override void PostSetupContent()
        {
            // LoadModContent(Autoload);
            for (int i = 0; i < ModLoader.Mods.Length; i++)
            {
                Mod mod = ModLoader.Mods[i];
                try
                {
                    Autoload(mod);
                }
                catch
                {
                    mod.Logger.InfoFormat("Failure to autoload dimensions for mod {0}", mod.DisplayName);
                }
            }

        }

        public override void HandlePacket(BinaryReader reader, int whoAmI) => DimensionNetwork.HandlePacket(reader, whoAmI);

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
            {
                return;
            }

            TypeInfo[] array = mod.Code.DefinedTypes.OrderBy(type => type.FullName)
                .ToArray();
            for (int i = 0; i < array.Length; i++)
            {
                Type type = array[i];

                /*
                 * if (type.IsAbstract || type.GetConstructor(new Type[0]) == null) //don't autoload things with no default constructor
                 * {
                 * continue;
                 * }
                 */

                // if (typeof(DimGenerator).IsAssignableFrom(type))
                if (type.IsSubclassOf(typeof(DimGenerator)))
                {
                    AutoloadDimension(type);
                }
            }
        }

        private void GetDimLibsCommand()
        {
            FieldInfo commandListInfo =
                typeof(CommandManager).GetField("Commands", BindingFlags.Static | BindingFlags.NonPublic);
            Dictionary<String, List<ModCommand>> tempDictionary = (Dictionary<string, List<ModCommand>>)commandListInfo.GetValue(null);
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
            DimGenerator dimension = (DimGenerator)Activator.CreateInstance(type);
            DimWorld.dimensionInstanceHandlers[dimension.dimensionName] = dimension.handler;
            ILog logger = LogManager.GetLogger("Kaboom");
            foreach (string str in DimWorld.dimensionInstanceHandlers.Keys)
            {
                logger.Info(str);
            }
        }

        internal static class ILPatching
        {
            public static void Load()
            {
                On.Terraria.Main.ClampScreenPositionToWorld += orig =>
                { return; };
                On.Terraria.Player.BordersMovement += (orig, player) =>
                { return; };
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
                Type[] array = GetAllTypeInCurrentAssembly(asm);
                for (int i = 0; i < array.Length; i++)
                {
                    Type typeInfo = array[i];
                    if (typeInfo.Namespace == null || typeInfo.Namespace.Contains("ModLoader"))
                    {
                        continue;
                    }
                    MethodInfo[] array1 = GetAllMethodInAType(typeInfo);
                    for (int i1 = 0; i1 < array1.Length; i1++)
                    {
                        MethodInfo methodInfo = array1[i1];
                        try
                        {
                            SetLoadingStatusText("Currently patching " + typeInfo.FullName);
                            HookEndpointManager.Modify(methodInfo, new ILManipulator(ILEditing));
                        }
                        catch (Exception e)
                        {
                            Instance.Logger.Error("Failed to patch ", e);
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
                    if (operandType != null && operandType is FieldReference fieldRef)
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
                setProgressText.Invoke(uiModLoadInstance, new object[] { statusText });
            }
        }

        internal static class GenHook
        {

            public static string ConvertToHook(Type type)
            {
                StringBuilder builder = new StringBuilder();

                foreach (var methodInfo in type.GetMethods())
                {
                    string hook;
                    builder.AppendLine(GenerateOrigDelegate(methodInfo, type));
                    builder.AppendLine(GenerateHookDelegate(methodInfo, type, out hook));
                    builder.AppendLine();
                    builder.AppendLine(GenerateEvent(hook, type, methodInfo));
                    builder.AppendLine();
                }
                return builder.ToString();
            }

            public static string GenerateHookDelegate(MethodInfo method, Type type, out string hook)
            {
                string str = "public delegate " + ((method.ReturnType.Name == "Void") ? "void" : method.ReturnType.Name) + " hook_" + method.Name + "(orig_" + method.Name + " orig";

                hook = "hook_" + method.Name;

                if (!method.IsStatic)
                {
                    str += ", " + type.Name + " self";
                }

                if (method.GetParameters().Length != 0)
                {
                    str += ", ";
                }

                for (int i = 0; i < method.GetParameters().Length; i++)
                {

                    str += method.GetParameters()[i].ParameterType.Name + " " + method.GetParameters()[i].Name;
                    if (i < method.GetParameters().Length - 1)
                    {
                        str += ", ";
                    }
                }


                str += ");";

                return str;
            }

            public static string GenerateOrigDelegate(MethodInfo method, Type type)
            {
                string str = "public delegate " + ((method.ReturnType.Name == "Void") ? "void" : method.ReturnType.Name) + " orig_" + method.Name + "(";

                if (!method.IsStatic)
                {
                    str += type.Name + " self";
                }

                if (method.GetParameters().Length != 0)
                {
                    str += ", ";
                }

                for (int i = 0; i < method.GetParameters().Length; i++)
                {

                    str += method.GetParameters()[i].ParameterType.Name + " " + method.GetParameters()[i].Name;
                    if (i < method.GetParameters().Length - 1)
                    {
                        str += ", ";
                    }
                }

                str.Replace("Void", "void");

                str += ");";

                return str;
            }

            public static string GenerateEvent(string hook, Type type, MethodInfo method)
            {
                StringBuilder str = new StringBuilder("public static event " + hook + " " + hook.Replace("hook_", "") + "_hook");
                str.AppendLine();
                str.AppendLine("{");
                str.AppendLine("    add");
                str.AppendLine("    {");
                str.AppendLine($"      HookEndpointManager.Add(typeof({type.Name}).GetMethod(\"{method.Name}\"), value);");
                str.AppendLine("    }");
                str.AppendLine("    remove");
                str.AppendLine("    {");
                str.AppendLine(
                    $"       HookEndpointManager.Remove(typeof({type.Name}).GetMethod(\"{method.Name}\"), value);");
                str.AppendLine("    }");
                str.AppendLine("}");

                return str.ToString();
            }
        }
    }
}
