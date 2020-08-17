using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StickFightTheGameTrainer.Trainer.PatcherMethods
{
    /// <summary>
    /// Replace the game's version constant with a method that determines the appropriate version for Public and Modder/Local lobies to prevent online cheating.
    /// </summary>
    internal static class AddTrainerVersionConstant
    {
        internal static async Task<int> Execute(ModuleDefMD targetModule)
        {
            /*

                Target's original IL:
                    {IL_0000: ldstr "X.X"}
                    {IL_0005: ret}

                Patched IL:
                {IL_0000: call TrainerOptions Singleton`1<TrainerOptions>::get_Instance()}	
                {IL_0005: ldstr "X.X"}
                {IL_000A: callvirt System.String TrainerOptions::GetVersion(System.String)}	
                {IL_000F: ret}	

            */

            // Load Type Defs
            var targetStickFightConstantsTypeDef = targetModule.Find("StickFightConstants", true);

            if (targetStickFightConstantsTypeDef == null)
            {
                return await Task.FromResult(1);
            }

            var targetTrainerOptionsTypeDef = targetModule.Find("TrainerOptions", true);

            if (targetTrainerOptionsTypeDef == null)
            {
                return await Task.FromResult(2);
            }

            // Fetch target MethodDef
            var targetGetVersionValueMethodDef = targetStickFightConstantsTypeDef.FindMethod("get_VERSION_VALUE");

            if (targetGetVersionValueMethodDef?.Body?.Instructions == null || targetGetVersionValueMethodDef.Body?.Instructions.Any() == false)
            {
                return await Task.FromResult(3);
            }

            var targetTrainerOptionsGetVersionMethodDef = targetTrainerOptionsTypeDef.FindMethod("GetVersion");

            if (targetTrainerOptionsGetVersionMethodDef?.Body?.Instructions == null || targetTrainerOptionsGetVersionMethodDef.Body?.Instructions.Any() == false)
            {
                return await Task.FromResult(4);
            }

            // Check if the IL has already been replaced before
            if (targetGetVersionValueMethodDef.Body.Instructions.Any(instr => instr.OpCode == OpCodes.Call))
            {
                return await Task.FromResult(5);
            }

            // Fetch the Ldstr instruction containing the target's current version
            var targetCurrentVersionInstruction = targetGetVersionValueMethodDef.Body.Instructions.FirstOrDefault(a => a.OpCode == OpCodes.Ldstr);

            if (targetCurrentVersionInstruction?.Operand == null)
            {
                return await Task.FromResult(6);
            }

            // Fetch the current version value
            var targetCurrentVersionValue = targetCurrentVersionInstruction.Operand as string;

            if (string.IsNullOrEmpty(targetCurrentVersionValue))
            {
                return await Task.FromResult(7);
            }

            var singletonTypeDef = targetModule.Find("TrainerManager", true);
            var checkCheatsEnabledMethodDef = singletonTypeDef.FindMethod("CheckCheatsEnabled");
            var targetSingletonTrainerOptionsInstantiation = checkCheatsEnabledMethodDef.Body.Instructions.FirstOrDefault();

            // Construct the new instructions that are going to be injected
            var logicInstructions = new List<Instruction>
            {
                targetSingletonTrainerOptionsInstantiation,
                new Instruction(OpCodes.Ldstr, targetCurrentVersionValue),
                new Instruction(OpCodes.Callvirt, targetTrainerOptionsGetVersionMethodDef),
                new Instruction(OpCodes.Ret)
            };

            // Replace old instructions with the new ones
            targetGetVersionValueMethodDef.Body.Instructions.Clear();
            foreach (var instruction in logicInstructions)
            {
                targetGetVersionValueMethodDef.Body.Instructions.Add(instruction);
            }

            return await Task.FromResult(0);
        }
    }
}
