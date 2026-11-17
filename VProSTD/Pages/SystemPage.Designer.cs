namespace VProSTD
{
    partial class SystemPage
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
            this.groupPixel = new System.Windows.Forms.GroupBox();
            this.lblPixelX = new System.Windows.Forms.Label();
            this.txtPixelX = new System.Windows.Forms.TextBox();
            this.lblPixelY = new System.Windows.Forms.Label();
            this.txtPixelY = new System.Windows.Forms.TextBox();
            this.groupPath = new System.Windows.Forms.GroupBox();
            this.lblBasePath = new System.Windows.Forms.Label();
            this.txtBasePath = new System.Windows.Forms.TextBox();
            this.btnBasePath = new System.Windows.Forms.Button();
            this.lblSystemPath = new System.Windows.Forms.Label();
            this.txtSystemPath = new System.Windows.Forms.TextBox();
            this.btnSystemPath = new System.Windows.Forms.Button();
            this.lblRecipePath = new System.Windows.Forms.Label();
            this.txtRecipePath = new System.Windows.Forms.TextBox();
            this.btnRecipePath = new System.Windows.Forms.Button();
            this.lblLogPath = new System.Windows.Forms.Label();
            this.txtLogPath = new System.Windows.Forms.TextBox();
            this.btnLogPath = new System.Windows.Forms.Button();
            this.lblResultPath = new System.Windows.Forms.Label();
            this.txtResultPath = new System.Windows.Forms.TextBox();
            this.btnResultPath = new System.Windows.Forms.Button();
            this.groupSave = new System.Windows.Forms.GroupBox();
            this.rdoBmp = new System.Windows.Forms.RadioButton();
            this.rdoJpg = new System.Windows.Forms.RadioButton();
            this.chkOriginSave = new System.Windows.Forms.CheckBox();
            this.chkResultSave = new System.Windows.Forms.CheckBox();
            this.btnLoad = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.panelMain.SuspendLayout();
            this.groupPixel.SuspendLayout();
            this.groupPath.SuspendLayout();
            this.groupSave.SuspendLayout();
            this.SuspendLayout();
            //
            // panelMain
            //
            this.panelMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(35)))), ((int)(((byte)(40)))));
            this.panelMain.Controls.Add(this.groupPixel);
            this.panelMain.Controls.Add(this.groupPath);
            this.panelMain.Controls.Add(this.groupSave);
            this.panelMain.Controls.Add(this.btnLoad);
            this.panelMain.Controls.Add(this.btnSave);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 0);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(1900, 890);
            this.panelMain.TabIndex = 0;
            //
            // groupPixel : Pixel Size
            //
            this.groupPixel.Controls.Add(this.lblPixelX);
            this.groupPixel.Controls.Add(this.txtPixelX);
            this.groupPixel.Controls.Add(this.lblPixelY);
            this.groupPixel.Controls.Add(this.txtPixelY);
            this.groupPixel.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold);
            this.groupPixel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(163)))), ((int)(((byte)(173)))));
            this.groupPixel.Location = new System.Drawing.Point(20, 15);
            this.groupPixel.Name = "groupPixel";
            this.groupPixel.Size = new System.Drawing.Size(880, 120);
            this.groupPixel.TabIndex = 0;
            this.groupPixel.TabStop = false;
            this.groupPixel.Text = "Pixel Size";
            //
            // lblPixelX
            //
            this.lblPixelX.Font = new System.Drawing.Font("맑은 고딕", 12F);
            this.lblPixelX.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(232)))), ((int)(((byte)(235)))));
            this.lblPixelX.Location = new System.Drawing.Point(20, 35);
            this.lblPixelX.Name = "lblPixelX";
            this.lblPixelX.Size = new System.Drawing.Size(150, 30);
            this.lblPixelX.TabIndex = 0;
            this.lblPixelX.Text = "Pixel X:";
            this.lblPixelX.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // txtPixelX
            //
            this.txtPixelX.Font = new System.Drawing.Font("맑은 고딕", 12F);
            this.txtPixelX.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(27)))), ((int)(((byte)(31)))));
            this.txtPixelX.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPixelX.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(232)))), ((int)(((byte)(235)))));
            this.txtPixelX.Location = new System.Drawing.Point(20, 70);
            this.txtPixelX.Name = "txtPixelX";
            this.txtPixelX.Size = new System.Drawing.Size(390, 29);
            this.txtPixelX.TabIndex = 1;
            //
            // lblPixelY
            //
            this.lblPixelY.Font = new System.Drawing.Font("맑은 고딕", 12F);
            this.lblPixelY.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(232)))), ((int)(((byte)(235)))));
            this.lblPixelY.Location = new System.Drawing.Point(460, 35);
            this.lblPixelY.Name = "lblPixelY";
            this.lblPixelY.Size = new System.Drawing.Size(150, 30);
            this.lblPixelY.TabIndex = 2;
            this.lblPixelY.Text = "Pixel Y:";
            this.lblPixelY.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // txtPixelY
            //
            this.txtPixelY.Font = new System.Drawing.Font("맑은 고딕", 12F);
            this.txtPixelY.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(27)))), ((int)(((byte)(31)))));
            this.txtPixelY.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPixelY.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(232)))), ((int)(((byte)(235)))));
            this.txtPixelY.Location = new System.Drawing.Point(460, 70);
            this.txtPixelY.Name = "txtPixelY";
            this.txtPixelY.Size = new System.Drawing.Size(390, 29);
            this.txtPixelY.TabIndex = 3;
            //
            // groupPath : Directory Path
            //
            this.groupPath.Controls.Add(this.lblBasePath);
            this.groupPath.Controls.Add(this.txtBasePath);
            this.groupPath.Controls.Add(this.btnBasePath);
            this.groupPath.Controls.Add(this.lblSystemPath);
            this.groupPath.Controls.Add(this.txtSystemPath);
            this.groupPath.Controls.Add(this.btnSystemPath);
            this.groupPath.Controls.Add(this.lblRecipePath);
            this.groupPath.Controls.Add(this.txtRecipePath);
            this.groupPath.Controls.Add(this.btnRecipePath);
            this.groupPath.Controls.Add(this.lblLogPath);
            this.groupPath.Controls.Add(this.txtLogPath);
            this.groupPath.Controls.Add(this.btnLogPath);
            this.groupPath.Controls.Add(this.lblResultPath);
            this.groupPath.Controls.Add(this.txtResultPath);
            this.groupPath.Controls.Add(this.btnResultPath);
            this.groupPath.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold);
            this.groupPath.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(163)))), ((int)(((byte)(173)))));
            this.groupPath.Location = new System.Drawing.Point(20, 150);
            this.groupPath.Name = "groupPath";
            this.groupPath.Size = new System.Drawing.Size(880, 310);
            this.groupPath.TabIndex = 1;
            this.groupPath.TabStop = false;
            this.groupPath.Text = "Directory Path";
            //
            // lblBasePath
            //
            this.lblBasePath.Font = new System.Drawing.Font("맑은 고딕", 12F);
            this.lblBasePath.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(232)))), ((int)(((byte)(235)))));
            this.lblBasePath.Location = new System.Drawing.Point(20, 40);
            this.lblBasePath.Name = "lblBasePath";
            this.lblBasePath.Size = new System.Drawing.Size(150, 34);
            this.lblBasePath.TabIndex = 0;
            this.lblBasePath.Text = "Base Path:";
            this.lblBasePath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // txtBasePath
            //
            this.txtBasePath.Font = new System.Drawing.Font("맑은 고딕", 12F);
            this.txtBasePath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(27)))), ((int)(((byte)(31)))));
            this.txtBasePath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtBasePath.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(232)))), ((int)(((byte)(235)))));
            this.txtBasePath.Location = new System.Drawing.Point(180, 41);
            this.txtBasePath.Name = "txtBasePath";
            this.txtBasePath.Size = new System.Drawing.Size(600, 29);
            this.txtBasePath.TabIndex = 1;
            //
            // btnBasePath : 폴더 선택
            //
            this.btnBasePath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(51)))), ((int)(((byte)(56)))));
            this.btnBasePath.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(64)))), ((int)(((byte)(72)))));
            this.btnBasePath.FlatAppearance.BorderSize = 1;
            this.btnBasePath.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(72)))), ((int)(((byte)(78)))), ((int)(((byte)(86)))));
            this.btnBasePath.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(65)))), ((int)(((byte)(72)))));
            this.btnBasePath.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBasePath.Font = new System.Drawing.Font("맑은 고딕", 12F);
            this.btnBasePath.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(232)))), ((int)(((byte)(235)))));
            this.btnBasePath.Location = new System.Drawing.Point(790, 40);
            this.btnBasePath.Name = "btnBasePath";
            this.btnBasePath.Size = new System.Drawing.Size(60, 32);
            this.btnBasePath.TabIndex = 2;
            this.btnBasePath.Text = "...";
            this.btnBasePath.UseVisualStyleBackColor = false;
            this.btnBasePath.Click += new System.EventHandler(this.btnBasePath_Click);
            //
            // lblSystemPath
            //
            this.lblSystemPath.Font = new System.Drawing.Font("맑은 고딕", 12F);
            this.lblSystemPath.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(232)))), ((int)(((byte)(235)))));
            this.lblSystemPath.Location = new System.Drawing.Point(20, 92);
            this.lblSystemPath.Name = "lblSystemPath";
            this.lblSystemPath.Size = new System.Drawing.Size(150, 34);
            this.lblSystemPath.TabIndex = 3;
            this.lblSystemPath.Text = "System Path:";
            this.lblSystemPath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // txtSystemPath
            //
            this.txtSystemPath.Font = new System.Drawing.Font("맑은 고딕", 12F);
            this.txtSystemPath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(27)))), ((int)(((byte)(31)))));
            this.txtSystemPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSystemPath.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(232)))), ((int)(((byte)(235)))));
            this.txtSystemPath.Location = new System.Drawing.Point(180, 93);
            this.txtSystemPath.Name = "txtSystemPath";
            this.txtSystemPath.Size = new System.Drawing.Size(600, 29);
            this.txtSystemPath.TabIndex = 4;
            //
            // btnSystemPath : 폴더 선택
            //
            this.btnSystemPath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(51)))), ((int)(((byte)(56)))));
            this.btnSystemPath.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(64)))), ((int)(((byte)(72)))));
            this.btnSystemPath.FlatAppearance.BorderSize = 1;
            this.btnSystemPath.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(72)))), ((int)(((byte)(78)))), ((int)(((byte)(86)))));
            this.btnSystemPath.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(65)))), ((int)(((byte)(72)))));
            this.btnSystemPath.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSystemPath.Font = new System.Drawing.Font("맑은 고딕", 12F);
            this.btnSystemPath.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(232)))), ((int)(((byte)(235)))));
            this.btnSystemPath.Location = new System.Drawing.Point(790, 92);
            this.btnSystemPath.Name = "btnSystemPath";
            this.btnSystemPath.Size = new System.Drawing.Size(60, 32);
            this.btnSystemPath.TabIndex = 5;
            this.btnSystemPath.Text = "...";
            this.btnSystemPath.UseVisualStyleBackColor = false;
            this.btnSystemPath.Click += new System.EventHandler(this.btnSystemPath_Click);
            //
            // lblRecipePath
            //
            this.lblRecipePath.Font = new System.Drawing.Font("맑은 고딕", 12F);
            this.lblRecipePath.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(232)))), ((int)(((byte)(235)))));
            this.lblRecipePath.Location = new System.Drawing.Point(20, 144);
            this.lblRecipePath.Name = "lblRecipePath";
            this.lblRecipePath.Size = new System.Drawing.Size(150, 34);
            this.lblRecipePath.TabIndex = 6;
            this.lblRecipePath.Text = "Recipe Path:";
            this.lblRecipePath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // txtRecipePath
            //
            this.txtRecipePath.Font = new System.Drawing.Font("맑은 고딕", 12F);
            this.txtRecipePath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(27)))), ((int)(((byte)(31)))));
            this.txtRecipePath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtRecipePath.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(232)))), ((int)(((byte)(235)))));
            this.txtRecipePath.Location = new System.Drawing.Point(180, 145);
            this.txtRecipePath.Name = "txtRecipePath";
            this.txtRecipePath.Size = new System.Drawing.Size(600, 29);
            this.txtRecipePath.TabIndex = 7;
            //
            // btnRecipePath : 폴더 선택
            //
            this.btnRecipePath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(51)))), ((int)(((byte)(56)))));
            this.btnRecipePath.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(64)))), ((int)(((byte)(72)))));
            this.btnRecipePath.FlatAppearance.BorderSize = 1;
            this.btnRecipePath.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(72)))), ((int)(((byte)(78)))), ((int)(((byte)(86)))));
            this.btnRecipePath.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(65)))), ((int)(((byte)(72)))));
            this.btnRecipePath.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRecipePath.Font = new System.Drawing.Font("맑은 고딕", 12F);
            this.btnRecipePath.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(232)))), ((int)(((byte)(235)))));
            this.btnRecipePath.Location = new System.Drawing.Point(790, 144);
            this.btnRecipePath.Name = "btnRecipePath";
            this.btnRecipePath.Size = new System.Drawing.Size(60, 32);
            this.btnRecipePath.TabIndex = 8;
            this.btnRecipePath.Text = "...";
            this.btnRecipePath.UseVisualStyleBackColor = false;
            this.btnRecipePath.Click += new System.EventHandler(this.btnRecipePath_Click);
            //
            // lblLogPath
            //
            this.lblLogPath.Font = new System.Drawing.Font("맑은 고딕", 12F);
            this.lblLogPath.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(232)))), ((int)(((byte)(235)))));
            this.lblLogPath.Location = new System.Drawing.Point(20, 196);
            this.lblLogPath.Name = "lblLogPath";
            this.lblLogPath.Size = new System.Drawing.Size(150, 34);
            this.lblLogPath.TabIndex = 9;
            this.lblLogPath.Text = "Log Path:";
            this.lblLogPath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // txtLogPath
            //
            this.txtLogPath.Font = new System.Drawing.Font("맑은 고딕", 12F);
            this.txtLogPath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(27)))), ((int)(((byte)(31)))));
            this.txtLogPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtLogPath.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(232)))), ((int)(((byte)(235)))));
            this.txtLogPath.Location = new System.Drawing.Point(180, 197);
            this.txtLogPath.Name = "txtLogPath";
            this.txtLogPath.Size = new System.Drawing.Size(600, 29);
            this.txtLogPath.TabIndex = 10;
            //
            // btnLogPath : 폴더 선택
            //
            this.btnLogPath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(51)))), ((int)(((byte)(56)))));
            this.btnLogPath.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(64)))), ((int)(((byte)(72)))));
            this.btnLogPath.FlatAppearance.BorderSize = 1;
            this.btnLogPath.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(72)))), ((int)(((byte)(78)))), ((int)(((byte)(86)))));
            this.btnLogPath.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(65)))), ((int)(((byte)(72)))));
            this.btnLogPath.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogPath.Font = new System.Drawing.Font("맑은 고딕", 12F);
            this.btnLogPath.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(232)))), ((int)(((byte)(235)))));
            this.btnLogPath.Location = new System.Drawing.Point(790, 196);
            this.btnLogPath.Name = "btnLogPath";
            this.btnLogPath.Size = new System.Drawing.Size(60, 32);
            this.btnLogPath.TabIndex = 11;
            this.btnLogPath.Text = "...";
            this.btnLogPath.UseVisualStyleBackColor = false;
            this.btnLogPath.Click += new System.EventHandler(this.btnLogPath_Click);
            //
            // lblResultPath
            //
            this.lblResultPath.Font = new System.Drawing.Font("맑은 고딕", 12F);
            this.lblResultPath.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(232)))), ((int)(((byte)(235)))));
            this.lblResultPath.Location = new System.Drawing.Point(20, 248);
            this.lblResultPath.Name = "lblResultPath";
            this.lblResultPath.Size = new System.Drawing.Size(150, 34);
            this.lblResultPath.TabIndex = 12;
            this.lblResultPath.Text = "Result Path:";
            this.lblResultPath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // txtResultPath
            //
            this.txtResultPath.Font = new System.Drawing.Font("맑은 고딕", 12F);
            this.txtResultPath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(27)))), ((int)(((byte)(31)))));
            this.txtResultPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtResultPath.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(232)))), ((int)(((byte)(235)))));
            this.txtResultPath.Location = new System.Drawing.Point(180, 249);
            this.txtResultPath.Name = "txtResultPath";
            this.txtResultPath.Size = new System.Drawing.Size(600, 29);
            this.txtResultPath.TabIndex = 13;
            //
            // btnResultPath : 폴더 선택
            //
            this.btnResultPath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(51)))), ((int)(((byte)(56)))));
            this.btnResultPath.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(64)))), ((int)(((byte)(72)))));
            this.btnResultPath.FlatAppearance.BorderSize = 1;
            this.btnResultPath.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(72)))), ((int)(((byte)(78)))), ((int)(((byte)(86)))));
            this.btnResultPath.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(65)))), ((int)(((byte)(72)))));
            this.btnResultPath.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnResultPath.Font = new System.Drawing.Font("맑은 고딕", 12F);
            this.btnResultPath.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(232)))), ((int)(((byte)(235)))));
            this.btnResultPath.Location = new System.Drawing.Point(790, 248);
            this.btnResultPath.Name = "btnResultPath";
            this.btnResultPath.Size = new System.Drawing.Size(60, 32);
            this.btnResultPath.TabIndex = 14;
            this.btnResultPath.Text = "...";
            this.btnResultPath.UseVisualStyleBackColor = false;
            this.btnResultPath.Click += new System.EventHandler(this.btnResultPath_Click);
            //
            // groupSave : Save Image Type
            //
            this.groupSave.Controls.Add(this.rdoBmp);
            this.groupSave.Controls.Add(this.rdoJpg);
            this.groupSave.Controls.Add(this.chkOriginSave);
            this.groupSave.Controls.Add(this.chkResultSave);
            this.groupSave.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold);
            this.groupSave.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(163)))), ((int)(((byte)(173)))));
            this.groupSave.Location = new System.Drawing.Point(20, 475);
            this.groupSave.Name = "groupSave";
            this.groupSave.Size = new System.Drawing.Size(880, 120);
            this.groupSave.TabIndex = 2;
            this.groupSave.TabStop = false;
            this.groupSave.Text = "Save Image Type";
            //
            // rdoBmp
            //
            this.rdoBmp.Checked = true;
            this.rdoBmp.Font = new System.Drawing.Font("맑은 고딕", 12F);
            this.rdoBmp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(232)))), ((int)(((byte)(235)))));
            this.rdoBmp.Location = new System.Drawing.Point(20, 35);
            this.rdoBmp.Name = "rdoBmp";
            this.rdoBmp.Size = new System.Drawing.Size(200, 32);
            this.rdoBmp.TabIndex = 0;
            this.rdoBmp.TabStop = true;
            this.rdoBmp.Text = "BMP";
            //
            // rdoJpg
            //
            this.rdoJpg.Font = new System.Drawing.Font("맑은 고딕", 12F);
            this.rdoJpg.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(232)))), ((int)(((byte)(235)))));
            this.rdoJpg.Location = new System.Drawing.Point(460, 35);
            this.rdoJpg.Name = "rdoJpg";
            this.rdoJpg.Size = new System.Drawing.Size(200, 32);
            this.rdoJpg.TabIndex = 1;
            this.rdoJpg.Text = "JPG";
            //
            // chkOriginSave
            //
            this.chkOriginSave.Checked = true;
            this.chkOriginSave.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkOriginSave.Font = new System.Drawing.Font("맑은 고딕", 12F);
            this.chkOriginSave.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(232)))), ((int)(((byte)(235)))));
            this.chkOriginSave.Location = new System.Drawing.Point(20, 75);
            this.chkOriginSave.Name = "chkOriginSave";
            this.chkOriginSave.Size = new System.Drawing.Size(250, 32);
            this.chkOriginSave.TabIndex = 2;
            this.chkOriginSave.Text = "Origin Save";
            //
            // chkResultSave
            //
            this.chkResultSave.Checked = true;
            this.chkResultSave.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkResultSave.Font = new System.Drawing.Font("맑은 고딕", 12F);
            this.chkResultSave.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(232)))), ((int)(((byte)(235)))));
            this.chkResultSave.Location = new System.Drawing.Point(460, 75);
            this.chkResultSave.Name = "chkResultSave";
            this.chkResultSave.Size = new System.Drawing.Size(250, 32);
            this.chkResultSave.TabIndex = 3;
            this.chkResultSave.Text = "Result Save";
            //
            // btnLoad
            //
            this.btnLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLoad.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(51)))), ((int)(((byte)(56)))));
            this.btnLoad.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(64)))), ((int)(((byte)(72)))));
            this.btnLoad.FlatAppearance.BorderSize = 2;
            this.btnLoad.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(72)))), ((int)(((byte)(78)))), ((int)(((byte)(86)))));
            this.btnLoad.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(65)))), ((int)(((byte)(72)))));
            this.btnLoad.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLoad.Font = new System.Drawing.Font("맑은 고딕", 14F);
            this.btnLoad.Location = new System.Drawing.Point(1590, 745);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(280, 52);
            this.btnLoad.TabIndex = 3;
            this.btnLoad.Text = "LOAD";
            this.btnLoad.UseVisualStyleBackColor = false;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            //
            // btnSave
            //
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(51)))), ((int)(((byte)(56)))));
            this.btnSave.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(64)))), ((int)(((byte)(72)))));
            this.btnSave.FlatAppearance.BorderSize = 2;
            this.btnSave.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(72)))), ((int)(((byte)(78)))), ((int)(((byte)(86)))));
            this.btnSave.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(65)))), ((int)(((byte)(72)))));
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("맑은 고딕", 14F);
            this.btnSave.Location = new System.Drawing.Point(1590, 809);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(280, 52);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "SAVE";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            //
            // SystemPage
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(25)))), ((int)(((byte)(29)))));
            this.Controls.Add(this.panelMain);
            this.Font = new System.Drawing.Font("맑은 고딕", 12F);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(232)))), ((int)(((byte)(235)))));
            this.Name = "SystemPage";
            this.Size = new System.Drawing.Size(1900, 890);
            this.panelMain.ResumeLayout(false);
            this.groupPixel.ResumeLayout(false);
            this.groupPixel.PerformLayout();
            this.groupPath.ResumeLayout(false);
            this.groupPath.PerformLayout();
            this.groupSave.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.GroupBox groupPixel;
        private System.Windows.Forms.Label lblPixelX;
        private System.Windows.Forms.TextBox txtPixelX;
        private System.Windows.Forms.Label lblPixelY;
        private System.Windows.Forms.TextBox txtPixelY;
        private System.Windows.Forms.GroupBox groupPath;
        private System.Windows.Forms.Label lblBasePath;
        private System.Windows.Forms.TextBox txtBasePath;
        private System.Windows.Forms.Button btnBasePath;
        private System.Windows.Forms.Label lblSystemPath;
        private System.Windows.Forms.TextBox txtSystemPath;
        private System.Windows.Forms.Button btnSystemPath;
        private System.Windows.Forms.Label lblRecipePath;
        private System.Windows.Forms.TextBox txtRecipePath;
        private System.Windows.Forms.Button btnRecipePath;
        private System.Windows.Forms.Label lblLogPath;
        private System.Windows.Forms.TextBox txtLogPath;
        private System.Windows.Forms.Button btnLogPath;
        private System.Windows.Forms.Label lblResultPath;
        private System.Windows.Forms.TextBox txtResultPath;
        private System.Windows.Forms.Button btnResultPath;
        private System.Windows.Forms.GroupBox groupSave;
        private System.Windows.Forms.RadioButton rdoBmp;
        private System.Windows.Forms.RadioButton rdoJpg;
        private System.Windows.Forms.CheckBox chkOriginSave;
        private System.Windows.Forms.CheckBox chkResultSave;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Button btnSave;
    }
}
