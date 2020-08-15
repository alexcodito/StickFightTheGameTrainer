using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StickFightTheGameTrainer.Trainer.PatcherMethods
{
    internal static class EnableNoPlayerDamage
    {
        internal static async Task<int> Execute(ModuleDefMD targetModule)
        {
            /*

                IL after the patch (at the top of the given method):

                    {IL_0049: call TrainerOptions Singleton`1<TrainerOptions>::get_Instance()}
                    {IL_004E: ldfld System.Boolean TrainerOptions::UnlimitedHealth}	
                    {IL_0053: brfalse.s IL_0063}
                    {IL_0055: call TrainerOptions Singleton`1<TrainerOptions>::get_Instance()}
                    {IL_005A: callvirt System.Boolean TrainerOptions::get_CheatsEnabled()}
                    {IL_005F: brfalse.s IL_0063}
                    {IL_0061: ldc.i4.0}	// - This instruction is only needed on the method that returns bool
                    {IL_0062: ret}	

             */

            var healthHandlerTypeDef = targetModule.Find("HealthHandler", true);

            if (healthHandlerTypeDef == null)
            {
                return await Task.FromResult(1);
            }

            // There are 2 method overloads that require patching
            var takeDamageMethodDefs = healthHandlerTypeDef.FindMethods("TakeDamage").ToArray();

            if (takeDamageMethodDefs.Length != 2)
            {
                return await Task.FromResult(2);
            }

            var targetTrainerOptionsTypeDef = targetModule.Find("TrainerOptions", true);

            if (targetTrainerOptionsTypeDef == null)
            {
                return await Task.FromResult(3);
            }

            var targetTrainerOptionsUnlimitedHealthField = targetTrainerOptionsTypeDef.FindField("UnlimitedHealth");

            if (targetTrainerOptionsUnlimitedHealthField == null)
            {
                return await Task.FromResult(4);
            }

            var targetTrainerOptionsCheatsEnabledMethodDef = targetTrainerOptionsTypeDef.FindMethod("get_CheatsEnabled");

            if (targetTrainerOptionsCheatsEnabledMethodDef?.Body?.Instructions == null || targetTrainerOptionsCheatsEnabledMethodDef.Body?.Instructions.Any() == false)
            {
                return await Task.FromResult(5);
            }

            var singletonTypeDef = targetModule.Find("TrainerManager", true);
            var checkCheatsEnabledMethodDef = singletonTypeDef.FindMethod("CheckCheatsEnabled");
            var trainerOptionsGetInstanceCallInstruction = checkCheatsEnabledMethodDef.Body.Instructions.FirstOrDefault();

            foreach (var takeDamageMethodDef in takeDamageMethodDefs)
            {
                if (takeDamageMethodDef.Body.Instructions[0].OpCode == OpCodes.Call)
                {
                    break;
                }

                // Where the branch should jump to continue execution if it is not satisfied
                // (i.e. the first instruction of the existing code)
                var branchOffsetInstruction = takeDamageMethodDef.Body.Instructions[0];

                // Construct the new instructions that are going to be injected
                var logicInstructions = new List<Instruction>
                {
                    trainerOptionsGetInstanceCallInstruction,
                    new Instruction(OpCodes.Ldfld, targetTrainerOptionsUnlimitedHealthField),
                    new Instruction(OpCodes.Brfalse_S, branchOffsetInstruction),
                    trainerOptionsGetInstanceCallInstruction,
                    new Instruction(OpCodes.Callvirt, targetTrainerOptionsCheatsEnabledMethodDef),
                    new Instruction(OpCodes.Brfalse_S, branchOffsetInstruction)
                };

                // Return false if the method is of type bool
                if (takeDamageMethodDef.ReturnType == targetModule.CorLibTypes.Boolean)
                {
                    logicInstructions.Add(new Instruction(OpCodes.Ldc_I4_0));
                }

                logicInstructions.Add(new Instruction(OpCodes.Ret));

                // Add new instructions to the top of the method
                for (var i = 0; i < logicInstructions.Count; i++)
                {
                    takeDamageMethodDef.Body.Instructions.Insert(i, logicInstructions[i]);
                }

                takeDamageMethodDef.Body.UpdateInstructionOffsets();
            }

            return await Task.FromResult(0);
        }
    }
}
