using StickFightTheGameTrainer.Common;
using StickFightTheGameTrainer.Reference;
using StickFightTheGameTrainer.Trainer;
using System;
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

            if (await _patcher.LoadTargetModule(txtGamePath.Text))
            {
                if (await _patcher.CheckTrainerAlreadyPatched())
                {
                    MessageBox.Show("A patch is already installed on the specified target. Please restore a backup of your Assembly-CSharp.dll file or re-install the game.", "Installation detected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    BtnRestoreBackup.Enabled = await _patcher.CheckBackupExists(txtGamePath.Text);
                    MessageBox.Show("The patch has been successfully installed");
                }

                progressBarInstallation.Visible = false;
            }
            else
            {
                MessageBox.Show("Could not locate or load the target Assembly-CSharp.dll file.");
            }
        }

        private async void BtnBrowseGamePath_Click(object sender, EventArgs e)
        {
            browseGamePathDialog.SelectedPath = Path.GetDirectoryName(txtGamePath.Text);

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

                BtnRestoreBackup.Enabled = await _patcher.CheckBackupExists(txtGamePath.Text);

                btnInstallMod.Enabled = true;

                await DebugLog("Library file loaded");
            }
        }

        private async void BtnRestoreBackup_Click(object sender, EventArgs e)
        {
            if (!await _patcher.RestoreLatestBackup(txtGamePath.Text))
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

            BtnRestoreBackup.Enabled = await _patcher.CheckBackupExists(txtGamePath.Text);
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

        private void btnDeveloperOptions_Click(object sender, EventArgs e)
        {
            var x = Location.X + ((Control)sender).Location.X;
            var y = Location.Y + ((Control)sender).Location.Y;

            contextMenuStripDeveloperTools.Show(x, y);
        }
    }
}
