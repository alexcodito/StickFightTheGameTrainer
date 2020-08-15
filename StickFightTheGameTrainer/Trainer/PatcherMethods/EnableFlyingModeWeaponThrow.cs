using dnlib.DotNet;
using dnlib.DotNet.Emit;
using StickFightTheGameTrainer.Trainer.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StickFightTheGameTrainer.Trainer.PatcherMethods
{
    internal static class EnableFlyingModeWeaponThrow
    {
        internal static async Task<int> Execute(ModuleDefMD targetModule)
        {
            var controllerTypeDef = targetModule.Find("Controller", true);

            var onCollisionEnterMethod = controllerTypeDef.FindMethod("Update");
            var canFlyFieldDef = controllerTypeDef.FindField("canFly");

            var instructionSignature = new List<Instruction> {
                new Instruction(OpCodes.Ldfld, canFlyFieldDef),
                new Instruction(OpCodes.Brtrue, new Instruction(OpCodes.Ldarg_0, null)),
                new Instruction(OpCodes.Ldarg_0, null)
            };

            var matchedInstructions = InjectionHelpers.FetchInstructionsBySignature(onCollisionEnterMethod.Body.Instructions, instructionSignature, false);

            if (matchedInstructions != null)
            {
                // NOP the matched instructions
                matchedInstructions.ForEach(matchedInstruction => matchedInstruction.OpCode = OpCodes.Nop);
            }
            else
            {
                return await Task.FromResult(1);
            }

            return await Task.FromResult(0);
        }
    }
}
