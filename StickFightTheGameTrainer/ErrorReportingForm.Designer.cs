namespace StickFightTheGameTrainer
{
    partial class ErrorReportingForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ErrorReportingForm));
            this.labelDisclaimer = new Telerik.WinControls.UI.RadLabel();
            this.linkLabelModdb = new System.Windows.Forms.LinkLabel();
            this.labelDisclaimer2 = new Telerik.WinControls.UI.RadLabel();
            this.btnCopyToClipboard = new Telerik.WinControls.UI.RadButton();
            this.radRichTextEditorErrorLog = new System.Windows.Forms.RichTextBox();
            this.btnDecLog = new Telerik.WinControls.UI.RadButton();
            ((System.ComponentModel.ISupportInitialize)(this.labelDisclaimer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.labelDisclaimer2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnCopyToClipboard)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnDecLog)).BeginInit();
            this.SuspendLayout();
            // 
            // labelDisclaimer
            // 
            this.labelDisclaimer.Location = new System.Drawing.Point(11, 12);
            this.labelDisclaimer.Name = "labelDisclaimer";
            this.labelDisclaimer.Size = new System.Drawing.Size(245, 18);
            this.labelDisclaimer.TabIndex = 0;
            this.labelDisclaimer.Text = "An error has occured while installing the trainer.";
            // 
            // linkLabelModdb
            // 
            this.linkLabelModdb.AutoSize = true;
            this.linkLabelModdb.Location = new System.Drawing.Point(329, 38);
            this.linkLabelModdb.Name = "linkLabelModdb";
            this.linkLabelModdb.Size = new System.Drawing.Size(196, 13);
            this.linkLabelModdb.TabIndex = 1;
            this.linkLabelModdb.TabStop = true;
            this.linkLabelModdb.Text = "https://www.moddb.com/members/loxa";
            this.linkLabelModdb.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelModdb_LinkClicked);
            // 
            // labelDisclaimer2
            // 
            this.labelDisclaimer2.Location = new System.Drawing.Point(11, 36);
            this.labelDisclaimer2.Name = "labelDisclaimer2";
            this.labelDisclaimer2.Size = new System.Drawing.Size(313, 18);
            this.labelDisclaimer2.TabIndex = 2;
            this.labelDisclaimer2.Text = "Please send a private message with the following error log to:";
            // 
            // btnCopyToClipboard
            // 
            this.btnCopyToClipboard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCopyToClipboard.Location = new System.Drawing.Point(12, 415);
            this.btnCopyToClipboard.Name = "btnCopyToClipboard";
            this.btnCopyToClipboard.Size = new System.Drawing.Size(110, 24);
            this.btnCopyToClipboard.TabIndex = 4;
            this.btnCopyToClipboard.Text = "Copy to clipboard";
            this.btnCopyToClipboard.Click += new System.EventHandler(this.btnCopyToClipboard_Click);
            // 
            // radRichTextEditorErrorLog
            // 
            this.radRichTextEditorErrorLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.radRichTextEditorErrorLog.BackColor = System.Drawing.Color.White;
            this.radRichTextEditorErrorLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.radRichTextEditorErrorLog.HideSelection = false;
            this.radRichTextEditorErrorLog.Location = new System.Drawing.Point(12, 60);
            this.radRichTextEditorErrorLog.Name = "radRichTextEditorErrorLog";
            this.radRichTextEditorErrorLog.ReadOnly = true;
            this.radRichTextEditorErrorLog.Size = new System.Drawing.Size(515, 343);
            this.radRichTextEditorErrorLog.TabIndex = 5;
            this.radRichTextEditorErrorLog.Text = "";
            // 
            // btnDecLog
            // 
            this.btnDecLog.Location = new System.Drawing.Point(128, 415);
            this.btnDecLog.Name = "btnDecLog";
            this.btnDecLog.Size = new System.Drawing.Size(63, 24);
            this.btnDecLog.TabIndex = 6;
            this.btnDecLog.Text = "Decrypt";
            this.btnDecLog.Visible = false;
            this.btnDecLog.Click += new System.EventHandler(this.btnDecLog_Click);
            // 
            // ErrorReportingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(538, 450);
            this.Controls.Add(this.btnDecLog);
            this.Controls.Add(this.radRichTextEditorErrorLog);
            this.Controls.Add(this.btnCopyToClipboard);
            this.Controls.Add(this.labelDisclaimer2);
            this.Controls.Add(this.linkLabelModdb);
            this.Controls.Add(this.labelDisclaimer);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ErrorReportingForm";
            this.Text = "Error report";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ErrorReportingForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.labelDisclaimer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.labelDisclaimer2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnCopyToClipboard)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnDecLog)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Telerik.WinControls.UI.RadLabel labelDisclaimer;
        private System.Windows.Forms.LinkLabel linkLabelModdb;
        private Telerik.WinControls.UI.RadLabel labelDisclaimer2;
        private Telerik.WinControls.UI.RadButton btnCopyToClipboard;
        private System.Windows.Forms.RichTextBox radRichTextEditorErrorLog;
        private Telerik.WinControls.UI.RadButton btnDecLog;
    }
}