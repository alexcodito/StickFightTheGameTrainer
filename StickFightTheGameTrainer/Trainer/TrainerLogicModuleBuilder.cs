using StickFightTheGameTrainer.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace StickFightTheGameTrainer.Trainer
{
    public class TrainerLogicModuleBuilder
    {
        private readonly ILogger _logger;
        private readonly List<string> _targetSourceCodeFileNames;

        public TrainerLogicModuleBuilder(ILogger logger)
        {
            _logger = logger;
            _targetSourceCodeFileNames = new List<string> { "AILogic", "Singleton", "TrainerManager", "TrainerOptions" };
        }

        public List<string> DecryptAndLoadLogicModuleSource(string key, string iv, string version)
        {
            List<string> decryptedModules = new List<string>();

#if DEBUG
            // Load source files without dealing with decryption.

            var path = Path.Combine(Environment.CurrentDirectory, "..", "..", "Trainer\\TrainerLogic");

            if (!Directory.Exists(path))
            {
                throw new Exception("Path does not exist: " + path);
            }

            foreach (var filename in _targetSourceCodeFileNames)
            {
                var filePath = Path.Combine(path, filename + ".cs");
                if (!File.Exists(filePath))
                {
                    throw new Exception("File could not be located: " + filePath);
                }

                var contents = File.ReadAllText(filePath);

                if (string.IsNullOrWhiteSpace(contents))
                {
                    throw new Exception($"File does not contain any data: '{filename}'");
                }

                contents = ProcessModuleSource(contents, version);

                decryptedModules.Add(contents);
            }
#endif

#if !DEBUG
            // Decrypt each encrypted source code file.

            foreach (var encryptedModuleData in TrainerLogicModule.EncryptedModuleDataDictionary)
            {
                var decryptedModule = AesUtility.DecryptStringFromBase64String(encryptedModuleData.Value, key, iv);
                decryptedModule = ProcessModuleSource(decryptedModule, version);

                decryptedModules.Add(decryptedModule);
            }
#endif

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

            using (var aesAlgo = new AesCryptoServiceProvider())
            {
                aesAlgo.Padding = PaddingMode.PKCS7;
                aesAlgo.KeySize = 256;

                var key = Convert.ToBase64String(aesAlgo.Key);
                var iv = Convert.ToBase64String(aesAlgo.IV);

                File.WriteAllText(Path.Combine(path, "key.data"), key);
                File.WriteAllText(Path.Combine(path, "iv.data"), iv);

                var encryptedData = new StringBuilder();

                encryptedData.AppendLine($"{{ \"Iv\", \"{iv}\" }},");
                encryptedData.AppendLine($"{{ \"Key\", \"{key}\" }},{Environment.NewLine}");

                foreach (var filename in _targetSourceCodeFileNames)
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

                    var base64EncryptedData = Convert.ToBase64String(encryptedBytes);
                    File.WriteAllText(filePath.Replace(".cs", ".data"), base64EncryptedData);

                    await _logger.Log($"File '{filename}' has been successfully encrypted and saved.");

                    encryptedData.AppendLine($"{{ \"{filename}\", \"{base64EncryptedData}\" }},");
                }

                await _logger.Log(encryptedData.ToString());
            }
        }

        private static string ProcessModuleSource(string decryptedModule, string version)
        {
            // Replace trainer version string
            decryptedModule = decryptedModule.Replace("{Application.ProductVersion}", System.Windows.Forms.Application.ProductVersion);

            var versionParsed = double.TryParse(version, out var dVersion);

            // Define version specific versions for compatibility if the module requires it.
            if (versionParsed && decryptedModule.IndexOf("REQUIRE_COMPATIBILITY_PATCHING") > -1)
            {
                if (dVersion >= 1.8)
                {
                    // Versions higher than 1.7
                    decryptedModule = decryptedModule.Insert(0, $"#define V1_8_POST{Environment.NewLine}");
                }
                else
                {
                    // Versions lower than 1.8
                    decryptedModule = decryptedModule.Insert(0, $"#define V1_8_PRE{Environment.NewLine}");
                }

                if (dVersion >= 24.0)
                {
                    // Versions higher than 23
                    decryptedModule = decryptedModule.Insert(0, $"#define V24_POST{Environment.NewLine}");
                } else
                {
                    // Versions lower than 24
                    decryptedModule = decryptedModule.Insert(0, $"#define V24_PRE{Environment.NewLine}");
                }
            }            

            return decryptedModule;
        }
    }
}
