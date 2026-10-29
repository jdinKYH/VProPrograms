using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

// ===============================================================================
//  261029@Claude [구조 변경 3] 기준값 입력 창 (Settings 버튼)
//  VPP(CogResultsAnalysisTool1)에 판정식을 두지 않고, 이 창에서 입력한 기준값으로 프로그램이 판정한다.
//
//    항목          공차 사용   Min        Max
//    Left Dist1      [v]      [ 0.000]   [ 0.000]
//    Left Dist2      [v]      ...
//    Right Dist1     [v]
//    Right Dist2     [v]
//    PMAlign 최소 점수         [0.600]      (Locate 출력 AlignScore 기준값)
//    [XML 파일 열기]                         [저장] [취소]
//  261029@Claude [VProManager02] 결과 분석 판정 반영 체크박스 제거
//
//  - 저장하면 InspectionSettings.xml 에 쓰고, 다음 검사부터 반영된다. (검사마다 파일을 다시 읽음)
//  - 공차를 쓰는 항목은 Min ≤ Max 여야 저장된다.
//  - 출력 이름·번호, 저장 여부 같은 나머지 항목은 "XML 파일 열기"로 메모장에서 고친다.
//  - Designer 없이 코드로만 만든 창이다. (TableLayoutPanel + AutoSize로 DPI·글꼴 크기에 맞춰 늘어남)
// ===============================================================================
namespace VProManager02
{
    internal class SettingsForm : Form
    {
        // 측정값 입력 범위·자릿수 (단위는 측정 ToolBlock 출력 단위 그대로: pixel 또는 mm)
        private const int ValueDecimalPlaces = 3;
        private const decimal ValueLimit = 1000000m;

        private readonly InspectionSettings settings;

        // 측정 항목 4개 (Form1의 txtLeftDist1~txtRightDist2 순서와 같다)
        private readonly MeasureSpec[] specs;
        private readonly CheckBox[] chkUseTolerance;
        private readonly NumericUpDown[] numMin;
        private readonly NumericUpDown[] numMax;

        private readonly NumericUpDown numPMAlignMinScore;


        /// <summary>
        /// settings의 현재 값으로 입력 칸을 채운다. 저장을 누르기 전까지 settings는 바꾸지 않는다.
        /// </summary>
        public SettingsForm(InspectionSettings settings)
        {
            this.settings = settings;
            specs = new[] { settings.LeftDist1, settings.LeftDist2, settings.RightDist1, settings.RightDist2 };
            chkUseTolerance = new CheckBox[specs.Length];
            numMin = new NumericUpDown[specs.Length];
            numMax = new NumericUpDown[specs.Length];

            Text = "검사 기준값 설정";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            AutoScaleMode = AutoScaleMode.Font;
            Font = new Font("맑은 고딕", 10F);
            Padding = new Padding(12);

            TableLayoutPanel table = new TableLayoutPanel
            {
                ColumnCount = 4,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Fill,
            };

            // 머리글
            table.Controls.Add(createLabel("항목", true), 0, 0);
            table.Controls.Add(createLabel("공차 사용", true), 1, 0);
            table.Controls.Add(createLabel("Min", true), 2, 0);
            table.Controls.Add(createLabel("Max", true), 3, 0);

            // 측정 항목 한 줄씩: 이름 | 공차 사용 | Min | Max
            for (int i = 0; i < specs.Length; i++)
            {
                int row = i + 1;
                MeasureSpec spec = specs[i];

                chkUseTolerance[i] = new CheckBox { Checked = spec.UseTolerance, AutoSize = true, Anchor = AnchorStyles.None };
                numMin[i] = createValueBox(spec.Min);
                numMax[i] = createValueBox(spec.Max);

                // 공차를 쓰지 않으면 Min/Max는 판정에 쓰이지 않으므로 입력을 막아 혼동을 줄인다.
                int index = i;      // 람다에서 쓸 반복 변수 복사
                chkUseTolerance[i].CheckedChanged += (s, e) => updateEnabled(index);
                updateEnabled(i);

                table.Controls.Add(createLabel(spec.DisplayName + "  (" + spec.OutputName + ")", false), 0, row);
                table.Controls.Add(chkUseTolerance[i], 1, row);
                table.Controls.Add(numMin[i], 2, row);
                table.Controls.Add(numMax[i], 3, row);
            }

            // PMAlign 최소 점수 (0~1, 이 값보다 작으면 위치 확인 실패)
            int scoreRow = specs.Length + 1;
            numPMAlignMinScore = new NumericUpDown
            {
                Minimum = 0m,
                Maximum = 1m,
                DecimalPlaces = 3,
                Increment = 0.01m,
                Width = 110,
                TextAlign = HorizontalAlignment.Right,
                Value = clamp((decimal)settings.PMAlignMinScore, 0m, 1m),
            };
            table.Controls.Add(createLabel("Align 최소 점수 (AlignScore, 0~1)", false), 0, scoreRow);
            table.Controls.Add(numPMAlignMinScore, 2, scoreRow);

            // 261029@Claude [VProManager02] 결과 분석 판정 반영 체크박스 제거 (결과 분석은 화면 Grid 표시 전용)

            // 버튼 줄: 왼쪽 XML 파일 열기, 오른쪽 저장·취소
            Button btnOpenXml = new Button { Text = "XML 파일 열기", AutoSize = true };
            Button btnSave = new Button { Text = "저장", AutoSize = true, MinimumSize = new Size(80, 0) };
            Button btnCancel = new Button { Text = "취소", AutoSize = true, MinimumSize = new Size(80, 0), DialogResult = DialogResult.Cancel };
            btnOpenXml.Click += btnOpenXml_Click;
            btnSave.Click += btnSave_Click;

            FlowLayoutPanel rightButtons = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight, Anchor = AnchorStyles.Right };
            rightButtons.Controls.Add(btnSave);
            rightButtons.Controls.Add(btnCancel);

            int buttonRow = scoreRow + 1;
            btnOpenXml.Margin = new Padding(3, 16, 3, 3);
            rightButtons.Margin = new Padding(3, 16, 3, 3);
            table.Controls.Add(btnOpenXml, 0, buttonRow);
            table.Controls.Add(rightButtons, 2, buttonRow);
            table.SetColumnSpan(rightButtons, 2);

            Controls.Add(table);
            AcceptButton = btnSave;
            CancelButton = btnCancel;
        }


        /// <summary>
        /// 저장: 입력값 확인 → settings에 반영 → InspectionSettings.xml 저장 → 창 닫기(OK)
        /// 확인에 실패하면 창을 닫지 않고 고칠 칸으로 커서를 옮긴다.
        /// </summary>
        private void btnSave_Click(object sender, EventArgs e)
        {
            // 설정 파일이 잘못된 상태면 Save()가 사용자 파일을 보호하려고 저장하지 않는다. (검토 보완 1)
            // 조용히 무시되면 저장된 줄 알고 넘어가므로 먼저 알린다.
            if (!string.IsNullOrEmpty(InspectionSettings.LastLoadError))
            {
                MessageBox.Show(this, "설정 파일에 오류가 있어 저장할 수 없습니다.\n'XML 파일 열기'로 파일을 먼저 고치세요.\n\n"
                                + InspectionSettings.LastLoadError, Text);
                return;
            }

            // 공차를 쓰는 항목은 Min ≤ Max (Min > Max면 항상 NG가 된다)
            for (int i = 0; i < specs.Length; i++)
            {
                if (chkUseTolerance[i].Checked && numMin[i].Value > numMax[i].Value)
                {
                    MessageBox.Show(this, specs[i].DisplayName + ": Min이 Max보다 큽니다.", Text);
                    numMin[i].Focus();
                    return;
                }
            }

            for (int i = 0; i < specs.Length; i++)
            {
                specs[i].UseTolerance = chkUseTolerance[i].Checked;
                specs[i].Min = (double)numMin[i].Value;
                specs[i].Max = (double)numMax[i].Value;
            }
            settings.PMAlignMinScore = (double)numPMAlignMinScore.Value;

            settings.Save();
            AppLogger.Info("기준값 저장: " + describe());

            DialogResult = DialogResult.OK;
            Close();
        }


        /// <summary>
        /// 나머지 설정(출력 이름·번호, 저장 여부 등)을 메모장으로 연다. (기존 Settings 버튼 동작)
        /// 메모장에서 고친 내용은 다음 검사 때 다시 읽힌다. 이 창의 입력값과 겹치는 항목은 이 창에서 저장한 값이 우선한다.
        /// </summary>
        private void btnOpenXml_Click(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists(InspectionSettings.FilePath))
                {
                    settings.Save();
                }
                System.Diagnostics.Process.Start("notepad.exe", "\"" + InspectionSettings.FilePath + "\"");
            }
            catch (Exception ex)
            {
                AppLogger.Error("설정 파일 열기 실패", ex);
                MessageBox.Show(this, $"설정 파일을 열 수 없습니다: {ex.Message}\n{InspectionSettings.FilePath}", Text);
            }
        }


        /// <summary>공차 사용 체크에 따라 Min/Max 입력 가능 여부</summary>
        private void updateEnabled(int index)
        {
            numMin[index].Enabled = chkUseTolerance[index].Checked;
            numMax[index].Enabled = chkUseTolerance[index].Checked;
        }


        /// <summary>로그용 요약 (예: "Left Dist1=[1.000~2.000], Left Dist2=미사용, ..., PMAlign=0.600")</summary>
        private string describe()
        {
            string text = "";
            foreach (MeasureSpec spec in specs)
            {
                text += spec.DisplayName + "=" + (spec.UseTolerance ? "[" + spec.Min.ToString("F3") + "~" + spec.Max.ToString("F3") + "]" : "미사용") + ", ";
            }
            return text + "PMAlign=" + settings.PMAlignMinScore.ToString("F3");
        }


        private static Label createLabel(string text, bool isHeader)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(3, 6, 12, 6),
                Font = isHeader ? new Font("맑은 고딕", 10F, FontStyle.Bold) : null,
            };
        }


        private static NumericUpDown createValueBox(double value)
        {
            // XML을 직접 고쳐 NaN이나 아주 큰 값이 들어 있으면 decimal 변환에서 예외가 나므로 먼저 범위 안으로
            if (double.IsNaN(value))
            {
                value = 0.0;
            }
            value = Math.Max((double)-ValueLimit, Math.Min((double)ValueLimit, value));

            return new NumericUpDown
            {
                Minimum = -ValueLimit,
                Maximum = ValueLimit,
                DecimalPlaces = ValueDecimalPlaces,
                Increment = 0.01m,
                Width = 110,
                TextAlign = HorizontalAlignment.Right,
                Value = clamp((decimal)value, -ValueLimit, ValueLimit),
            };
        }


        /// <summary>NumericUpDown 범위를 벗어난 값이면 예외가 나므로 범위 안으로 맞춘다.</summary>
        private static decimal clamp(decimal value, decimal min, decimal max)
        {
            return Math.Max(min, Math.Min(max, value));
        }
    }
}
