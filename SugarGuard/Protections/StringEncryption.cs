using dnlib.DotNet;
using dnlib.DotNet.Emit;
using SugarGuard.Core;
using SugarGuard.Helpers.Injection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SugarGuard.Protections
{
    public class StringEncryption : Protection
    {
        public override string Name => "StringEncryption";

        public override void Execute(Context module)
        {
            int Amount = 0;
            ModuleDefMD typeModule = ModuleDefMD.Load(typeof(StringDecoder).Module);
            TypeDef typeDef = typeModule.ResolveTypeDef(MDToken.ToRID(typeof(StringDecoder).MetadataToken));
            IEnumerable<IDnlibDef> members = InjectHelper.Inject(typeDef, module.Module.GlobalType, module.Module);
            MethodDef init = (MethodDef)members.Single(method => method.Name == "Decrypt");

            foreach (MethodDef method in module.Module.GlobalType.Methods)
                if (method.Name.Equals(".ctor"))
                {
                    module.Module.GlobalType.Remove(method);
                    break;
                }

            foreach (TypeDef type in module.Module.Types)
                foreach (MethodDef method in type.Methods)
                {
                    if (!method.HasBody)
                        continue;

                    method.Body.SimplifyBranches();

                    for (int i = 0; i < method.Body.Instructions.Count; i++)
                        if (method.Body.Instructions[i] != null && method.Body.Instructions[i].OpCode == OpCodes.Ldstr)
                        {
                            int key = IntEncryption.Next();
                            object op = method.Body.Instructions[i].Operand;

                            if (op == null)
                                continue;

                            method.Body.Instructions[i].Operand = Encrypt(op.ToString(), key);
                            method.Body.Instructions.Insert(i + 1, OpCodes.Ldc_I4.ToInstruction(IntEncryption.Next()));
                            method.Body.Instructions.Insert(i + 2, OpCodes.Ldc_I4.ToInstruction(key));
                            method.Body.Instructions.Insert(i + 3, OpCodes.Ldc_I4.ToInstruction(IntEncryption.Next()));
                            method.Body.Instructions.Insert(i + 4, OpCodes.Ldc_I4.ToInstruction(IntEncryption.Next()));
                            method.Body.Instructions.Insert(i + 5, OpCodes.Ldc_I4.ToInstruction(IntEncryption.Next()));
                            method.Body.Instructions.Insert(i + 6, OpCodes.Call.ToInstruction(init));

                            ++Amount;
                        }

                    method.Body.OptimizeBranches();
                }
        }

        public static string Encrypt(string str, int key)
        {
            StringBuilder builder = new StringBuilder();
            foreach (char c in str.ToCharArray())
                builder.Append((char)(c + key));

            return builder.ToString();
        }

        public static class StringDecoder
        {
            public static string Decrypt(string str, int min, int key, int hash, int length, int max)
            {
                if (max > 78787878) ;
                if (length > 485941) ;

                StringBuilder builder = new StringBuilder();
                foreach (char c in str.ToCharArray())
                    builder.Append((char)(c - key));

                if (min < 14141) ;
                if (length < 1548174) ;

                return builder.ToString();
            }
        }
    }
}