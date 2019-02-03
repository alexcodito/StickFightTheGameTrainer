namespace StickFightTheGameTrainer
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.BtnBrowseGamePath = new Telerik.WinControls.UI.RadButton();
            this.telerikMetroTheme = new Telerik.WinControls.Themes.TelerikMetroTheme();
            this.txtGamePath = new Telerik.WinControls.UI.RadTextBox();
            this.radLabel1 = new Telerik.WinControls.UI.RadLabel();
            this.BtnRestoreBackup = new Telerik.WinControls.UI.RadButton();
            this.btnInstallMod = new Telerik.WinControls.UI.RadButton();
            this.browseGamePathDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.btnDeveloperOptions = new Telerik.WinControls.UI.RadButton();
            this.contextMenuStripDeveloperTools = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.generateEncryptedSourcesAndKeysToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkBoxCreateBackup = new System.Windows.Forms.CheckBox();
            this.progressBarInstallation = new System.Windows.Forms.ProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this.BtnBrowseGamePath)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtGamePath)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radLabel1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BtnRestoreBackup)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnInstallMod)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnDeveloperOptions)).BeginInit();
            this.contextMenuStripDeveloperTools.SuspendLayout();
            this.SuspendLayout();
            // 
            // BtnBrowseGamePath
            // 
            this.BtnBrowseGamePath.Image = global::StickFightTheGameTrainer.Properties.Resources.image_win_folder;
            this.BtnBrowseGamePath.Location = new System.Drawing.Point(672, 31);
            this.BtnBrowseGamePath.Name = "BtnBrowseGamePath";
            this.BtnBrowseGamePath.Padding = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.BtnBrowseGamePath.Size = new System.Drawing.Size(83, 24);
            this.BtnBrowseGamePath.TabIndex = 1;
            this.BtnBrowseGamePath.Text = "Browse...";
            this.BtnBrowseGamePath.TextAlignment = System.Drawing.ContentAlignment.MiddleRight;
            this.BtnBrowseGamePath.ThemeName = "TelerikMetro";
            this.BtnBrowseGamePath.Click += new System.EventHandler(this.BtnBrowseGamePath_Click);
            // 
            // txtGamePath
            // 
            this.txtGamePath.Location = new System.Drawing.Point(10, 31);
            this.txtGamePath.MinimumSize = new System.Drawing.Size(0, 24);
            this.txtGamePath.Name = "txtGamePath";
            this.txtGamePath.Padding = new System.Windows.Forms.Padding(5, 1, 0, 0);
            // 
            // 
            // 
            this.txtGamePath.RootElement.MinSize = new System.Drawing.Size(0, 24);
            this.txtGamePath.Size = new System.Drawing.Size(653, 24);
            this.txtGamePath.TabIndex = 2;
            this.txtGamePath.Text = "C:\\";
            this.txtGamePath.TextChanged += new System.EventHandler(this.TxtGamePath_TextChanged);
            // 
            // radLabel1
            // 
            this.radLabel1.Location = new System.Drawing.Point(7, 7);
            this.radLabel1.Name = "radLabel1";
            this.radLabel1.Size = new System.Drawing.Size(64, 18);
            this.radLabel1.TabIndex = 3;
            this.radLabel1.Text = "Game path:";
            // 
            // BtnRestoreBackup
            // 
            this.BtnRestoreBackup.Enabled = false;
            this.BtnRestoreBackup.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtnRestoreBackup.Image = global::StickFightTheGameTrainer.Properties.Resources.image_arrows;
            this.BtnRestoreBackup.Location = new System.Drawing.Point(637, 65);
            this.BtnRestoreBackup.Name = "BtnRestoreBackup";
            this.BtnRestoreBackup.Padding = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.BtnRestoreBackup.Size = new System.Drawing.Size(118, 24);
            this.BtnRestoreBackup.TabIndex = 5;
            this.BtnRestoreBackup.Text = "Restore Backup";
            this.BtnRestoreBackup.TextAlignment = System.Drawing.ContentAlignment.MiddleRight;
            this.BtnRestoreBackup.ThemeName = "TelerikMetro";
            this.BtnRestoreBackup.Click += new System.EventHandler(this.BtnRestoreBackup_Click);
            // 
            // btnInstallMod
            // 
            this.btnInstallMod.Enabled = false;
            this.btnInstallMod.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnInstallMod.Image = global::StickFightTheGameTrainer.Properties.Resources.image_plus;
            this.btnInstallMod.Location = new System.Drawing.Point(10, 65);
            this.btnInstallMod.Name = "btnInstallMod";
            this.btnInstallMod.Padding = new System.Windows.Forms.Padding(5, 0, 5, 1);
            this.btnInstallMod.Size = new System.Drawing.Size(93, 24);
            this.btnInstallMod.TabIndex = 4;
            this.btnInstallMod.Text = "Install Mod";
            this.btnInstallMod.TextAlignment = System.Drawing.ContentAlignment.MiddleRight;
            this.btnInstallMod.ThemeName = "TelerikMetro";
            this.btnInstallMod.Click += new System.EventHandler(this.BtnInstallMod_Click);
            // 
            // browseGamePathDialog
            // 
            this.browseGamePathDialog.Description = "Stick Fight: The Game installation path";
            this.browseGamePathDialog.RootFolder = System.Environment.SpecialFolder.MyComputer;
            this.browseGamePathDialog.SelectedPath = "C:\\";
            // 
            // btnDeveloperOptions
            // 
            this.btnDeveloperOptions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnDeveloperOptions.ContextMenuStrip = this.contextMenuStripDeveloperTools;
            this.btnDeveloperOptions.ForeColor = System.Drawing.Color.Lime;
            this.btnDeveloperOptions.Location = new System.Drawing.Point(533, 65);
            this.btnDeveloperOptions.Name = "btnDeveloperOptions";
            this.btnDeveloperOptions.Size = new System.Drawing.Size(98, 24);
            this.btnDeveloperOptions.TabIndex = 6;
            this.btnDeveloperOptions.Text = "Dev. Options";
            this.btnDeveloperOptions.ThemeName = "TelerikMetro";
            this.btnDeveloperOptions.Visible = false;
            this.btnDeveloperOptions.Click += new System.EventHandler(this.btnDeveloperOptions_Click);
            // 
            // contextMenuStripDeveloperTools
            // 
            this.contextMenuStripDeveloperTools.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.generateEncryptedSourcesAndKeysToolStripMenuItem});
            this.contextMenuStripDeveloperTools.Name = "contextMenuStrip1";
            this.contextMenuStripDeveloperTools.Size = new System.Drawing.Size(336, 26);
            // 
            // generateEncryptedSourcesAndKeysToolStripMenuItem
            // 
            this.generateEncryptedSourcesAndKeysToolStripMenuItem.Image = global::StickFightTheGameTrainer.Properties.Resources.image_key;
            this.generateEncryptedSourcesAndKeysToolStripMenuItem.Name = "generateEncryptedSourcesAndKeysToolStripMenuItem";
            this.generateEncryptedSourcesAndKeysToolStripMenuItem.Size = new System.Drawing.Size(335, 22);
            this.generateEncryptedSourcesAndKeysToolStripMenuItem.Text = "Generate encrypted Logic Module sources && keys";
            this.generateEncryptedSourcesAndKeysToolStripMenuItem.Click += new System.EventHandler(this.GenerateEncryptedSourcesAndKeysToolStripMenuItem_ClickAsync);
            // 
            // checkBoxCreateBackup
            // 
            this.checkBoxCreateBackup.AutoSize = true;
            this.checkBoxCreateBackup.Checked = true;
            this.checkBoxCreateBackup.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxCreateBackup.Location = new System.Drawing.Point(109, 69);
            this.checkBoxCreateBackup.Name = "checkBoxCreateBackup";
            this.checkBoxCreateBackup.Size = new System.Drawing.Size(100, 17);
            this.checkBoxCreateBackup.TabIndex = 7;
            this.checkBoxCreateBackup.Text = "Create Backup";
            this.checkBoxCreateBackup.UseVisualStyleBackColor = true;
            // 
            // progressBarInstallation
            // 
            this.progressBarInstallation.Location = new System.Drawing.Point(215, 72);
            this.progressBarInstallation.Name = "progressBarInstallation";
            this.progressBarInstallation.Size = new System.Drawing.Size(100, 11);
            this.progressBarInstallation.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBarInstallation.TabIndex = 8;
            this.progressBarInstallation.Visible = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(765, 99);
            this.Controls.Add(this.progressBarInstallation);
            this.Controls.Add(this.checkBoxCreateBackup);
            this.Controls.Add(this.btnDeveloperOptions);
            this.Controls.Add(this.BtnRestoreBackup);
            this.Controls.Add(this.btnInstallMod);
            this.Controls.Add(this.radLabel1);
            this.Controls.Add(this.txtGamePath);
            this.Controls.Add(this.BtnBrowseGamePath);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(781, 138);
            this.Name = "MainForm";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Stick Fight: The Game - Trainer by loxa";
            ((System.ComponentModel.ISupportInitialize)(this.BtnBrowseGamePath)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtGamePath)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radLabel1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BtnRestoreBackup)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnInstallMod)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnDeveloperOptions)).EndInit();
            this.contextMenuStripDeveloperTools.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private Telerik.WinControls.UI.RadButton BtnBrowseGamePath;
        private Telerik.WinControls.Themes.TelerikMetroTheme telerikMetroTheme;
        private Telerik.WinControls.UI.RadTextBox txtGamePath;
        private Telerik.WinControls.UI.RadLabel radLabel1;
        private Telerik.WinControls.UI.RadButton btnInstallMod;
        private Telerik.WinControls.UI.RadButton BtnRestoreBackup;
        private System.Windows.Forms.FolderBrowserDialog browseGamePathDialog;
        private Telerik.WinControls.UI.RadButton btnDeveloperOptions;
        private System.Windows.Forms.CheckBox checkBoxCreateBackup;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripDeveloperTools;
        private System.Windows.Forms.ToolStripMenuItem generateEncryptedSourcesAndKeysToolStripMenuItem;
        private System.Windows.Forms.ProgressBar progressBarInstallation;
    }
}

