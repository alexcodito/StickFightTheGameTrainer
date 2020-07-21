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
using System.Runtime.CompilerServices;
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
            new KeyValuePair<string, string>("MultiplayerManager", "mGameManager"),
            new KeyValuePair<string, string>("GameManager", "controllerHandler"),
            new KeyValuePair<string, string>("GameManager", "m_WeaponSelectionHandler"),
            new KeyValuePair<string, string>("GameManager", "mNetworkManager"),
            new KeyValuePair<string, string>("GameManager", "mSpawnedWeapons"),
            new KeyValuePair<string, string>("GameManager", "levelSelector"),
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

            // Set the below fields and methods as public in order to allow TrainerLogic.dll's code to reference them and compile.
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

            await _logger.Log("Injecting TrainerOptions and TrainerManager");

            // Inject TrainerOptions class
            if (!InjectTrainerLogicModuleTypes())
            {
                await _logger.Log("Could not inject Logic Module's types", LogLevel.Error);
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

            SaveAndReload(false);

            DeleteTrainerLogicModule();

            await _logger.Log("Deleting trainer logic module");

            await _logger.Log("Patching completed");

            return await Task.FromResult(true);
        }

        private void DeleteTrainerLogicModule()
        {
            var location = _logicModule.Location;

            _logicModule.Dispose();
            File.Delete(location);
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

            var gameManagerStartMethodSig = MethodSig.CreateInstanceGeneric(0, new GenericMVar(0, gameManagerStartMethodDef));
            gameManagerStartMethodSig.GenParamCount = 1;

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
                //GenerateExecutable = false,
                //GenerateInMemory = true,

                IncludeDebugInformation = false,
                OutputAssembly = trainerLogicDllPath
            };

            // The game is compiled under .NET 3.5, so this has to be matched.
            var codeProvider = CodeDomProvider.CreateProvider("CSharp", new Dictionary<string, string> { { "CompilerVersion", "v3.5" } });

            var referenceLibraries = new List<string> { "Assembly-CSharp.dll", "Assembly-CSharp-firstpass.dll", "UnityEngine.dll", "UnityEngine.UI.dll" }; // 

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

        private async Task EnableFlyingModeWeaponPickup()
        {
            var bodyPartTypeDef = _targetModule.Find("BodyPart", true);
            var controllerTypeDef = _targetModule.Find("Controller", true);

            var onCollisionEnterMethod = bodyPartTypeDef.FindMethod("OnCollisionEnter");
            var canFlyFieldDef = controllerTypeDef.FindField("canFly");

            var bodyPartControllerFieldDef = bodyPartTypeDef.FindField("controller");

            //var opcodeSignature = new List<OpCode> { OpCodes.Ldfld, OpCodes.Brtrue, OpCodes.Ldarg_0 };
            var instructionSignature = new List<Instruction> { new Instruction(OpCodes.Ldfld, bodyPartControllerFieldDef), new Instruction(OpCodes.Ldfld, canFlyFieldDef), new Instruction(OpCodes.Brtrue, new Instruction(OpCodes.Ldarg_1, null)), new Instruction(OpCodes.Ldarg_0, null) };

            //var matchedInstructions = InjectionHelpers.FetchInstructionsBySignature(onCollisionEnterMethod.Body.Instructions, opcodeSignature);
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

            var instructionSignature = new List<Instruction> { new Instruction(OpCodes.Ldfld, canFlyFieldDef), new Instruction(OpCodes.Brtrue, new Instruction(OpCodes.Ldarg_0, null)), new Instruction(OpCodes.Ldarg_0, null) };

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

            // There are 2 methods that require patching
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

        private void ReloadLogicModule(bool reload = true)
        {
            var location = _logicModule.Location;
            _logicModule.Dispose();
            _logicModule = ModuleDefMD.Load(location);
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

        private void SaveAndReload(bool reload = true)
        {
            _injectionHelpers.Save(_targetModule, true);

            if (reload)
            {
                _targetModule = ModuleDefMD.Load(_targetModulePath);
            }
        }
    }
}
