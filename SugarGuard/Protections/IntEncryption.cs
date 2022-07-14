using System;
using System.Security.Cryptography;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using SugarGuard.Core;

namespace SugarGuard.Protections
{
    public class IntEncryption : Protection
    {
        public override string Name => "IntEncryption";

        public override void Execute(Context Module)
        {
            int Amount = 0;
            IMethod absMethod = Module.Module.Import(typeof(Math).GetMethod("Abs", new Type[] { typeof(int) }));
            IMethod minMethod = Module.Module.Import(typeof(Math).GetMethod("Min", new Type[] { typeof(int), typeof(int) }));

            foreach (TypeDef type in Module.Module.Types)
                foreach (MethodDef method in type.Methods)
                {
                    if (!method.HasBody)
                        continue;

                    for (int i = 0; i < method.Body.Instructions.Count; i++)
                        if (method.Body.Instructions[i] != null && method.Body.Instructions[i].IsLdcI4())
                        {
                            int operand = method.Body.Instructions[i].GetLdcI4Value();
                            if (operand <= 0)
                                continue;

                            method.Body.Instructions.Insert(i + 1, OpCodes.Call.ToInstruction(absMethod));

                            int neg = Next(StringLength(), 8);
                            if (neg % 2 != 0)
                                neg += 1;

                            for (var j = 0; j < neg; j++)
                                method.Body.Instructions.Insert(i + j + 1, Instruction.Create(OpCodes.Neg));

                            if (operand < int.MaxValue)
                            {
                                method.Body.Instructions.Insert(i + 1, OpCodes.Ldc_I4.ToInstruction(int.MaxValue));
                                method.Body.Instructions.Insert(i + 2, OpCodes.Call.ToInstruction(minMethod));
                            }

                            ++Amount;
                        }
                }
        }

        private static readonly RandomNumberGenerator csp = RandomNumberGenerator.Create();

        public static string String(int size)
        {
            return Encoding.UTF7.GetString(RandomBytes(size))
                .Replace("\0", ".")
                .Replace("\n", ".")
                .Replace("\r", ".");
        }

        public static int Next()
        {
            return BitConverter.ToInt32(RandomBytes(sizeof(int)), 0);
        }

        private static uint RandomUInt()
        {
            return BitConverter.ToUInt32(RandomBytes(sizeof(uint)), 0);
        }

        private static byte[] RandomBytes(int bytes)
        {
            byte[] buffer = new byte[bytes];
            csp.GetBytes(buffer);
            return buffer;
        }

        public static int Next(int maxValue, int minValue = 0)
        {
            if (minValue >= maxValue)
                throw new ArgumentOutOfRangeException(nameof(minValue));

            long diff = (long)maxValue - minValue;
            long upperBound = uint.MaxValue / diff * diff;
            uint ui;
            do { ui = RandomUInt(); } while (ui >= upperBound);
            return (int)(minValue + (ui % diff));
        }

        public static int StringLength()
        {
            return Next(120, 30);
        }
    }
}