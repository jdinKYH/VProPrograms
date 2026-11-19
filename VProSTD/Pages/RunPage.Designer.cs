namespace VProSTD
{
    partial class RunPage
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
            System.Windows.Forms.DataGridViewCellStyle headerStyle = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle cellStyle = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle alternateStyle = new System.Windows.Forms.DataGridViewCellStyle();
            this.panelMain = new System.Windows.Forms.Panel();
            this.panelDisplay = new System.Windows.Forms.Panel();
            this.panelRight = new System.Windows.Forms.Panel();
            this.lblJudge = new System.Windows.Forms.Label();
            this.cardTotal = new VProSTD.CountCard();
            this.cardOk = new VProSTD.CountCard();
            this.cardNg = new VProSTD.CountCard();
            this.cardYield = new VProSTD.CountCard();
            this.btnCountReset = new System.Windows.Forms.Button();
            this.btnAutoRun = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnRunOnce = new System.Windows.Forms.Button();
            this.listLog = new System.Windows.Forms.ListBox();
            this.btnModelLoad = new System.Windows.Forms.Button();
            this.btnLogClear = new System.Windows.Forms.Button();
            this.panelGap = new System.Windows.Forms.Panel();
            this.gridResult = new System.Windows.Forms.DataGridView();
            this.panelMain.SuspendLayout();
            this.panelRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridResult)).BeginInit();
            this.SuspendLayout();
            //
            // panelMain : 위쪽 큰 영역 (이미지 화면 + 오른쪽 조작부)
            //
            this.panelMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(35)))), ((int)(((byte)(40)))));
            this.panelMain.Controls.Add(this.panelDisplay);
            this.panelMain.Controls.Add(this.panelRight);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 0);
            this.panelMain.Name = "panelMain";
            this.panelMain.Padding = new System.Windows.Forms.Padding(8);
            this.panelMain.Size = new System.Drawing.Size(1900, 756);
            this.panelMain.TabIndex = 0;
            //
            // panelDisplay : 이미지 화면 자리 (4단계에서 CogRecordDisplay 를 넣음)
            //
            this.panelDisplay.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(16)))), ((int)(((byte)(19)))));
            this.panelDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelDisplay.Location = new System.Drawing.Point(8, 8);
            this.panelDisplay.Name = "panelDisplay";
            this.panelDisplay.Size = new System.Drawing.Size(1242, 740);
            this.panelDisplay.TabIndex = 0;
            //
            // panelRight : 오른쪽 조작부
            //
            this.panelRight.Controls.Add(this.lblJudge);
            this.panelRight.Controls.Add(this.cardTotal);
            this.panelRight.Controls.Add(this.cardOk);
            this.panelRight.Controls.Add(this.cardNg);
            this.panelRight.Controls.Add(this.cardYield);
            this.panelRight.Controls.Add(this.btnCountReset);
            this.panelRight.Controls.Add(this.btnAutoRun);
            this.panelRight.Controls.Add(this.btnStop);
            this.panelRight.Controls.Add(this.btnRunOnce);
            this.panelRight.Controls.Add(this.listLog);
            this.panelRight.Controls.Add(this.btnModelLoad);
            this.panelRight.Controls.Add(this.btnLogClear);
            this.panelRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelRight.Location = new System.Drawing.Point(1250, 8);
            this.panelRight.Name = "panelRight";
            this.panelRight.Size = new System.Drawing.Size(642, 740);
            this.panelRight.TabIndex = 1;
            //
            // lblJudge : OK / NG 판정 표시 (색은 RunPage.cs 의 ShowJudge 에서 바꿈)
            //
            this.lblJudge.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(51)))), ((int)(((byte)(56)))));
            this.lblJudge.Font = new System.Drawing.Font("맑은 고딕", 54F, System.Drawing.FontStyle.Bold);
            this.lblJudge.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(163)))), ((int)(((byte)(173)))));
            this.lblJudge.Location = new System.Drawing.Point(10, 0);
            this.lblJudge.Name = "lblJudge";
            this.lblJudge.Size = new System.Drawing.Size(340, 236);
            this.lblJudge.TabIndex = 0;
            this.lblJudge.Text = "-";
            this.lblJudge.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // cardTotal
            //
            this.cardTotal.Location = new System.Drawing.Point(362, 0);
            this.cardTotal.Name = "cardTotal";
            this.cardTotal.Size = new System.Drawing.Size(130, 90);
            this.cardTotal.TabIndex = 1;
            this.cardTotal.Title = "TOTAL COUNT";
            //
            // cardOk : OK 는 초록 테두리 + 초록 숫자
            //
            this.cardOk.AccentColor = System.Drawing.Color.FromArgb(((int)(((byte)(61)))), ((int)(((byte)(220)))), ((int)(((byte)(151)))));
            this.cardOk.Location = new System.Drawing.Point(500, 0);
            this.cardOk.Name = "cardOk";
            this.cardOk.Size = new System.Drawing.Size(130, 90);
            this.cardOk.TabIndex = 2;
            this.cardOk.Title = "OK COUNT";
            this.cardOk.ValueColor = System.Drawing.Color.FromArgb(((int)(((byte)(61)))), ((int)(((byte)(220)))), ((int)(((byte)(151)))));
            //
            // cardNg : NG 는 빨강 테두리 + 빨강 숫자
            //
            this.cardNg.AccentColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(38)))), ((int)(((byte)(30)))));
            this.cardNg.Location = new System.Drawing.Point(500, 98);
            this.cardNg.Name = "cardNg";
            this.cardNg.Size = new System.Drawing.Size(130, 90);
            this.cardNg.TabIndex = 3;
            this.cardNg.Title = "NG COUNT";
            this.cardNg.ValueColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(99)))), ((int)(((byte)(88)))));
            //
            // cardYield : YIELD 는 일반 테두리(TOTAL 과 같음) + 주황 숫자
            //
            this.cardYield.Location = new System.Drawing.Point(362, 98);
            this.cardYield.Name = "cardYield";
            this.cardYield.Size = new System.Drawing.Size(130, 90);
            this.cardYield.TabIndex = 4;
            this.cardYield.Title = "YIELD (%)";
            this.cardYield.Value = "0.00";
            this.cardYield.ValueColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(180)))), ((int)(((byte)(0)))));
            //
            // btnCountReset : 카운트 초기화
            //
            this.btnCountReset.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(51)))), ((int)(((byte)(56)))));
            this.btnCountReset.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(64)))), ((int)(((byte)(72)))));
            this.btnCountReset.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(72)))), ((int)(((byte)(78)))), ((int)(((byte)(86)))));
            this.btnCountReset.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(65)))), ((int)(((byte)(72)))));
            this.btnCountReset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCountReset.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold);
            this.btnCountReset.Location = new System.Drawing.Point(362, 196);
            this.btnCountReset.Name = "btnCountReset";
            this.btnCountReset.Size = new System.Drawing.Size(268, 40);
            this.btnCountReset.TabIndex = 5;
            this.btnCountReset.Text = "COUNT RESET";
            this.btnCountReset.UseVisualStyleBackColor = false;
            this.btnCountReset.Click += new System.EventHandler(this.btnCountReset_Click);
            //
            // btnAutoRun : 파랑
            //
            this.btnAutoRun.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(111)))), ((int)(((byte)(178)))));
            this.btnAutoRun.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(111)))), ((int)(((byte)(178)))));
            this.btnAutoRun.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(88)))), ((int)(((byte)(142)))));
            this.btnAutoRun.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(130)))), ((int)(((byte)(204)))));
            this.btnAutoRun.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAutoRun.Font = new System.Drawing.Font("맑은 고딕", 14F, System.Drawing.FontStyle.Bold);
            this.btnAutoRun.Location = new System.Drawing.Point(10, 250);
            this.btnAutoRun.Name = "btnAutoRun";
            this.btnAutoRun.Size = new System.Drawing.Size(200, 56);
            this.btnAutoRun.TabIndex = 6;
            this.btnAutoRun.Text = "AUTO RUN";
            this.btnAutoRun.UseVisualStyleBackColor = false;
            this.btnAutoRun.Click += new System.EventHandler(this.btnAutoRun_Click);
            //
            // btnStop : 빨강
            //
            this.btnStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(38)))), ((int)(((byte)(30)))));
            this.btnStop.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(38)))), ((int)(((byte)(30)))));
            this.btnStop.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(28)))), ((int)(((byte)(22)))));
            this.btnStop.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(52)))), ((int)(((byte)(43)))));
            this.btnStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStop.Font = new System.Drawing.Font("맑은 고딕", 14F, System.Drawing.FontStyle.Bold);
            this.btnStop.Location = new System.Drawing.Point(220, 250);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(200, 56);
            this.btnStop.TabIndex = 7;
            this.btnStop.Text = "STOP";
            this.btnStop.UseVisualStyleBackColor = false;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            //
            // btnRunOnce
            //
            this.btnRunOnce.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(51)))), ((int)(((byte)(56)))));
            this.btnRunOnce.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(64)))), ((int)(((byte)(72)))));
            this.btnRunOnce.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(72)))), ((int)(((byte)(78)))), ((int)(((byte)(86)))));
            this.btnRunOnce.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(65)))), ((int)(((byte)(72)))));
            this.btnRunOnce.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRunOnce.Font = new System.Drawing.Font("맑은 고딕", 14F, System.Drawing.FontStyle.Bold);
            this.btnRunOnce.Location = new System.Drawing.Point(430, 250);
            this.btnRunOnce.Name = "btnRunOnce";
            this.btnRunOnce.Size = new System.Drawing.Size(200, 56);
            this.btnRunOnce.TabIndex = 8;
            this.btnRunOnce.Text = "RUN ONCE";
            this.btnRunOnce.UseVisualStyleBackColor = false;
            this.btnRunOnce.Click += new System.EventHandler(this.btnRunOnce_Click);
            //
            // listLog : 실행 기록 (종류별 글자색은 RunPage.cs 의 listLog_DrawItem 에서 그림)
            //
            this.listLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(27)))), ((int)(((byte)(31)))));
            this.listLog.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listLog.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.listLog.Font = new System.Drawing.Font("Consolas", 11F);
            this.listLog.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(232)))), ((int)(((byte)(235)))));
            this.listLog.HorizontalScrollbar = true;
            this.listLog.IntegralHeight = false;
            this.listLog.ItemHeight = 22;
            this.listLog.Location = new System.Drawing.Point(10, 320);
            this.listLog.Name = "listLog";
            this.listLog.Size = new System.Drawing.Size(620, 352);
            this.listLog.TabIndex = 9;
            this.listLog.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.listLog_DrawItem);
            //
            // btnModelLoad
            //
            this.btnModelLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnModelLoad.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(51)))), ((int)(((byte)(56)))));
            this.btnModelLoad.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(64)))), ((int)(((byte)(72)))));
            this.btnModelLoad.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(72)))), ((int)(((byte)(78)))), ((int)(((byte)(86)))));
            this.btnModelLoad.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(65)))), ((int)(((byte)(72)))));
            this.btnModelLoad.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnModelLoad.Location = new System.Drawing.Point(10, 684);
            this.btnModelLoad.Name = "btnModelLoad";
            this.btnModelLoad.Size = new System.Drawing.Size(220, 48);
            this.btnModelLoad.TabIndex = 10;
            this.btnModelLoad.Text = "Load Recipe";
            this.btnModelLoad.UseVisualStyleBackColor = false;
            this.btnModelLoad.Click += new System.EventHandler(this.btnModelLoad_Click);
            //
            // btnLogClear
            //
            this.btnLogClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLogClear.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(51)))), ((int)(((byte)(56)))));
            this.btnLogClear.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(64)))), ((int)(((byte)(72)))));
            this.btnLogClear.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(72)))), ((int)(((byte)(78)))), ((int)(((byte)(86)))));
            this.btnLogClear.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(65)))), ((int)(((byte)(72)))));
            this.btnLogClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogClear.Location = new System.Drawing.Point(410, 684);
            this.btnLogClear.Name = "btnLogClear";
            this.btnLogClear.Size = new System.Drawing.Size(220, 48);
            this.btnLogClear.TabIndex = 11;
            this.btnLogClear.Text = "Log Clear";
            this.btnLogClear.UseVisualStyleBackColor = false;
            this.btnLogClear.Click += new System.EventHandler(this.btnLogClear_Click);
            //
            // panelGap : 위 영역과 결과 표 사이 간격
            //
            this.panelGap.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelGap.Location = new System.Drawing.Point(0, 756);
            this.panelGap.Name = "panelGap";
            this.panelGap.Size = new System.Drawing.Size(1900, 8);
            this.panelGap.TabIndex = 1;
            //
            // gridResult : 검사 결과 표 (CogResultsAnalysisTool 의 Input / Output 항목, 3줄 보이고 스크롤)
            //
            this.gridResult.AllowUserToAddRows = false;
            this.gridResult.AllowUserToDeleteRows = false;
            this.gridResult.AllowUserToResizeRows = false;
            alternateStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(35)))), ((int)(((byte)(40)))));
            this.gridResult.AlternatingRowsDefaultCellStyle = alternateStyle;
            this.gridResult.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.gridResult.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(27)))), ((int)(((byte)(31)))));
            this.gridResult.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.gridResult.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.gridResult.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            headerStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            headerStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(35)))), ((int)(((byte)(40)))));
            headerStyle.Font = new System.Drawing.Font("맑은 고딕", 11F, System.Drawing.FontStyle.Bold);
            headerStyle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(232)))), ((int)(((byte)(235)))));
            headerStyle.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            headerStyle.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(35)))), ((int)(((byte)(40)))));
            headerStyle.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(232)))), ((int)(((byte)(235)))));
            this.gridResult.ColumnHeadersDefaultCellStyle = headerStyle;
            this.gridResult.ColumnHeadersHeight = 34;
            this.gridResult.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            cellStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(27)))), ((int)(((byte)(31)))));
            cellStyle.Font = new System.Drawing.Font("Consolas", 11F);
            cellStyle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(232)))), ((int)(((byte)(235)))));
            cellStyle.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            cellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(62)))), ((int)(((byte)(88)))));
            cellStyle.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(232)))), ((int)(((byte)(235)))));
            this.gridResult.DefaultCellStyle = cellStyle;
            this.gridResult.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gridResult.EnableHeadersVisualStyles = false;
            this.gridResult.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(64)))), ((int)(((byte)(72)))));
            this.gridResult.Location = new System.Drawing.Point(0, 764);
            this.gridResult.MultiSelect = false;
            this.gridResult.Name = "gridResult";
            this.gridResult.ReadOnly = true;
            this.gridResult.RowHeadersVisible = false;
            this.gridResult.RowTemplate.Height = 30;
            this.gridResult.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridResult.Size = new System.Drawing.Size(1900, 126);
            this.gridResult.TabIndex = 2;
            //
            // RunPage
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(25)))), ((int)(((byte)(29)))));
            this.Controls.Add(this.panelMain);
            this.Controls.Add(this.panelGap);
            this.Controls.Add(this.gridResult);
            this.Font = new System.Drawing.Font("맑은 고딕", 12F);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(232)))), ((int)(((byte)(235)))));
            this.Name = "RunPage";
            this.Size = new System.Drawing.Size(1900, 890);
            this.panelMain.ResumeLayout(false);
            this.panelRight.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridResult)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.Panel panelDisplay;
        private System.Windows.Forms.Panel panelRight;
        private System.Windows.Forms.Label lblJudge;
        private VProSTD.CountCard cardTotal;
        private VProSTD.CountCard cardOk;
        private VProSTD.CountCard cardNg;
        private VProSTD.CountCard cardYield;
        private System.Windows.Forms.Button btnCountReset;
        private System.Windows.Forms.Button btnAutoRun;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnRunOnce;
        private System.Windows.Forms.ListBox listLog;
        private System.Windows.Forms.Button btnModelLoad;
        private System.Windows.Forms.Button btnLogClear;
        private System.Windows.Forms.Panel panelGap;
        private System.Windows.Forms.DataGridView gridResult;
    }
}
