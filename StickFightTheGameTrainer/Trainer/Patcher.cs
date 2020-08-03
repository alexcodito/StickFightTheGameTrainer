using dnlib.DotNet;
using dnlib.DotNet.Emit;
using StickFightTheGameTrainer.Common;
using StickFightTheGameTrainer.Trainer.Helpers;
using StickFightTheGameTrainer.Trainer.TrainerLogic;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StickFightTheGameTrainer.Trainer
{
    public sealed class Patcher
    {
        private ModuleDefMD _logicModule;
        private ModuleDefMD _targetModule;
        private string _targetModulePath;
        private readonly InjectionHelpers _injectionHelpers;
        private readonly Common.ILogger _logger;
        private readonly TrainerLogicModuleBuilder _trainerLogicModuleBuilder;

        // Private game fields that are accessed by TrainerManager.
        private readonly List<KeyValuePair<string, string>> _targetFields = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("AI", "aimer"),
            new KeyValuePair<string, string>("AI", "counter"),
            new KeyValuePair<string, string>("AI", "controller"),
            new KeyValuePair<string, string>("AI", "controllerHandler"),
            new KeyValuePair<string, string>("AI", "fighting"),
            new KeyValuePair<string, string>("AI", "head"),
            new KeyValuePair<string, string>("AI", "info"),
            new KeyValuePair<string, string>("AI", "reactionCounter"),
            new KeyValuePair<string, string>("AI", "targetInformation"),
            new KeyValuePair<string, string>("AI", "velocity"),
            new KeyValuePair<string, string>("MultiplayerManager", "mGameManager"),
            new KeyValuePair<string, string>("GameManager", "controllerHandler"),
            new KeyValuePair<string, string>("GameManager", "m_WeaponSelectionHandler"),
            new KeyValuePair<string, string>("GameManager", "mNetworkManager"),
            new KeyValuePair<string, string>("GameManager", "mSpawnedWeapons"),
            new KeyValuePair<string, string>("GameManager", "levelSelector"),
            new KeyValuePair<string, string>("GameManager", "hoardHandler"),
            new KeyValuePair<string, string>("Controller", "fighting"),
            new KeyValuePair<string, string>("Controller", "mPlayerActions"),
            new KeyValuePair<string, string>("Fighting", "stats"),
            new KeyValuePair<string, string>("Fighting", "mNetworkPlayer"),
            new KeyValuePair<string, string>("Fighting", "weapons"),
            new KeyValuePair<string, string>("Fighting", "bulletsLeft"),
            new KeyValuePair<string, string>("Weapon", "reloads"),
            new KeyValuePair<string, string>("NetworkPlayer", "mChatManager"),
            new KeyValuePair<string, string>("InControl.PlayerActionSet", "activeDevice"),
            new KeyValuePair<string, string>("CharacterActions", "mInputType")
        };

        // Private game methods that are accessed by TrainerManager.
        private readonly List<KeyValuePair<string, string>> _targetMethods = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("GameManager", "AllButOnePlayersDied"),
            new KeyValuePair<string, string>("HoardHandler", "SpawnAI"),
            new KeyValuePair<string, string>("Controller", "Start"),
            new KeyValuePair<string, string>("MultiplayerManager", "UnReadyAllPlayers"),
            new KeyValuePair<string, string>("MultiplayerManager", "SendMessageToAllClients"),
        };

        public Patcher(Common.ILogger logger, InjectionHelpers injectionHelpers, TrainerLogicModuleBuilder trainerLogicModuleBuilder)
        {
            _logger = logger;
            _injectionHelpers = injectionHelpers;
            _trainerLogicModuleBuilder = trainerLogicModuleBuilder;
        }

        public async Task<bool> LoadTargetModule(string targetModulePath)
        {
            if (!File.Exists(targetModulePath))
            {
                await _logger.Log("Could not locate target module", LogLevel.Error);
                return await Task.FromResult(false);
            }

            _targetModule = ModuleDefMD.Load(targetModulePath);

            if (_targetModule == null)
            {
                await _logger.Log("Could not load target module", LogLevel.Error);
                return await Task.FromResult(false);
            }

            _targetModulePath = _targetModule.Location;

            return await Task.FromResult(true);
        }

        private void ReloadLogicModule(bool reload = true)
        {
            var location = _logicModule.Location;
            _logicModule.Dispose();
            _logicModule = ModuleDefMD.Load(location);
        }

        private void DeleteTrainerLogicModule()
        {
            var location = _logicModule.Location;

            _logicModule.Dispose();
            File.Delete(location);
        }

        private void SaveAndReload(bool reload = true)
        {
            _injectionHelpers.Save(_targetModule, true);

            if (reload)
            {
                _targetModule = ModuleDefMD.Load(_targetModulePath);
            }
        }

        public async Task EncryptLogicModuleSource()
        {
            await _trainerLogicModuleBuilder.EncryptLogicModuleSource();
        }

        public async Task<bool> CheckTrainerAlreadyPatched(ModuleDefMD module = null)
        {
            if (module == null)
            {
                module = _targetModule;
            }

            // Check if trainer code already exists in the target module.
            if (module.Find("TrainerManager", true) != null)
            {
                return await Task.FromResult(true);
            }

            return await Task.FromResult(false);
        }

        private async Task CreateBackup()
        {
            if (_targetModulePath != null)
            {
                if (await CheckTrainerAlreadyPatched())
                {
                    return;
                }

                var targetGameVersion = await GetGameVersion();

                _targetModule.Dispose();

                File.Copy(_targetModulePath, _targetModulePath.Replace(".dll", $"_{targetGameVersion}_{DateTime.Now.ToString("s").Replace(":", "")}_backup.dll"));

                _targetModule = ModuleDefMD.Load(_targetModulePath);
            }
        }

        public async Task<bool> CheckBackupExists(string path = null)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                if (!string.IsNullOrWhiteSpace(_targetModule?.Location))
                {
                    path = _targetModule?.Location;
                }
                else
                {
                    return await Task.FromResult(false);
                }
            }

            var directory = Path.GetDirectoryName(path);

            if (directory == null)
            {
                return await Task.FromResult(false);
            }

            var matches = Directory.GetFiles(directory, "Assembly-CSharp_*_backup.dll");

            return await Task.FromResult(matches.Any());
        }

        public async Task<bool> RestoreLatestBackup(string path = null)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                if (!string.IsNullOrWhiteSpace(_targetModule?.Location))
                {
                    path = _targetModule?.Location;
                }
                else
                {
                    return await Task.FromResult(false);
                }
            }

            var directory = Path.GetDirectoryName(path);

            if (directory == null)
            {
                return await Task.FromResult(false);
            }

            var targetGameVersion = $"{await GetGameVersion()}";

            var matches = Directory.GetFiles(directory, $"Assembly-CSharp_{targetGameVersion}*_backup.dll");

            if (!matches.Any())
            {
                await _logger.Log("Could not locate any backups.");
                return await Task.FromResult(false);
            }

            var firstMatchPath = matches.OrderByDescending(s => s).First();
            var destinationPath = Path.GetDirectoryName(firstMatchPath);

            if (destinationPath == null)
            {
                await _logger.Log("The destination path for backup restoration is invalid.");
                return await Task.FromResult(false);
            }

            await _logger.Log($"Restoring latest backup: {Path.GetFileName(firstMatchPath)}");

            _targetModule?.Dispose();

            File.Copy(firstMatchPath, Path.Combine(destinationPath, "Assembly-CSharp.dll"), true);

            if (!string.IsNullOrWhiteSpace(_targetModulePath))
            {
                _targetModule = ModuleDefMD.Load(_targetModulePath);
            }

            return await Task.FromResult(true);
        }

        public async Task<bool> PatchTargetModule(bool takeBackup)
        {
            if (_targetModule == null)
            {
                await _logger.Log("Target module is not loaded. Patching aborted.");
                return await Task.FromResult(false);
            }

            if (takeBackup)
            {
                await CreateBackup();
            }

            await _logger.Log("Initiating patching");

            var targetGameVersion = $"Game version: {await GetGameVersion()}";
            await _logger.Log(targetGameVersion);

            var patcherVersion = $"Patcher version: {Application.ProductVersion}";
            await _logger.Log(patcherVersion);

            // Set target fields and methods as public in order to allow TrainerLogic.dll's code to reference them and compile.
            // Note: While it would have been convenient to simply set *every* field and method as public/internal, it causes in-game glitches (e.g. ambiguous references and null reference exceptions)
            await _logger.Log("Setting fields as public");
            _targetFields.ForEach(async targetField => await _injectionHelpers.SetFieldAccessModifier(_targetModule, targetField.Key, targetField.Value, FieldAttributes.Public));

            await _logger.Log("Setting methods as public");
            _targetMethods.ForEach(async targetMethod => await _injectionHelpers.SetMethodAccessModifier(_targetModule, targetMethod.Key, targetMethod.Value, MethodAttributes.Public));

            // Add a public int field to keep track of the current weapon selection index
            await _logger.Log("Adding TrainerWeaponIndex field to fighting class");
            await _injectionHelpers.AddField(_targetModule, "Fighting", "TrainerWeaponIndex", _targetModule.CorLibTypes.Int32, FieldAttributes.Public);

            // This has to be done every time in order to be able to reference changes that are made within other parts 
            // of the code. Need to find a better way.
            SaveAndReload();

            // Compile TrainerLogic.dll 
            await _logger.Log("Compiling Trainer Logic module");
            if (!await CompileTrainerLogicModule())
            {
                await _logger.Log("Could not compile Trainer Logic module", LogLevel.Error);
                return await Task.FromResult(false);
            }

            SaveAndReload();

            await _logger.Log("Setting fields and methods as internal");

            // Keeping the fields as public causes very strange in-game glitches, so they need to be 
            // made internal following the DLL's successful compilation.
            _targetFields.ForEach(async targetField => await _injectionHelpers.SetFieldAccessModifier(_targetModule, targetField.Key, targetField.Value, FieldAttributes.Assembly));
            _targetMethods.ForEach(async targetMethod => await _injectionHelpers.SetMethodAccessModifier(_targetModule, targetMethod.Key, targetMethod.Value, MethodAttributes.Assembly));

            ReloadLogicModule();
            SaveAndReload();

            await _logger.Log("Injecting trainer logic modules");

            // Inject TrainerOptions class
            if (!InjectTrainerLogicModuleTypes())
            {
                await _logger.Log("Could not inject trainer logic modules", LogLevel.Error);
            }

            SaveAndReload();

            if (!InjectCustomAILogic())
            {
                await _logger.Log("Could not inject AI logic module", LogLevel.Error);
            }

            SaveAndReload();

            await _logger.Log("Adding TrainerManager to GameManager");

            // Inject declaration of TrainerManager class into GameManager class and inject its
            // instantiation code into GameManager's Start() method
            await AddTrainerManagerToGameManager();

            SaveAndReload();

            await _logger.Log("Clearing ChatManager Awake method");

            // Enable chat bubble in all lobies (used for weapon selection - displays name of currently held weapon)
            await _injectionHelpers.ClearMethodBody(_targetModule, "ChatManager", "Awake");

            await _logger.Log("Enabling flying mode weapon pickup");

            // Enable weapon pickup in flying mode
            await EnableFlyingModeWeaponPickup();

            await _logger.Log("Enabling flying mode weapon throw");

            // Enable throwing weapon while flying
            await EnableFlyingModeWeaponThrow();

            await _logger.Log("Enabling unlimited health");

            // Enable no player damage
            await EnableNoPlayerDamage();

            await _logger.Log("Replacing game version constant");

            // Replace the game's version constant with a method that determines 
            // the appropriate version for Online and Local lobies to prevent online cheating.
            await ReplaceVersionConstant();

            await _logger.Log("Adding win counter");

            // Inject win counter to GameManager's AllButOnePlayersDied method
            await AddWinCounter();

            await _logger.Log("Applying bot jump fix");

            await ApplyBotJumpFix();

            await _logger.Log("Applying bot death fix");

            await ApplyBotDeathFix();
            
            await _logger.Log("Applying bot collision fix");

            await ApplyBotCollisionFix();
            
            await _logger.Log("Applying bot HasControl fix");

            await ApplyBotHasControlFix();

            SaveAndReload(false);

            DeleteTrainerLogicModule();

            await _logger.Log("Deleting trainer logic module");

            await _logger.Log("Patching completed");

            return await Task.FromResult(true);
        }

        private async Task AddTrainerManagerToGameManager()
        {
            /*
                {IL_0046: ldarg.0}
                {IL_0047: ldarg.0}
                {IL_0048: call UnityEngine.GameObject UnityEngine.Component::get_gameObject()}
                {IL_004D: callvirt TrainerManager UnityEngine.GameObject::AddComponent<TrainerManager>()}
                {IL_0052: stfld TrainerManager GameManager::trainerManager}
                {IL_0057: ret}
             */

            var gameManagerTypeDef = _targetModule.Find("GameManager", true);
            var trainerManagerTypeDef = _targetModule.Find("TrainerManager", true);

            var trainerManagerFieldDef = await _injectionHelpers.AddField(_targetModule, "GameManager", "trainerManager", trainerManagerTypeDef.ToTypeSig(), FieldAttributes.Private);

            var gameManagerStartMethodDef = gameManagerTypeDef.FindMethod("Start");

            var unityEngine = _targetModule.GetAssemblyRef(new UTF8String("UnityEngine"));
            var unityEngineComponentTypeRefUser = new TypeRefUser(_targetModule, new UTF8String("UnityEngine"), new UTF8String("Component"), unityEngine);
            var unityEngineGameObjectTypeRefUser = new TypeRefUser(_targetModule, new UTF8String("UnityEngine"), new UTF8String("GameObject"), unityEngine);

            var gameObjectTypeSig = unityEngineGameObjectTypeRefUser.ToTypeSig();

            var getGameObjectMethodSig = MethodSig.CreateInstance(gameObjectTypeSig);
            var gameManagerStartMethodSig = MethodSig.CreateInstanceGeneric(1, new GenericMVar(0, gameManagerStartMethodDef));

            // {UnityEngine.GameObject UnityEngine.Component::get_gameObject()}
            var getGameObjectMethodRefUser = new MemberRefUser(_targetModule, new UTF8String("get_gameObject"), getGameObjectMethodSig, unityEngineComponentTypeRefUser);

            // {TrainerManager UnityEngine.GameObject::AddComponent<TrainerManager>()}
            var addComponentMethodRefUser = new MemberRefUser(_targetModule, new UTF8String("AddComponent"), gameManagerStartMethodSig, unityEngineGameObjectTypeRefUser);

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
        }

        private async Task AddWinCounter()
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

            var gameManagerTypeDef = _targetModule.Find("GameManager", true);
            var targetTrainerOptionsTypeDef = _targetModule.Find("TrainerOptions", true);
            var controllerTypeDef = _targetModule.Find("Controller", true);
            var fightingTypeDef = _targetModule.Find("Fighting", true);
            var characterStatsTypeDef = _targetModule.Find("CharacterStats", true);

            if (targetTrainerOptionsTypeDef == null)
            {
                await _logger.Log("TrainerOptions type def could not be located", LogLevel.Error);
                return;
            }

            var gameManagerAllButOnePlayersDiedMethodDef = gameManagerTypeDef.FindMethod("AllButOnePlayersDied");

            var singletonTypeDef = _targetModule.Find("TrainerManager", true);
            var checkCheatsEnabledMethodDef = singletonTypeDef.FindMethod("CheckCheatsEnabled");
            var targetSingletonTrainerOptionsInstantiationInstruction = checkCheatsEnabledMethodDef.Body.Instructions.FirstOrDefault();

            var targetAssemblyRef = _targetModule.CorLibTypes.AssemblyRef;

            var gameManagerPlayersAliveFieldDef = gameManagerTypeDef.FindField("playersAlive");
            var trainerOptionsNoWinnersFieldDef = targetTrainerOptionsTypeDef.FindField("NoWinners");

            var genericListTypeRef = new TypeRefUser(_targetModule, @"System.Collections.Generic", "List`1", targetAssemblyRef);
            var genericListControllerGenericInstSig = new GenericInstSig(new ClassSig(genericListTypeRef), controllerTypeDef.ToTypeSig());

            // Create TypeSpec from GenericInstSig
            var genericListControllerTypeSpec = new TypeSpecUser(genericListControllerGenericInstSig);

            var genericListControllerGetItemMemberRefUser = new MemberRefUser(_targetModule, "get_Item", MethodSig.CreateInstance(new GenericVar(0), _targetModule.CorLibTypes.Int32), genericListControllerTypeSpec);

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
        }

        private async Task<bool> CompileTrainerLogicModule()
        {
            var referenceLibraryDirectory = Path.GetDirectoryName(_targetModulePath);

            if (string.IsNullOrWhiteSpace(referenceLibraryDirectory))
            {
                await _logger.Log("Directory for reference libraries could not be located", LogLevel.Error);
                return await Task.FromResult(false);
            }

            var trainerLogicDllPath = Path.Combine(referenceLibraryDirectory, "TrainerLogic.dll");

            var parameters = new CompilerParameters
            {
#if DEBUG
                CompilerOptions = "/d:DEBUG",
#endif
                IncludeDebugInformation = false,
                OutputAssembly = trainerLogicDllPath
            };

            // The game is compiled under .NET 3.5, so this has to be matched.
            var codeProvider = CodeDomProvider.CreateProvider("CSharp", new Dictionary<string, string> { { "CompilerVersion", "v3.5" } });

            var referenceLibraries = new List<string> { "Assembly-CSharp.dll", "Assembly-CSharp-firstpass.dll", "UnityEngine.dll", "UnityEngine.UI.dll" };

            // Find appropriate TextMeshPro version (e.g. TextMeshPro-1.0.55.56.0b9.dll)
            var textMeshProReferenceLibrary = Directory.GetFiles(referenceLibraryDirectory, "TextMeshPro-*.dll");

            if (textMeshProReferenceLibrary.Length == 0)
            {
                await _logger.Log("Could not locate TextMeshPro reference library");
                return await Task.FromResult(false);
            }

            referenceLibraries.Add(textMeshProReferenceLibrary.First());

            // Add reference libraries
            foreach (var referenceLibrary in referenceLibraries)
            {
                parameters.ReferencedAssemblies.Add(Path.Combine(referenceLibraryDirectory, referenceLibrary));
            }

            // Decrypt and load sources for the Trainer Logic Module
            var moduleSources = _trainerLogicModuleBuilder.DecryptAndLoadLogicModuleSource(TrainerLogicModule.ModuleDataPrivatesDictionary["Key"], TrainerLogicModule.ModuleDataPrivatesDictionary["Iv"], await GetGameVersion());

            // Compile from sources
            CompilerResults results = codeProvider.CompileAssemblyFromSource(parameters, moduleSources.ToArray());

            if (results.Errors.Count > 0)
            {
                foreach (CompilerError compErr in results.Errors)
                {
                    await _logger.Log($"Line number {compErr.Line}{Environment.NewLine}Error Number: {compErr.ErrorNumber}{Environment.NewLine}{compErr.ErrorText}{Environment.NewLine}", LogLevel.Error);
                }

                return await Task.FromResult(false);
            }

            // Load the compiled DLL 
            await LoadLogicModule(trainerLogicDllPath);

            return await Task.FromResult(true);
        }

        /// <summary>
        /// Fix "infinite jumping" glitch for bots.
        /// </summary>
        private async Task ApplyBotJumpFix()
        {
            // Fetch target type defs
            var controllerTypeDef = _targetModule.Find("Controller", true);

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
                await _logger.Log("Apply bot jump fix: Controller jump method signature does not match any instructions", LogLevel.Error);
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
            var movementTypeDef = _targetModule.Find("Movement", true);

            // Fetch target method defs
            var movementJumpMethodDef = movementTypeDef.FindMethod("Jump");
            var movementDoJumpMethodDef = movementTypeDef.FindMethod("DoJump");

            // Fetch target field defs
            var movementAuFieldDef = movementTypeDef.FindField("au");
            var movementJumpClipsFieldDef = movementTypeDef.FindField("jumpClips");

            // Fetch reference assembly refs
            var mscorlibAssemblyRef = _targetModule.GetAssemblyRef(new UTF8String("mscorlib"));
            var unityEngineAssemblyRef = _targetModule.GetAssemblyRef(new UTF8String("UnityEngine"));

            // Construct type ref users
            var systemMathTypeRefUser = new TypeRefUser(_targetModule, new UTF8String("System"), new UTF8String("Math"), mscorlibAssemblyRef);
            var unityEngineAudioClipTypeRefUser = new TypeRefUser(_targetModule, new UTF8String("UnityEngine"), new UTF8String("AudioClip"), unityEngineAssemblyRef);
            var unityEngineRandomTypeRefUser = new TypeRefUser(_targetModule, new UTF8String("UnityEngine"), new UTF8String("Random"), unityEngineAssemblyRef);
            var unityEngineAudioSourceTypeRefUser = new TypeRefUser(_targetModule, new UTF8String("UnityEngine"), new UTF8String("AudioSource"), unityEngineAssemblyRef);

            // Construct member ref users
            var maxMethodRefUser = new MemberRefUser(_targetModule, new UTF8String("Max"), MethodSig.CreateStatic(_targetModule.CorLibTypes.Int32, _targetModule.CorLibTypes.Int32, _targetModule.CorLibTypes.Int32), systemMathTypeRefUser);
            var randomRangeMethodRefUser = new MemberRefUser(_targetModule, new UTF8String("Range"), MethodSig.CreateStatic(_targetModule.CorLibTypes.Int32, _targetModule.CorLibTypes.Int32, _targetModule.CorLibTypes.Int32), unityEngineRandomTypeRefUser);
            var playOneShotMethodRefUser = new MemberRefUser(_targetModule, new UTF8String("PlayOneShot"), MethodSig.CreateInstance(_targetModule.CorLibTypes.Void, unityEngineAudioClipTypeRefUser.ToTypeSig()), unityEngineAudioSourceTypeRefUser);

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
        }

        /// <summary>
        /// Make bots that are added to ControllerHandler.Players "die" instead of simply being "removed" like normal AIs.
        /// </summary>
        /// <returns></returns>
        private async Task ApplyBotDeathFix()
        {
            // Fetch target type defs
            var controllerTypeDef = _targetModule.Find("Controller", true);
            var healthHandlerTypeDef = _targetModule.Find("HealthHandler", true);

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
                await _logger.Log("Apply bot death fix: Health handler die method signature does not match any instructions", LogLevel.Error);
            }
        }

        /// <summary>
        /// Make bots collide with each other. Normally AIs do not collide with each other as they are set on the same GameObject layer index. 
        /// </summary>
        /// <returns></returns>
        private async Task ApplyBotCollisionFix()
        {
            // Fetch target type defs
            var controllerTypeDef = _targetModule.Find("Controller", true);

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
                await _logger.Log("Apply bot collision fix: Controller set collision method signature does not match any instructions", LogLevel.Error);
            }
        }
        
        /// <summary>
        /// Enable bots to be considered as in control and to go for guns. 
        /// </summary>
        /// <returns></returns>
        private async Task ApplyBotHasControlFix()
        {
            // Fetch assembly refs
            var unityEngine = _targetModule.GetAssemblyRef("UnityEngine");

            // Fetch target type defs
            var characterActionsTypeDef = _targetModule.Find("CharacterActions", true);
            var controllerTypeDef = _targetModule.Find("Controller", true);
            var aiTypeDef = _targetModule.Find("AI", true);

            // Fetch target method defs
            var aiStartMethodDef = aiTypeDef.FindMethod("Start");
            var controllerStartMethodDef = controllerTypeDef.FindMethod("Start");
            var characterActionsCreateWithControllerBindingsMethodDef = characterActionsTypeDef.FindMethod("CreateWithControllerBindings");

            // Fetch target field defs
            var aiGoForGunsFieldDef = aiTypeDef.FindField("goForGuns");
            var controllerHasControlFieldDef = controllerTypeDef.FindField("mHasControl");
            var controllerPlayerActionsFieldDef = controllerTypeDef.FindField("mPlayerActions");

            // Fetch type ref users
            var unityEngineComponentTypeRefUser = new TypeRefUser(_targetModule, "UnityEngine", "Component", unityEngine);
            var unityEngineBehaviourTypeRefUser = new TypeRefUser(_targetModule, "UnityEngine", "Behaviour", unityEngine);

            // Create method ref users
            var getComponentMethodRefUser = new MemberRefUser(_targetModule, "GetComponent", MethodSig.CreateInstanceGeneric(1, new GenericMVar(0, aiStartMethodDef)), unityEngineComponentTypeRefUser);
            //var getComponentMethodRefUser = new MemberRefUser(_targetModule, "GetComponent", MethodSig.CreateInstanceGeneric(1, aiTypeDef.ToTypeSig()), unityEngineComponentTypeRefUser);
            //var getComponentMethodRefUser = new MemberRefUser(_targetModule, "GetComponent", MethodSig.CreateInstanceGeneric(1, new GenericVar(0, aiTypeDef)), unityEngineComponentTypeRefUser);
            var setEnabledMethodRefUser = new MemberRefUser(_targetModule, "set_enabled", MethodSig.CreateInstance(_targetModule.CorLibTypes.Void, _targetModule.CorLibTypes.Boolean), unityEngineBehaviourTypeRefUser);

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
                await _logger.Log("Apply bot HasControl fix: Controller start method signature does not match any instructions", LogLevel.Error);
            }
        }

        private async Task EnableFlyingModeWeaponPickup()
        {
            var bodyPartTypeDef = _targetModule.Find("BodyPart", true);
            var controllerTypeDef = _targetModule.Find("Controller", true);

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
                await _logger.Log("Enable Flying Mode Weapon Pickup: Signature does not match any instructions", LogLevel.Error);
            }
        }

        private async Task EnableFlyingModeWeaponThrow()
        {
            var controllerTypeDef = _targetModule.Find("Controller", true);

            var onCollisionEnterMethod = controllerTypeDef.FindMethod("Update");
            var canFlyFieldDef = controllerTypeDef.FindField("canFly");

            var instructionSignature = new List<Instruction> {
                new Instruction(OpCodes.Ldfld, canFlyFieldDef),
                new Instruction(OpCodes.Brtrue, new Instruction(OpCodes.Ldarg_0, null)),
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
                await _logger.Log("Enable Flying Mode Weapon Pickup:  Signature does not match any instructions", LogLevel.Error);
            }
        }

        private async Task EnableNoPlayerDamage()
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

            var healthHandlerTypeDef = _targetModule.Find("HealthHandler", true);

            if (healthHandlerTypeDef == null)
            {
                await _logger.Log("Could not locate HealthHandler type def", LogLevel.Error);
                return;
            }

            // There are 2 method overloads that require patching
            var takeDamageMethodDefs = healthHandlerTypeDef.FindMethods("TakeDamage").ToArray();

            if (takeDamageMethodDefs.Length != 2)
            {
                await _logger.Log("Found more or less than two 'TakeDamage' method defs in HealthHandler type def", LogLevel.Error);
                return;
            }

            var targetTrainerOptionsTypeDef = _targetModule.Find("TrainerOptions", true);

            if (targetTrainerOptionsTypeDef == null)
            {
                await _logger.Log("TrainerOptions type def could not be located", LogLevel.Error);
                return;
            }

            var targetTrainerOptionsUnlimitedHealthField = targetTrainerOptionsTypeDef.FindField("UnlimitedHealth");

            if (targetTrainerOptionsUnlimitedHealthField == null)
            {
                await _logger.Log("TrainerOptions UnlimitedHealth fieldDef could not be located", LogLevel.Error);
                return;
            }

            var targetTrainerOptionsCheatsEnabledMethodDef = targetTrainerOptionsTypeDef.FindMethod("get_CheatsEnabled");

            if (targetTrainerOptionsCheatsEnabledMethodDef?.Body?.Instructions == null || targetTrainerOptionsCheatsEnabledMethodDef.Body?.Instructions.Any() == false)
            {
                await _logger.Log("TrainerOptions CheatsEnabled methodDef could not be located or it does not contain a body/instructions", LogLevel.Error);
                return;
            }

            var singletonTypeDef = _targetModule.Find("TrainerManager", true);
            var checkCheatsEnabledMethodDef = singletonTypeDef.FindMethod("CheckCheatsEnabled");
            var trainerOptionsGetInstanceCallInstruction = checkCheatsEnabledMethodDef.Body.Instructions.FirstOrDefault();

            foreach (var takeDamageMethodDef in takeDamageMethodDefs)
            {
                if (takeDamageMethodDef.Body.Instructions[0].OpCode == OpCodes.Call)
                {
                    await _logger.Log("HealthHandler TakeDamage method def - patching has already been performed", LogLevel.Error);
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
                if (takeDamageMethodDef.ReturnType == _targetModule.CorLibTypes.Boolean)
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
        }

        private bool InjectTrainerLogicModuleTypes()
        {
            var trainerOptionsTypeDef = _logicModule.Find("TrainerOptions", false);
            var trainerManagerTypeDef = _logicModule.Find("TrainerManager", false);
            var singletonTypeDef = _logicModule.Find("Singleton`1", true);

            var singletonSucceeded = _injectionHelpers.AddTypeToModule(singletonTypeDef, _targetModule);
            var trainerOptionsSucceeded = _injectionHelpers.AddTypeToModule(trainerOptionsTypeDef, _targetModule);
            var trainerManagerSucceeded = _injectionHelpers.AddTypeToModule(trainerManagerTypeDef, _targetModule);

            return singletonSucceeded && trainerOptionsSucceeded && trainerManagerSucceeded;
        }

        private bool InjectCustomAILogic()
        {
            var sourceModuleAiTypeDef = _targetModule.Find("AI", false);
            var destinationAiTypeDef = _logicModule.Find("AILogic", false);

            if (sourceModuleAiTypeDef == null || destinationAiTypeDef == null)
            {
                return false;
            }

            var updateMethodDef = sourceModuleAiTypeDef.FindMethod("Update");
            var customUpdateMethodDef = destinationAiTypeDef.FindMethod("UpdateHandler");

            // Replace all variables, instructions and exception handler in AI.Update from AILogic.UpdateHandler
            updateMethodDef.Body.Variables.Clear();
            updateMethodDef.Body.Instructions.Clear();
            updateMethodDef.Body.ExceptionHandlers.Clear();

            foreach (var variable in customUpdateMethodDef.Body.Variables)
            {
                updateMethodDef.Body.Variables.Add(variable);
            }

            foreach (var instruction in customUpdateMethodDef.Body.Instructions)
            {
                updateMethodDef.Body.Instructions.Add(instruction);
            }

            foreach (var exceptionHandler in customUpdateMethodDef.Body.ExceptionHandlers)
            {
                updateMethodDef.Body.ExceptionHandlers.Add(exceptionHandler);
            }

            updateMethodDef.Body.UpdateInstructionOffsets();

            return true;
        }

        private async Task LoadLogicModule(string logicModulePath)
        {
            if (!File.Exists(logicModulePath))
            {
                await _logger.Log("Could not locate logic module", LogLevel.Error);
                return;
            }

            _logicModule = ModuleDefMD.Load(logicModulePath);

            if (_logicModule == null)
            {
                await _logger.Log("Could not load logic module", LogLevel.Error);
            }
        }

        public async Task<string> GetGameVersion()
        {
            if (_targetModule == null)
            {
                await _logger.Log("Could not get game version (1)");
                return string.Empty;
            }

            // Load Type Defs
            var targetStickFightConstantsTypeDef = _targetModule.Find("StickFightConstants", true);

            if (targetStickFightConstantsTypeDef == null)
            {
                await _logger.Log("Could not get game version (2)");
                return string.Empty;
            }

            // Fetch target MethodDef
            var targetGetVersionValueMethodDef = targetStickFightConstantsTypeDef.FindMethod("get_VERSION_VALUE");

            if (targetGetVersionValueMethodDef?.Body?.Instructions == null || targetGetVersionValueMethodDef.Body?.Instructions.Any() == false)
            {
                await _logger.Log("Could not get game version (3)");
                return string.Empty;
            }

            // Fetch the Ldstr instruction containing the target's current version
            var targetCurrentVersionInstruction = targetGetVersionValueMethodDef.Body.Instructions.FirstOrDefault(a => a.OpCode == OpCodes.Ldstr);

            if (targetCurrentVersionInstruction?.Operand == null)
            {
                await _logger.Log("Could not get game version (4)");
                return string.Empty;
            }

            // Fetch the current version value
            var targetCurrentVersionValue = targetCurrentVersionInstruction.Operand as string;

            if (string.IsNullOrEmpty(targetCurrentVersionValue))
            {
                await _logger.Log("Could not get game version (5)");
                return string.Empty;
            }

            return targetCurrentVersionValue;
        }

        private async Task ReplaceVersionConstant()
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
            var targetStickFightConstantsTypeDef = _targetModule.Find("StickFightConstants", true);

            if (targetStickFightConstantsTypeDef == null)
            {
                await _logger.Log("StickFightConstants type def could not be located", LogLevel.Error);
                return;
            }

            var targetTrainerOptionsTypeDef = _targetModule.Find("TrainerOptions", true);

            if (targetTrainerOptionsTypeDef == null)
            {
                await _logger.Log("TrainerOptions type def could not be located", LogLevel.Error);
                return;
            }

            // Fetch target MethodDef
            var targetGetVersionValueMethodDef = targetStickFightConstantsTypeDef.FindMethod("get_VERSION_VALUE");

            if (targetGetVersionValueMethodDef?.Body?.Instructions == null || targetGetVersionValueMethodDef.Body?.Instructions.Any() == false)
            {
                await _logger.Log("StickFightConstants get_VERSION_VALUE methodDef could not be located or it does not contain a body/instructions", LogLevel.Error);
                return;
            }

            var targetTrainerOptionsGetVersionMethodDef = targetTrainerOptionsTypeDef.FindMethod("GetVersion");

            if (targetTrainerOptionsGetVersionMethodDef?.Body?.Instructions == null || targetTrainerOptionsGetVersionMethodDef.Body?.Instructions.Any() == false)
            {
                await _logger.Log("TrainerOptions GetVersion methodDef could not be located or it does not contain a body/instructions", LogLevel.Error);
                return;
            }

            // Check if the IL has already been replaced before
            if (targetGetVersionValueMethodDef.Body.Instructions.Any(instr => instr.OpCode == OpCodes.Call))
            {
                await _logger.Log("Version Value getter has already been replaced", LogLevel.Error);
                return;
            }

            // Fetch the Ldstr instruction containing the target's current version
            var targetCurrentVersionInstruction = targetGetVersionValueMethodDef.Body.Instructions.FirstOrDefault(a => a.OpCode == OpCodes.Ldstr);

            if (targetCurrentVersionInstruction?.Operand == null)
            {
                await _logger.Log("Could not locate current version instruction", LogLevel.Error);
                return;
            }

            // Fetch the current version value
            var targetCurrentVersionValue = targetCurrentVersionInstruction.Operand as string;

            if (string.IsNullOrEmpty(targetCurrentVersionValue))
            {
                await _logger.Log("Could not locate current version value", LogLevel.Error);
                return;
            }

            var singletonTypeDef = _targetModule.Find("TrainerManager", true);
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
        }
    }
}
