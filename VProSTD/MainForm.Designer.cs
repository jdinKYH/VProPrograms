namespace VProSTD
{
    partial class MainForm
    {
        /// <summary>
        /// 디자이너가 사용하는 변수 (자동으로 정리됨)
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.panelTitle = new System.Windows.Forms.Panel();
            this.picLogo = new System.Windows.Forms.PictureBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lampCamera = new VProSTD.StatusLamp();
            this.lampPlc = new VProSTD.StatusLamp();
            this.panelRecipeBox = new System.Windows.Forms.Panel();
            this.lblRecipe = new System.Windows.Forms.Label();
            this.lblRecipeCaption = new System.Windows.Forms.Label();
            this.panelClockBox = new System.Windows.Forms.Panel();
            this.lblDateTime = new System.Windows.Forms.Label();
            this.panelTabs = new System.Windows.Forms.Panel();
            this.btnTabRun = new System.Windows.Forms.Button();
            this.btnTabRecipe = new System.Windows.Forms.Button();
            this.btnTabCamera = new System.Windows.Forms.Button();
            this.btnTabSystem = new System.Windows.Forms.Button();
            this.btnMode = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.panelPage = new System.Windows.Forms.Panel();
            this.panelStatus = new System.Windows.Forms.Panel();
            this.lblStatus = new System.Windows.Forms.Label();
            this.timerClock = new System.Windows.Forms.Timer(this.components);
            this.panelTitle.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            this.panelRecipeBox.SuspendLayout();
            this.panelClockBox.SuspendLayout();
            this.panelTabs.SuspendLayout();
            this.panelStatus.SuspendLayout();
            this.SuspendLayout();
            //
            // panelTitle : 맨 위 타이틀 바
            //
            this.panelTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(35)))), ((int)(((byte)(40)))));
            this.panelTitle.Controls.Add(this.picLogo);
            this.panelTitle.Controls.Add(this.lblTitle);
            this.panelTitle.Controls.Add(this.lampCamera);
            this.panelTitle.Controls.Add(this.lampPlc);
            this.panelTitle.Controls.Add(this.panelRecipeBox);
            this.panelTitle.Controls.Add(this.panelClockBox);
            this.panelTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTitle.Location = new System.Drawing.Point(0, 0);
            this.panelTitle.Name = "panelTitle";
            this.panelTitle.Size = new System.Drawing.Size(1920, 90);
            this.panelTitle.TabIndex = 0;
            //
            // picLogo : 회사 로고 (MainForm.cs 의 LoadLogo 에서 그림을 읽음)
            //
            this.picLogo.BackColor = System.Drawing.Color.Transparent;
            this.picLogo.Location = new System.Drawing.Point(24, 22);
            this.picLogo.Name = "picLogo";
            this.picLogo.Size = new System.Drawing.Size(276, 46);
            this.picLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picLogo.TabIndex = 0;
            this.picLogo.TabStop = false;
            //
            // lblTitle
            //
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("맑은 고딕", 24F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(232)))), ((int)(((byte)(235)))));
            this.lblTitle.Location = new System.Drawing.Point(330, 22);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(276, 45);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "Vision Inspection";
            //
            // lampCamera : 카메라 연결 표시등
            //
            this.lampCamera.Font = new System.Drawing.Font("맑은 고딕", 12F);
            this.lampCamera.Location = new System.Drawing.Point(1150, 10);
            this.lampCamera.Name = "lampCamera";
            this.lampCamera.Size = new System.Drawing.Size(170, 32);
            this.lampCamera.TabIndex = 2;
            this.lampCamera.Text = "Camera";
            //
            // lampPlc : PLC 연결 표시등 (지금은 항상 꺼짐)
            //
            this.lampPlc.Font = new System.Drawing.Font("맑은 고딕", 12F);
            this.lampPlc.Location = new System.Drawing.Point(1150, 48);
            this.lampPlc.Name = "lampPlc";
            this.lampPlc.Size = new System.Drawing.Size(170, 32);
            this.lampPlc.TabIndex = 3;
            this.lampPlc.Text = "PLC";
            //
            // panelRecipeBox : Recipe 상자 (바탕색 = 테두리색, 안쪽 1px 여백으로 테두리처럼 보임)
            //
            this.panelRecipeBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(64)))), ((int)(((byte)(72)))));
            this.panelRecipeBox.Controls.Add(this.lblRecipe);
            this.panelRecipeBox.Controls.Add(this.lblRecipeCaption);
            this.panelRecipeBox.Location = new System.Drawing.Point(1335, 14);
            this.panelRecipeBox.Name = "panelRecipeBox";
            this.panelRecipeBox.Padding = new System.Windows.Forms.Padding(1);
            this.panelRecipeBox.Size = new System.Drawing.Size(410, 62);
            this.panelRecipeBox.TabIndex = 4;
            //
            // lblRecipeCaption
            //
            this.lblRecipeCaption.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(27)))), ((int)(((byte)(31)))));
            this.lblRecipeCaption.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblRecipeCaption.Font = new System.Drawing.Font("맑은 고딕", 13F);
            this.lblRecipeCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(163)))), ((int)(((byte)(173)))));
            this.lblRecipeCaption.Location = new System.Drawing.Point(1, 1);
            this.lblRecipeCaption.Name = "lblRecipeCaption";
            this.lblRecipeCaption.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.lblRecipeCaption.Size = new System.Drawing.Size(100, 60);
            this.lblRecipeCaption.TabIndex = 0;
            this.lblRecipeCaption.Text = "Recipe :";
            this.lblRecipeCaption.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblRecipe : 현재 레시피 이름
            //
            this.lblRecipe.AutoEllipsis = true;
            this.lblRecipe.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(27)))), ((int)(((byte)(31)))));
            this.lblRecipe.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRecipe.Font = new System.Drawing.Font("Consolas", 16F);
            this.lblRecipe.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(61)))), ((int)(((byte)(220)))), ((int)(((byte)(151)))));
            this.lblRecipe.Location = new System.Drawing.Point(101, 1);
            this.lblRecipe.Name = "lblRecipe";
            this.lblRecipe.Size = new System.Drawing.Size(308, 60);
            this.lblRecipe.TabIndex = 1;
            this.lblRecipe.Text = "-";
            this.lblRecipe.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // panelClockBox : 날짜/시간 상자
            //
            this.panelClockBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(64)))), ((int)(((byte)(72)))));
            this.panelClockBox.Controls.Add(this.lblDateTime);
            this.panelClockBox.Location = new System.Drawing.Point(1760, 14);
            this.panelClockBox.Name = "panelClockBox";
            this.panelClockBox.Padding = new System.Windows.Forms.Padding(1);
            this.panelClockBox.Size = new System.Drawing.Size(150, 62);
            this.panelClockBox.TabIndex = 5;
            //
            // lblDateTime : 날짜 / 시간 (2줄)
            //
            this.lblDateTime.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(27)))), ((int)(((byte)(31)))));
            this.lblDateTime.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDateTime.Font = new System.Drawing.Font("Consolas", 13F, System.Drawing.FontStyle.Bold);
            this.lblDateTime.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(232)))), ((int)(((byte)(235)))));
            this.lblDateTime.Location = new System.Drawing.Point(1, 1);
            this.lblDateTime.Name = "lblDateTime";
            this.lblDateTime.Size = new System.Drawing.Size(148, 60);
            this.lblDateTime.TabIndex = 0;
            this.lblDateTime.Text = "2026-01-01\r\n00:00:00";
            this.lblDateTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // panelTabs : 탭 버튼 줄
            //
            this.panelTabs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(25)))), ((int)(((byte)(29)))));
            this.panelTabs.Controls.Add(this.btnTabRun);
            this.panelTabs.Controls.Add(this.btnTabRecipe);
            this.panelTabs.Controls.Add(this.btnTabCamera);
            this.panelTabs.Controls.Add(this.btnTabSystem);
            this.panelTabs.Controls.Add(this.btnMode);
            this.panelTabs.Controls.Add(this.btnExit);
            this.panelTabs.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTabs.Location = new System.Drawing.Point(0, 90);
            this.panelTabs.Name = "panelTabs";
            this.panelTabs.Size = new System.Drawing.Size(1920, 56);
            this.panelTabs.TabIndex = 1;
            //
            // btnTabRun
            //
            this.btnTabRun.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(51)))), ((int)(((byte)(56)))));
            this.btnTabRun.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(64)))), ((int)(((byte)(72)))));
            this.btnTabRun.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(72)))), ((int)(((byte)(78)))), ((int)(((byte)(86)))));
            this.btnTabRun.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(65)))), ((int)(((byte)(72)))));
            this.btnTabRun.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTabRun.Font = new System.Drawing.Font("맑은 고딕", 14F, System.Drawing.FontStyle.Bold);
            this.btnTabRun.Location = new System.Drawing.Point(10, 8);
            this.btnTabRun.Name = "btnTabRun";
            this.btnTabRun.Size = new System.Drawing.Size(260, 42);
            this.btnTabRun.TabIndex = 0;
            this.btnTabRun.Text = "RUN";
            this.btnTabRun.UseVisualStyleBackColor = false;
            this.btnTabRun.Click += new System.EventHandler(this.btnTabRun_Click);
            //
            // btnTabRecipe
            //
            this.btnTabRecipe.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(51)))), ((int)(((byte)(56)))));
            this.btnTabRecipe.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(64)))), ((int)(((byte)(72)))));
            this.btnTabRecipe.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(72)))), ((int)(((byte)(78)))), ((int)(((byte)(86)))));
            this.btnTabRecipe.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(65)))), ((int)(((byte)(72)))));
            this.btnTabRecipe.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTabRecipe.Font = new System.Drawing.Font("맑은 고딕", 14F, System.Drawing.FontStyle.Bold);
            this.btnTabRecipe.Location = new System.Drawing.Point(280, 8);
            this.btnTabRecipe.Name = "btnTabRecipe";
            this.btnTabRecipe.Size = new System.Drawing.Size(260, 42);
            this.btnTabRecipe.TabIndex = 1;
            this.btnTabRecipe.Text = "RECIPE";
            this.btnTabRecipe.UseVisualStyleBackColor = false;
            this.btnTabRecipe.Click += new System.EventHandler(this.btnTabRecipe_Click);
            //
            // btnTabCamera
            //
            this.btnTabCamera.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(51)))), ((int)(((byte)(56)))));
            this.btnTabCamera.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(64)))), ((int)(((byte)(72)))));
            this.btnTabCamera.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(72)))), ((int)(((byte)(78)))), ((int)(((byte)(86)))));
            this.btnTabCamera.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(65)))), ((int)(((byte)(72)))));
            this.btnTabCamera.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTabCamera.Font = new System.Drawing.Font("맑은 고딕", 14F, System.Drawing.FontStyle.Bold);
            this.btnTabCamera.Location = new System.Drawing.Point(550, 8);
            this.btnTabCamera.Name = "btnTabCamera";
            this.btnTabCamera.Size = new System.Drawing.Size(260, 42);
            this.btnTabCamera.TabIndex = 2;
            this.btnTabCamera.Text = "CONNECT CAMERA";
            this.btnTabCamera.UseVisualStyleBackColor = false;
            this.btnTabCamera.Click += new System.EventHandler(this.btnTabCamera_Click);
            //
            // btnTabSystem
            //
            this.btnTabSystem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(51)))), ((int)(((byte)(56)))));
            this.btnTabSystem.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(64)))), ((int)(((byte)(72)))));
            this.btnTabSystem.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(72)))), ((int)(((byte)(78)))), ((int)(((byte)(86)))));
            this.btnTabSystem.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(65)))), ((int)(((byte)(72)))));
            this.btnTabSystem.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTabSystem.Font = new System.Drawing.Font("맑은 고딕", 14F, System.Drawing.FontStyle.Bold);
            this.btnTabSystem.Location = new System.Drawing.Point(820, 8);
            this.btnTabSystem.Name = "btnTabSystem";
            this.btnTabSystem.Size = new System.Drawing.Size(260, 42);
            this.btnTabSystem.TabIndex = 3;
            this.btnTabSystem.Text = "SYSTEM";
            this.btnTabSystem.UseVisualStyleBackColor = false;
            this.btnTabSystem.Click += new System.EventHandler(this.btnTabSystem_Click);
            //
            // btnMode : 작업자 / 엔지니어 모드 표시와 전환 (색과 글자는 MainForm.cs 의 SetEngineerMode 에서 바꿈)
            //
            this.btnMode.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(51)))), ((int)(((byte)(56)))));
            this.btnMode.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(64)))), ((int)(((byte)(72)))));
            this.btnMode.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(72)))), ((int)(((byte)(78)))), ((int)(((byte)(86)))));
            this.btnMode.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(65)))), ((int)(((byte)(72)))));
            this.btnMode.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMode.Font = new System.Drawing.Font("맑은 고딕", 13F, System.Drawing.FontStyle.Bold);
            this.btnMode.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(232)))), ((int)(((byte)(235)))));
            this.btnMode.Location = new System.Drawing.Point(1540, 8);
            this.btnMode.Name = "btnMode";
            this.btnMode.Size = new System.Drawing.Size(200, 42);
            this.btnMode.TabIndex = 5;
            this.btnMode.Text = "작업자 모드";
            this.btnMode.UseVisualStyleBackColor = false;
            this.btnMode.Click += new System.EventHandler(this.btnMode_Click);
            //
            // btnExit
            //
            this.btnExit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(38)))), ((int)(((byte)(30)))));
            this.btnExit.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(38)))), ((int)(((byte)(30)))));
            this.btnExit.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(28)))), ((int)(((byte)(22)))));
            this.btnExit.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(52)))), ((int)(((byte)(43)))));
            this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExit.Font = new System.Drawing.Font("맑은 고딕", 14F, System.Drawing.FontStyle.Bold);
            this.btnExit.Location = new System.Drawing.Point(1760, 8);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(150, 42);
            this.btnExit.TabIndex = 4;
            this.btnExit.Text = "EXIT";
            this.btnExit.UseVisualStyleBackColor = false;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            //
            // panelPage : 탭 화면(페이지)이 들어가는 자리
            //
            this.panelPage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(25)))), ((int)(((byte)(29)))));
            this.panelPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelPage.Location = new System.Drawing.Point(0, 146);
            this.panelPage.Name = "panelPage";
            this.panelPage.Padding = new System.Windows.Forms.Padding(10, 4, 10, 6);
            this.panelPage.Size = new System.Drawing.Size(1920, 900);
            this.panelPage.TabIndex = 2;
            //
            // panelStatus : 맨 아래 상태줄
            //
            this.panelStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(35)))), ((int)(((byte)(40)))));
            this.panelStatus.Controls.Add(this.lblStatus);
            this.panelStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelStatus.Location = new System.Drawing.Point(0, 1046);
            this.panelStatus.Name = "panelStatus";
            this.panelStatus.Size = new System.Drawing.Size(1920, 34);
            this.panelStatus.TabIndex = 3;
            //
            // lblStatus
            //
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStatus.Font = new System.Drawing.Font("맑은 고딕", 11F);
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(163)))), ((int)(((byte)(173)))));
            this.lblStatus.Location = new System.Drawing.Point(0, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.lblStatus.Size = new System.Drawing.Size(1920, 34);
            this.lblStatus.TabIndex = 0;
            this.lblStatus.Text = "상태 표시";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // timerClock : 1초마다 날짜/시간 갱신
            //
            this.timerClock.Interval = 1000;
            this.timerClock.Tick += new System.EventHandler(this.timerClock_Tick);
            //
            // MainForm : FHD(1920x1080) 고정, 윈도우 기본 제목줄 없음
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(25)))), ((int)(((byte)(29)))));
            this.ClientSize = new System.Drawing.Size(1920, 1080);
            this.Controls.Add(this.panelPage);
            this.Controls.Add(this.panelTabs);
            this.Controls.Add(this.panelTitle);
            this.Controls.Add(this.panelStatus);
            this.Font = new System.Drawing.Font("맑은 고딕", 12F);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(232)))), ((int)(((byte)(235)))));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Vision Inspection";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.panelTitle.ResumeLayout(false);
            this.panelTitle.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            this.panelRecipeBox.ResumeLayout(false);
            this.panelClockBox.ResumeLayout(false);
            this.panelTabs.ResumeLayout(false);
            this.panelStatus.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        // 타이틀 바
        private System.Windows.Forms.Panel panelTitle;
        private System.Windows.Forms.PictureBox picLogo;
        private System.Windows.Forms.Label lblTitle;
        private VProSTD.StatusLamp lampCamera;
        private VProSTD.StatusLamp lampPlc;
        private System.Windows.Forms.Panel panelRecipeBox;
        private System.Windows.Forms.Label lblRecipeCaption;
        private System.Windows.Forms.Label lblRecipe;
        private System.Windows.Forms.Panel panelClockBox;
        private System.Windows.Forms.Label lblDateTime;

        // 탭 버튼 줄
        private System.Windows.Forms.Panel panelTabs;
        private System.Windows.Forms.Button btnTabRun;
        private System.Windows.Forms.Button btnTabRecipe;
        private System.Windows.Forms.Button btnTabCamera;
        private System.Windows.Forms.Button btnTabSystem;
        private System.Windows.Forms.Button btnMode;
        private System.Windows.Forms.Button btnExit;

        // 페이지 / 상태줄
        private System.Windows.Forms.Panel panelPage;
        private System.Windows.Forms.Panel panelStatus;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Timer timerClock;
    }
}
