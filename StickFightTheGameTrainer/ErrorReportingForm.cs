using StickFightTheGameTrainer.Common;
using StickFightTheGameTrainer.Trainer.TrainerLogic;
using System;
using System.Linq;
using System.Windows.Forms;

namespace StickFightTheGameTrainer
{
    public partial class ErrorReportingForm : Form
    {
        private readonly ILogger _logger;

        public ErrorReportingForm(ILogger logger)
        {
            InitializeComponent();

            _logger = logger;

#if DEBUG
            btnDecLog.Visible = true;
#endif
        }

        public void GenerateLogReport()
        {
            var logs = _logger.GetLogs();
            var formattedLogs = string.Join(Environment.NewLine, logs.Select(log => log.ToString()));
            var encryptedLogs = AesUtility.EncryptStringToBase64String(formattedLogs, TrainerLogicModule.ModuleDataPrivatesDictionary["Key"], TrainerLogicModule.ModuleDataPrivatesDictionary["Iv"]);

            radRichTextEditorErrorLog.Text = encryptedLogs;
        }

        private void DecryptLogReport()
        {
            var decryptedLogs = AesUtility.DecryptStringFromBase64String(radRichTextEditorErrorLog.Text, TrainerLogicModule.ModuleDataPrivatesDictionary["Key"], TrainerLogicModule.ModuleDataPrivatesDictionary["Iv"]);
            MessageBox.Show(decryptedLogs);
        }

        private void btnCopyToClipboard_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(radRichTextEditorErrorLog.Text);
            MessageBox.Show(@"Successfully copied to the clipboard");
        }

        private void linkLabelModdb_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabelModdb.LinkVisited = true;
            System.Diagnostics.Process.Start(linkLabelModdb.Text);
        }

        private void btnDecLog_Click(object sender, EventArgs e)
        {
            DecryptLogReport();
        }

        private void ErrorReportingForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
