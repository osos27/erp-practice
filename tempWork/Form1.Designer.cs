namespace tempWork
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnDown = new System.Windows.Forms.Button();
            this.th = new Controls.THMonitor();
            this.SuspendLayout();
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(12, 266);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(113, 47);
            this.btnAdd.TabIndex = 1;
            this.btnAdd.Text = "up";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnDown
            // 
            this.btnDown.Location = new System.Drawing.Point(298, 266);
            this.btnDown.Name = "btnDown";
            this.btnDown.Size = new System.Drawing.Size(113, 47);
            this.btnDown.TabIndex = 1;
            this.btnDown.Text = "down";
            this.btnDown.UseVisualStyleBackColor = true;
            this.btnDown.Click += new System.EventHandler(this.btnDown_Click);
            // 
            // th
            // 
            this.th.Location = new System.Drawing.Point(12, 12);
            this.th.MoistValue = 0;
            this.th.MoistWarning = 45;
            this.th.Name = "th";
            this.th.Size = new System.Drawing.Size(399, 234);
            this.th.TabIndex = 2;
            this.th.TemperatureValue = 0;
            this.th.TemperatureWarning = 40;
            this.th.TitleNum = 1;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(424, 326);
            this.Controls.Add(this.th);
            this.Controls.Add(this.btnDown);
            this.Controls.Add(this.btnAdd);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnDown;
        private Controls.THMonitor thMonitor1;
        private Controls.THMonitor th;
    }
}

