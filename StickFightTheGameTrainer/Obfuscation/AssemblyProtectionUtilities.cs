using Confuser.Core;
using Confuser.Core.Project;
using dnlib.DotNet;
using StickFightTheGameTrainer.Common;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace StickFightTheGameTrainer.Obfuscation
{
    public class AssemblyProtectionUtilities : Confuser.Core.ILogger
    {
        private readonly Common.ILogger _logger;
        private readonly LogLevel _errorLogLevel = LogLevel.Error;
        private bool _successfullyProtected;

        public AssemblyProtectionUtilities(Common.ILogger logger)
        {
            _logger = logger;
        }

        internal async Task<bool> ConfuseAssembly(string targetPath, ModuleDefMD targetModule)
        {
            if (string.IsNullOrWhiteSpace(targetPath))
            {
                throw new ArgumentNullException(nameof(targetPath));
            }

            if(targetModule == null)
            {
                throw new ArgumentNullException(nameof(targetModule));
            }

            var targetFile = Path.GetFileName(targetPath);
            var targetDirectory = Path.GetDirectoryName(targetPath);

            var project = new ConfuserProject
            {                
                BaseDirectory = targetDirectory,
                OutputDirectory = targetDirectory
            };

            // Confuser plugins are merged into the main assembly at build time. Instruct the project to load them from the running executable.
            project.PluginPaths.Add(Assembly.GetExecutingAssembly().Location);

            byte[] rawData;
            using (var ms = new MemoryStream())
            {
                targetModule.Write(ms);
                targetModule.Dispose();
                ms.Seek(0, SeekOrigin.Begin);
                rawData = ms.ToArray();
            }

            if (rawData == null || rawData.Count() == 0)
            {
                return false;
            }

            ProjectModule projectModule = new ProjectModule
            {                 
                Path = targetFile,
                RawData = rawData
            };

            projectModule.Rules.Add(new Rule
            {
                new SettingItem<Protection>("watermark", SettingItemAction.Remove),
                new SettingItem<Protection>("anti debug"),
                new SettingItem<Protection>("anti ildasm"),
                new SettingItem<Protection>("ctrl flow"),
                new SettingItem<Protection>("ref proxy"),
                new SettingItem<Protection>("harden"),
            });

            project.Add(projectModule);

            var parameters = new ConfuserParameters
            {
                Project = project,        
                Logger = this
            };

            await ConfuserEngine.Run(parameters);

            return _successfullyProtected;
        }

        #region Logging

        public void Debug(string msg)
        {
            _logger.Log(msg, LogLevel.Info);
        }

        public void DebugFormat(string format, params object[] args)
        {
            _logger.Log(string.Format(format, args), LogLevel.Info);
        }

        public void EndProgress()
        {
        }

        public void Error(string msg)
        {
            _logger.Log(msg, _errorLogLevel);
        }

        public void ErrorException(string msg, Exception ex)
        {
            _logger.Log(msg + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace, _errorLogLevel);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            _logger.Log(string.Format(format, args), _errorLogLevel);
        }

        public void Finish(bool successful)
        {
            _successfullyProtected = successful;
        }

        public void Info(string msg)
        {
            _logger.Log(msg, LogLevel.Info);
        }

        public void InfoFormat(string format, params object[] args)
        {
            _logger.Log(string.Format(format, args), LogLevel.Info);
        }

        public void Progress(int progress, int overall)
        {
        }

        public void Warn(string msg)
        {
            _logger.Log(msg, LogLevel.Warning);
        }

        public void WarnException(string msg, Exception ex)
        {
            _logger.Log(msg + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace, LogLevel.Warning);
        }

        public void WarnFormat(string format, params object[] args)
        {
            _logger.Log(string.Format(format, args), LogLevel.Warning);
        }
        #endregion
    }
}
