using System.Windows.Forms;

namespace StickFightTheGameTrainer
{
    partial class DebugForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DebugForm));
            this.txtDebugLog = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // txtDebugLog
            // 
            this.txtDebugLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.txtDebugLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtDebugLog.ForeColor = System.Drawing.Color.White;
            this.txtDebugLog.Location = new System.Drawing.Point(2, 2);
            this.txtDebugLog.Name = "txtDebugLog";
            this.txtDebugLog.ReadOnly = true;
            this.txtDebugLog.Size = new System.Drawing.Size(547, 247);
            this.txtDebugLog.TabIndex = 0;
            this.txtDebugLog.Text = "";
            this.txtDebugLog.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DebugForm_MouseDown);
            // 
            // DebugForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(551, 251);
            this.ControlBox = false;
            this.Controls.Add(this.txtDebugLog);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DebugForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Debug";
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DebugForm_MouseDown);
            this.ResumeLayout(false);

        }

        #endregion

        private RichTextBox txtDebugLog;
    }
}