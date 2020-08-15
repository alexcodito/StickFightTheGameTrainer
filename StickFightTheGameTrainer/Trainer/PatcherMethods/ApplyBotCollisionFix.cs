using dnlib.DotNet;
using dnlib.DotNet.Emit;
using StickFightTheGameTrainer.Common;
using StickFightTheGameTrainer.Trainer.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StickFightTheGameTrainer.Trainer.PatcherMethods
{
    /// <summary>
    /// Make bots collide with each other. Normally AIs do not collide with each other as they are set on the same GameObject layer index. 
    /// </summary>
    internal static class ApplyBotCollisionFix
    {
        internal static async Task<int> Execute(ModuleDefMD targetModule)
        {
            // Fetch target type defs
            var controllerTypeDef = targetModule.Find("Controller", true);

            // Fetch target method defs
            var controllerSetCollisionMethodDef = controllerTypeDef.FindMethod("SetCollision");

            // Fetch target field defs
            var controllerIsAiFieldDef = controllerTypeDef.FindField("isAI");
            var controllerPlayerIdFieldDef = controllerTypeDef.FindField("playerID");

            /*
            
                32	004C	ldarg.0
                33	004D	ldfld	    bool Controller::isAI
                34	0052	brfalse.s	40 (0063) ldloc.2 

             */

            // Signature of the condition to which the check will be added
            var controllerSetCollisionAiCheckInstructionSignature = new List<Instruction> {
                new Instruction(OpCodes.Ldarg_0),
                new Instruction(OpCodes.Ldfld, controllerIsAiFieldDef),
                new Instruction(OpCodes.Brfalse, null)
            };

            var matchedControllerSetCollisionAiCheckMethodInstructions = InjectionHelpers.FetchInstructionsBySignature(controllerSetCollisionMethodDef.Body.Instructions, controllerSetCollisionAiCheckInstructionSignature, false);

            if (matchedControllerSetCollisionAiCheckMethodInstructions != null)
            {
                var branchInstruction = matchedControllerSetCollisionAiCheckMethodInstructions.Last();
                var injectionIndex = controllerSetCollisionMethodDef.Body.Instructions.IndexOf(branchInstruction) + 1;

                // Set unique gameObject layer in Controller.SetCollision for AIs with Player IDs.
                var controllerSetCollisionAiCheckInstructionsToInject = new List<Instruction>
                {
                    new Instruction(OpCodes.Ldarg_0),
                    new Instruction(OpCodes.Ldfld, controllerPlayerIdFieldDef),
                    new Instruction(OpCodes.Ldc_I4_0),
                    new Instruction(OpCodes.Bge_S, branchInstruction.Operand),
                };

                // Add new instructions after the matched signature
                for (var i = 0; i < controllerSetCollisionAiCheckInstructionsToInject.Count; i++)
                {
                    controllerSetCollisionMethodDef.Body.Instructions.Insert(injectionIndex + i, controllerSetCollisionAiCheckInstructionsToInject[i]);
                }

                controllerSetCollisionMethodDef.Body.UpdateInstructionOffsets();
            }
            else
            {
                return await Task.FromResult(1);
            }

            return await Task.FromResult(0);
        }
    }
}
