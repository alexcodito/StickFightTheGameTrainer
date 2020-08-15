using dnlib.DotNet;
using dnlib.DotNet.Emit;
using StickFightTheGameTrainer.Trainer.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StickFightTheGameTrainer.Trainer.PatcherMethods
{
    internal static class AddTrainerManagerToGameManager
    {
        internal static async Task<int> Execute(ModuleDefMD targetModule)
        {
            /*
                {IL_0046: ldarg.0}
                {IL_0047: ldarg.0}
                {IL_0048: call UnityEngine.GameObject UnityEngine.Component::get_gameObject()}
                {IL_004D: callvirt TrainerManager UnityEngine.GameObject::AddComponent<TrainerManager>()}
                {IL_0052: stfld TrainerManager GameManager::trainerManager}
                {IL_0057: ret}
             */

            var gameManagerTypeDef = targetModule.Find("GameManager", true);
            var trainerManagerTypeDef = targetModule.Find("TrainerManager", true);

            var trainerManagerFieldDef = InjectionHelpers.AddField(targetModule, "GameManager", "trainerManager", trainerManagerTypeDef.ToTypeSig(), FieldAttributes.Private);

            if (trainerManagerFieldDef == null)
            {
                return await Task.FromResult(1);
            }

            var gameManagerStartMethodDef = gameManagerTypeDef.FindMethod("Start");

            var unityEngine = targetModule.GetAssemblyRef(new UTF8String("UnityEngine"));
            var unityEngineComponentTypeRefUser = new TypeRefUser(targetModule, new UTF8String("UnityEngine"), new UTF8String("Component"), unityEngine);
            var unityEngineGameObjectTypeRefUser = new TypeRefUser(targetModule, new UTF8String("UnityEngine"), new UTF8String("GameObject"), unityEngine);

            var gameObjectTypeSig = unityEngineGameObjectTypeRefUser.ToTypeSig();

            var getGameObjectMethodSig = MethodSig.CreateInstance(gameObjectTypeSig);
            var gameManagerStartMethodSig = MethodSig.CreateInstanceGeneric(1, new GenericMVar(0, gameManagerStartMethodDef));

            // {UnityEngine.GameObject UnityEngine.Component::get_gameObject()}
            var getGameObjectMethodRefUser = new MemberRefUser(targetModule, new UTF8String("get_gameObject"), getGameObjectMethodSig, unityEngineComponentTypeRefUser);

            // {TrainerManager UnityEngine.GameObject::AddComponent<TrainerManager>()}
            var addComponentMethodRefUser = new MemberRefUser(targetModule, new UTF8String("AddComponent"), gameManagerStartMethodSig, unityEngineGameObjectTypeRefUser);

            var trainerManagerGenericInstMethodSig = new GenericInstMethodSig(trainerManagerTypeDef.ToTypeSig());
            var addComponentMethodSpecUser = new MethodSpecUser(addComponentMethodRefUser, trainerManagerGenericInstMethodSig);

            var trainerManagerDefinitionMethodInstructions = new List<Instruction>
            {
                new Instruction(OpCodes.Ldarg_0),
                new Instruction(OpCodes.Ldarg_0),
                new Instruction(OpCodes.Call, getGameObjectMethodRefUser),
                new Instruction(OpCodes.Callvirt, addComponentMethodSpecUser),
                new Instruction(OpCodes.Stfld, trainerManagerFieldDef),
                new Instruction(OpCodes.Ret),
            };

            var retInstruction = gameManagerStartMethodDef.Body.Instructions.LastOrDefault();
            if (retInstruction != null && retInstruction.OpCode == OpCodes.Ret)
            {
                gameManagerStartMethodDef.Body.Instructions.Remove(retInstruction);
            }

            foreach (var instruction in trainerManagerDefinitionMethodInstructions)
            {
                gameManagerStartMethodDef.Body.Instructions.Add(instruction);
            }

            return await Task.FromResult(0);
        }
    }
}
