using dnlib.DotNet;
using dnlib.DotNet.Emit;
using SugarGuard.Core;
using SugarGuard.Helpers.Injection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace SugarGuard.Protections
{
    public class StringEncryption2 : Protection
    {
        public static bool done = false;
        public static MethodDef decryptmeth;
        public static MethodDef basedecode1;
        private static List<TypeDef> types_with_strings = new List<TypeDef>();

        public override string Name => "StringEncryption";

        public override void Execute(Context moduleDef)
        {
            foreach (TypeDef type in moduleDef.Module.Types)
            {
                foreach (MethodDef method in type.Methods)
                {
                    if (method.Body == null) continue;
                    for (int i = 0; i < method.Body.Instructions.Count(); i++)
                    {
                        if (method.Body.Instructions[i].OpCode == OpCodes.Ldstr)
                        {
                            types_with_strings.Add(type);
                            break;
                        }
                    }
                }
            }
            InjectClass(moduleDef.Module);
            foreach (TypeDef type in moduleDef.Module.Types)
            {
                foreach (MethodDef method in type.Methods)
                {
                    if (method.Body == null) continue;
                    for (int i = 0; i < method.Body.Instructions.Count(); i++)
                    {
                        if (method.Body.Instructions[i].OpCode == OpCodes.Ldstr)
                        {
                            String oldString = method.Body.Instructions[i].Operand.ToString();
                            String newString = String.Concat(Enumerable.Repeat("SKIDDED! ", 2000)) + "L" + base69420(oldString, 2);
                            method.Body.Instructions[i].OpCode = OpCodes.Nop;

                            method.Body.Instructions.Insert(i + 1, new Instruction(OpCodes.Ldstr, newString));
                            method.Body.Instructions.Insert(i + 2, new Instruction(OpCodes.Call, decryptmeth));
                            method.Body.Instructions.Insert(i + 3, new Instruction(OpCodes.Ldc_I4_2));
                            method.Body.Instructions.Insert(i + 4, new Instruction(OpCodes.Call, basedecode1));
                            i += 4;
                            CorLibTypeSig corLibTypeSig = moduleDef.Module.CorLibTypes.String;
                            method.Body.SimplifyBranches();
                            method.Body.OptimizeBranches();
                        }
                    }
                    if (!done)
                    {
                        InjectClass(moduleDef.Module);
                        done = true;
                    }
                }
            }
        }

        public static string EncodeBase64(string value)
        {
            var valueBytes = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(valueBytes);
        }

        public static string base69420(string start, int times)
        {
            string curstr = start;
            for (int a = 0; a <= times; a++)
            {
                curstr = EncodeBase64(curstr);
            }
            return curstr;
        }

        public static void InjectClass(ModuleDef module)
        {
            var typeModule = ModuleDefMD.Load(typeof(injectme).Module);
            var typeDef = typeModule.ResolveTypeDef(MDToken.ToRID(typeof(injectme).MetadataToken));
            foreach (TypeDef type in types_with_strings)
            {
                var members = InjectHelper.Inject(typeDef, type, module);
                decryptmeth = (MethodDef)members.Single(method => method.Name == "decrypt");
                basedecode1 = (MethodDef)members.Single(method => method.Name == "base69420");
            }
            var cctor = module.GlobalType.FindStaticConstructor();

            foreach (var md in module.GlobalType.Methods)
            {
                if (md.Name == ".ctor")
                {
                    module.GlobalType.Remove(md);
                    break;
                }
            }
        }
    }

    internal class injectme
    {
        public static string decrypt(string splittable)
        {
            string[] subs = splittable.Split('L');

            return subs[subs.Length - 1];
        }

        public static string base69420(string baseb, int times)
        {
            string curstr = baseb;
            for (int a = 0; a <= times; a++)
            {
                curstr = b(curstr);
            }
            return curstr;
        }

        public static string b(string value)
        {
            var valueBytes = System.Convert.FromBase64String(value);
            return Encoding.UTF8.GetString(valueBytes);
        }
    }
}