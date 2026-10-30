namespace tempWork
{
    partial class FrmMain
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
            this.panelBG = new System.Windows.Forms.Panel();
            this.th4 = new Controls.THMonitor();
            this.th2 = new Controls.THMonitor();
            this.th3 = new Controls.THMonitor();
            this.th1 = new Controls.THMonitor();
            this.colorZone = new System.Windows.Forms.Panel();
            this.btnExit = new System.Windows.Forms.Panel();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnHistory = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnWarn = new System.Windows.Forms.Panel();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btnOption = new System.Windows.Forms.Panel();
            this.img1 = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.imgLogo = new System.Windows.Forms.PictureBox();
            this.panelBG.SuspendLayout();
            this.colorZone.SuspendLayout();
            this.btnExit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.btnHistory.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.btnWarn.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            this.btnOption.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.img1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // panelBG
            // 
            this.panelBG.Controls.Add(this.th4);
            this.panelBG.Controls.Add(this.th2);
            this.panelBG.Controls.Add(this.th3);
            this.panelBG.Controls.Add(this.th1);
            this.panelBG.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBG.Location = new System.Drawing.Point(0, 0);
            this.panelBG.Name = "panelBG";
            this.panelBG.Size = new System.Drawing.Size(871, 650);
            this.panelBG.TabIndex = 0;
            // 
            // th4
            // 
            this.th4.Location = new System.Drawing.Point(458, 376);
            this.th4.MoistValue = 0;
            this.th4.MoistWarning = 45;
            this.th4.Name = "th4";
            this.th4.Size = new System.Drawing.Size(399, 234);
            this.th4.TabIndex = 0;
            this.th4.TemperatureValue = 0;
            this.th4.TemperatureWarning = 40;
            this.th4.TitleNum = 4;
            // 
            // th2
            // 
            this.th2.Location = new System.Drawing.Point(458, 109);
            this.th2.MoistValue = 0;
            this.th2.MoistWarning = 45;
            this.th2.Name = "th2";
            this.th2.Size = new System.Drawing.Size(399, 234);
            this.th2.TabIndex = 0;
            this.th2.TemperatureValue = 0;
            this.th2.TemperatureWarning = 40;
            this.th2.TitleNum = 2;
            // 
            // th3
            // 
            this.th3.Location = new System.Drawing.Point(12, 376);
            this.th3.MoistValue = 0;
            this.th3.MoistWarning = 45;
            this.th3.Name = "th3";
            this.th3.Size = new System.Drawing.Size(399, 234);
            this.th3.TabIndex = 0;
            this.th3.TemperatureValue = 0;
            this.th3.TemperatureWarning = 40;
            this.th3.TitleNum = 3;
            // 
            // th1
            // 
            this.th1.Location = new System.Drawing.Point(12, 109);
            this.th1.MoistValue = 0;
            this.th1.MoistWarning = 45;
            this.th1.Name = "th1";
            this.th1.Size = new System.Drawing.Size(399, 234);
            this.th1.TabIndex = 0;
            this.th1.TemperatureValue = 0;
            this.th1.TemperatureWarning = 40;
            this.th1.TitleNum = 1;
            // 
            // colorZone
            // 
            this.colorZone.BackColor = System.Drawing.Color.Gray;
            this.colorZone.Controls.Add(this.btnExit);
            this.colorZone.Controls.Add(this.btnHistory);
            this.colorZone.Controls.Add(this.btnWarn);
            this.colorZone.Controls.Add(this.btnOption);
            this.colorZone.Controls.Add(this.label1);
            this.colorZone.Controls.Add(this.lblTitle);
            this.colorZone.Controls.Add(this.imgLogo);
            this.colorZone.Dock = System.Windows.Forms.DockStyle.Top;
            this.colorZone.Location = new System.Drawing.Point(0, 0);
            this.colorZone.Name = "colorZone";
            this.colorZone.Size = new System.Drawing.Size(871, 90);
            this.colorZone.TabIndex = 0;
            // 
            // btnExit
            // 
            this.btnExit.Controls.Add(this.pictureBox2);
            this.btnExit.Controls.Add(this.label4);
            this.btnExit.Location = new System.Drawing.Point(790, 8);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(75, 75);
            this.btnExit.TabIndex = 2;
            // 
            // pictureBox2
            // 
            this.pictureBox2.BackgroundImage = global::tempWork.Properties.Resources.退出;
            this.pictureBox2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pictureBox2.Dock = System.Windows.Forms.DockStyle.Top;
            this.pictureBox2.Location = new System.Drawing.Point(0, 0);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(75, 50);
            this.pictureBox2.TabIndex = 0;
            this.pictureBox2.TabStop = false;
            // 
            // label4
            // 
            this.label4.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.label4.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.label4.Location = new System.Drawing.Point(0, 58);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(75, 17);
            this.label4.TabIndex = 1;
            this.label4.Text = "退出系统";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnHistory
            // 
            this.btnHistory.Controls.Add(this.pictureBox1);
            this.btnHistory.Controls.Add(this.label3);
            this.btnHistory.Location = new System.Drawing.Point(690, 8);
            this.btnHistory.Name = "btnHistory";
            this.btnHistory.Size = new System.Drawing.Size(75, 75);
            this.btnHistory.TabIndex = 2;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImage = global::tempWork.Properties.Resources.待办;
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(75, 50);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // label3
            // 
            this.label3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.label3.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.label3.Location = new System.Drawing.Point(0, 58);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(75, 17);
            this.label3.TabIndex = 1;
            this.label3.Text = "历史查询";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnWarn
            // 
            this.btnWarn.Controls.Add(this.pictureBox3);
            this.btnWarn.Controls.Add(this.label5);
            this.btnWarn.Location = new System.Drawing.Point(590, 8);
            this.btnWarn.Name = "btnWarn";
            this.btnWarn.Size = new System.Drawing.Size(75, 75);
            this.btnWarn.TabIndex = 2;
            // 
            // pictureBox3
            // 
            this.pictureBox3.BackgroundImage = global::tempWork.Properties.Resources.安全;
            this.pictureBox3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pictureBox3.Dock = System.Windows.Forms.DockStyle.Top;
            this.pictureBox3.Location = new System.Drawing.Point(0, 0);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(75, 50);
            this.pictureBox3.TabIndex = 0;
            this.pictureBox3.TabStop = false;
            // 
            // label5
            // 
            this.label5.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.label5.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label5.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.label5.Location = new System.Drawing.Point(0, 58);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(75, 17);
            this.label5.TabIndex = 1;
            this.label5.Text = "安全设置";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnOption
            // 
            this.btnOption.Controls.Add(this.img1);
            this.btnOption.Controls.Add(this.label2);
            this.btnOption.Location = new System.Drawing.Point(490, 8);
            this.btnOption.Name = "btnOption";
            this.btnOption.Size = new System.Drawing.Size(75, 75);
            this.btnOption.TabIndex = 2;
            // 
            // img1
            // 
            this.img1.BackgroundImage = global::tempWork.Properties.Resources.参数设置;
            this.img1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.img1.Dock = System.Windows.Forms.DockStyle.Top;
            this.img1.Location = new System.Drawing.Point(0, 0);
            this.img1.Name = "img1";
            this.img1.Size = new System.Drawing.Size(75, 50);
            this.img1.TabIndex = 0;
            this.img1.TabStop = false;
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.label2.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.label2.Location = new System.Drawing.Point(0, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 17);
            this.label2.TabIndex = 1;
            this.label2.Text = "参数设置";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.ForeColor = System.Drawing.SystemColors.Control;
            this.label1.Location = new System.Drawing.Point(89, 51);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(239, 28);
            this.label1.TabIndex = 1;
            this.label1.Text = "TH Acquisition System";
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("微软雅黑", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblTitle.ForeColor = System.Drawing.SystemColors.Control;
            this.lblTitle.Location = new System.Drawing.Point(88, 9);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(182, 31);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "温湿度采集系统";
            // 
            // imgLogo
            // 
            this.imgLogo.BackgroundImage = global::tempWork.Properties.Resources.Head;
            this.imgLogo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.imgLogo.Location = new System.Drawing.Point(10, 11);
            this.imgLogo.Name = "imgLogo";
            this.imgLogo.Size = new System.Drawing.Size(82, 66);
            this.imgLogo.TabIndex = 0;
            this.imgLogo.TabStop = false;
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(871, 650);
            this.Controls.Add(this.colorZone);
            this.Controls.Add(this.panelBG);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FrmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FrmMaincs";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FrmMain_FormClosed);
            this.panelBG.ResumeLayout(false);
            this.colorZone.ResumeLayout(false);
            this.colorZone.PerformLayout();
            this.btnExit.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.btnHistory.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.btnWarn.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            this.btnOption.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.img1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgLogo)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelBG;
        private System.Windows.Forms.Panel colorZone;
        private System.Windows.Forms.PictureBox imgLogo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel btnOption;
        private System.Windows.Forms.PictureBox img1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel btnWarn;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Panel btnExit;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel btnHistory;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label3;
        private Controls.THMonitor th2;
        private Controls.THMonitor th1;
        private Controls.THMonitor th4;
        private Controls.THMonitor th3;
    }
}