using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dnlib.DotNet;
using SugarGuard.Core;

namespace SugarGuard.Protections
{
    public class JunkProtection : Protection
    {
        public override string Name => "JunkProtection";

        public override void Execute(Context module)
        {
            int Number = 15;

            module.Module.GlobalType.Name = RandomString(40);
            for (int i = 0; i < 3285; i++)
            {
                TypeDef typeDef = new TypeDefUser(RandomString(Number) + "難読化VESTIGE_NET難読化" + RandomString(Number), RandomString(Number) + "難読化VESTIGE_NET難読化" + RandomString(Number));
                typeDef.Attributes = 0;
                module.Module.Types.Add(typeDef);
            }
        }

        public static string RandomString(int length)
        {
            return new string((from s in Enumerable.Repeat<string>("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ", length)
                               select s[random.Next(s.Length)]).ToArray<char>());
        }

        private static Random random = new Random();
    }
}