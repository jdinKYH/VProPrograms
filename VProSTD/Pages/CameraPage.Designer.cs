namespace VProSTD
{
    partial class CameraPage
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 구성 요소 디자이너에서 생성한 코드

        private void InitializeComponent()
        {
            this.panelMain = new System.Windows.Forms.Panel();
            this.panelDisplay = new System.Windows.Forms.Panel();
            this.panelRight = new System.Windows.Forms.Panel();
            this.lblCameraModel = new System.Windows.Forms.Label();
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnTrigger = new System.Windows.Forms.Button();
            this.lblExposure = new System.Windows.Forms.Label();
            this.numExposure = new System.Windows.Forms.NumericUpDown();
            this.chkFileMode = new System.Windows.Forms.CheckBox();
            this.btnSingleImage = new System.Windows.Forms.Button();
            this.btnImageFolder = new System.Windows.Forms.Button();
            this.panelMain.SuspendLayout();
            this.panelRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numExposure)).BeginInit();
            this.SuspendLayout();
            //
            // panelMain
            //
            this.panelMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(35)))), ((int)(((byte)(40)))));
            this.panelMain.Controls.Add(this.panelDisplay);
            this.panelMain.Controls.Add(this.panelRight);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 0);
            this.panelMain.Name = "panelMain";
            this.panelMain.Padding = new System.Windows.Forms.Padding(8);
            this.panelMain.Size = new System.Drawing.Size(1900, 890);
            this.panelMain.TabIndex = 0;
            //
            // panelDisplay : 취득 이미지 화면 자리 (7단계에서 CogDisplay 를 넣음)
            //
            this.panelDisplay.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(16)))), ((int)(((byte)(19)))));
            this.panelDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelDisplay.Location = new System.Drawing.Point(8, 8);
            this.panelDisplay.Name = "panelDisplay";
            this.panelDisplay.Size = new System.Drawing.Size(1582, 872);
            this.panelDisplay.TabIndex = 0;
            //
            // panelRight : 카메라 / File Mode 조작부
            //
            this.panelRight.Controls.Add(this.lblCameraModel);
            this.panelRight.Controls.Add(this.btnConnect);
            this.panelRight.Controls.Add(this.btnTrigger);
            this.panelRight.Controls.Add(this.lblExposure);
            this.panelRight.Controls.Add(this.numExposure);
            this.panelRight.Controls.Add(this.chkFileMode);
            this.panelRight.Controls.Add(this.btnSingleImage);
            this.panelRight.Controls.Add(this.btnImageFolder);
            this.panelRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelRight.Location = new System.Drawing.Point(1590, 8);
            this.panelRight.Name = "panelRight";
            this.panelRight.Size = new System.Drawing.Size(300, 872);
            this.panelRight.TabIndex = 1;
            //
            // lblCameraModel : 카메라 모델 (연결 전에는 "...")
            //
            this.lblCameraModel.AutoEllipsis = true;
            this.lblCameraModel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(27)))), ((int)(((byte)(31)))));
            this.lblCameraModel.Font = new System.Drawing.Font("Consolas", 13F);
            this.lblCameraModel.Location = new System.Drawing.Point(20, 0);
            this.lblCameraModel.Name = "lblCameraModel";
            this.lblCameraModel.Size = new System.Drawing.Size(260, 52);
            this.lblCameraModel.TabIndex = 0;
            this.lblCameraModel.Text = "...";
            this.lblCameraModel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // btnConnect
            //
            this.btnConnect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(51)))), ((int)(((byte)(56)))));
            this.btnConnect.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(64)))), ((int)(((byte)(72)))));
            this.btnConnect.FlatAppearance.BorderSize = 2;
            this.btnConnect.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(72)))), ((int)(((byte)(78)))), ((int)(((byte)(86)))));
            this.btnConnect.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(65)))), ((int)(((byte)(72)))));
            this.btnConnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConnect.Font = new System.Drawing.Font("맑은 고딕", 14F);
            this.btnConnect.Location = new System.Drawing.Point(20, 64);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(260, 52);
            this.btnConnect.TabIndex = 1;
            this.btnConnect.Text = "CONNECT";
            this.btnConnect.UseVisualStyleBackColor = false;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            //
            // btnTrigger
            //
            this.btnTrigger.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(51)))), ((int)(((byte)(56)))));
            this.btnTrigger.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(64)))), ((int)(((byte)(72)))));
            this.btnTrigger.FlatAppearance.BorderSize = 2;
            this.btnTrigger.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(72)))), ((int)(((byte)(78)))), ((int)(((byte)(86)))));
            this.btnTrigger.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(65)))), ((int)(((byte)(72)))));
            this.btnTrigger.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTrigger.Font = new System.Drawing.Font("맑은 고딕", 14F);
            this.btnTrigger.Location = new System.Drawing.Point(20, 128);
            this.btnTrigger.Name = "btnTrigger";
            this.btnTrigger.Size = new System.Drawing.Size(260, 52);
            this.btnTrigger.TabIndex = 2;
            this.btnTrigger.Text = "TRIGGER";
            this.btnTrigger.UseVisualStyleBackColor = false;
            this.btnTrigger.Click += new System.EventHandler(this.btnTrigger_Click);
            //
            // lblExposure
            //
            this.lblExposure.Location = new System.Drawing.Point(20, 196);
            this.lblExposure.Name = "lblExposure";
            this.lblExposure.Size = new System.Drawing.Size(260, 30);
            this.lblExposure.TabIndex = 3;
            this.lblExposure.Text = "노출 (ms)";
            this.lblExposure.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // numExposure : 노출 시간 (ms)
            //
            this.numExposure.DecimalPlaces = 2;
            this.numExposure.Font = new System.Drawing.Font("맑은 고딕", 16F);
            this.numExposure.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(27)))), ((int)(((byte)(31)))));
            this.numExposure.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numExposure.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(232)))), ((int)(((byte)(235)))));
            this.numExposure.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            this.numExposure.Location = new System.Drawing.Point(20, 228);
            this.numExposure.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            this.numExposure.Minimum = new decimal(new int[] { 1, 0, 0, 131072 });
            this.numExposure.Name = "numExposure";
            this.numExposure.Size = new System.Drawing.Size(260, 36);
            this.numExposure.TabIndex = 4;
            this.numExposure.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numExposure.Value = new decimal(new int[] { 10, 0, 0, 0 });
            this.numExposure.ValueChanged += new System.EventHandler(this.numExposure_ValueChanged);
            //
            // chkFileMode : 켜면 카메라 대신 이미지 파일로 검사
            //
            this.chkFileMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkFileMode.Font = new System.Drawing.Font("맑은 고딕", 14F);
            this.chkFileMode.Location = new System.Drawing.Point(20, 690);
            this.chkFileMode.Name = "chkFileMode";
            this.chkFileMode.Size = new System.Drawing.Size(260, 40);
            this.chkFileMode.TabIndex = 5;
            this.chkFileMode.Text = "File Mode";
            this.chkFileMode.CheckedChanged += new System.EventHandler(this.chkFileMode_CheckedChanged);
            //
            // btnSingleImage
            //
            this.btnSingleImage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSingleImage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(51)))), ((int)(((byte)(56)))));
            this.btnSingleImage.Enabled = false;
            this.btnSingleImage.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(64)))), ((int)(((byte)(72)))));
            this.btnSingleImage.FlatAppearance.BorderSize = 2;
            this.btnSingleImage.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(72)))), ((int)(((byte)(78)))), ((int)(((byte)(86)))));
            this.btnSingleImage.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(65)))), ((int)(((byte)(72)))));
            this.btnSingleImage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSingleImage.Font = new System.Drawing.Font("맑은 고딕", 14F);
            this.btnSingleImage.Location = new System.Drawing.Point(20, 736);
            this.btnSingleImage.Name = "btnSingleImage";
            this.btnSingleImage.Size = new System.Drawing.Size(260, 52);
            this.btnSingleImage.TabIndex = 6;
            this.btnSingleImage.Text = "Single Image";
            this.btnSingleImage.UseVisualStyleBackColor = false;
            this.btnSingleImage.Click += new System.EventHandler(this.btnSingleImage_Click);
            //
            // btnImageFolder
            //
            this.btnImageFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnImageFolder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(51)))), ((int)(((byte)(56)))));
            this.btnImageFolder.Enabled = false;
            this.btnImageFolder.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(64)))), ((int)(((byte)(72)))));
            this.btnImageFolder.FlatAppearance.BorderSize = 2;
            this.btnImageFolder.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(72)))), ((int)(((byte)(78)))), ((int)(((byte)(86)))));
            this.btnImageFolder.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(65)))), ((int)(((byte)(72)))));
            this.btnImageFolder.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnImageFolder.Font = new System.Drawing.Font("맑은 고딕", 14F);
            this.btnImageFolder.Location = new System.Drawing.Point(20, 800);
            this.btnImageFolder.Name = "btnImageFolder";
            this.btnImageFolder.Size = new System.Drawing.Size(260, 52);
            this.btnImageFolder.TabIndex = 7;
            this.btnImageFolder.Text = "Image Folder";
            this.btnImageFolder.UseVisualStyleBackColor = false;
            this.btnImageFolder.Click += new System.EventHandler(this.btnImageFolder_Click);
            //
            // CameraPage
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(25)))), ((int)(((byte)(29)))));
            this.Controls.Add(this.panelMain);
            this.Font = new System.Drawing.Font("맑은 고딕", 12F);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(232)))), ((int)(((byte)(235)))));
            this.Name = "CameraPage";
            this.Size = new System.Drawing.Size(1900, 890);
            this.panelMain.ResumeLayout(false);
            this.panelRight.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numExposure)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.Panel panelDisplay;
        private System.Windows.Forms.Panel panelRight;
        private System.Windows.Forms.Label lblCameraModel;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnTrigger;
        private System.Windows.Forms.Label lblExposure;
        private System.Windows.Forms.NumericUpDown numExposure;
        private System.Windows.Forms.CheckBox chkFileMode;
        private System.Windows.Forms.Button btnSingleImage;
        private System.Windows.Forms.Button btnImageFolder;
    }
}
