using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StickFightTheGameTrainer.Trainer.PatcherMethods
{
    internal static class AddBotChatMessages
    {
        internal static async Task<int> Execute(ModuleDefMD targetModule)
        {
            // Fetch type defs
            var controllerTypeDef = targetModule.Find("Controller", true);
            var fightingTypeDef = targetModule.Find("Fighting", true);
            var networkPlayerTypeDef = targetModule.Find("NetworkPlayer", true);
            var chatManagerTypeDef = targetModule.Find("ChatManager", true);

            if(controllerTypeDef == null)
            {
                return await Task.FromResult(1);
            }
            
            if(fightingTypeDef == null)
            {
                return await Task.FromResult(2);
            }
            
            if(networkPlayerTypeDef == null)
            {
                return await Task.FromResult(3);
            }
            
            if(chatManagerTypeDef == null)
            {
                return await Task.FromResult(4);
            }

            // Fetch field defs
            var isAiFieldDef = controllerTypeDef.FindField("isAI");
            var fightingFieldDef = controllerTypeDef.FindField("fighting");            
            var mNetworkPlayerFieldDef = fightingTypeDef.FindField("mNetworkPlayer");
            var mChatManagerFieldDef = networkPlayerTypeDef.FindField("mChatManager");

            if (isAiFieldDef == null)
            {
                return await Task.FromResult(5);
            }

            if (fightingFieldDef == null)
            {
                return await Task.FromResult(6);
            }
            
            if (mNetworkPlayerFieldDef == null)
            {
                return await Task.FromResult(7);
            }

            if (mChatManagerFieldDef == null)
            {
                return await Task.FromResult(8);
            }

            // Fetch method defs
            var onTakeDamageMethodDef = controllerTypeDef.FindMethod("OnTakeDamage");
            var talkMethodDef = chatManagerTypeDef.FindMethod("Talk");

            if (onTakeDamageMethodDef == null)
            {
                return await Task.FromResult(9);
            }
            
            if (talkMethodDef == null)
            {
                return await Task.FromResult(10);
            }

            // Fetch reference assembly refs
            var unityEngineAssemblyRef = targetModule.GetAssemblyRef(new UTF8String("UnityEngine"));

            // Construct type ref users
            var unityEngineRandomTypeRefUser = new TypeRefUser(targetModule, new UTF8String("UnityEngine"), new UTF8String("Random"), unityEngineAssemblyRef);

            // Construct member ref users
            var randomRangeMethodRefUser = new MemberRefUser(targetModule, new UTF8String("Range"), MethodSig.CreateStatic(targetModule.CorLibTypes.Int32, targetModule.CorLibTypes.Int32, targetModule.CorLibTypes.Int32), unityEngineRandomTypeRefUser);

            /*
            
                // Instructions to inject

                0	0000	ldarg.0
                1	0001	ldfld	bool Controller::isAI
                2	0006	brfalse.s	40 (0085) ldarg.0 
                3	0008	ldarg.0
                4	0009	ldfld	class Fighting Controller::fighting
                5	000E	ldfld	class NetworkPlayer Fighting::mNetworkPlayer
                6	0013	brfalse.s	40 (0085) ldarg.0 
                7	0015	ldc.i4.0
                8	0016	ldc.i4	10
                9	001B	call	int32 [UnityEngine]UnityEngine.Random::Range(int32, int32)
                10	0020	stloc.0
                11	0021	ldloc.0
                12	0022	ldc.i4.s	1
                13	0024	bne.un.s	21 (0042) ldloc.0 
                14	0026	ldarg.0
                15	0027	ldfld	class Fighting Controller::fighting
                16	002C	ldfld	class NetworkPlayer Fighting::mNetworkPlayer
                17	0031	ldfld	class ChatManager NetworkPlayer::mChatManager
                18	0036	ldstr	"Ouch!"
                19	003B	callvirt	instance void ChatManager::Talk(string)
                20	0040	br.s	40 (0085) ldarg.0 
                21	0042	ldloc.0
                22	0043	ldc.i4	0x1D1
                23	0048	bne.un.s	31 (0066) ldloc.0 
                24	004A	ldarg.0
                25	004B	ldfld	class Fighting Controller::fighting
                26	0050	ldfld	class NetworkPlayer Fighting::mNetworkPlayer
                27	0055	ldfld	class ChatManager NetworkPlayer::mChatManager
                28	005A	ldstr	"Ow!"
                29	005F	callvirt	instance void ChatManager::Talk(string)
                30	0064	br.s	40 (0085) ldarg.0 
                31	0066	ldloc.0
                32	0067	ldc.i4.s	10
                33	0069	bne.un.s	40 (0085) ldarg.0 
                34	006B	ldarg.0
                35	006C	ldfld	class Fighting Controller::fighting
                36	0071	ldfld	class NetworkPlayer Fighting::mNetworkPlayer
                37	0076	ldfld	class ChatManager NetworkPlayer::mChatManager
                38	007B	ldstr	"That's monk-y business!"
                39	0080	callvirt	instance void ChatManager::Talk(string)

            */

            var firstInstruction = onTakeDamageMethodDef.Body.Instructions.First();

            var branches = new List<Instruction>()
            {
                new Instruction(OpCodes.Ldloc_0),
                new Instruction(OpCodes.Ldloc_0)
            };

            var condition1 = new List<Instruction>()
            {
                new Instruction(OpCodes.Ldloc_0),
                new Instruction(OpCodes.Ldc_I4_S, Convert.ToByte(10)),
                new Instruction(OpCodes.Bne_Un_S, branches[0]),
                new Instruction(OpCodes.Ldarg_0),
                new Instruction(OpCodes.Ldfld, fightingFieldDef),
                new Instruction(OpCodes.Ldfld, mNetworkPlayerFieldDef),
                new Instruction(OpCodes.Ldfld, mChatManagerFieldDef),
                new Instruction(OpCodes.Ldstr, "Ouch!"),
                new Instruction(OpCodes.Callvirt, talkMethodDef),
                new Instruction(OpCodes.Br_S, firstInstruction)
            };
            
            var condition2 = new List<Instruction>()
            {
                branches[0],
                new Instruction(OpCodes.Ldc_I4, 500),
                new Instruction(OpCodes.Bne_Un_S, branches[1]),
                new Instruction(OpCodes.Ldarg_0),
                new Instruction(OpCodes.Ldfld, fightingFieldDef),
                new Instruction(OpCodes.Ldfld, mNetworkPlayerFieldDef),
                new Instruction(OpCodes.Ldfld, mChatManagerFieldDef),
                new Instruction(OpCodes.Ldstr, "Existence is pain!"),
                new Instruction(OpCodes.Callvirt, talkMethodDef),
                new Instruction(OpCodes.Br_S, firstInstruction)
            };

            var condition3 = new List<Instruction>()
            {
                branches[1],
                new Instruction(OpCodes.Ldc_I4_S, Convert.ToByte(100)),
                new Instruction(OpCodes.Bne_Un_S, firstInstruction),
                new Instruction(OpCodes.Ldarg_0),
                new Instruction(OpCodes.Ldfld, fightingFieldDef),
                new Instruction(OpCodes.Ldfld, mNetworkPlayerFieldDef),
                new Instruction(OpCodes.Ldfld, mChatManagerFieldDef),
                new Instruction(OpCodes.Ldstr, "That's monk-y business!"),
                new Instruction(OpCodes.Callvirt, talkMethodDef)
            };

            var instructionsToInject = new List<Instruction>()
            {
                new Instruction(OpCodes.Ldarg_0),
                new Instruction(OpCodes.Ldfld, isAiFieldDef),
                new Instruction(OpCodes.Brfalse_S, firstInstruction),
                new Instruction(OpCodes.Ldarg_0),
                new Instruction(OpCodes.Ldfld, fightingFieldDef),
                new Instruction(OpCodes.Ldfld, mNetworkPlayerFieldDef),
                new Instruction(OpCodes.Brfalse_S, firstInstruction),
                new Instruction(OpCodes.Ldc_I4_0),
                new Instruction(OpCodes.Ldc_I4, 800),
                new Instruction(OpCodes.Call, randomRangeMethodRefUser),
                new Instruction(OpCodes.Stloc_0)
            }
            .Concat(condition1)
            .Concat(condition2)
            .Concat(condition3)
            .ToList();

            for(var i = 0; i < instructionsToInject.Count; i++)
            {
                onTakeDamageMethodDef.Body.Instructions.Insert(i, instructionsToInject[i]);
            }

            onTakeDamageMethodDef.Body.Variables.Add(new Local(targetModule.CorLibTypes.Int32, "num"));

            onTakeDamageMethodDef.Body.OptimizeBranches();
            onTakeDamageMethodDef.Body.UpdateInstructionOffsets();

            return await Task.FromResult(0);
        }
    }
}
