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
            radRichTextEditorErrorLog.ReadOnly = false;
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
            try
            {
                var decryptedLogs = AesUtility.DecryptStringFromBase64String(radRichTextEditorErrorLog.Text, TrainerLogicModule.ModuleDataPrivatesDictionary["Key"], TrainerLogicModule.ModuleDataPrivatesDictionary["Iv"]);
                radRichTextEditorErrorLog.Text = decryptedLogs;
            } 
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Could not decrypt or display the error log.");
            }
        }

        private void BtnCopyToClipboard_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(radRichTextEditorErrorLog.Text);
            MessageBox.Show(@"Successfully copied to the clipboard");
        }

        private void LinkLabelModdb_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabelModdb.LinkVisited = true;
            System.Diagnostics.Process.Start(linkLabelModdb.Text);
        }

        private void BtnDecLog_Click(object sender, EventArgs e)
        {
            DecryptLogReport();
        }

        private void ErrorReportingForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void BtnMouseEnter(object sender, EventArgs e)
        {
            var btnSender = (Button)sender;
            btnSender.BackgroundImageLayout = ImageLayout.Tile;
        }

        private void BtnMouseLeave(object sender, EventArgs e)
        {
            var btnSender = (Button)sender;
            btnSender.BackgroundImageLayout = ImageLayout.Stretch;
        }

        private void BtnMouseDown(object sender, MouseEventArgs e)
        {
            var btnSender = (Button)sender;
            btnSender.Tag = btnSender.BackgroundImage;
            btnSender.BackgroundImage = null;
        }

        private void BtnMouseUp(object sender, MouseEventArgs e)
        {
            var btnSender = (Button)sender;
            btnSender.BackgroundImage = (System.Drawing.Image)btnSender.Tag;
            btnSender.Tag = null;
        }
    }
}
