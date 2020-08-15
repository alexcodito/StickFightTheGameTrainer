using System;
using StickFightTheGameTrainer.Common;
using System.Windows.Forms;

namespace StickFightTheGameTrainer
{
    public partial class DebugForm : Form
    {
        private readonly ILogger _logger;

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        public DebugForm(ILogger logger)
        {
            InitializeComponent();
            _logger = logger;
            _logger.NewLogEvent += NewEventLogHandler;
            _logger.ClearLogsEvent += () => txtDebugLog.Clear();
        }

        private void NewEventLogHandler(LogMessage logMessage)
        {
            var initialLength = txtDebugLog.TextLength;
            var formattedMessage = $@"- [{logMessage.DateCreated:G}]: {logMessage.Message}{Environment.NewLine}";

            txtDebugLog.AppendText(formattedMessage);

            txtDebugLog.Select(initialLength, formattedMessage.Length);

            if (logMessage.LogLevel == LogLevel.Info)
            {
                txtDebugLog.SelectionColor = System.Drawing.Color.White;
            }
            else if (logMessage.LogLevel == LogLevel.Warning)
            {
                txtDebugLog.SelectionColor = System.Drawing.Color.Yellow;
            }
            else if (logMessage.LogLevel == LogLevel.Error)
            {
                txtDebugLog.SelectionColor = System.Drawing.Color.Red;
            }
        }

        private void DebugForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
    }
}
