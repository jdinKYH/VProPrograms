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

        // 261029@Claude [FHD 고정] 화면 구성을 FHD(1920x1080) 기준 고정 배치로 변경
        //  - 클라이언트 영역 1900 x 980 (테두리·제목줄 포함 약 1916 x 1019 → FHD 작업 영역 안에 들어감)
        //  - 창 크기 조절·최대화 불가 (FixedSingle, MaximizeBox=false), 화면 가운데에 띄움
        //  - AutoScaleMode=None, 글꼴 단위 Pixel: Windows 화면 배율(125% 등)과 관계없이 픽셀 위치·크기가 그대로
        //  - 배치 (x 좌표 기준)
        //      10 ~ 790     Input Image  (CogRecordDisplay1, 780 x 884, Form1.cs에서 생성)
        //      800 ~ 1580   Result Image (CogRecordDisplay2, 780 x 884)
        //      1590 ~ 1890  오른쪽 패널: FileMode, Model Load, 측정값 4개, Align, 판정(OK/NG),
        //                   아래쪽 Folder Run, Settings, Run
        //      y 940 ~ 970  상태 표시줄 (화면 두 개 아래, 넘치는 글자는 ... 으로 표시)
        //  - 기존 배치(1668 x 903, 최대화, 오른쪽 Anchor)는 Git 이력(이전 커밋)에 있다.
        //  - 글꼴: 굴림(Point) → 맑은 고딕(Pixel)으로 통일
        //    단, 폼 자체 글꼴(this.Font)은 Point여야 한다. CogRecordDisplay는 ActiveX 컨트롤이라 폼 글꼴을 물려받는데
        //    Pixel 글꼴이면 "ActiveX controls only accept fonts that are defined in GraphicsUnit.Point" 예외로 창이 뜨지 않는다.
        //    (AutoScaleMode=None이라 폼 글꼴이 Point여도 배치 크기는 바뀌지 않는다)
        // 주의: Visual Studio 디자이너에서 저장하면 InitializeComponent 안의 주석은 지워지므로 설명은 여기에 둔다.

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
            // chkFileMode
            //
            this.chkFileMode.AutoSize = true;
            this.chkFileMode.Font = new System.Drawing.Font("맑은 고딕", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.chkFileMode.Location = new System.Drawing.Point(1590, 12);
            this.chkFileMode.Name = "chkFileMode";
            this.chkFileMode.Size = new System.Drawing.Size(107, 29);
            this.chkFileMode.TabIndex = 1;
            this.chkFileMode.Text = "FileMode";
            this.chkFileMode.UseVisualStyleBackColor = true;
            this.chkFileMode.CheckedChanged += new System.EventHandler(this.chkFileMode_CheckedChanged);
            //
            // btnLoadModel
            //
            this.btnLoadModel.Font = new System.Drawing.Font("맑은 고딕", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.btnLoadModel.Location = new System.Drawing.Point(1590, 48);
            this.btnLoadModel.Name = "btnLoadModel";
            this.btnLoadModel.Size = new System.Drawing.Size(300, 44);
            this.btnLoadModel.TabIndex = 2;
            this.btnLoadModel.Text = "Model Load";
            this.btnLoadModel.UseVisualStyleBackColor = true;
            this.btnLoadModel.Click += new System.EventHandler(this.btnLoadModel_Click);
            //
            // lblInputImage
            //
            this.lblInputImage.Font = new System.Drawing.Font("맑은 고딕", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.lblInputImage.Location = new System.Drawing.Point(10, 8);
            this.lblInputImage.Name = "lblInputImage";
            this.lblInputImage.Size = new System.Drawing.Size(780, 34);
            this.lblInputImage.TabIndex = 20;
            this.lblInputImage.Text = "Input Image";
            this.lblInputImage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lblResultImage
            //
            this.lblResultImage.Font = new System.Drawing.Font("맑은 고딕", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.lblResultImage.Location = new System.Drawing.Point(800, 8);
            this.lblResultImage.Name = "lblResultImage";
            this.lblResultImage.Size = new System.Drawing.Size(780, 34);
            this.lblResultImage.TabIndex = 21;
            this.lblResultImage.Text = "Result Image";
            this.lblResultImage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lblLeftDist1
            //
            this.lblLeftDist1.Font = new System.Drawing.Font("맑은 고딕", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.lblLeftDist1.Location = new System.Drawing.Point(1590, 112);
            this.lblLeftDist1.Name = "lblLeftDist1";
            this.lblLeftDist1.Size = new System.Drawing.Size(130, 34);
            this.lblLeftDist1.TabIndex = 3;
            this.lblLeftDist1.Text = "Left Dist1";
            this.lblLeftDist1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // txtLeftDist1
            //
            this.txtLeftDist1.Font = new System.Drawing.Font("맑은 고딕", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.txtLeftDist1.Location = new System.Drawing.Point(1725, 112);
            this.txtLeftDist1.Name = "txtLeftDist1";
            this.txtLeftDist1.ReadOnly = true;
            this.txtLeftDist1.Size = new System.Drawing.Size(165, 34);
            this.txtLeftDist1.TabIndex = 4;
            this.txtLeftDist1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            //
            // lblLeftDist2
            //
            this.lblLeftDist2.Font = new System.Drawing.Font("맑은 고딕", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.lblLeftDist2.Location = new System.Drawing.Point(1590, 152);
            this.lblLeftDist2.Name = "lblLeftDist2";
            this.lblLeftDist2.Size = new System.Drawing.Size(130, 34);
            this.lblLeftDist2.TabIndex = 5;
            this.lblLeftDist2.Text = "Left Dist2";
            this.lblLeftDist2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // txtLeftDist2
            //
            this.txtLeftDist2.Font = new System.Drawing.Font("맑은 고딕", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.txtLeftDist2.Location = new System.Drawing.Point(1725, 152);
            this.txtLeftDist2.Name = "txtLeftDist2";
            this.txtLeftDist2.ReadOnly = true;
            this.txtLeftDist2.Size = new System.Drawing.Size(165, 34);
            this.txtLeftDist2.TabIndex = 6;
            this.txtLeftDist2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            //
            // lblRightDist1
            //
            this.lblRightDist1.Font = new System.Drawing.Font("맑은 고딕", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.lblRightDist1.Location = new System.Drawing.Point(1590, 192);
            this.lblRightDist1.Name = "lblRightDist1";
            this.lblRightDist1.Size = new System.Drawing.Size(130, 34);
            this.lblRightDist1.TabIndex = 7;
            this.lblRightDist1.Text = "Right Dist1";
            this.lblRightDist1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // txtRightDist1
            //
            this.txtRightDist1.Font = new System.Drawing.Font("맑은 고딕", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.txtRightDist1.Location = new System.Drawing.Point(1725, 192);
            this.txtRightDist1.Name = "txtRightDist1";
            this.txtRightDist1.ReadOnly = true;
            this.txtRightDist1.Size = new System.Drawing.Size(165, 34);
            this.txtRightDist1.TabIndex = 8;
            this.txtRightDist1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            //
            // lblRightDist2
            //
            this.lblRightDist2.Font = new System.Drawing.Font("맑은 고딕", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.lblRightDist2.Location = new System.Drawing.Point(1590, 232);
            this.lblRightDist2.Name = "lblRightDist2";
            this.lblRightDist2.Size = new System.Drawing.Size(130, 34);
            this.lblRightDist2.TabIndex = 9;
            this.lblRightDist2.Text = "Right Dist2";
            this.lblRightDist2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // txtRightDist2
            //
            this.txtRightDist2.Font = new System.Drawing.Font("맑은 고딕", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.txtRightDist2.Location = new System.Drawing.Point(1725, 232);
            this.txtRightDist2.Name = "txtRightDist2";
            this.txtRightDist2.ReadOnly = true;
            this.txtRightDist2.Size = new System.Drawing.Size(165, 34);
            this.txtRightDist2.TabIndex = 10;
            this.txtRightDist2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            //
            // lblAlign
            //
            this.lblAlign.Font = new System.Drawing.Font("맑은 고딕", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.lblAlign.Location = new System.Drawing.Point(1590, 282);
            this.lblAlign.Name = "lblAlign";
            this.lblAlign.Size = new System.Drawing.Size(130, 34);
            this.lblAlign.TabIndex = 11;
            this.lblAlign.Text = "Align";
            this.lblAlign.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // txtAlign
            //
            this.txtAlign.Font = new System.Drawing.Font("맑은 고딕", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.txtAlign.Location = new System.Drawing.Point(1725, 282);
            this.txtAlign.Name = "txtAlign";
            this.txtAlign.ReadOnly = true;
            this.txtAlign.Size = new System.Drawing.Size(165, 34);
            this.txtAlign.TabIndex = 12;
            this.txtAlign.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            //
            // lblJudge
            //
            this.lblJudge.Font = new System.Drawing.Font("맑은 고딕", 64F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.lblJudge.Location = new System.Drawing.Point(1590, 336);
            this.lblJudge.Name = "lblJudge";
            this.lblJudge.Size = new System.Drawing.Size(300, 130);
            this.lblJudge.TabIndex = 13;
            this.lblJudge.Text = "-";
            this.lblJudge.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblJudge.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            //
            // btnFolderRun
            //
            this.btnFolderRun.Font = new System.Drawing.Font("맑은 고딕", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.btnFolderRun.Location = new System.Drawing.Point(1590, 780);
            this.btnFolderRun.Name = "btnFolderRun";
            this.btnFolderRun.Size = new System.Drawing.Size(300, 44);
            this.btnFolderRun.TabIndex = 14;
            this.btnFolderRun.Text = "Folder Run";
            this.btnFolderRun.UseVisualStyleBackColor = true;
            this.btnFolderRun.Click += new System.EventHandler(this.btnFolderRun_Click);
            //
            // btnSettings
            //
            this.btnSettings.Font = new System.Drawing.Font("맑은 고딕", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.btnSettings.Location = new System.Drawing.Point(1590, 834);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(300, 44);
            this.btnSettings.TabIndex = 15;
            this.btnSettings.Text = "Settings";
            this.btnSettings.UseVisualStyleBackColor = true;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            //
            // btnRun
            //
            this.btnRun.Font = new System.Drawing.Font("맑은 고딕", 30F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.btnRun.Location = new System.Drawing.Point(1590, 890);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(300, 80);
            this.btnRun.TabIndex = 0;
            this.btnRun.Text = "Run";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            //
            // lblStatus
            //
            this.lblStatus.Font = new System.Drawing.Font("맑은 고딕", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblStatus.Location = new System.Drawing.Point(10, 940);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(1570, 30);
            this.lblStatus.TabIndex = 22;
            this.lblStatus.Text = "";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblStatus.AutoEllipsis = true;
            this.lblStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            //
            // Form1
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1900, 980);
            this.Controls.Add(this.btnFolderRun);
            this.Controls.Add(this.btnSettings);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lblJudge);
            this.Controls.Add(this.btnLoadModel);
            this.Controls.Add(this.lblAlign);
            this.Controls.Add(this.txtAlign);
            this.Controls.Add(this.txtRightDist2);
            this.Controls.Add(this.txtRightDist1);
            this.Controls.Add(this.txtLeftDist2);
            this.Controls.Add(this.txtLeftDist1);
            this.Controls.Add(this.lblRightDist1);
            this.Controls.Add(this.lblRightDist2);
            this.Controls.Add(this.lblLeftDist2);
            this.Controls.Add(this.lblLeftDist1);
            this.Controls.Add(this.lblResultImage);
            this.Controls.Add(this.lblInputImage);
            this.Controls.Add(this.chkFileMode);
            this.Controls.Add(this.btnRun);
            this.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "VProManager01";
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

