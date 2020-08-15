using dnlib.DotNet;
using dnlib.DotNet.Emit;
using StickFightTheGameTrainer.Trainer.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StickFightTheGameTrainer.Trainer.PatcherMethods
{
    internal static class EnableFlyingModeWeaponPickup
    {
        internal static async Task<int> Execute(ModuleDefMD targetModule)
        {
            var bodyPartTypeDef = targetModule.Find("BodyPart", true);
            var controllerTypeDef = targetModule.Find("Controller", true);

            var onCollisionEnterMethod = bodyPartTypeDef.FindMethod("OnCollisionEnter");
            var canFlyFieldDef = controllerTypeDef.FindField("canFly");

            var bodyPartControllerFieldDef = bodyPartTypeDef.FindField("controller");

            var instructionSignature = new List<Instruction> {
                new Instruction(OpCodes.Ldfld, bodyPartControllerFieldDef),
                new Instruction(OpCodes.Ldfld, canFlyFieldDef),
                new Instruction(OpCodes.Brtrue, new Instruction(OpCodes.Ldarg_1, null)),
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
                return await Task.FromResult(2);
            }

           return await Task.FromResult(0);
        }
    }
}
