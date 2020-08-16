using dnlib.DotNet;
using dnlib.DotNet.Emit;
using StickFightTheGameTrainer.Common;
using StickFightTheGameTrainer.Obfuscation;
using StickFightTheGameTrainer.Trainer.Helpers;
using StickFightTheGameTrainer.Trainer.PatcherMethods;
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
        private readonly AssemblyProtectionUtilities _protectionUtilities;
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
            new KeyValuePair<string, string>("HoardHandler", "charactersAlive"),
            new KeyValuePair<string, string>("MultiplayerManager", "mGameManager"),
            new KeyValuePair<string, string>("GameManager", "controllerHandler"),
            new KeyValuePair<string, string>("GameManager", "m_WeaponSelectionHandler"),
            new KeyValuePair<string, string>("GameManager", "mNetworkManager"),
            new KeyValuePair<string, string>("GameManager", "mSpawnedWeapons"),
            new KeyValuePair<string, string>("GameManager", "levelSelector"),
            new KeyValuePair<string, string>("GameManager", "hoardHandler"),
            new KeyValuePair<string, string>("Controller", "fighting"),
            new KeyValuePair<string, string>("Controller", "movement"),
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

        public Patcher(Common.ILogger logger, TrainerLogicModuleBuilder trainerLogicModuleBuilder, AssemblyProtectionUtilities protectionUtilities)
        {
            _logger = logger;
            _trainerLogicModuleBuilder = trainerLogicModuleBuilder;
            _protectionUtilities = protectionUtilities;
        }

        public async Task<bool> LoadTargetModule(string targetModulePath)
        {
            if (!File.Exists(targetModulePath))
            {
                await _logger.Log("Could not locate target module", LogLevel.Error);
                return await Task.FromResult(false);
            }

            if (_targetModule != null)
            {
                _targetModule.Dispose();
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

        private void SaveAndReloadTargetModule(bool reload = true)
        {
            InjectionHelpers.Save(_targetModule);

            if (reload)
            {
                _targetModule = ModuleDefMD.Load(_targetModulePath);
            }
        }

        public void UnloadTargetModule()
        {
            _targetModule?.Dispose();
        }

        public async Task EncryptLogicModuleSource()
        {
            await _trainerLogicModuleBuilder.EncryptLogicModuleSource();
        }

        public async Task<bool> CheckTrainerAlreadyPatched()
        {
            var stickFightConstantsTypeDef = _targetModule.Find("StickFightConstants", false);

            // Check if patch indicator exists in the target module.
            if (stickFightConstantsTypeDef != null)
            {
                var trainerPatchIndicator = stickFightConstantsTypeDef.FindField("LoxaTrainerPatch");

                if (trainerPatchIndicator != null)
                {
                    return await Task.FromResult(true);
                }
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

                File.Copy(_targetModulePath, _targetModulePath.Replace(".dll", $"_{targetGameVersion}_{Application.ProductVersion}_{DateTime.Now.ToFileTime()}_backup.dll"));

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

            if (string.IsNullOrWhiteSpace(targetGameVersion))
            {
                targetGameVersion = "*";
            }

            var matches = Directory.GetFiles(directory, $"Assembly-CSharp_{targetGameVersion}_{Application.ProductVersion}_*_backup.dll");

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
            int patchingStatus = 0;

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
            await _logger.Log($"Game version: {await GetGameVersion()}");
            await _logger.Log($"Patcher version: {Application.ProductVersion}");

            // Add a private field indicating that the module has been modified.
            await _logger.Log("Adding trainer patch indicator");
            patchingStatus = await AddTrainerPatchIndicator.Execute(_targetModule);

            if (patchingStatus > 0)
            {
                await _logger.Log($"Could not add trainer patch indicator: {patchingStatus}", LogLevel.Error);
                return await Task.FromResult(false);
            }

            SaveAndReloadTargetModule();

            // Set target fields and methods as public in order to allow TrainerLogic.dll's code to reference them and compile.
            // Note: While it would have been convenient to simply set *every* field and method as public/internal, it causes in-game glitches (e.g. ambiguous references and null reference exceptions)
            await _logger.Log("Setting fields as public");
            _targetFields.ForEach(targetField => InjectionHelpers.SetFieldAccessModifier(_targetModule, targetField.Key, targetField.Value, FieldAttributes.Public));

            await _logger.Log("Setting methods as public");
            _targetMethods.ForEach(targetMethod => InjectionHelpers.SetMethodAccessModifier(_targetModule, targetMethod.Key, targetMethod.Value, MethodAttributes.Public));

            await _logger.Log("Adding TrainerWeaponIndex field to fighting class");
            patchingStatus = await AddTrainerWeaponIndex.Execute(_targetModule);

            if (patchingStatus > 0)
            {
                await _logger.Log($"Could not add TrainerWeaponIndex field to fighting class: {patchingStatus}");
                return await Task.FromResult(false);
            }

            SaveAndReloadTargetModule();

            // Compile and load TrainerLogic module
            await _logger.Log("Compiling Trainer Logic module");

            if (!await CompileTrainerLogicModule())
            {
                await _logger.Log("Could not compile Trainer Logic module", LogLevel.Error);
                return await Task.FromResult(false);
            }

            SaveAndReloadTargetModule();

            // Keeping the fields as public causes exceptions due to reference ambiguity, so they need to be made internal following successful compilation.
            await _logger.Log("Setting fields as internal");
            _targetFields.ForEach(targetField => InjectionHelpers.SetFieldAccessModifier(_targetModule, targetField.Key, targetField.Value, FieldAttributes.Assembly));

            await _logger.Log("Setting methods as internal");
            _targetMethods.ForEach(targetMethod => InjectionHelpers.SetMethodAccessModifier(_targetModule, targetMethod.Key, targetMethod.Value, MethodAttributes.Assembly));

            SaveAndReloadTargetModule();

            // Inject TrainerOptions class
            await _logger.Log("Injecting trainer logic modules");

            if (InjectTrainerLogicModuleTypes() == false)
            {
                await _logger.Log("Could not inject trainer logic modules", LogLevel.Error);
                return await Task.FromResult(false);
            }

            await _logger.Log("Injecting custom AI logic");

            if (InjectCustomAILogic() == false)
            {
                await _logger.Log("Could not inject AI logic module", LogLevel.Error);
                return await Task.FromResult(false);
            }

            SaveAndReloadTargetModule();

            // Inject declaration of TrainerManager class into GameManager class and inject its instantiation code into GameManager's Start() method
            await _logger.Log("Adding TrainerManager to GameManager");
            patchingStatus = await AddTrainerManagerToGameManager.Execute(_targetModule);

            if (patchingStatus > 0)
            {
                await _logger.Log($"Could not add TrainerManager to GameManager: {patchingStatus}", LogLevel.Error);
                return await Task.FromResult(false);
            }

            SaveAndReloadTargetModule();

            await _logger.Log("Add trainer game version constant");
            patchingStatus = await AddTrainerVersionConstant.Execute(_targetModule);

            if (patchingStatus > 0)
            {
                await _logger.Log($"Could not add trainer game version constant: {patchingStatus}", LogLevel.Error);
                return await Task.FromResult(false);
            }

            await _logger.Log("Enabling chat bubble in all lobbies");
            patchingStatus = await EnableChatBubbleInAllLobbies.Execute(_targetModule);

            if (patchingStatus > 0)
            {
                await _logger.Log($"Could not enable chat bubble in all lobies: {patchingStatus}", LogLevel.Warning);
            }

            await _logger.Log("Enabling flying mode weapon pickup");
            patchingStatus = await EnableFlyingModeWeaponPickup.Execute(_targetModule);

            if (patchingStatus > 0)
            {
                await _logger.Log($"Could not enable flying mode weapon pickup: {patchingStatus}", LogLevel.Warning);
            }

            await _logger.Log("Enabling flying mode weapon throw");
            patchingStatus = await EnableFlyingModeWeaponThrow.Execute(_targetModule);

            if (patchingStatus > 0)
            {
                await _logger.Log($"Could not enable flying mode weapon throw: {patchingStatus}", LogLevel.Warning);
            }

            await _logger.Log("Enabling unlimited health option");
            patchingStatus = await EnableNoPlayerDamage.Execute(_targetModule);

            if (patchingStatus > 0)
            {
                await _logger.Log($"Could not enable unlimited health option: {patchingStatus}", LogLevel.Warning);
            }

            await _logger.Log("Adding win counter");
            patchingStatus = await AddWinCounter.Execute(_targetModule);

            if (patchingStatus > 0)
            {
                await _logger.Log($"Could not add win counter: {patchingStatus}", LogLevel.Warning);
            }

            await _logger.Log("Applying bot jump fix");
            patchingStatus = await ApplyBotJumpFix.Execute(_targetModule);

            if (patchingStatus > 0)
            {
                await _logger.Log($"Could not apply bot jump fix: {patchingStatus}", LogLevel.Warning);
            }

            await _logger.Log("Applying bot death fix");
            patchingStatus = await ApplyBotDeathFix.Execute(_targetModule);

            if (patchingStatus > 0)
            {
                await _logger.Log($"Could not apply bot death fix: {patchingStatus}", LogLevel.Warning);
            }

            await _logger.Log("Applying bot collision fix");
            patchingStatus = await ApplyBotCollisionFix.Execute(_targetModule);

            if (patchingStatus > 0)
            {
                await _logger.Log($"Could not apply bot collision fix: {patchingStatus}", LogLevel.Warning);
            }

            await _logger.Log("Applying bot HasControl fix");
            patchingStatus = await ApplyBotHasControlFix.Execute(_targetModule);

            if (patchingStatus > 0)
            {
                await _logger.Log($"Could not apply bot HasControl fix: {patchingStatus}", LogLevel.Warning);
            }
            
            await _logger.Log("Adding bot chat messages");
            patchingStatus = await AddBotChatMessages.Execute(_targetModule);

            if (patchingStatus > 0)
            {
                await _logger.Log($"Could not add bot chat messages: {patchingStatus}", LogLevel.Warning);
            }

            SaveAndReloadTargetModule();

#if !DEBUG
            await _logger.Log("Protecting assembly");

            patchingStatus = await TrainerProtectionUtilities.ObfuscateTrainer(_targetModule);

            if (patchingStatus > 0)
            {
                await _logger.Log($"Could not obfuscate trainer: {patchingStatus}", LogLevel.Error);
                return await Task.FromResult(false);
            }

            SaveAndReloadTargetModule(false);

            if (await _protectionUtilities.ConfuseAssembly(_targetModulePath) == false)
            {
                await _logger.Log("Could not confuse assembly", LogLevel.Error);
                return await Task.FromResult(false);
            }
#endif

            await _logger.Log("Patching completed");

            return await Task.FromResult(true);
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
                GenerateInMemory = true
            };

            // The game is compiled under .NET 3.5, so this has to be matched.
            var codeProvider = CodeDomProvider.CreateProvider("CSharp", new Dictionary<string, string> { { "CompilerVersion", "v3.5" } });

            var referenceLibraries = new List<string> { "Assembly-CSharp.dll", "Assembly-CSharp-firstpass.dll", "UnityEngine.dll", "UnityEngine.UI.dll" };

            // Find appropriate TextMeshPro version (e.g. TextMeshPro-1.0.55.56.0b9.dll)
            var textMeshProReferenceLibrary = Directory.GetFiles(referenceLibraryDirectory, "TextMeshPro-*.dll");

            if (textMeshProReferenceLibrary.Length == 0)
            {
                await _logger.Log("Could not locate TextMeshPro reference library", LogLevel.Error);
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

            // Compile loaded sources
            CompilerResults results = codeProvider.CompileAssemblyFromSource(parameters, moduleSources.ToArray());

            if (results.Errors.Count > 0)
            {
                foreach (CompilerError compErr in results.Errors)
                {
                    await _logger.Log($"Line number {compErr.Line}{Environment.NewLine}Error Number: {compErr.ErrorNumber}{Environment.NewLine}{compErr.ErrorText}{Environment.NewLine}", LogLevel.Error);
                }

                return await Task.FromResult(false);
            }

            var logicModuleCompiledAssembly = results.CompiledAssembly;

            if (logicModuleCompiledAssembly == null)
            {
                await _logger.Log("Trainer Logic assembly could not be loaded from memory", LogLevel.Error);
                return await Task.FromResult(false);
            }

            var modules = logicModuleCompiledAssembly.GetModules();

            if (modules.Length == 0)
            {
                await _logger.Log("Trainer Logic assembly does not contain any modules", LogLevel.Error);
                return await Task.FromResult(false);
            }

            // Load the compiled DLL 
            await LoadLogicModule(modules[0]);

            return await Task.FromResult(true);
        }

        private bool InjectTrainerLogicModuleTypes()
        {
            var trainerOptionsTypeDef = _logicModule.Find("TrainerOptions", false);
            var trainerManagerTypeDef = _logicModule.Find("TrainerManager", false);
            var singletonTypeDef = _logicModule.Find("Singleton`1", true);

            var singletonSucceeded = InjectionHelpers.AddTypeToModule(singletonTypeDef, _targetModule);
            var trainerOptionsSucceeded = InjectionHelpers.AddTypeToModule(trainerOptionsTypeDef, _targetModule);
            var trainerManagerSucceeded = InjectionHelpers.AddTypeToModule(trainerManagerTypeDef, _targetModule);

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

            // Replace all variables, instructions and exception handlers in AI.Update from AILogic.UpdateHandler
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

        private async Task LoadLogicModule(System.Reflection.Module logicModule)
        {
            if (logicModule == null)
            {
                await _logger.Log("Could not locate logic module", LogLevel.Error);
                return;
            }

            _logicModule = ModuleDefMD.Load(logicModule);

            if (_logicModule == null)
            {
                await _logger.Log("Could not load logic module", LogLevel.Error);
            }
        }

        public async Task<string> GetGameVersion(string targetModulePath = null)
        {
            if (string.IsNullOrWhiteSpace(targetModulePath) == false)
            {
                if (await LoadTargetModule(targetModulePath) == false)
                {
                    await _logger.Log("Could not get game version (1)");
                    return string.Empty;
                }
            }

            if (_targetModule == null)
            {
                await _logger.Log("Could not get game version (2)");
                return string.Empty;
            }

            // Load Type Defs
            var targetStickFightConstantsTypeDef = _targetModule.Find("StickFightConstants", true);

            if (targetStickFightConstantsTypeDef == null)
            {
                await _logger.Log("Could not get game version (3)");
                return string.Empty;
            }

            // Fetch target MethodDef
            var targetGetVersionValueMethodDef = targetStickFightConstantsTypeDef.FindMethod("get_VERSION_VALUE");

            if (targetGetVersionValueMethodDef?.Body?.Instructions == null || targetGetVersionValueMethodDef.Body?.Instructions.Any() == false)
            {
                await _logger.Log("Could not get game version (4)");
                return string.Empty;
            }

            // Fetch the Ldstr instruction containing the target's current version
            var targetCurrentVersionInstruction = targetGetVersionValueMethodDef.Body.Instructions.FirstOrDefault(a => a.OpCode == OpCodes.Ldstr);

            if (targetCurrentVersionInstruction?.Operand == null)
            {
                await _logger.Log("Could not get game version (5)");
                return string.Empty;
            }

            // Fetch the current version value
            var targetCurrentVersionValue = targetCurrentVersionInstruction.Operand as string;

            if (string.IsNullOrEmpty(targetCurrentVersionValue))
            {
                await _logger.Log("Could not get game version (6)");
                return string.Empty;
            }

            return targetCurrentVersionValue;
        }
    }
}