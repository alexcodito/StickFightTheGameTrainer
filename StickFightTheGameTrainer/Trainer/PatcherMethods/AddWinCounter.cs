using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StickFightTheGameTrainer.Trainer.PatcherMethods
{
    /// <summary>         
    /// Inject win counter to GameManager's AllButOnePlayersDied method.
    /// </summary>
    internal static class AddWinCounter
    {
        internal static async Task<int> Execute(ModuleDefMD targetModule)
        {
            /* 

                 Insert the following IL code into GameManager.AllButOnePlayersDied():

                     {IL_00FE: call TrainerOptions Singleton`1<TrainerOptions>::get_Instance()}
                     {IL_0103: ldfld System.Boolean TrainerOptions::NoWinners} 
                     {IL_0108: brtrue.s IL_012E}
                     {IL_010A: ldarg.0}
                     {IL_010B: ldfld System.Collections.Generic.List`1<Controller> GameManager::playersAlive}
                     {IL_0110: ldc.i4.0}
                     {IL_0111: callvirt Controller System.Collections.Generic.List`1<Controller>::get_Item(System.Int32)}
                     {IL_0116: ldfld Fighting Controller::fighting}
                     {IL_011B: ldfld CharacterStats Fighting::stats}
                     {IL_0120: dup}
                     {IL_0121: ldfld System.Int32 CharacterStats::wins}
                     {IL_0126: ldc.i4.1}
                     {IL_0127: add}
                     {IL_0128: stfld System.Int32 CharacterStats::wins}
                     {IL_012D: br.s ldarg.0}
                     {IL_012E: call TrainerOptions Singleton`1<TrainerOptions>::get_Instance()}
                     {IL_0133: ldc.i4.0}
                     {IL_0134: stfld System.Boolean TrainerOptions::NoWinners}

              */

            var gameManagerTypeDef = targetModule.Find("GameManager", true);
            var targetTrainerOptionsTypeDef = targetModule.Find("TrainerOptions", true);
            var controllerTypeDef = targetModule.Find("Controller", true);
            var fightingTypeDef = targetModule.Find("Fighting", true);
            var characterStatsTypeDef = targetModule.Find("CharacterStats", true);

            if (targetTrainerOptionsTypeDef == null)
            {
                return await Task.FromResult(1);
            }

            var gameManagerAllButOnePlayersDiedMethodDef = gameManagerTypeDef.FindMethod("AllButOnePlayersDied");

            if (gameManagerAllButOnePlayersDiedMethodDef == null)
            {
                return await Task.FromResult(2);
            }

            var singletonTypeDef = targetModule.Find("TrainerManager", true);
            var checkCheatsEnabledMethodDef = singletonTypeDef.FindMethod("CheckCheatsEnabled");
            var targetSingletonTrainerOptionsInstantiationInstruction = checkCheatsEnabledMethodDef.Body.Instructions.FirstOrDefault();

            var targetAssemblyRef = targetModule.CorLibTypes.AssemblyRef;

            var gameManagerPlayersAliveFieldDef = gameManagerTypeDef.FindField("playersAlive");
            var trainerOptionsNoWinnersFieldDef = targetTrainerOptionsTypeDef.FindField("NoWinners");

            var genericListTypeRef = new TypeRefUser(targetModule, @"System.Collections.Generic", "List`1", targetAssemblyRef);
            var genericListControllerGenericInstSig = new GenericInstSig(new ClassSig(genericListTypeRef), controllerTypeDef.ToTypeSig());

            // Create TypeSpec from GenericInstSig
            var genericListControllerTypeSpec = new TypeSpecUser(genericListControllerGenericInstSig);

            var genericListControllerGetItemMemberRefUser = new MemberRefUser(targetModule, "get_Item", MethodSig.CreateInstance(new GenericVar(0), targetModule.CorLibTypes.Int32), genericListControllerTypeSpec);

            var fightingFieldDef = controllerTypeDef.FindField("fighting");
            var characterStatsFieldDef = fightingTypeDef.FindField("stats");
            var winsFieldDef = characterStatsTypeDef.FindField("wins");

            var firstExistingInstruction = gameManagerAllButOnePlayersDiedMethodDef.Body.Instructions.FirstOrDefault();

            // Construct the new instructions that are going to be injected
            var logicInstructions = new List<Instruction>
            {
                targetSingletonTrainerOptionsInstantiationInstruction,
                new Instruction(OpCodes.Ldfld, trainerOptionsNoWinnersFieldDef),
                new Instruction(OpCodes.Brtrue_S, targetSingletonTrainerOptionsInstantiationInstruction),
                new Instruction(OpCodes.Ldarg_0),
                new Instruction(OpCodes.Ldfld, gameManagerPlayersAliveFieldDef),
                new Instruction(OpCodes.Ldc_I4_0),
                new Instruction(OpCodes.Callvirt, genericListControllerGetItemMemberRefUser),
                new Instruction(OpCodes.Ldfld, fightingFieldDef),
                new Instruction(OpCodes.Ldfld, characterStatsFieldDef),
                new Instruction(OpCodes.Dup),
                new Instruction(OpCodes.Ldfld, winsFieldDef),
                new Instruction(OpCodes.Ldc_I4_1),
                new Instruction(OpCodes.Add),
                new Instruction(OpCodes.Stfld, winsFieldDef),
                new Instruction(OpCodes.Br_S, firstExistingInstruction),
                targetSingletonTrainerOptionsInstantiationInstruction,
                new Instruction(OpCodes.Ldc_I4_0),
                new Instruction(OpCodes.Stfld, trainerOptionsNoWinnersFieldDef)
            };

            // Add new instructions to the top of the method
            for (var i = 0; i < logicInstructions.Count; i++)
            {
                gameManagerAllButOnePlayersDiedMethodDef.Body.Instructions.Insert(i, logicInstructions[i]);
            }

            gameManagerAllButOnePlayersDiedMethodDef.Body.UpdateInstructionOffsets();

            return await Task.FromResult(0);
        }
    }
}
