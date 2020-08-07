using Confuser.Core;
using Confuser.Core.Project;
using StickFightTheGameTrainer.Common;
using System;
using System.IO;
using System.Threading.Tasks;

namespace StickFightTheGameTrainer.Obfuscation
{
    public class ProtectionUtilities : Confuser.Core.ILogger
    {
        private readonly Common.ILogger _logger;
        private readonly LogLevel _errorLogLevel = LogLevel.Warning; // Do not abort if the protection stage fails.
        private bool _successfullyProtected;

        public ProtectionUtilities(Common.ILogger logger)
        {
            _logger = logger;
        }

        internal async Task<bool> ProtectAssembly(string targetPath)
        {
            if (string.IsNullOrWhiteSpace(targetPath))
            {
                throw new ArgumentNullException(targetPath);
            }

            var targetFile = Path.GetFileName(targetPath);
            var targetDirectory = Path.GetDirectoryName(targetPath);

            var project = new ConfuserProject
            {
                BaseDirectory = targetDirectory,
                OutputDirectory = targetDirectory                
            };

            ProjectModule module = new ProjectModule
            {
                Path = targetFile
            };

            module.Rules.Add(new Rule
            {
                new SettingItem<Protection>("anti debug"),
                new SettingItem<Protection>("anti ildasm"),
                new SettingItem<Protection>("ctrl flow"),
                new SettingItem<Protection>("ref proxy"),
                new SettingItem<Protection>("harden"),
            });

            project.Add(module);

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
            _logger.Log(msg, _errorLogLevel);
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
            _logger.Log(msg, LogLevel.Warning);
        }

        public void WarnFormat(string format, params object[] args)
        {
            _logger.Log(string.Format(format, args), LogLevel.Warning);
        }
        #endregion
    }
}
