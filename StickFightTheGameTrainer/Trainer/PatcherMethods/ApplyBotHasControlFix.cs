using dnlib.DotNet;
using dnlib.DotNet.Emit;
using StickFightTheGameTrainer.Trainer.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StickFightTheGameTrainer.Trainer.PatcherMethods
{
    /// <summary>
    /// Enable bots to be considered as in control and to go for guns. 
    /// </summary>
    internal static class ApplyBotHasControlFix
    {
        internal static async Task<int> Execute(ModuleDefMD targetModule)
        {
            // Fetch assembly refs
            var unityEngine = targetModule.GetAssemblyRef("UnityEngine");

            // Fetch target type defs
            var characterActionsTypeDef = targetModule.Find("CharacterActions", true);
            var controllerTypeDef = targetModule.Find("Controller", true);
            var aiTypeDef = targetModule.Find("AI", true);

            // Fetch target method defs
            var aiStartMethodDef = aiTypeDef.FindMethod("Start");
            var controllerStartMethodDef = controllerTypeDef.FindMethod("Start");
            var characterActionsCreateWithControllerBindingsMethodDef = characterActionsTypeDef.FindMethod("CreateWithControllerBindings");

            // Fetch target field defs
            var aiGoForGunsFieldDef = aiTypeDef.FindField("goForGuns");
            var controllerHasControlFieldDef = controllerTypeDef.FindField("mHasControl");
            var controllerPlayerActionsFieldDef = controllerTypeDef.FindField("mPlayerActions");

            // Fetch type ref users
            var unityEngineComponentTypeRefUser = new TypeRefUser(targetModule, "UnityEngine", "Component", unityEngine);
            var unityEngineBehaviourTypeRefUser = new TypeRefUser(targetModule, "UnityEngine", "Behaviour", unityEngine);

            // Create method ref users
            var getComponentMethodRefUser = new MemberRefUser(targetModule, "GetComponent", MethodSig.CreateInstanceGeneric(1, new GenericMVar(0, aiStartMethodDef)), unityEngineComponentTypeRefUser);
            var setEnabledMethodRefUser = new MemberRefUser(targetModule, "set_enabled", MethodSig.CreateInstance(targetModule.CorLibTypes.Void, targetModule.CorLibTypes.Boolean), unityEngineBehaviourTypeRefUser);

            // Create method spec users
            var getComponentMethodSpecUser = new MethodSpecUser(getComponentMethodRefUser, new GenericInstMethodSig(aiTypeDef.ToTypeSig()));

            /*
             
                {IL_00C3: ldarg.0}
                {IL_00C4: call AI UnityEngine.Component::GetComponent<AI>()}
                {IL_00C9: ldc.i4.1}
                {IL_00CA: callvirt System.Void UnityEngine.Behaviour::set_enabled(System.Boolean)}

             */

            var controllerStartInstructionSignature = new List<Instruction> {
                new Instruction(OpCodes.Ldarg_0),
                new Instruction(OpCodes.Call, getComponentMethodSpecUser),
                new Instruction(OpCodes.Ldc_I4_1),
                new Instruction(OpCodes.Callvirt, setEnabledMethodRefUser)
            };

            var matchedControllerStartMethodInstructions = InjectionHelpers.FetchInstructionsBySigComparerSignature(controllerStartMethodDef.Body.Instructions, controllerStartInstructionSignature);

            if (matchedControllerStartMethodInstructions != null)
            {
                var lastMatchedInstruction = matchedControllerStartMethodInstructions.Last();
                var injectionIndex = controllerStartMethodDef.Body.Instructions.IndexOf(lastMatchedInstruction) + 1;

                var controllerStartInstructionsToInject = new List<Instruction>
                {
                    new Instruction(OpCodes.Ldarg_0),
                    new Instruction(OpCodes.Call, getComponentMethodSpecUser),
                    new Instruction(OpCodes.Ldc_I4_1),
                    new Instruction(OpCodes.Stfld, aiGoForGunsFieldDef),
                    new Instruction(OpCodes.Ldarg_0),
                    new Instruction(OpCodes.Ldc_I4_1),
                    new Instruction(OpCodes.Stfld, controllerHasControlFieldDef),
                    new Instruction(OpCodes.Ldarg_0),
                    new Instruction(OpCodes.Call, characterActionsCreateWithControllerBindingsMethodDef),
                    new Instruction(OpCodes.Stfld, controllerPlayerActionsFieldDef),
                };

                // Add new instructions after the matched signature
                for (var i = 0; i < controllerStartInstructionsToInject.Count; i++)
                {
                    controllerStartMethodDef.Body.Instructions.Insert(injectionIndex + i, controllerStartInstructionsToInject[i]);
                }

                controllerStartMethodDef.Body.UpdateInstructionOffsets();
            }
            else
            {
                return await Task.FromResult(1);
            }

            return await Task.FromResult(0);
        }
    }
}
