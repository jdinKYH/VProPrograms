using Cognex.VisionPro;
using Cognex.VisionPro.Display;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace VProSTD
{
    /// <summary>
    /// Log 한 줄의 종류. 종류마다 글자색이 다르다.
    /// </summary>
    public enum LogKind
    {
        Info,       // 일반 (흰색)
        Ok,         // OK 판정 (초록)
        Ng,         // NG 판정 / 오류 (빨강)
        Warning     // 경고 (주황)
    }

    /// <summary>
    /// [RUN] 탭: 검사 실행 화면
    ///   왼쪽   : 검사 결과 이미지 (CogRecordDisplay)
    ///   오른쪽 : OK/NG, 카운트 카드, AUTO RUN / STOP / RUN ONCE, Log
    ///   아래   : 검사 결과 표 (CogResultsAnalysisTool 의 출력 항목, 최신이 맨 위)
    /// </summary>
    public partial class RunPage : UserControl
    {
        private const int MaxGridRows = 1000;   // 결과 표 최대 줄 수 (넘으면 오래된 줄부터 지움)
        private const int MaxLogLines = 2000;   // 화면 Log 최대 줄 수

        private VisionApp app;
        private CogRecordDisplay display;       // 검사 결과 이미지 화면
        private Timer autoTimer;                // AUTO RUN 반복용

        /// <summary>
        /// Log 목록에 넣는 한 줄 (글자 + 종류)
        /// </summary>
        private class LogLine
        {
            public string Text;
            public LogKind Kind;

            public override string ToString()
            {
                return Text;
            }
        }

        public RunPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// MainForm 이 프로그램 시작 때 한 번 부른다.
        /// </summary>
        public void Init(VisionApp visionApp)
        {
            app = visionApp;
            app.LogAdded += AddLog;
            app.RecipeChanged += OnRecipeChanged;

            // 이미지 화면은 Cognex 컨트롤이라 디자이너 대신 코드로 넣는다
            display = new CogRecordDisplay();
            display.Dock = DockStyle.Fill;
            panelDisplay.Controls.Add(display);

            autoTimer = new Timer();
            autoTimer.Interval = 10;
            autoTimer.Tick += autoTimer_Tick;

            ShowJudge(null);
            ShowCounts(0, 0, 0);
            UpdateButtons();
        }

        private void OnRecipeChanged()
        {
            gridResult.Rows.Clear();
            gridResult.Columns.Clear();
            ShowJudge(null);
        }

        #region 검사 실행

        private void btnRunOnce_Click(object sender, EventArgs e)
        {
            RunOneInspection();
        }

        /// <summary>
        /// 검사 1회 실행 후 화면 갱신. 결과를 돌려준다 (실행하지 못했으면 null).
        /// </summary>
        private InspectionResult RunOneInspection()
        {
            if (app.Job == null)
            {
                app.WriteLog("레시피를 먼저 불러오세요. (Load Recipe)", LogKind.Warning);
                return null;
            }

            InspectionResult result;
            try
            {
                result = app.InspectOnce();
            }
            catch (Exception ex)
            {
                app.WriteLog("RUN 오류: " + ex.Message, LogKind.Ng);
                return null;
            }

            ShowResult(result);
            return result;
        }

        /// <summary>
        /// 결과를 화면에 표시하고, 이미지를 저장한다.
        /// </summary>
        private void ShowResult(InspectionResult result)
        {
            ShowJudge(result.Ok);
            ShowCounts(app.TotalCount, app.OkCount, app.NgCount);
            AddGridRow(result);

            // 이미지 화면
            try
            {
                if (result.Record != null) display.Record = result.Record;
                else if (result.Image != null) display.Image = result.Image;
                display.Fit(true);
            }
            catch (Exception ex)
            {
                app.WriteLog("화면 표시 실패: " + ex.Message, LogKind.Warning);
            }

            // 이미지 저장 (Result Save 는 검사 그래픽이 그려진 화면을 그림으로 만들어 저장)
            if (result.Image == null) return;

            Image resultBitmap = null;
            try
            {
                if (app.System.ResultSave)
                {
                    resultBitmap = display.CreateContentBitmap(CogDisplayContentBitmapConstants.Image, null, 0);
                }
            }
            catch (Exception ex)
            {
                app.WriteLog("결과 화면 그림 만들기 실패: " + ex.Message, LogKind.Warning);
            }

            app.SaveImages(result, resultBitmap);
            if (resultBitmap != null) resultBitmap.Dispose();
        }

        #endregion

        #region AUTO RUN / STOP

        private void btnAutoRun_Click(object sender, EventArgs e)
        {
            if (app.Job == null)
            {
                app.WriteLog("레시피를 먼저 불러오세요. (Load Recipe)", LogKind.Warning);
                return;
            }

            // File Mode 에서 이미 끝까지 읽었으면 처음부터 다시
            if (app.FileMode && !app.Files.HasNext) app.Files.Rewind();

            app.IsAutoRunning = true;
            app.WriteLog("AUTO RUN 시작");
            UpdateButtons();
            autoTimer.Start();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            StopAutoRun("STOP");
        }

        /// <summary>
        /// 반복 실행: 검사 1회가 끝나면 다음 검사를 예약한다.
        /// </summary>
        private void autoTimer_Tick(object sender, EventArgs e)
        {
            autoTimer.Stop();   // 검사가 끝날 때까지 다음 Tick 이 오지 않게

            InspectionResult result = RunOneInspection();
            if (!app.IsAutoRunning) return;   // 검사 중에 STOP 을 눌렀음

            if (result == null)
            {
                StopAutoRun("실행 오류");
                return;
            }
            // 카메라 모드에서 이미지를 얻지 못하면 멈춘다 (연결 끊김 등).
            // File Mode 는 읽을 수 없는 파일을 오류로 기록하고 다음 파일로 넘어간다.
            if (!app.FileMode && result.IsError && result.Image == null)
            {
                StopAutoRun("카메라에서 이미지를 얻지 못함");
                return;
            }
            if (app.FileMode && !app.Files.HasNext)
            {
                StopAutoRun("폴더의 마지막 이미지까지 검사함");
                return;
            }

            autoTimer.Start();
        }

        private void StopAutoRun(string reason)
        {
            if (!app.IsAutoRunning) return;

            autoTimer.Stop();
            app.IsAutoRunning = false;
            app.WriteLog("AUTO RUN 정지: " + reason, LogKind.Warning);
            UpdateButtons();
        }

        /// <summary>
        /// MainForm 이 종료할 때 부른다.
        /// </summary>
        public void StopIfRunning()
        {
            StopAutoRun("프로그램 종료");
        }

        private void UpdateButtons()
        {
            bool running = app != null && app.IsAutoRunning;
            btnAutoRun.Enabled = !running;
            btnRunOnce.Enabled = !running;
            btnModelLoad.Enabled = !running;
            btnStop.Enabled = running;
        }

        #endregion

        #region 판정 / 카운트 표시

        /// <summary>
        /// 판정 칸 표시. ok = true(OK, 초록) / false(NG, 빨강) / null(대기, 회색)
        /// </summary>
        public void ShowJudge(bool? ok)
        {
            if (ok == true)
            {
                lblJudge.Text = "OK";
                lblJudge.BackColor = Theme.OkFill;
                lblJudge.ForeColor = Color.White;
            }
            else if (ok == false)
            {
                lblJudge.Text = "NG";
                lblJudge.BackColor = Theme.NgFill;
                lblJudge.ForeColor = Color.White;
            }
            else
            {
                lblJudge.Text = "-";
                lblJudge.BackColor = Theme.Button;
                lblJudge.ForeColor = Theme.SubText;
            }
        }

        /// <summary>
        /// 카운트 카드 4개를 갱신한다. YIELD = OK / TOTAL x 100
        /// </summary>
        public void ShowCounts(int total, int ok, int ng)
        {
            double yield = 0;
            if (total > 0) yield = ok * 100.0 / total;

            cardTotal.Value = total.ToString("N0");   // 1,420 처럼 쉼표 넣기
            cardOk.Value = ok.ToString("N0");
            cardNg.Value = ng.ToString("N0");
            cardYield.Value = yield.ToString("0.00");
        }

        private void btnCountReset_Click(object sender, EventArgs e)
        {
            DialogResult answer = MessageBox.Show("카운트를 0 으로 초기화할까요?", "COUNT RESET",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (answer != DialogResult.Yes) return;

            app.ResetCounts();
            ShowCounts(0, 0, 0);
            app.WriteLog("COUNT RESET");
        }

        #endregion

        #region 결과 표

        /// <summary>
        /// 결과 표 맨 위에 한 줄 추가. 열 = Time, Judge, 출력 항목들.
        /// 출력 항목이 바뀌면 열을 다시 만든다.
        /// </summary>
        private void AddGridRow(InspectionResult result)
        {
            // 오류 결과(출력 값 없음)는 열을 바꾸지 않고 Time / Judge 만 채운다 (표가 지워지지 않게)
            bool noOutputs = result.OutputNames.Count == 0 && gridResult.Columns.Count > 0;

            if (!noOutputs && !SameColumns(result))
            {
                gridResult.Rows.Clear();
                gridResult.Columns.Clear();
                gridResult.Columns.Add("Time", "Time");
                gridResult.Columns.Add("Judge", "Judge");
                foreach (string name in result.OutputNames)
                {
                    gridResult.Columns.Add(name, name);
                }
            }

            object[] cells = new object[gridResult.Columns.Count];
            cells[0] = DateTime.Now.ToString("HH:mm:ss.fff");
            cells[1] = result.JudgeText;
            for (int i = 0; i < result.OutputValues.Count && 2 + i < cells.Length; i++)
            {
                cells[2 + i] = result.OutputValues[i];
            }

            gridResult.Rows.Insert(0, cells);

            // Judge 칸 글자색
            Color judgeColor = Theme.NgText;
            if (result.Ok) judgeColor = Theme.OkText;
            gridResult.Rows[0].Cells[1].Style.ForeColor = judgeColor;

            while (gridResult.Rows.Count > MaxGridRows)
            {
                gridResult.Rows.RemoveAt(gridResult.Rows.Count - 1);
            }
            gridResult.ClearSelection();
        }

        private bool SameColumns(InspectionResult result)
        {
            if (gridResult.Columns.Count != 2 + result.OutputNames.Count) return false;
            for (int i = 0; i < result.OutputNames.Count; i++)
            {
                if (gridResult.Columns[2 + i].Name != result.OutputNames[i]) return false;
            }
            return true;
        }

        #endregion

        #region Load Recipe

        private void btnModelLoad_Click(object sender, EventArgs e)
        {
            string name = RecipeSelectForm.Select(app.System.RecipePath, app.RecipeName);
            if (name == null) return;

            try
            {
                app.LoadRecipe(name);
            }
            catch (Exception ex)
            {
                app.WriteLog("레시피 불러오기 실패: " + ex.Message, LogKind.Ng);
            }
        }

        #endregion

        #region Log

        /// <summary>
        /// Log 에 한 줄 추가 (시간과 함께, 맨 아래에)
        /// </summary>
        public void AddLog(string text, LogKind kind)
        {
            LogLine line = new LogLine();
            line.Text = "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "] " + text;
            line.Kind = kind;

            listLog.Items.Add(line);
            while (listLog.Items.Count > MaxLogLines) listLog.Items.RemoveAt(0);
            listLog.TopIndex = listLog.Items.Count - 1;   // 마지막 줄이 보이게
        }

        /// <summary>
        /// Log 한 줄을 종류별 색으로 그린다 (listLog.DrawMode = OwnerDrawFixed).
        /// </summary>
        private void listLog_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index < 0) return;

            LogLine line = listLog.Items[e.Index] as LogLine;
            if (line == null) return;

            Color color = Theme.Text;
            if (line.Kind == LogKind.Ok) color = Theme.OkText;
            else if (line.Kind == LogKind.Ng) color = Theme.NgText;
            else if (line.Kind == LogKind.Warning) color = Theme.Amber;

            TextRenderer.DrawText(e.Graphics, line.Text, e.Font, e.Bounds, color,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        }

        private void btnLogClear_Click(object sender, EventArgs e)
        {
            listLog.Items.Clear();
        }

        #endregion
    }
}
