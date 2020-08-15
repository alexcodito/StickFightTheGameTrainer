using dnlib.DotNet;
using dnlib.DotNet.Emit;
using StickFightTheGameTrainer.Trainer.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StickFightTheGameTrainer.Trainer.PatcherMethods
{
    /// <summary>
    /// Make bots that are added to ControllerHandler.Players "die" instead of simply being "removed" like normal AIs.
    /// </summary>
    internal static class ApplyBotDeathFix
    {
        internal static async Task<int> Execute(ModuleDefMD targetModule)
        {
            // Fetch target type defs
            var controllerTypeDef = targetModule.Find("Controller", true);
            var healthHandlerTypeDef = targetModule.Find("HealthHandler", true);

            // Fetch target method defs
            var healthHandlerDieMethodDef = healthHandlerTypeDef.FindMethod("Die");

            // Fetch target field defs
            var controllerIsAiFieldDef = controllerTypeDef.FindField("isAI");
            var controllerPlayerIdFieldDef = controllerTypeDef.FindField("playerID");
            var healthHandlerControllerFieldDef = healthHandlerTypeDef.FindField("controller");

            /*
            
                55	009F	brfalse.s	69 (00CB) call bool MatchmakingHandler::get_IsNetworkMatch()
                56	00A1	ldarg.0
                57	00A2	ldfld	    class Controller HealthHandler::controller
                58	00A7	ldfld	    bool Controller::isAI
                59	00AC	brfalse.s	69 (00CB) call bool MatchmakingHandler::get_IsNetworkMatch()

             */

            // Signature of the condition to which the check will be added
            var healthHandlerAiCheckInstructionSignature = new List<Instruction> {
                new Instruction(OpCodes.Brfalse, null),
                new Instruction(OpCodes.Ldarg_0),
                new Instruction(OpCodes.Ldfld, healthHandlerControllerFieldDef),
                new Instruction(OpCodes.Ldfld, controllerIsAiFieldDef),
                new Instruction(OpCodes.Brfalse, null)
            };

            var matchedHealthHandlerDieMethodInstructions = InjectionHelpers.FetchInstructionsBySignature(healthHandlerDieMethodDef.Body.Instructions, healthHandlerAiCheckInstructionSignature, false);

            if (matchedHealthHandlerDieMethodInstructions != null)
            {
                var branchInstruction = matchedHealthHandlerDieMethodInstructions.Last();
                var injectionIndex = healthHandlerDieMethodDef.Body.Instructions.IndexOf(branchInstruction) + 1;

                // Add check for a valid playerID when checking that the controller is an AI
                var healtHandlerAiCheckInstructionsToInject = new List<Instruction>
                {
                    new Instruction(OpCodes.Ldarg_0),
                    new Instruction(OpCodes.Ldfld, healthHandlerControllerFieldDef),
                    new Instruction(OpCodes.Ldfld, controllerPlayerIdFieldDef),
                    new Instruction(OpCodes.Ldc_I4_M1),
                    new Instruction(OpCodes.Bgt_S, branchInstruction.Operand),
                };

                // Add new instructions after the matched signature
                for (var i = 0; i < healtHandlerAiCheckInstructionsToInject.Count; i++)
                {
                    healthHandlerDieMethodDef.Body.Instructions.Insert(injectionIndex + i, healtHandlerAiCheckInstructionsToInject[i]);
                }

                healthHandlerDieMethodDef.Body.UpdateInstructionOffsets();
            }
            else
            {
                return await Task.FromResult(1);
            }

            return await Task.FromResult(0);
        }
    }
}
