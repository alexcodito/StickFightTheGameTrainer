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
            this.browseGamePathDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.contextMenuStripDeveloperTools = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.generateEncryptedSourcesAndKeysToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkBoxCreateBackup = new System.Windows.Forms.CheckBox();
            this.progressBarInstallation = new System.Windows.Forms.ProgressBar();
            this.btnInstallMod = new System.Windows.Forms.Button();
            this.btnRestoreBackup = new System.Windows.Forms.Button();
            this.btnDeveloperOptions = new System.Windows.Forms.Button();
            this.lblGamePath = new System.Windows.Forms.Label();
            this.btnBrowseGamePath = new System.Windows.Forms.Button();
            this.panelGamePath = new StickFightTheGameTrainer.Controls.BorderedPanel();
            this.txtGamePath = new System.Windows.Forms.TextBox();
            this.contextMenuStripDeveloperTools.SuspendLayout();
            this.panelGamePath.SuspendLayout();
            this.SuspendLayout();
            // 
            // browseGamePathDialog
            // 
            this.browseGamePathDialog.Description = "Stick Fight: The Game installation path";
            this.browseGamePathDialog.RootFolder = System.Environment.SpecialFolder.MyComputer;
            this.browseGamePathDialog.SelectedPath = "C:\\";
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
            this.checkBoxCreateBackup.Location = new System.Drawing.Point(109, 70);
            this.checkBoxCreateBackup.Name = "checkBoxCreateBackup";
            this.checkBoxCreateBackup.Size = new System.Drawing.Size(99, 17);
            this.checkBoxCreateBackup.TabIndex = 7;
            this.checkBoxCreateBackup.Text = "Create Backup";
            this.checkBoxCreateBackup.UseVisualStyleBackColor = true;
            // 
            // progressBarInstallation
            // 
            this.progressBarInstallation.Location = new System.Drawing.Point(215, 73);
            this.progressBarInstallation.Name = "progressBarInstallation";
            this.progressBarInstallation.Size = new System.Drawing.Size(100, 11);
            this.progressBarInstallation.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBarInstallation.TabIndex = 8;
            this.progressBarInstallation.Visible = false;
            // 
            // btnInstallMod
            // 
            this.btnInstallMod.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btnInstallMod.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.btnInstallMod.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
            this.btnInstallMod.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnInstallMod.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnInstallMod.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.btnInstallMod.Image = global::StickFightTheGameTrainer.Properties.Resources.image_plus;
            this.btnInstallMod.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnInstallMod.Location = new System.Drawing.Point(10, 65);
            this.btnInstallMod.Name = "btnInstallMod";
            this.btnInstallMod.Size = new System.Drawing.Size(93, 26);
            this.btnInstallMod.TabIndex = 9;
            this.btnInstallMod.Text = "Install Mod";
            this.btnInstallMod.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnInstallMod.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnInstallMod.UseVisualStyleBackColor = true;
            this.btnInstallMod.Click += new System.EventHandler(this.BtnInstallMod_Click);
            // 
            // btnRestoreBackup
            // 
            this.btnRestoreBackup.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btnRestoreBackup.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.btnRestoreBackup.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
            this.btnRestoreBackup.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRestoreBackup.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnRestoreBackup.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.btnRestoreBackup.Image = global::StickFightTheGameTrainer.Properties.Resources.image_arrows;
            this.btnRestoreBackup.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnRestoreBackup.Location = new System.Drawing.Point(637, 65);
            this.btnRestoreBackup.Name = "btnRestoreBackup";
            this.btnRestoreBackup.Size = new System.Drawing.Size(118, 26);
            this.btnRestoreBackup.TabIndex = 10;
            this.btnRestoreBackup.Text = "Restore Backup";
            this.btnRestoreBackup.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnRestoreBackup.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnRestoreBackup.UseVisualStyleBackColor = true;
            this.btnRestoreBackup.Click += new System.EventHandler(this.BtnRestoreBackup_Click);
            // 
            // btnDeveloperOptions
            // 
            this.btnDeveloperOptions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnDeveloperOptions.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btnDeveloperOptions.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.btnDeveloperOptions.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.btnDeveloperOptions.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDeveloperOptions.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnDeveloperOptions.ForeColor = System.Drawing.Color.Lime;
            this.btnDeveloperOptions.Location = new System.Drawing.Point(538, 65);
            this.btnDeveloperOptions.Name = "btnDeveloperOptions";
            this.btnDeveloperOptions.Size = new System.Drawing.Size(93, 26);
            this.btnDeveloperOptions.TabIndex = 11;
            this.btnDeveloperOptions.Text = "Dev. Options";
            this.btnDeveloperOptions.UseVisualStyleBackColor = false;
            this.btnDeveloperOptions.Visible = false;
            this.btnDeveloperOptions.Click += new System.EventHandler(this.BtnDeveloperOptions_Click);
            // 
            // lblGamePath
            // 
            this.lblGamePath.AutoSize = true;
            this.lblGamePath.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.lblGamePath.Location = new System.Drawing.Point(7, 9);
            this.lblGamePath.Name = "lblGamePath";
            this.lblGamePath.Size = new System.Drawing.Size(66, 13);
            this.lblGamePath.TabIndex = 13;
            this.lblGamePath.Text = "Game path:";
            this.lblGamePath.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnBrowseGamePath
            // 
            this.btnBrowseGamePath.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btnBrowseGamePath.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.btnBrowseGamePath.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
            this.btnBrowseGamePath.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBrowseGamePath.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnBrowseGamePath.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.btnBrowseGamePath.Image = global::StickFightTheGameTrainer.Properties.Resources.image_win_folder;
            this.btnBrowseGamePath.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnBrowseGamePath.Location = new System.Drawing.Point(672, 31);
            this.btnBrowseGamePath.Name = "btnBrowseGamePath";
            this.btnBrowseGamePath.Size = new System.Drawing.Size(83, 25);
            this.btnBrowseGamePath.TabIndex = 14;
            this.btnBrowseGamePath.Text = "Browse...";
            this.btnBrowseGamePath.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnBrowseGamePath.UseVisualStyleBackColor = true;
            this.btnBrowseGamePath.Click += new System.EventHandler(this.BtnBrowseGamePath_Click);
            // 
            // panelGamePath
            // 
            this.panelGamePath.Controls.Add(this.txtGamePath);
            this.panelGamePath.Location = new System.Drawing.Point(10, 31);
            this.panelGamePath.Name = "panelGamePath";
            this.panelGamePath.Padding = new System.Windows.Forms.Padding(5, 5, 4, 4);
            this.panelGamePath.Size = new System.Drawing.Size(653, 24);
            this.panelGamePath.TabIndex = 12;
            // 
            // txtGamePath
            // 
            this.txtGamePath.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtGamePath.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtGamePath.Location = new System.Drawing.Point(5, 5);
            this.txtGamePath.Margin = new System.Windows.Forms.Padding(0);
            this.txtGamePath.Name = "txtGamePath";
            this.txtGamePath.Size = new System.Drawing.Size(644, 15);
            this.txtGamePath.TabIndex = 0;
            this.txtGamePath.Text = "C:\\";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(765, 100);
            this.Controls.Add(this.btnBrowseGamePath);
            this.Controls.Add(this.lblGamePath);
            this.Controls.Add(this.panelGamePath);
            this.Controls.Add(this.btnDeveloperOptions);
            this.Controls.Add(this.btnRestoreBackup);
            this.Controls.Add(this.btnInstallMod);
            this.Controls.Add(this.progressBarInstallation);
            this.Controls.Add(this.checkBoxCreateBackup);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(781, 139);
            this.MinimumSize = new System.Drawing.Size(781, 139);
            this.Name = "MainForm";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Stick Fight: The Game - Trainer by loxa";
            this.contextMenuStripDeveloperTools.ResumeLayout(false);
            this.panelGamePath.ResumeLayout(false);
            this.panelGamePath.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.FolderBrowserDialog browseGamePathDialog;
        private System.Windows.Forms.CheckBox checkBoxCreateBackup;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripDeveloperTools;
        private System.Windows.Forms.ToolStripMenuItem generateEncryptedSourcesAndKeysToolStripMenuItem;
        private System.Windows.Forms.ProgressBar progressBarInstallation;
        private System.Windows.Forms.Button btnInstallMod;
        private System.Windows.Forms.Button btnRestoreBackup;
        private System.Windows.Forms.Button btnDeveloperOptions;
        private Controls.BorderedPanel panelGamePath;
        private System.Windows.Forms.TextBox txtGamePath;
        private System.Windows.Forms.Label lblGamePath;
        private System.Windows.Forms.Button btnBrowseGamePath;
    }
}

