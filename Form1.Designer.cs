namespace VProManager01
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
            this.btnRun = new System.Windows.Forms.Button();
            this.chkFileMode = new System.Windows.Forms.CheckBox();
            this.lblInputImage = new System.Windows.Forms.Label();
            this.lblResultImage = new System.Windows.Forms.Label();
            this.lblLeftDist1 = new System.Windows.Forms.Label();
            this.lblLeftDist2 = new System.Windows.Forms.Label();
            this.lblRightDist2 = new System.Windows.Forms.Label();
            this.lblRightDist1 = new System.Windows.Forms.Label();
            this.txtLeftDist1 = new System.Windows.Forms.TextBox();
            this.txtLeftDist2 = new System.Windows.Forms.TextBox();
            this.txtRightDist1 = new System.Windows.Forms.TextBox();
            this.txtRightDist2 = new System.Windows.Forms.TextBox();
            this.txtAlign = new System.Windows.Forms.TextBox();
            this.lblAlign = new System.Windows.Forms.Label();
            this.btnLoadModel = new System.Windows.Forms.Button();
            this.lblJudge = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnSettings = new System.Windows.Forms.Button();
            this.btnFolderRun = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnRun
            // 
            this.btnRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRun.Font = new System.Drawing.Font("굴림", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnRun.Location = new System.Drawing.Point(1454, 844);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(183, 41);
            this.btnRun.TabIndex = 0;
            this.btnRun.Text = "Run";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // chkFileMode
            // 
            this.chkFileMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkFileMode.AutoSize = true;
            this.chkFileMode.Font = new System.Drawing.Font("굴림", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.chkFileMode.Location = new System.Drawing.Point(1530, 17);
            this.chkFileMode.Name = "chkFileMode";
            this.chkFileMode.Size = new System.Drawing.Size(107, 23);
            this.chkFileMode.TabIndex = 1;
            this.chkFileMode.Text = "FileMode";
            this.chkFileMode.UseVisualStyleBackColor = true;
            this.chkFileMode.CheckedChanged += new System.EventHandler(this.chkFileMode_CheckedChanged);
            // 
            // lblInputImage
            // 
            this.lblInputImage.AutoSize = true;
            this.lblInputImage.Font = new System.Drawing.Font("굴림", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblInputImage.Location = new System.Drawing.Point(265, 16);
            this.lblInputImage.Name = "lblInputImage";
            this.lblInputImage.Size = new System.Drawing.Size(137, 24);
            this.lblInputImage.TabIndex = 2;
            this.lblInputImage.Text = "Input Image";
            // 
            // lblResultImage
            // 
            this.lblResultImage.AutoSize = true;
            this.lblResultImage.Font = new System.Drawing.Font("굴림", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblResultImage.Location = new System.Drawing.Point(969, 16);
            this.lblResultImage.Name = "lblResultImage";
            this.lblResultImage.Size = new System.Drawing.Size(152, 24);
            this.lblResultImage.TabIndex = 3;
            this.lblResultImage.Text = "Result Image";
            // 
            // lblLeftDist1
            // 
            this.lblLeftDist1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLeftDist1.AutoSize = true;
            this.lblLeftDist1.Font = new System.Drawing.Font("굴림", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblLeftDist1.Location = new System.Drawing.Point(1417, 129);
            this.lblLeftDist1.Name = "lblLeftDist1";
            this.lblLeftDist1.Size = new System.Drawing.Size(79, 14);
            this.lblLeftDist1.TabIndex = 4;
            this.lblLeftDist1.Text = "Left Dist1";
            // 
            // lblLeftDist2
            // 
            this.lblLeftDist2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLeftDist2.AutoSize = true;
            this.lblLeftDist2.Font = new System.Drawing.Font("굴림", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblLeftDist2.Location = new System.Drawing.Point(1417, 159);
            this.lblLeftDist2.Name = "lblLeftDist2";
            this.lblLeftDist2.Size = new System.Drawing.Size(79, 14);
            this.lblLeftDist2.TabIndex = 5;
            this.lblLeftDist2.Text = "Left Dist2";
            // 
            // lblRightDist2
            // 
            this.lblRightDist2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblRightDist2.AutoSize = true;
            this.lblRightDist2.Font = new System.Drawing.Font("굴림", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblRightDist2.Location = new System.Drawing.Point(1407, 210);
            this.lblRightDist2.Name = "lblRightDist2";
            this.lblRightDist2.Size = new System.Drawing.Size(89, 14);
            this.lblRightDist2.TabIndex = 7;
            this.lblRightDist2.Text = "Right Dist2";
            // 
            // lblRightDist1
            // 
            this.lblRightDist1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblRightDist1.AutoSize = true;
            this.lblRightDist1.Font = new System.Drawing.Font("굴림", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblRightDist1.Location = new System.Drawing.Point(1407, 183);
            this.lblRightDist1.Name = "lblRightDist1";
            this.lblRightDist1.Size = new System.Drawing.Size(89, 14);
            this.lblRightDist1.TabIndex = 6;
            this.lblRightDist1.Text = "Right Dist1";
            // 
            // txtLeftDist1
            // 
            this.txtLeftDist1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLeftDist1.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.txtLeftDist1.Location = new System.Drawing.Point(1502, 123);
            this.txtLeftDist1.Name = "txtLeftDist1";
            this.txtLeftDist1.ReadOnly = true;
            this.txtLeftDist1.Size = new System.Drawing.Size(135, 26);
            this.txtLeftDist1.TabIndex = 8;
            this.txtLeftDist1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtLeftDist2
            // 
            this.txtLeftDist2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLeftDist2.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.txtLeftDist2.Location = new System.Drawing.Point(1502, 153);
            this.txtLeftDist2.Name = "txtLeftDist2";
            this.txtLeftDist2.ReadOnly = true;
            this.txtLeftDist2.Size = new System.Drawing.Size(135, 26);
            this.txtLeftDist2.TabIndex = 9;
            this.txtLeftDist2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtRightDist1
            // 
            this.txtRightDist1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtRightDist1.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.txtRightDist1.Location = new System.Drawing.Point(1502, 177);
            this.txtRightDist1.Name = "txtRightDist1";
            this.txtRightDist1.ReadOnly = true;
            this.txtRightDist1.Size = new System.Drawing.Size(135, 26);
            this.txtRightDist1.TabIndex = 10;
            this.txtRightDist1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtRightDist2
            // 
            this.txtRightDist2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtRightDist2.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.txtRightDist2.Location = new System.Drawing.Point(1502, 204);
            this.txtRightDist2.Name = "txtRightDist2";
            this.txtRightDist2.ReadOnly = true;
            this.txtRightDist2.Size = new System.Drawing.Size(135, 26);
            this.txtRightDist2.TabIndex = 11;
            this.txtRightDist2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtAlign
            // 
            this.txtAlign.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAlign.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.txtAlign.Location = new System.Drawing.Point(1502, 265);
            this.txtAlign.Name = "txtAlign";
            this.txtAlign.ReadOnly = true;
            this.txtAlign.Size = new System.Drawing.Size(135, 26);
            this.txtAlign.TabIndex = 13;
            this.txtAlign.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // lblAlign
            // 
            this.lblAlign.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAlign.AutoSize = true;
            this.lblAlign.Font = new System.Drawing.Font("굴림", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblAlign.Location = new System.Drawing.Point(1447, 271);
            this.lblAlign.Name = "lblAlign";
            this.lblAlign.Size = new System.Drawing.Size(49, 14);
            this.lblAlign.TabIndex = 12;
            this.lblAlign.Text = "Align ";
            // 
            // btnLoadModel
            // 
            this.btnLoadModel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLoadModel.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnLoadModel.Location = new System.Drawing.Point(1454, 57);
            this.btnLoadModel.Name = "btnLoadModel";
            this.btnLoadModel.Size = new System.Drawing.Size(183, 32);
            this.btnLoadModel.TabIndex = 14;
            this.btnLoadModel.Text = "Model Load";
            this.btnLoadModel.UseVisualStyleBackColor = true;
            this.btnLoadModel.Click += new System.EventHandler(this.btnLoadModel_Click);
            // 
            // lblJudge
            // 
            this.lblJudge.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblJudge.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblJudge.Font = new System.Drawing.Font("굴림", 28F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblJudge.Location = new System.Drawing.Point(1410, 323);
            this.lblJudge.Name = "lblJudge";
            this.lblJudge.Size = new System.Drawing.Size(227, 91);
            this.lblJudge.TabIndex = 15;
            this.lblJudge.Text = "-";
            this.lblJudge.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("굴림", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblStatus.Location = new System.Drawing.Point(30, 870);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 15);
            this.lblStatus.TabIndex = 16;
            // 
            // btnSettings
            // 
            this.btnSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSettings.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnSettings.Location = new System.Drawing.Point(1454, 795);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(183, 32);
            this.btnSettings.TabIndex = 17;
            this.btnSettings.Text = "Settings";
            this.btnSettings.UseVisualStyleBackColor = true;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            // 
            // btnFolderRun
            // 
            this.btnFolderRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFolderRun.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnFolderRun.Location = new System.Drawing.Point(1454, 749);
            this.btnFolderRun.Name = "btnFolderRun";
            this.btnFolderRun.Size = new System.Drawing.Size(183, 32);
            this.btnFolderRun.TabIndex = 18;
            this.btnFolderRun.Text = "Folder Run";
            this.btnFolderRun.UseVisualStyleBackColor = true;
            this.btnFolderRun.Click += new System.EventHandler(this.btnFolderRun_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1668, 903);
            this.Controls.Add(this.btnFolderRun);
            this.Controls.Add(this.btnSettings);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lblJudge);
            this.Controls.Add(this.btnLoadModel);
            this.Controls.Add(this.txtAlign);
            this.Controls.Add(this.lblAlign);
            this.Controls.Add(this.txtRightDist2);
            this.Controls.Add(this.txtRightDist1);
            this.Controls.Add(this.txtLeftDist2);
            this.Controls.Add(this.txtLeftDist1);
            this.Controls.Add(this.lblRightDist2);
            this.Controls.Add(this.lblRightDist1);
            this.Controls.Add(this.lblLeftDist2);
            this.Controls.Add(this.lblLeftDist1);
            this.Controls.Add(this.lblResultImage);
            this.Controls.Add(this.lblInputImage);
            this.Controls.Add(this.chkFileMode);
            this.Controls.Add(this.btnRun);
            this.MinimumSize = new System.Drawing.Size(1024, 700);
            this.Name = "Form1";
            this.Text = "VProManager01";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        // 261028@Claude [4단계] 컨트롤 이름 정리 (기존 이름)
        // inspect→btnRun, button1→btnLoadModel,
        // textBox1→txtLeftDist1, textBox2→txtLeftDist2, textBox3→txtRightDist1, textBox4→txtRightDist2, textBox5→txtAlign,
        // label1→lblInputImage, label2→lblResultImage, label3→lblLeftDist1, label4→lblLeftDist2,
        // label6→lblRightDist1, label5→lblRightDist2, label7→lblAlign

        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.CheckBox chkFileMode;
        private System.Windows.Forms.Label lblInputImage;
        private System.Windows.Forms.Label lblResultImage;
        private System.Windows.Forms.Label lblLeftDist1;
        private System.Windows.Forms.Label lblLeftDist2;
        private System.Windows.Forms.Label lblRightDist2;
        private System.Windows.Forms.Label lblRightDist1;
        private System.Windows.Forms.TextBox txtLeftDist1;
        private System.Windows.Forms.TextBox txtLeftDist2;
        private System.Windows.Forms.TextBox txtRightDist1;
        private System.Windows.Forms.TextBox txtRightDist2;
        private System.Windows.Forms.TextBox txtAlign;
        private System.Windows.Forms.Label lblAlign;
        private System.Windows.Forms.Button btnLoadModel;
        // 261028@Claude [3단계] 판정 표시, 상태 표시줄, 설정 버튼
        private System.Windows.Forms.Label lblJudge;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.Button btnFolderRun;   // 261028@Claude [4단계]
    }
}

