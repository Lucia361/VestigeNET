using dnlib.DotNet;
using dnlib.DotNet.Emit;
using SugarGuard.Core;

namespace SugarGuard.Protections
{
    public class Watermark : Protection
    {
        public override string Name => "LocalToField";

        public override void Execute(Context md)
        {
            foreach (var moduleDef in md.Module.Assembly.Modules)
            {
                var module = (ModuleDefMD)moduleDef;
                var attrRef = module.CorLibTypes.GetTypeRef("System", "Attribute");

                var attrType = new TypeDefUser("", "[Protected_By_VestigeNET]", attrRef);
                var attrType2 = new TypeDefUser("", "[Obfusacted_By_VesitgeNET]", attrRef);
                var attrType3 = new TypeDefUser("", "[Vestige#0001]", attrRef);
                module.Types.Add(attrType);
                module.Types.Add(attrType2);
                module.Types.Add(attrType3);

                var ctor = new MethodDefUser(
                    ".ctor",
                    MethodSig.CreateInstance(module.CorLibTypes.Void, module.CorLibTypes.String),
                    MethodImplAttributes.Managed,
                    MethodAttributes.HideBySig | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName)
                {
                    Body = new CilBody()
                };
                ctor.Body.MaxStack = 1;
                ctor.Body.Instructions.Add(OpCodes.Ldarg_0.ToInstruction());
                ctor.Body.Instructions.Add(OpCodes.Call.ToInstruction(new MemberRefUser(module, ".ctor", MethodSig.CreateInstance(module.CorLibTypes.Void), attrRef)));
                ctor.Body.Instructions.Add(OpCodes.Ret.ToInstruction());
                attrType.Methods.Add(ctor);
            }
        }
    }
}