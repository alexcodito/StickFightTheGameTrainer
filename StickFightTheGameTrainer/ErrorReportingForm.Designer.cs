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
            this.linkLabelModdb = new System.Windows.Forms.LinkLabel();
            this.radRichTextEditorErrorLog = new System.Windows.Forms.RichTextBox();
            this.labelDisclaimer = new System.Windows.Forms.Label();
            this.labelDisclaimer2 = new System.Windows.Forms.Label();
            this.btnDecLog = new System.Windows.Forms.Button();
            this.btnCopyToClipboard = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // linkLabelModdb
            // 
            this.linkLabelModdb.AutoSize = true;
            this.linkLabelModdb.Location = new System.Drawing.Point(336, 38);
            this.linkLabelModdb.Name = "linkLabelModdb";
            this.linkLabelModdb.Size = new System.Drawing.Size(196, 13);
            this.linkLabelModdb.TabIndex = 1;
            this.linkLabelModdb.TabStop = true;
            this.linkLabelModdb.Text = "https://www.moddb.com/members/loxa";
            this.linkLabelModdb.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabelModdb_LinkClicked);
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
            // labelDisclaimer
            // 
            this.labelDisclaimer.AutoSize = true;
            this.labelDisclaimer.BackColor = System.Drawing.SystemColors.Control;
            this.labelDisclaimer.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.labelDisclaimer.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.labelDisclaimer.Location = new System.Drawing.Point(11, 14);
            this.labelDisclaimer.Name = "labelDisclaimer";
            this.labelDisclaimer.Size = new System.Drawing.Size(256, 13);
            this.labelDisclaimer.TabIndex = 7;
            this.labelDisclaimer.Text = "An error has occured while installing the trainer.";
            // 
            // labelDisclaimer2
            // 
            this.labelDisclaimer2.AutoSize = true;
            this.labelDisclaimer2.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.labelDisclaimer2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.labelDisclaimer2.Location = new System.Drawing.Point(11, 38);
            this.labelDisclaimer2.Name = "labelDisclaimer2";
            this.labelDisclaimer2.Size = new System.Drawing.Size(325, 13);
            this.labelDisclaimer2.TabIndex = 8;
            this.labelDisclaimer2.Text = "Please send a private message with the following error log to:";
            // 
            // btnDecLog
            // 
            this.btnDecLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDecLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(227)))), ((int)(((byte)(249)))));
            this.btnDecLog.BackgroundImage = global::StickFightTheGameTrainer.Properties.Resources.image_button_background;
            this.btnDecLog.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnDecLog.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(148)))), ((int)(((byte)(186)))));
            this.btnDecLog.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDecLog.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.btnDecLog.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(21)))), ((int)(((byte)(66)))), ((int)(((byte)(139)))));
            this.btnDecLog.Location = new System.Drawing.Point(140, 415);
            this.btnDecLog.Name = "btnDecLog";
            this.btnDecLog.Size = new System.Drawing.Size(63, 24);
            this.btnDecLog.TabIndex = 9;
            this.btnDecLog.Text = "Decrypt";
            this.btnDecLog.UseVisualStyleBackColor = false;
            this.btnDecLog.Click += new System.EventHandler(this.BtnDecLog_Click);
            this.btnDecLog.MouseDown += new System.Windows.Forms.MouseEventHandler(this.BtnMouseDown);
            this.btnDecLog.MouseEnter += new System.EventHandler(this.BtnMouseEnter);
            this.btnDecLog.MouseLeave += new System.EventHandler(this.BtnMouseLeave);
            this.btnDecLog.MouseUp += new System.Windows.Forms.MouseEventHandler(this.BtnMouseUp);
            // 
            // btnCopyToClipboard
            // 
            this.btnCopyToClipboard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCopyToClipboard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(227)))), ((int)(((byte)(249)))));
            this.btnCopyToClipboard.BackgroundImage = global::StickFightTheGameTrainer.Properties.Resources.image_button_background;
            this.btnCopyToClipboard.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnCopyToClipboard.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(148)))), ((int)(((byte)(186)))));
            this.btnCopyToClipboard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCopyToClipboard.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.btnCopyToClipboard.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(21)))), ((int)(((byte)(66)))), ((int)(((byte)(139)))));
            this.btnCopyToClipboard.Location = new System.Drawing.Point(12, 415);
            this.btnCopyToClipboard.Name = "btnCopyToClipboard";
            this.btnCopyToClipboard.Size = new System.Drawing.Size(116, 24);
            this.btnCopyToClipboard.TabIndex = 10;
            this.btnCopyToClipboard.Text = "Copy to clipboard";
            this.btnCopyToClipboard.UseVisualStyleBackColor = false;
            this.btnCopyToClipboard.Click += new System.EventHandler(this.BtnCopyToClipboard_Click);
            this.btnCopyToClipboard.MouseDown += new System.Windows.Forms.MouseEventHandler(this.BtnMouseDown);
            this.btnCopyToClipboard.MouseEnter += new System.EventHandler(this.BtnMouseEnter);
            this.btnCopyToClipboard.MouseLeave += new System.EventHandler(this.BtnMouseLeave);
            this.btnCopyToClipboard.MouseUp += new System.Windows.Forms.MouseEventHandler(this.BtnMouseUp);
            // 
            // ErrorReportingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(538, 450);
            this.Controls.Add(this.btnCopyToClipboard);
            this.Controls.Add(this.btnDecLog);
            this.Controls.Add(this.labelDisclaimer2);
            this.Controls.Add(this.labelDisclaimer);
            this.Controls.Add(this.radRichTextEditorErrorLog);
            this.Controls.Add(this.linkLabelModdb);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(554, 489);
            this.Name = "ErrorReportingForm";
            this.Text = "Error report";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ErrorReportingForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.LinkLabel linkLabelModdb;
        private System.Windows.Forms.RichTextBox radRichTextEditorErrorLog;
        private System.Windows.Forms.Label labelDisclaimer2;
        private System.Windows.Forms.Label labelDisclaimer;
        private System.Windows.Forms.Button btnDecLog;
        private System.Windows.Forms.Button btnCopyToClipboard;
    }
}