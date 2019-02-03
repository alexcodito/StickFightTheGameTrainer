using StickFightTheGameTrainer.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using StickFightTheGameTrainer.Trainer.TrainerLogic;

namespace StickFightTheGameTrainer.Trainer
{
    public class TrainerLogicModuleBuilder
    {
        private readonly ILogger _logger;

        public TrainerLogicModuleBuilder(ILogger logger)
        {
            _logger = logger;
        }

        public List<string> DecryptAndLoadLogicModuleSource(string key, string iv, string version)
        {
            List<string> decryptedModules = new List<string>();

            foreach (var encryptedModuleData in TrainerLogicModule.EncryptedModuleDataDictionary)
            {
                var decryptedModule = AesUtility.DecryptStringFromBase64String(encryptedModuleData.Value, key, iv);
                decryptedModule = ProcessModuleSource(decryptedModule, version);

                decryptedModules.Add(decryptedModule);
            }

            return decryptedModules;
        }

        public async Task EncryptLogicModuleSource()
        {
            var path = Path.Combine(Environment.CurrentDirectory, "..", "..", "Trainer\\TrainerLogic");

            if (!Directory.Exists(path))
            {
                await _logger.Log("Path does not exist: " + path, LogLevel.Error);
                return;
            }

            var targetFileNames = new List<string> { "Singleton", "TrainerManager", "TrainerOptions" };

            using (var aesAlgo = new AesCryptoServiceProvider())
            {
                aesAlgo.Padding = PaddingMode.PKCS7;

                foreach (var filename in targetFileNames)
                {
                    var filePath = Path.Combine(path, filename + ".cs");
                    if (!File.Exists(filePath))
                    {
                        await _logger.Log("File could not be located: " + filePath, LogLevel.Error);
                        return;
                    }

                    var contents = File.ReadAllText(filePath);

                    if (string.IsNullOrWhiteSpace(contents))
                    {
                        await _logger.Log($"File does not contain any data: '{filename}'", LogLevel.Error);
                        return;
                    }

                    var encryptedBytes = AesUtility.EncryptStringToBytes(contents, aesAlgo.Key, aesAlgo.IV);

                    if (encryptedBytes == null)
                    {
                        await _logger.Log($"Could not encrypt data for {filename}", LogLevel.Error);
                        return;
                    }

                    var encrypted = Convert.ToBase64String(encryptedBytes);
                    File.WriteAllText(filePath.Replace(".cs", ".data"), encrypted);

                    await _logger.Log($"File '{filename}' has been successfully encrypted and saved.");
                }

                File.WriteAllText(Path.Combine(path, "key.data"), Convert.ToBase64String(aesAlgo.Key));
                File.WriteAllText(Path.Combine(path, "iv.data"), Convert.ToBase64String(aesAlgo.IV));
            }
        }

        private static string ProcessModuleSource(string decryptedModule, string version)
        {
            // Replace trainer version string
            decryptedModule = decryptedModule.Replace("{Application.ProductVersion}", System.Windows.Forms.Application.ProductVersion);

            var versionParsed = double.TryParse(version, out var dVersion);

            // SpawnRandomWeapon takes 3 arguments since version 1.2.08 (1.8) 
            if (versionParsed && dVersion < 1.8)
            {
                decryptedModule = decryptedModule.Replace("//{TrainerCompatibility.TrainerManager.SpawnRandomWeapon.Pre1_2_08_arg_1}", "");
            }
            else
            {
                decryptedModule = decryptedModule.Replace("//{TrainerCompatibility.TrainerManager.SpawnRandomWeapon.Post1_2_08_arg_1}", "");
            }

            return decryptedModule;
        }
    }
}
