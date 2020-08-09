using StickFightTheGameTrainer.Common;
using StickFightTheGameTrainer.Reference;
using StickFightTheGameTrainer.Trainer;
using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StickFightTheGameTrainer
{
    public partial class MainForm : Form
    {
        private readonly Patcher _patcher;
        private readonly ILogger _logger;
        private readonly ErrorReportingForm _errorReportingForm;

        private async Task DebugLog(string message, LogLevel logLevel = LogLevel.Info)
        {
#if DEBUG
            await _logger.Log(message, logLevel);
#endif
        }

        public MainForm(ILogger logger, Patcher patcher, ErrorReportingForm errorReportingForm)
        {
            InitializeComponent();

            _logger = logger;
            _patcher = patcher;
            _errorReportingForm = errorReportingForm;
            lblTrainerVersionValue.Text = Application.ProductVersion;

            if (File.Exists(GameDirectories.CommonPath))
            {
                txtGamePath.Text = GameDirectories.CommonPath;
                browseGamePathDialog.SelectedPath = Path.GetDirectoryName(txtGamePath.Text);
                btnInstallMod.Enabled = true;
            }

#if DEBUG
            btnDeveloperOptions.Visible = true;
#endif
        }

        private async void BtnInstallMod_Click(object sender, EventArgs e)
        {
            _logger.ClearLogs();
            _errorReportingForm.Hide();
            btnInstallMod.Enabled = false;

            var targetPath = GetAdjustedTargetFilePath(txtGamePath.Text);

            if (await _patcher.LoadTargetModule(targetPath))
            {
                if (await _patcher.CheckTrainerAlreadyPatched())
                {
                    MessageBox.Show("A patch is already installed on the specified target. Please restore a backup of your Assembly-CSharp.dll file or re-install the game.", "Installation detected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    btnInstallMod.Enabled = true;
                    return;
                }

                progressBarInstallation.Visible = true;

                if (!await _patcher.PatchTargetModule(checkBoxCreateBackup.Checked) || _logger.HasErrors)
                {
                    _errorReportingForm.GenerateLogReport();
                    _errorReportingForm.Show();
                }
                else
                {
                    btnRestoreBackup.Enabled = await _patcher.CheckBackupExists(targetPath);
                    MessageBox.Show("The patch has been successfully installed");
                }

                btnInstallMod.Enabled = true;
                progressBarInstallation.Visible = false;
            }
            else
            {
                btnInstallMod.Enabled = true;
                MessageBox.Show("Could not locate or load the target Assembly-CSharp.dll file.");
            }
        }

        private async void BtnBrowseGamePath_Click(object sender, EventArgs e)
        {
            if (CheckValidPath(txtGamePath.Text)) {
                var targetPath = GetAdjustedTargetFilePath(txtGamePath.Text);
                browseGamePathDialog.SelectedPath = Path.GetDirectoryName(targetPath);
            } 
            else
            {
                browseGamePathDialog.SelectedPath = Directory.GetLogicalDrives()[0];
            }

            var dialogResult = browseGamePathDialog.ShowDialog();

            if (dialogResult == DialogResult.OK)
            {
                var selectedPath = browseGamePathDialog.SelectedPath;
                var attemptedPath = Path.Combine(selectedPath, GameDirectories.Library);

                if (!File.Exists(attemptedPath))
                {
                    attemptedPath = Path.Combine(selectedPath, GameDirectories.Level1, GameDirectories.Library);
                    if (!File.Exists(attemptedPath))
                    {
                        attemptedPath = Path.Combine(selectedPath, GameDirectories.Level2, GameDirectories.Library);
                        if (!File.Exists(attemptedPath))
                        {
                            attemptedPath = Path.Combine(selectedPath, GameDirectories.Level1, GameDirectories.Level2, GameDirectories.Library);
                            if (!File.Exists(attemptedPath))
                            {
                                MessageBox.Show($@"{GameDirectories.Library} could not be located in the selected directory");
                                return;
                            }
                        }
                    }
                }

                txtGamePath.Text = attemptedPath;

                btnRestoreBackup.Enabled = await _patcher.CheckBackupExists(txtGamePath.Text);

                btnInstallMod.Enabled = true;

                await DebugLog("Library file loaded");
            }
        }

        private async void BtnRestoreBackup_Click(object sender, EventArgs e)
        {
            if (progressBarInstallation.Visible)
            {
                MessageBox.Show("Installation is currently in progress.");
                return;
            }

            var targetPath = GetAdjustedTargetFilePath(txtGamePath.Text);

            if (!await _patcher.RestoreLatestBackup(targetPath))
            {
                MessageBox.Show("Could not restore latest backup!");
            }
            else
            {
                MessageBox.Show("Backup successfully restored");
            }
        }

        private async void TxtGamePath_TextChanged(object sender, EventArgs e)
        {            
            // Strip double quotes
            txtGamePath.Text = txtGamePath.Text.Replace("\"", "");

            if (CheckValidPath(txtGamePath.Text) == false)
            {
                btnInstallMod.Enabled = false;
                btnRestoreBackup.Enabled = false;
                return;
            }

            var targetPath = GetAdjustedTargetFilePath(txtGamePath.Text);
            
            btnInstallMod.Enabled = File.Exists(targetPath);
            btnRestoreBackup.Enabled = await _patcher.CheckBackupExists(targetPath);

            if (btnInstallMod.Enabled)
            {
                await _patcher.LoadTargetModule(targetPath);
                var gameVersion = await _patcher.GetGameVersion(targetPath);
                lblGameVersionValue.Text = gameVersion.Length > 0 ? gameVersion : "-";
            }
            else
            {
                lblGameVersionValue.Text = "-";
            }
        }

        private async void GenerateEncryptedSourcesAndKeysToolStripMenuItem_ClickAsync(object sender, EventArgs e)
        {
            try
            {
                await _patcher.EncryptLogicModuleSource();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not encrypy logic module source: {ex.Message}{Environment.NewLine}{ex.InnerException}{Environment.NewLine}{ex.StackTrace}");
            }
        }

        private bool CheckValidPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            // Check valid directory
            var invalidPathChars = Path.GetInvalidPathChars();
            var invalidFileNameChars = Path.GetInvalidFileNameChars();
            if (path.Any(t => invalidPathChars.Contains(t)))
            {
                return false;
            }

            var directory = Path.GetDirectoryName(path);

            // Check directory exists
            if (string.IsNullOrEmpty(directory) == false && Directory.Exists(directory) == false)
            {
                return false;
            }

            // Check valid filename
            var fileName = Path.GetFileName(path);
            if (string.IsNullOrEmpty(fileName) == false && fileName.Any(t => invalidFileNameChars.Contains(t)))
            {
                return false;
            }

            return true;
        }

        private string GetAdjustedTargetFilePath(string targetPath)
        {
            var targetFileName = Path.GetFileName(targetPath);
            var targetFileNameExtension = Path.GetExtension(targetPath);
            var targetFileNameWithoutExtension = Path.GetFileNameWithoutExtension(targetPath);

            var directory = Path.GetDirectoryName(targetPath);

            if (string.IsNullOrEmpty(directory))
            {
                targetPath = Path.Combine(".\\", targetPath);
            }

            if (string.IsNullOrEmpty(targetFileName))
            {
                // Append file name if only a directory is supplied
                targetPath = Path.Combine(targetPath, "Assembly-CSharp.dll");
            }
            else if (string.IsNullOrEmpty(targetFileNameExtension) && string.IsNullOrEmpty(targetFileNameWithoutExtension) == false)
            {
                // Append extension if only the file name is supplied
                targetPath += ".dll";
            }

            return targetPath;
        }

        private void BtnDeveloperOptions_Click(object sender, EventArgs e)
        {
            var x = Location.X + ((Control)sender).Location.X;
            var y = Location.Y + ((Control)sender).Location.Y;

            contextMenuStripDeveloperTools.Show(x, y);
        }

        private void DisplayErrorReportingFormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _errorReportingForm.Show();
        }
    }
}
