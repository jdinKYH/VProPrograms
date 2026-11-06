namespace VproBlack
{
    partial class Form1
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnLoadJob = new System.Windows.Forms.Button();
            this.btnSetInput = new System.Windows.Forms.Button();
            this.btnRunJob = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblJobInfo = new System.Windows.Forms.Label();
            this.lblResult = new System.Windows.Forms.Label();
            this.pBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pBox)).BeginInit();
            this.SuspendLayout();
            // 
            // btnLoadJob
            // 
            this.btnLoadJob.Location = new System.Drawing.Point(68, 155);
            this.btnLoadJob.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnLoadJob.Name = "btnLoadJob";
            this.btnLoadJob.Size = new System.Drawing.Size(171, 40);
            this.btnLoadJob.TabIndex = 0;
            this.btnLoadJob.Text = "Job Load";
            this.btnLoadJob.UseVisualStyleBackColor = true;
            this.btnLoadJob.Click += new System.EventHandler(this.btnLoadJob_Click);
            // 
            // btnSetInput
            // 
            this.btnSetInput.Location = new System.Drawing.Point(68, 200);
            this.btnSetInput.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnSetInput.Name = "btnSetInput";
            this.btnSetInput.Size = new System.Drawing.Size(171, 40);
            this.btnSetInput.TabIndex = 1;
            this.btnSetInput.Text = "이미지 입력 설정";
            this.btnSetInput.UseVisualStyleBackColor = true;
            this.btnSetInput.Click += new System.EventHandler(this.btnSetInput_Click);
            // 
            // btnRunJob
            // 
            this.btnRunJob.Location = new System.Drawing.Point(68, 245);
            this.btnRunJob.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnRunJob.Name = "btnRunJob";
            this.btnRunJob.Size = new System.Drawing.Size(171, 40);
            this.btnRunJob.TabIndex = 2;
            this.btnRunJob.Text = "Job 실행";
            this.btnRunJob.UseVisualStyleBackColor = true;
            this.btnRunJob.Click += new System.EventHandler(this.btnRunJob_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(66, 38);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(81, 12);
            this.lblStatus.TabIndex = 3;
            this.lblStatus.Text = "상태 : 대기 중";
            // 
            // lblJobInfo
            // 
            this.lblJobInfo.AutoSize = true;
            this.lblJobInfo.Location = new System.Drawing.Point(66, 75);
            this.lblJobInfo.Name = "lblJobInfo";
            this.lblJobInfo.Size = new System.Drawing.Size(71, 12);
            this.lblJobInfo.TabIndex = 4;
            this.lblJobInfo.Text = "Job 정보 : -";
            // 
            // lblResult
            // 
            this.lblResult.AutoSize = true;
            this.lblResult.Location = new System.Drawing.Point(75, 119);
            this.lblResult.Name = "lblResult";
            this.lblResult.Size = new System.Drawing.Size(47, 12);
            this.lblResult.TabIndex = 5;
            this.lblResult.Text = "결과 : -";
            // 
            // pBox
            // 
            this.pBox.Location = new System.Drawing.Point(1977, 16);
            this.pBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pBox.Name = "pBox";
            this.pBox.Size = new System.Drawing.Size(141, 224);
            this.pBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pBox.TabIndex = 6;
            this.pBox.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(2140, 966);
            this.Controls.Add(this.pBox);
            this.Controls.Add(this.lblResult);
            this.Controls.Add(this.lblJobInfo);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnRunJob);
            this.Controls.Add(this.btnSetInput);
            this.Controls.Add(this.btnLoadJob);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.pBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnLoadJob;
        private System.Windows.Forms.Button btnSetInput;
        private System.Windows.Forms.Button btnRunJob;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblJobInfo;
        private System.Windows.Forms.Label lblResult;
        private System.Windows.Forms.PictureBox pBox;
    }
}

