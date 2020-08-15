using dnlib.DotNet;
using dnlib.DotNet.Emit;
using StickFightTheGameTrainer.Trainer.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StickFightTheGameTrainer.Trainer.PatcherMethods
{
    /// <summary>
    /// Fix "infinite jumping" glitch for bots.
    /// </summary>
    internal static class ApplyBotJumpFix
    {
        internal static async Task<int> Execute(ModuleDefMD targetModule)
        {
            // Fetch target type defs
            var controllerTypeDef = targetModule.Find("Controller", true);

            // Fetch target method defs
            var controllerJumpMethodDef = controllerTypeDef.FindMethod("Jump");

            // Fetch target field defs
            var controllerMovementStateFieldDef = controllerTypeDef.FindField("m_MovementState");

            /*

                Remove redundant code that always sets 'movementState' to 'GroundJump' in the 'Controller.Jump' method.

                Target instructions to patch:

                    62	00EE	ldarg.0
                    63	00EF	ldloc.0
                    64	00F0	brfalse	67 (00FB) ldc.i4.8 
                    65	00F5	ldc.i4.4
                    66	00F6	br	68 (00FC) stfld valuetype Controller/MovementStateEnum Controller::m_MovementState
                    67	00FB	ldc.i4.8
                    68	00FC	stfld	valuetype Controller/MovementStateEnum Controller::m_MovementState

                */

            var controllerJumpInstructionSignature = new List<Instruction> {
                new Instruction(OpCodes.Ldarg_0),
                new Instruction(OpCodes.Ldloc_0),
                new Instruction(OpCodes.Brfalse, new Instruction(OpCodes.Ldc_I4_8, null)),
                new Instruction(OpCodes.Ldc_I4_4),
                new Instruction(OpCodes.Br, new Instruction(OpCodes.Stfld, null)),
                new Instruction(OpCodes.Ldc_I4_8),
                new Instruction(OpCodes.Stfld, controllerMovementStateFieldDef)
            };

            var matchedControllerJumpMethodInstructions = InjectionHelpers.FetchInstructionsBySignature(controllerJumpMethodDef.Body.Instructions, controllerJumpInstructionSignature, false);

            if (matchedControllerJumpMethodInstructions != null)
            {
                // NOP the matched instructions
                matchedControllerJumpMethodInstructions.ForEach(matchedInstruction => matchedInstruction.OpCode = OpCodes.Nop);
            }
            else
            {
                return await Task.FromResult(1);
            }

            /*

                The 'Movement.Jump' method unsafely accesses the AudioClip array when selecting a random 'jump' sound to play.
                Add appropriate length check to fix the index out of bounds exception.      

                Target instructions to patch:

                    0	0000	ldarg.0
                    1	0001	ldarg.1
                    2	0002	ldarg.2
                    3	0003	call	instance bool Movement::DoJump(bool, bool)
                    4	0008	stloc.0
                    5	0009	ldarg.0
                    6	000A	ldfld	class [UnityEngine]UnityEngine.AudioSource Movement::au
                    7	000F	ldarg.0
                    8	0010	ldfld	class [UnityEngine]UnityEngine.AudioClip[] Movement::jumpClips
                    9	0015	ldc.i4.0
                    10	0016	ldarg.0
                    11	0017	ldfld	class [UnityEngine]UnityEngine.AudioClip[] Movement::jumpClips
                    12	001C	ldlen
                    13	001D	conv.i4
                    14	001E	call	int32 [UnityEngine]UnityEngine.Random::Range(int32, int32)
                    15	0023	ldelem.ref
                    16	0024	callvirt	instance void [UnityEngine]UnityEngine.AudioSource::PlayOneShot(class [UnityEngine]UnityEngine.AudioClip)
                    17	0029	ldloc.0
                    18	002A	ret


                Resulting instructions after patching:

                    0	0000	ldarg.0
                    1	0001	ldarg.1
                    2	0002	ldarg.2
                    3	0003	call	instance bool Movement::DoJump(bool, bool)
                    4	0008	ldarg.0
                    5	0009	ldfld	class [UnityEngine]UnityEngine.AudioClip[] Movement::jumpClips
                    6	000E	ldlen
                    7	000F	brfalse.s	24 (0039) ret 
                    8	0011	ldarg.0
                    9	0012	ldfld	class [UnityEngine]UnityEngine.AudioSource Movement::au
                    10	0017	ldarg.0
                    11	0018	ldfld	class [UnityEngine]UnityEngine.AudioClip[] Movement::jumpClips
                    12	001D	ldc.i4.0
                    13	001E	ldarg.0
                    14	001F	ldfld	class [UnityEngine]UnityEngine.AudioClip[] Movement::jumpClips
                    15	0024	ldlen
                    16	0025	conv.i4
                    17	0026	ldc.i4.1
                    18	0027	sub
                    19	0028	ldc.i4.0
                    20	0029	call	int32 [mscorlib]System.Math::Max(int32, int32)
                    21	002E	call	int32 [UnityEngine]UnityEngine.Random::Range(int32, int32)
                    22	0033	ldelem.ref
                    23	0034	callvirt	instance void [UnityEngine]UnityEngine.AudioSource::PlayOneShot(class [UnityEngine]UnityEngine.AudioClip)
                    24	0039	ret

                */

            // Fetch target type defs
            var movementTypeDef = targetModule.Find("Movement", true);

            // Fetch target method defs
            var movementJumpMethodDef = movementTypeDef.FindMethod("Jump");
            var movementDoJumpMethodDef = movementTypeDef.FindMethod("DoJump");

            // Fetch target field defs
            var movementAuFieldDef = movementTypeDef.FindField("au");
            var movementJumpClipsFieldDef = movementTypeDef.FindField("jumpClips");

            // Fetch reference assembly refs
            var mscorlibAssemblyRef = targetModule.GetAssemblyRef(new UTF8String("mscorlib"));
            var unityEngineAssemblyRef = targetModule.GetAssemblyRef(new UTF8String("UnityEngine"));

            // Construct type ref users
            var systemMathTypeRefUser = new TypeRefUser(targetModule, new UTF8String("System"), new UTF8String("Math"), mscorlibAssemblyRef);
            var unityEngineAudioClipTypeRefUser = new TypeRefUser(targetModule, new UTF8String("UnityEngine"), new UTF8String("AudioClip"), unityEngineAssemblyRef);
            var unityEngineRandomTypeRefUser = new TypeRefUser(targetModule, new UTF8String("UnityEngine"), new UTF8String("Random"), unityEngineAssemblyRef);
            var unityEngineAudioSourceTypeRefUser = new TypeRefUser(targetModule, new UTF8String("UnityEngine"), new UTF8String("AudioSource"), unityEngineAssemblyRef);

            // Construct member ref users
            var maxMethodRefUser = new MemberRefUser(targetModule, new UTF8String("Max"), MethodSig.CreateStatic(targetModule.CorLibTypes.Int32, targetModule.CorLibTypes.Int32, targetModule.CorLibTypes.Int32), systemMathTypeRefUser);
            var randomRangeMethodRefUser = new MemberRefUser(targetModule, new UTF8String("Range"), MethodSig.CreateStatic(targetModule.CorLibTypes.Int32, targetModule.CorLibTypes.Int32, targetModule.CorLibTypes.Int32), unityEngineRandomTypeRefUser);
            var playOneShotMethodRefUser = new MemberRefUser(targetModule, new UTF8String("PlayOneShot"), MethodSig.CreateInstance(targetModule.CorLibTypes.Void, unityEngineAudioClipTypeRefUser.ToTypeSig()), unityEngineAudioSourceTypeRefUser);

            // Construct list of instructions to be injected
            var retInstruction = new Instruction(OpCodes.Ret);

            var movementJumpInstructions = new List<Instruction>()
            {
                new Instruction(OpCodes.Ldarg_0),
                new Instruction(OpCodes.Ldarg_1),
                new Instruction(OpCodes.Ldarg_2),
                new Instruction(OpCodes.Call, movementDoJumpMethodDef),
                new Instruction(OpCodes.Ldarg_0),
                new Instruction(OpCodes.Ldfld, movementJumpClipsFieldDef),
                new Instruction(OpCodes.Ldlen),
                new Instruction(OpCodes.Brfalse_S, retInstruction),
                new Instruction(OpCodes.Ldarg_0),
                new Instruction(OpCodes.Ldfld, movementAuFieldDef),
                new Instruction(OpCodes.Ldarg_0),
                new Instruction(OpCodes.Ldfld, movementJumpClipsFieldDef),
                new Instruction(OpCodes.Ldc_I4_0),
                new Instruction(OpCodes.Ldarg_0),
                new Instruction(OpCodes.Ldfld, movementJumpClipsFieldDef),
                new Instruction(OpCodes.Ldlen),
                new Instruction(OpCodes.Conv_I4),
                new Instruction(OpCodes.Ldc_I4_1),
                new Instruction(OpCodes.Sub),
                new Instruction(OpCodes.Ldc_I4_0),
                new Instruction(OpCodes.Call, maxMethodRefUser),
                new Instruction(OpCodes.Call, randomRangeMethodRefUser),
                new Instruction(OpCodes.Ldelem_Ref),
                new Instruction(OpCodes.Callvirt, playOneShotMethodRefUser),
                retInstruction
            };

            // Replace all instructions in the method with the new instructions
            movementJumpMethodDef.Body.Instructions.Clear();
            movementJumpInstructions.ForEach(movementJumpInstruction => movementJumpMethodDef.Body.Instructions.Add(movementJumpInstruction));
            movementJumpMethodDef.Body.UpdateInstructionOffsets();

            return await Task.FromResult(0);
        }
    }
}
