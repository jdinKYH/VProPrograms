// Cognex
using Cognex.VisionPro;
using Cognex.VisionPro.Display;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

// 261028@Claude [4단계] 사용하지 않는 using 주석 처리 (VisionPro 처리는 VisionService.cs로 이동)
//using Cognex.VisionPro.CalibFix;
//using Cognex.VisionPro.ImageFile;
//using Cognex.VisionPro.ImageProcessing;
//using Cognex.VisionPro.PMAlign;
//using Cognex.VisionPro.QuickBuild;
//using Cognex.VisionPro.QuickBuild.Implementation.Internal;   // QuickBuild 내부 API (4-4)
//using Cognex.VisionPro.ToolBlock;
//using Cognex.VisionPro.ToolGroup;
//using System.ComponentModel;
//using System.Data;
//using System.Text;
//using System.Threading.Tasks;



// ===============================================================================
//  1023@jdin "1023_QB_CBoxNoSwitch.vpp" 사용할 것
//  tbImageSource를 사용하지 않고
//  CogIPOneImageTool1을 사용하여 InputImage를 직접 주입하는 방식으로 변경
//  입력은 CogAcqFifoTool로 입력이 기본, CheckBox를 통해 파일 입력 모드로 변경
//
//  261028@Claude [4단계] 구조 정리
//  - Form1.cs            : 화면 표시와 사용자 입력
//  - VisionService.cs    : VisionPro 로드·실행·결과 추출
//  - InspectionSettings  : 설정 파일 (InspectionSettings.xml)
//  - InspectionResult    : 검사 결과, CSV·NG 이미지 저장
//  - BatchInspection     : 폴더 일괄 검사, 회귀 테스트 (expected.csv)
//  - AppLogger           : 파일 로그 (Logs\yyyyMMdd.log)
//  이 파일 아래쪽에 3단계까지의 Form1.cs 전체가 주석으로 남아 있다.
// ===============================================================================
namespace VproManageBlack02
{
    public partial class Form1 : Form
    {
        // 261028@Claude [4단계 6] VisionPro 객체는 VisionService가 관리
        private readonly VisionService vision = new VisionService();

        // 261019@jdin 폼 클래스 내부에 선언
        // 참조에 Cognex.VisionPro.Controls가 있어야 된다.
        private CogRecordDisplay cogRecordDisplay1;
        private CogRecordDisplay cogRecordDisplay2;

        // 261028@Claude [3단계] 검사 설정, 판정·상태 표시, 결과 저장용
        private InspectionSettings settings;
        private string currentImageSource = "";     // 이번 검사 이미지 출처 (파일 경로 또는 "Camera")
        private ICogImage currentInputImage;        // 이번 검사 입력 이미지 (NG 이미지 저장용)
        private long lastElapsedMs;
        private int okCount;
        private int ngCount;
        private string lastErrorMessage = "";       // 261028@Claude [4단계] 일괄 검사에서 오류 내용 기록용

        // 261029@Claude [검토 보완 2] 검사 중 상태 (창 닫기 막기, 일괄 검사 중지)
        private bool isBusy;
        private bool isBatchRunning;
        private bool isStopRequested;
        private bool isCloseRequested;

        // 261029@Claude [검토 보완 1, 6, 9] 상태 표시줄 경고
        private string lastSaveWarning = "";            // CSV 저장 경고
        private string lastShownSettingsError = "";     // 이미 MessageBox로 알린 설정 파일 오류

        // 261028@Claude [4단계] 화면 배치 (여백)
        // 261029@Claude [검토 보완 3] 오른쪽 패널 폭·화면 위치는 고정값 대신 실제 컨트롤 위치로 계산 (DPI 배율 대응)
        //private const int RightPanelWidth = 230;
        //private const int DisplayTop = 50;
        private const int DisplayMargin = 30;
        private const int DisplayGap = 20;


        public Form1()
        {
            InitializeComponent();

            // 261028@Claude [3단계 3-2] 검사 설정 읽기 (InspectionSettings.xml)
            // (읽기 오류는 화면이 뜬 뒤 Form1_Shown에서 알림)
            settings = InspectionSettings.Load();

            // 코드로 CogRecordDisplay 생성 (위치·크기는 layoutDisplays()에서 창 크기에 맞춰 정함)
            cogRecordDisplay1 = new CogRecordDisplay();
            cogRecordDisplay2 = new CogRecordDisplay();

            // 폼에 컨트롤 추가
            this.Controls.Add(cogRecordDisplay1);   // input image
            this.Controls.Add(cogRecordDisplay2);   // result image

            // 261028@Claude [4단계 5-4] 창 크기가 바뀌면 화면 배치를 다시 계산
            this.Resize += (sender, e) => layoutDisplays();
            layoutDisplays();

            // 261028@Claude [2단계 2-1] 종료 시 VisionPro 자원 해제
            this.FormClosing += Form1_FormClosing;

            // 261028@Claude [3단계] 화면이 뜬 뒤 마지막 VPP 자동 로드
            this.Shown += Form1_Shown;
            clearInspectResults();
            updateStatusDisplay();
        }


        /// <summary>
        /// 261028@Claude [4단계 5-4] 두 화면을 창 크기에 맞춰 배치
        /// 오른쪽 패널(버튼·결과)은 Designer에서 오른쪽에 고정(Anchor)되어 있다.
        /// </summary>
        private void layoutDisplays()
        {
            if (cogRecordDisplay1 == null || cogRecordDisplay2 == null)
            {
                return;
            }

            // 261029@Claude [검토 보완 3] DPI 배율에 맞춰 여백을 키우고,
            // 화면 오른쪽 끝은 오른쪽 패널 컨트롤 중 가장 왼쪽 위치, 화면 위쪽은 제목 라벨 아래로 계산한다.
            float scale = DeviceDpi / 96f;
            int margin = (int)(DisplayMargin * scale);
            int gap = (int)(DisplayGap * scale);

            int rightPanelLeft = new Control[]
            {
                chkFileMode, btnLoadModel, lblLeftDist1, lblLeftDist2, lblRightDist1, lblRightDist2, lblAlign,
                txtLeftDist1, lblJudge, btnRun, btnSettings, btnFolderRun
            }.Min(c => c.Left);
            int top = Math.Max(lblInputImage.Bottom, lblResultImage.Bottom) + gap / 2;
            int bottom = lblStatus.Top - gap / 2;

            //int bottom = lblStatus.Top - 10;
            //int availableWidth = ClientSize.Width - RightPanelWidth - DisplayMargin;
            //int width = Math.Max(100, (availableWidth - DisplayGap) / 2);
            //int height = Math.Max(100, bottom - DisplayTop);
            int availableWidth = rightPanelLeft - gap - margin;
            int width = Math.Max(100, (availableWidth - gap) / 2);
            int height = Math.Max(100, bottom - top);

            //cogRecordDisplay1.SetBounds(DisplayMargin, DisplayTop, width, height);
            //cogRecordDisplay2.SetBounds(DisplayMargin + width + DisplayGap, DisplayTop, width, height);
            cogRecordDisplay1.SetBounds(margin, top, width, height);
            cogRecordDisplay2.SetBounds(margin + width + gap, top, width, height);

            // 화면 제목은 각 화면의 가운데 위에
            lblInputImage.Left = cogRecordDisplay1.Left + (width - lblInputImage.Width) / 2;
            lblResultImage.Left = cogRecordDisplay2.Left + (width - lblResultImage.Width) / 2;
        }


        /// <summary>
        /// 261028@Claude [3단계] 마지막으로 로드에 성공한 VPP를 자동으로 로드
        /// </summary>
        private void Form1_Shown(object sender, EventArgs e)
        {
            layoutDisplays();

            // 261029@Claude [검토 보완 1] 시작할 때 설정 파일이 잘못되어 있으면 알린다.
            showSettingsLoadError();

            if (!string.IsNullOrEmpty(settings.LastVppPath) && File.Exists(settings.LastVppPath))
            {
                AppLogger.Info("마지막 VPP 자동 로드: " + settings.LastVppPath);
                // 261029@Claude [검토 보완] 자동 로드 때는 카메라 툴 없음 경고창을 띄우지 않고 상태 표시줄에만 표시
                // LoadVisionProJob(settings.LastVppPath);
                LoadVisionProJob(settings.LastVppPath, false);
            }
            updateStatusDisplay();
        }


        /// <summary>
        /// 261028@Claude [2단계 2-1] 폼 종료 시 Job Manager와 카메라(FrameGrabber) 해제
        /// </summary>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 261029@Claude [검토 보완 2] 일괄 검사 중에는 바로 닫지 않고, 현재 이미지 검사가 끝나면 멈춘 뒤 닫는다.
            // (기존: 검사 반복이 도는 중에 Shutdown되어 정리된 툴·화면을 계속 사용할 수 있었음)
            if (isBusy)
            {
                e.Cancel = true;
                isStopRequested = true;
                isCloseRequested = true;
                lblStatus.Text = "검사를 멈춘 뒤 프로그램을 닫습니다...";
                return;
            }

            vision.Shutdown();
        }


        /// <summary>
        /// Model Load 버튼: QuickBuild 응용프로그램 Job 파일(VPP) 선택
        /// 261028@Claude [4단계] 이름 변경: button1_Click_1 → btnLoadModel_Click, btnLoadJob_Click() → selectAndLoadJob()
        /// </summary>
        private void btnLoadModel_Click(object sender, EventArgs e)
        {
            selectAndLoadJob();
        }


        private void selectAndLoadJob()
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "VisionPro Job Files (*.vpp)|*.vpp";
                    openFileDialog.Title = "Select a VisionPro Job File";
                    // 마지막 VPP 폴더에서 열기
                    if (!string.IsNullOrEmpty(settings.LastVppPath))
                    {
                        openFileDialog.InitialDirectory = Path.GetDirectoryName(settings.LastVppPath);
                    }
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        LoadVisionProJob(openFileDialog.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error("VPP 파일 선택/로드 오류", ex);
                MessageBox.Show("Error loading job file: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        /// <summary>
        /// VPP 로드 (검증·롤백은 VisionService.LoadJob, 여기서는 화면 처리만)
        /// </summary>
        // 261029@Claude [검토 보완] 자동 로드 때는 카메라 툴 없음 경고창을 띄우지 않도록 인자 추가
        // private void LoadVisionProJob(string vppPath)
        private void LoadVisionProJob(string vppPath, bool showCameraWarning = true)
        {
            Cursor previousCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                VisionService.LoadResult loadResult = vision.LoadJob(vppPath);
                if (!loadResult.IsSuccess)
                {
                    MessageBox.Show(loadResult.Message, "VPP 로드 실패", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 마지막 VPP 경로 저장(다음 실행 시 자동 로드), 상태 표시 갱신
                settings.LastVppPath = vppPath;
                settings.Save();
                updateStatusDisplay();

                // 카메라 툴은 파일 모드만 사용할 때는 없어도 되므로 경고만 한다. (상태 표시줄에도 표시)
                // if (!vision.HasCamera)
                if (!vision.HasCamera && showCameraWarning)
                {
                    MessageBox.Show("툴그룹 내부에 'CogAcqFifoTool1' 툴이 존재하지 않습니다. 파일 모드만 사용할 수 있습니다.");
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error("VPP 파일 로드 실패: " + vppPath, ex);
                MessageBox.Show($"VPP 파일 로드 실패: {AppLogger.GetFullMessage(ex)}");
            }
            finally
            {
                Cursor.Current = previousCursor;
            }
        }


        /// <summary>
        /// Run 버튼: 검사 실행 (라이브 vs 오프라인 파일 모드 분기)
        /// 261028@Claude [4단계] 이름 변경: button1_Click → btnRun_Click
        /// </summary>
        private void btnRun_Click(object sender, EventArgs e)
        {
            // 메모장으로 바꾼 설정을 검사마다 다시 읽어 반영 (3단계 3-2)
            // 261029@Claude [검토 보완 1] 읽기에 실패하면 직전 설정을 유지하고 알림
            // settings = InspectionSettings.Load();
            reloadSettings();

            // UI의 체크박스 상태로 모드 판별 (true: 파일 검사, false: 라이브 카메라)
            bool isOfflineFileMode = chkFileMode.Checked;

            // 모드별로 필요한 객체만 확인 (1단계 1-5)
            string notReadyMessage = vision.GetNotReadyMessage(isOfflineFileMode);
            if (notReadyMessage != null)
            {
                MessageBox.Show(notReadyMessage);
                return;
            }

            // 파일 모드는 검사 전에 이미지를 고른다. 취소·실패는 검사가 아니므로 판정·저장하지 않는다. (1단계 1-2)
            ICogImage fileImage = null;
            string fileSource = "";
            if (isOfflineFileMode)
            {
                fileImage = LoadImageFromFile(out fileSource);
                if (fileImage == null)
                {
                    return;
                }
            }

            // 261028@Claude [4단계 2-5] 검사 중 버튼 재진입 방지
            setBusy(true);
            try
            {
                runInspection(isOfflineFileMode, fileImage, fileSource, true);
            }
            finally
            {
                setBusy(false);
            }
        }


        /// <summary>
        /// 261028@Claude [4단계] 검사 1회 (단건 검사와 폴더 일괄 검사가 함께 사용)
        /// 실행 → 위치 확인 → 측정 판정 → OK/NG 표시 → 결과 저장 (3단계 흐름 유지)
        /// - showMessages가 false(일괄 검사)면 MessageBox를 띄우지 않고 lastErrorMessage에 남긴다.
        /// - 검사를 하지 못하면(카메라 취득 실패, 예외) null을 반환한다.
        /// </summary>
        private InspectionResult runInspection(bool isOfflineFileMode, ICogImage fileImage, string fileSource, bool showMessages)
        {
            lastErrorMessage = "";

            // Clear Display 1, 2, 이전 검사 결과
            clearDisplayAll();
            clearInspectResults();

            InspectionResult result = new InspectionResult();
            result.IsOfflineFileMode = isOfflineFileMode;
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            currentInputImage = null;

            try
            {
                if (isOfflineFileMode)
                {
                    // --- [1] 오프라인 파일 모드 ---
                    currentInputImage = fileImage;
                    currentImageSource = fileSource;

                    cogRecordDisplay1.Image = fileImage;
                    cogRecordDisplay1.Fit();

                    vision.RunWithImage(fileImage);
                }
                else
                {
                    // --- [2] 라이브 카메라 모드 ---
                    ICogImage acquiredImage;
                    string acquireError = vision.RunWithCamera(out acquiredImage);
                    if (acquireError != null)
                    {
                        lastErrorMessage = acquireError;
                        if (showMessages)
                        {
                            MessageBox.Show(acquireError + "\n카메라 취득에 실패했습니다.");
                        }
                        return null;
                    }
                    currentInputImage = acquiredImage;
                    currentImageSource = "Camera";
                }

                // Tool 실행 후 처리된 이미지를 입력 화면에 표시
                cogRecordDisplay1.Image = vision.ProcessedImage;
                cogRecordDisplay1.Fit();
                result.ImageSource = currentImageSource;

                // 위치 확인 (3단계 3-2: 기준 점수는 설정 파일)
                VisionService.LocateResult locateResult = vision.CheckLocate(settings.PMAlignMinScore);
                txtAlign.Text = locateResult.Text;
                result.IsLocateOk = locateResult.IsOk;
                result.AlignText = locateResult.Text;
                result.AlignScore = locateResult.Score;

                if (result.IsLocateOk)
                {
                    // 261029@Claude [검토 보완 4] 결과 화면 표시가 실패해도 판정과 저장은 계속한다.
                    // (기존: 레코드 키가 없는 등으로 예외가 나면 판정·CSV 기록까지 건너뜀)
                    try
                    {
                        // 결과 디스플레이 바인딩 (Locate.CogFixtureTool1.OutputImage)
                        cogRecordDisplay2.Record = vision.CreateResultRecord();
                        cogRecordDisplay2.Fit();
                    }
                    catch (Exception ex)
                    {
                        AppLogger.Error("결과 화면 표시 실패 (판정은 계속)", ex);
                    }

                    // LEFTside/RIGHTside 측정 그래픽 표시 (3단계 3-7, 내부에서 예외 처리)
                    showMeasureGraphics();
                }

                // 측정값 읽기와 공차 판정 (위치 확인 실패 시 "-" 표시)
                evaluateMeasures(result, result.IsLocateOk);

                stopwatch.Stop();
                result.ElapsedMs = stopwatch.ElapsedMilliseconds;

                // OK/NG 표시, 카운트, 결과 저장
                // 261029@Claude [검토 보완 5] 일괄 검사(showMessages=false)는 생산 기록(일일 CSV, NG 이미지, 카운트)에 넣지 않음
                // completeInspection(result);
                completeInspection(result, !showMessages);

                if (!result.IsLocateOk)
                {
                    lastErrorMessage = "위치 확인 실패 (" + locateResult.Text + ")";
                    if (showMessages)
                    {
                        MessageBox.Show("위치 확인에 실패했습니다.");
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                AppLogger.Error("검사 실행 오류 (FileMode=" + isOfflineFileMode + ", Source=" + fileSource + ")", ex);
                lastErrorMessage = AppLogger.GetFullMessage(ex);
                if (showMessages)
                {
                    MessageBox.Show($"검사 실행 오류 : {ex.Message}");
                }
                return null;
            }
        }


        /// <summary>
        /// 이미지 파일 선택 후 ICogImage로 읽기. 취소하면 조용히 null, 읽기 실패는 안내 후 null
        /// </summary>
        private ICogImage LoadImageFromFile(out string imagePath)
        {
            imagePath = "";
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "비전 검사할 이미지 선택";
                openFileDialog.Filter = "이미지 파일 (*.bmp;*.jpg;*.png)|*.bmp;*.jpg;*.png|모든 파일 (*.*)|*.*";
                // 마지막으로 이미지를 연 폴더에서 시작 (없으면 기존 경로)
                openFileDialog.InitialDirectory = Directory.Exists(settings.LastImageFolder)
                    ? settings.LastImageFolder
                    : @"D:\0_JDIN\JDIN_VisionProgram\jdinVPro\VPro\VproBlack";

                if (openFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return null;    // 사용자가 취소
                }

                imagePath = openFileDialog.FileName;
                settings.LastImageFolder = Path.GetDirectoryName(imagePath);
                settings.Save();

                try
                {
                    return VisionService.LoadImageFile(imagePath);
                }
                catch (Exception ex)
                {
                    AppLogger.Error("이미지 파일 로드 오류: " + imagePath, ex);
                    MessageBox.Show($"이미지 처리 중 에러 발생: {ex.Message}\n이미지 파일을 불러오지 못했습니다.");
                    return null;
                }
            }
        }


        /// <summary>
        /// 화면 1, 2의 이미지·그래픽 제거
        /// </summary>
        private void clearDisplayAll()
        {
            cogRecordDisplay1.StaticGraphics.Clear();       // 고정 그래픽 제거
            cogRecordDisplay1.InteractiveGraphics.Clear();  // 인터랙티브 그래픽 제거
            cogRecordDisplay1.Record = null;                // Record로 표시했다면 제거
            cogRecordDisplay1.Image = null;                 // 이미지 제거
            cogRecordDisplay1.Fit();

            cogRecordDisplay2.StaticGraphics.Clear();
            cogRecordDisplay2.InteractiveGraphics.Clear();
            cogRecordDisplay2.Record = null;
            cogRecordDisplay2.Image = null;
            cogRecordDisplay2.Fit();
        }


        /// <summary>
        /// 검사 실행 전에 검사 결과를 초기화
        /// </summary>
        private void clearInspectResults()
        {
            txtLeftDist1.Text = "";
            txtLeftDist2.Text = "";
            txtRightDist1.Text = "";
            txtRightDist2.Text = "";
            txtAlign.Text = "";

            // 판정 표시와 NG 강조 색 초기화
            lblJudge.Text = "-";
            lblJudge.BackColor = SystemColors.Control;
            lblJudge.ForeColor = SystemColors.ControlText;
            foreach (TextBox box in measureTextBoxes())
            {
                box.BackColor = SystemColors.Control;
            }
        }


        private TextBox[] measureTextBoxes()
        {
            return new[] { txtLeftDist1, txtLeftDist2, txtRightDist1, txtRightDist2 };
        }


        /// <summary>
        /// 측정값 4개를 판정해서 TextBox에 표시, 공차를 벗어난 값은 빨간색 (3단계 3-1, 3-4)
        /// </summary>
        private void evaluateMeasures(InspectionResult result, bool readValues)
        {
            result.Measures.Add(vision.EvaluateMeasure(true, settings.LeftDist1, readValues));
            result.Measures.Add(vision.EvaluateMeasure(true, settings.LeftDist2, readValues));
            result.Measures.Add(vision.EvaluateMeasure(false, settings.RightDist1, readValues));
            result.Measures.Add(vision.EvaluateMeasure(false, settings.RightDist2, readValues));

            TextBox[] boxes = measureTextBoxes();
            for (int i = 0; i < boxes.Length; i++)
            {
                boxes[i].Text = result.Measures[i].Text;
                boxes[i].BackColor = (readValues && !result.Measures[i].IsOk) ? Color.LightPink : SystemColors.Control;
            }
        }


        /// <summary>
        /// LEFTside/RIGHTside의 측정 그래픽을 결과 화면(Display2)에 겹쳐 표시 (3단계 3-7)
        /// 그래픽 표시에 실패해도 검사 결과에는 영향이 없도록 로그만 남긴다.
        /// </summary>
        private void showMeasureGraphics()
        {
            try
            {
                foreach (ICogRecord record in vision.CreateMeasureRecords())
                {
                    addRecordGraphics(record);
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error("측정 그래픽 표시 실패", ex);
            }
        }


        /// <summary>
        /// 레코드(하위 레코드 포함)에 들어 있는 그래픽을 Display2에 추가
        /// </summary>
        private void addRecordGraphics(ICogRecord record)
        {
            if (record == null)
            {
                return;
            }

            if (record.Content is CogGraphicCollection graphics)
            {
                foreach (ICogGraphic graphic in graphics)
                {
                    cogRecordDisplay2.StaticGraphics.Add(graphic, "Measure");
                }
            }
            else if (record.Content is ICogGraphic graphic)
            {
                cogRecordDisplay2.StaticGraphics.Add(graphic, "Measure");
            }

            if (record.SubRecords != null)
            {
                for (int i = 0; i < record.SubRecords.Count; i++)
                {
                    addRecordGraphics(record.SubRecords[i]);
                }
            }
        }


        /// <summary>
        /// 검사 마무리: OK/NG 표시, 카운트, CSV·NG 이미지 저장, 상태 표시 (3단계 3-1, 5-2)
        /// </summary>
        /// 261029@Claude [검토 보완 5] isBatch(폴더 일괄 검사)면 판정만 표시하고
        /// 생산 기록(OK/NG 카운트, 일일 CSV, NG 이미지)에는 넣지 않는다. 결과는 일괄 검사 보고서에 남는다.
        /// 261029@Claude [검토 보완 6] CSV 저장 경고를 상태 표시줄에 표시
        private void completeInspection(InspectionResult result, bool isBatch)
        {
            showJudge(result.IsOk);
            lastElapsedMs = result.ElapsedMs;

            if (isBatch)
            {
                return;
            }

            if (result.IsOk)
            {
                okCount++;
            }
            else
            {
                ngCount++;
            }

            if (settings.SaveResultCsv)
            {
                // ResultRecorder.AppendCsv(result);
                lastSaveWarning = ResultRecorder.AppendCsv(result) ?? "";
            }
            if (!result.IsOk && settings.SaveNgImage)
            {
                ResultRecorder.SaveNgImage(currentInputImage, result.Time);
            }

            updateStatusDisplay();
        }

        //private void completeInspection(InspectionResult result)
        //{
        //    showJudge(result.IsOk);

        //    if (result.IsOk)
        //    {
        //        okCount++;
        //    }
        //    else
        //    {
        //        ngCount++;
        //    }
        //    lastElapsedMs = result.ElapsedMs;

        //    if (settings.SaveResultCsv)
        //    {
        //        ResultRecorder.AppendCsv(result);
        //    }
        //    if (!result.IsOk && settings.SaveNgImage)
        //    {
        //        ResultRecorder.SaveNgImage(currentInputImage, result.Time);
        //    }

        //    updateStatusDisplay();
        //}


        /// <summary>
        /// 최종 OK/NG를 크게 표시
        /// </summary>
        private void showJudge(bool isOk)
        {
            lblJudge.Text = isOk ? "OK" : "NG";
            lblJudge.BackColor = isOk ? Color.LimeGreen : Color.Red;
            lblJudge.ForeColor = Color.White;
        }


        /// <summary>
        /// 제목줄과 상태 표시줄 갱신 (VPP, 모드, 검사 시간, OK/NG 카운트) (3단계 5-2)
        /// </summary>
        private void updateStatusDisplay()
        {
            string vppName = string.IsNullOrEmpty(vision.LoadedVppPath) ? "(VPP 없음)" : Path.GetFileName(vision.LoadedVppPath);
            string mode = chkFileMode.Checked ? "파일" : "카메라";

            this.Text = "VproManageBlack02 - " + vppName;
            lblStatus.Text = $"VPP: {vppName}    |    모드: {mode}    |    검사 시간: {lastElapsedMs} ms    |    OK: {okCount}    NG: {ngCount}";

            // 261029@Claude [검토 보완 1, 6, 9] 설정 파일 오류, 공차 미설정, CSV 저장 문제, 카메라 툴 없음을 함께 표시
            List<string> warnings = new List<string>();
            if (!string.IsNullOrEmpty(InspectionSettings.LastLoadError))
            {
                warnings.Add("설정 파일 오류(직전 설정 사용)");
            }
            string settingsWarning = settings.GetWarningText();
            if (settingsWarning.Length > 0)
            {
                warnings.Add(settingsWarning);
            }
            if (lastSaveWarning.Length > 0)
            {
                warnings.Add(lastSaveWarning);
            }
            if (!string.IsNullOrEmpty(vision.LoadedVppPath) && !vision.HasCamera)
            {
                warnings.Add("카메라 툴 없음(파일 모드만 가능)");
            }

            if (warnings.Count > 0)
            {
                lblStatus.Text += "    |    ⚠ " + string.Join(" / ", warnings);
                lblStatus.ForeColor = Color.Red;
            }
            else
            {
                lblStatus.ForeColor = SystemColors.ControlText;
            }
        }


        /// <summary>
        /// 261029@Claude [검토 보완 1] 설정 파일을 다시 읽는다. 실패하면 직전 설정을 유지하고 알린다.
        /// </summary>
        private void reloadSettings()
        {
            settings = InspectionSettings.Load(settings);
            showSettingsLoadError();
            updateStatusDisplay();
        }


        /// <summary>
        /// 261029@Claude [검토 보완 1] 설정 파일 읽기 오류를 MessageBox로 알린다. (같은 오류는 한 번만)
        /// </summary>
        private void showSettingsLoadError()
        {
            string error = InspectionSettings.LastLoadError;
            if (string.IsNullOrEmpty(error))
            {
                lastShownSettingsError = "";
                return;
            }
            if (error == lastShownSettingsError)
            {
                return;
            }
            lastShownSettingsError = error;
            MessageBox.Show(
                "설정 파일을 읽을 수 없어 직전 설정으로 검사합니다.\n"
                + "파일을 고치기 전까지 설정(마지막 VPP·폴더 등)은 저장되지 않습니다.\n\n"
                + InspectionSettings.FilePath + "\n" + error,
                "설정 파일 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }


        /// <summary>
        /// 261028@Claude [4단계 2-5] 검사 중에는 버튼과 모드 선택을 막는다.
        /// </summary>
        /// 261029@Claude [검토 보완 2] 일괄 검사 중에는 Folder Run 버튼이 "Stop" 버튼이 된다.
        /// 261029@Claude [검토 보완] DoEvents 후에도 대기 커서가 유지되도록 UseWaitCursor 사용
        private void setBusy(bool busy, bool batch = false)
        {
            isBusy = busy;
            isBatchRunning = busy && batch;

            btnRun.Enabled = !busy;
            btnLoadModel.Enabled = !busy;
            btnFolderRun.Enabled = !busy || batch;
            btnFolderRun.Text = isBatchRunning ? "Stop" : "Folder Run";
            btnSettings.Enabled = !busy;
            chkFileMode.Enabled = !busy;
            // Cursor.Current = isBusy ? Cursors.WaitCursor : Cursors.Default;
            UseWaitCursor = busy;
            btnFolderRun.UseWaitCursor = false;     // Stop 버튼은 일반 커서
        }

        //private void setBusy(bool isBusy)
        //{
        //    btnRun.Enabled = !isBusy;
        //    btnLoadModel.Enabled = !isBusy;
        //    btnFolderRun.Enabled = !isBusy;
        //    btnSettings.Enabled = !isBusy;
        //    chkFileMode.Enabled = !isBusy;
        //    Cursor.Current = isBusy ? Cursors.WaitCursor : Cursors.Default;
        //}


        /// <summary>
        /// 설정 파일(InspectionSettings.xml)을 메모장으로 열기. 저장 후 다음 검사부터 반영 (3단계 3-2)
        /// </summary>
        private void btnSettings_Click(object sender, EventArgs e)
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
                MessageBox.Show($"설정 파일을 열 수 없습니다: {ex.Message}\n{InspectionSettings.FilePath}");
            }
        }


        private void chkFileMode_CheckedChanged(object sender, EventArgs e)
        {
            // 모드가 바뀌면 상태 표시 갱신 (3단계 5-2)
            updateStatusDisplay();
        }


        /// <summary>
        /// 261028@Claude [4단계] Folder Run 버튼: 폴더의 이미지를 모두 파일 모드로 검사
        /// - 결과 보고서: Results\Batch_yyyyMMdd_HHmmss.csv
        /// - 폴더에 expected.csv가 있으면 기대값과 비교(회귀 테스트)해서 PASS/FAIL 표시
        /// - 검사 중에는 MessageBox를 띄우지 않고, 끝난 뒤 요약만 보여준다.
        /// </summary>
        private void btnFolderRun_Click(object sender, EventArgs e)
        {
            // 261029@Claude [검토 보완 2] 일괄 검사 중에는 Stop 버튼: 현재 이미지 검사가 끝나면 멈춘다.
            if (isBatchRunning)
            {
                isStopRequested = true;
                btnFolderRun.Enabled = false;
                lblStatus.Text = "현재 이미지 검사가 끝나면 멈춥니다...";
                return;
            }

            // 261029@Claude [검토 보완 1] 읽기에 실패하면 직전 설정을 유지하고 알림
            // settings = InspectionSettings.Load();
            reloadSettings();

            string notReadyMessage = vision.GetNotReadyMessage(true);
            if (notReadyMessage != null)
            {
                MessageBox.Show(notReadyMessage);
                return;
            }

            string folder;
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "일괄 검사할 이미지 폴더 선택 (expected.csv가 있으면 회귀 테스트)";
                if (Directory.Exists(settings.LastImageFolder))
                {
                    folderDialog.SelectedPath = settings.LastImageFolder;
                }
                if (folderDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                folder = folderDialog.SelectedPath;
            }

            List<string> imageFiles = BatchInspection.GetImageFiles(folder);
            if (imageFiles.Count == 0)
            {
                MessageBox.Show("폴더에 검사할 이미지(bmp, jpg, png, tif)가 없습니다.\n" + folder);
                return;
            }
            settings.LastImageFolder = folder;
            settings.Save();

            Dictionary<string, Dictionary<string, string>> expected;
            try
            {
                expected = BatchInspection.LoadExpected(folder);
            }
            catch (Exception ex)
            {
                AppLogger.Error(BatchInspection.ExpectedFileName + " 읽기 실패: " + folder, ex);
                MessageBox.Show($"{BatchInspection.ExpectedFileName}을(를) 읽을 수 없어 회귀 테스트 없이 검사합니다.\n{ex.Message}");
                expected = null;
            }

            AppLogger.Info($"폴더 일괄 검사 시작: {folder} ({imageFiles.Count}장, 기대값 {(expected == null ? "없음" : expected.Count + "건")})");
            List<BatchItem> items = new List<BatchItem>();

            // setBusy(true);
            isStopRequested = false;
            setBusy(true, true);
            try
            {
                for (int i = 0; i < imageFiles.Count; i++)
                {
                    // 261029@Claude [검토 보완 2] Stop 버튼 또는 창 닫기 요청 시 중지
                    if (isStopRequested)
                    {
                        AppLogger.Info($"폴더 일괄 검사 중지: {i}/{imageFiles.Count}장 검사");
                        break;
                    }

                    BatchItem item = new BatchItem { FilePath = imageFiles[i] };
                    lblStatus.Text = $"폴더 검사 중 {i + 1}/{imageFiles.Count}: {item.FileName}";
                    lblStatus.Refresh();

                    try
                    {
                        ICogImage image = VisionService.LoadImageFile(item.FilePath);
                        item.Result = runInspection(true, image, item.FilePath, false);
                        if (item.Result == null || !item.Result.IsLocateOk)
                        {
                            item.Error = lastErrorMessage;
                        }
                    }
                    catch (Exception ex)
                    {
                        AppLogger.Error("일괄 검사 이미지 로드 오류: " + item.FilePath, ex);
                        item.Error = "이미지 로드 실패: " + ex.Message;
                    }

                    BatchInspection.Compare(item, expected, settings.RegressionValueTolerance);
                    items.Add(item);

                    // 검사 화면이 갱신되도록 메시지 처리 (버튼은 setBusy로 막혀 있음)
                    Application.DoEvents();
                }
            }
            finally
            {
                setBusy(false);
                updateStatusDisplay();
            }

            List<string> measureNames = new List<string>
            {
                settings.LeftDist1.DisplayName, settings.LeftDist2.DisplayName,
                settings.RightDist1.DisplayName, settings.RightDist2.DisplayName
            };
            string reportPath = BatchInspection.WriteReport(folder, items, measureNames);

            int okItems = items.Count(x => x.JudgeText == "OK");
            int ngItems = items.Count(x => x.JudgeText == "NG");
            int errorItems = items.Count(x => x.JudgeText == "ERROR");
            // string summary = $"폴더 일괄 검사 완료: {items.Count}장\nOK {okItems} / NG {ngItems} / ERROR {errorItems}";
            // 261029@Claude [검토 보완 2] 중지했으면 검사한 장수를 함께 표시
            string summary = (items.Count < imageFiles.Count
                                ? $"폴더 일괄 검사 중지: {items.Count}/{imageFiles.Count}장 검사"
                                : $"폴더 일괄 검사 완료: {items.Count}장")
                             + $"\nOK {okItems} / NG {ngItems} / ERROR {errorItems}";

            if (expected != null)
            {
                int passItems = items.Count(x => x.RegressionStatus == "PASS");
                int failItems = items.Count(x => x.RegressionStatus == "FAIL");
                int noExpected = items.Count(x => x.RegressionStatus == "-");
                summary += $"\n\n회귀 테스트 ({BatchInspection.ExpectedFileName}): PASS {passItems} / FAIL {failItems} / 기대값 없음 {noExpected}";
                foreach (BatchItem failed in items.Where(x => x.RegressionStatus == "FAIL").Take(10))
                {
                    summary += $"\n - {failed.FileName}: {failed.RegressionDetail}";
                }
            }
            summary += "\n\n보고서: " + (reportPath ?? "(저장 실패 - 로그 확인)");

            AppLogger.Info(summary.Replace("\n", " "));

            // 261029@Claude [검토 보완 2] 검사 중에 창 닫기를 눌렀으면 보고서를 남긴 뒤 닫는다.
            if (isCloseRequested)
            {
                BeginInvoke(new Action(Close));
                return;
            }

            MessageBox.Show(summary, "폴더 일괄 검사", MessageBoxButtons.OK,
                items.Any(x => x.RegressionStatus == "FAIL" || x.JudgeText == "ERROR") ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }
    }
}


// ###################################################################################################
// 261028@Claude [4단계] 아래는 3단계까지의 Form1.cs 전체 (주석 처리)
// - VisionPro 처리는 VisionService.cs로, 컨트롤·이벤트 이름은 정리된 이름으로 바뀌었다.
//   (inspect→btnRun, button1→btnLoadModel, textBox1~5→txtLeftDist1/txtLeftDist2/txtRightDist1/txtRightDist2/txtAlign,
//    button1_Click→btnRun_Click, button1_Click_1→btnLoadModel_Click, btnLoadJob_Click()→selectAndLoadJob())
// - 사용하지 않던 필드 fixtureTool, cogDisplayOut, pmAlignTool, jobFilePath는 새 코드에 없다.
// ###################################################################################################
//// Cognex
//using Cognex.VisionPro;
//using Cognex.VisionPro.CalibFix;
//using Cognex.VisionPro.Display;
//using Cognex.VisionPro.ImageFile;
//using Cognex.VisionPro.ImageProcessing;
//using Cognex.VisionPro.PMAlign;
//using Cognex.VisionPro.QuickBuild;
//using Cognex.VisionPro.QuickBuild.Implementation.Internal;
//using Cognex.VisionPro.ToolBlock;
//using Cognex.VisionPro.ToolGroup;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Forms;



//// ===============================================================================
////  1023@jdin "1023_QB_CBoxNoSwitch.vpp" 사용할 것
////  tbImageSource를 사용하지 않고
////  CogIPOneImageTool1을 사용하여 InputImage를 직접 주입하는 방식으로 변경
////  입력은 CogAcqFifoTool로 입력이 기본, CheckBox를 통해 파일 입력 모드로 변경 
//// ===============================================================================
//namespace VproManageBlack02
//{
//    public partial class Form1 : Form
//    {
//        CogJobManager jobManager;
//        CogToolGroup myToolGroup;
//        CogToolGroup myToolGroupGroup;         // 261023@jdin 추가
//        // CogToolBlock tbImageSource;
//        CogToolBlock locate;
//        CogToolBlock leftSide;
//        CogToolBlock rightSide;
//        CogFixtureTool fixtureTool;
//        CogPMAlignTool pmAlignTool;
//        CogIPOneImageTool IPOneImageTool;
//        CogAcqFifoTool acqFifoTool; 

//        // 261019@jdin 폼 클래스 내부에 선언
//        // 참조에 Cognex.VisionPro.Controls가 있어야 된다.
//        private CogRecordDisplay cogRecordDisplay1;
//        private CogRecordDisplay cogRecordDisplay2;


//        // 261019@jdin CogDisplay 선언
//        private CogDisplay cogDisplayOut;


//        //List<CogJob> job = new List<CogJob>();
//        //List<CogJobIndependent> jobIndependent = new List<CogJobIndependent>();

//        string jobFilePath;

//        // 261028@Claude [3단계] 검사 설정, 판정·상태 표시, 결과 저장용
//        private InspectionSettings settings;
//        private string loadedVppPath = "";          // 로드에 성공한 VPP (jobFilePath는 선택만 하고 실패할 수도 있음)
//        private string currentImageSource = "";     // 이번 검사 이미지 출처 (파일 경로 또는 "Camera")
//        private ICogImage currentInputImage;        // 이번 검사 입력 이미지 (NG 이미지 저장용)
//        private double? lastAlignScore;             // 이번 검사의 PMAlign 점수
//        private long lastElapsedMs;
//        private int okCount;
//        private int ngCount;

//        public Form1()
//        {
//            InitializeComponent();

//            // 261028@Claude [3단계 3-2] 검사 설정 읽기 (InspectionSettings.xml)
//            settings = InspectionSettings.Load();


//            // 코드로 CogRecordDisplay 생성 및 설정
//            cogRecordDisplay1 = new CogRecordDisplay();
//            cogRecordDisplay1.Location = new System.Drawing.Point(30, 50); // (가로, 세로)
//            cogRecordDisplay1.Size = new System.Drawing.Size(600, 600);


//            // 코드로 CogRecordDisplay 생성 및 설정
//            cogRecordDisplay2 = new CogRecordDisplay();
//            cogRecordDisplay2.Location = new System.Drawing.Point(750, 50); // (가로, 세로)
//            cogRecordDisplay2.Size = new System.Drawing.Size(600, 600);

//            // 폼에 컨트롤 추가
//            this.Controls.Add(cogRecordDisplay1);   // input image
//            this.Controls.Add(cogRecordDisplay2);   // result image


//            //// 261019@jdin CogDisplay
//            //cogDisplayOut = new CogDisplay();
//            //cogDisplayOut.Location = new System.Drawing.Point(1500, 700); // (가로, 세로)
//            //cogDisplayOut.Size = new System.Drawing.Size(100, 100);
//            //cogDisplayOut.Dock = DockStyle.Fill;
//            //this.Controls.Add(cogDisplayOut);

//            // 261028@Claude [2단계 2-1] 종료 시 VisionPro 자원 해제
//            this.FormClosing += Form1_FormClosing;

//            // 261028@Claude [3단계] 화면이 뜬 뒤 마지막 VPP 자동 로드
//            this.Shown += Form1_Shown;
//            clearInspectResults();
//            updateStatusDisplay();
//        }


//        /// <summary>
//        /// 261028@Claude [3단계] 마지막으로 로드에 성공한 VPP를 자동으로 로드
//        /// (예전 버전은 생성자에서 VPP 선택 창을 띄웠음 → 화면이 뜬 뒤 저장된 경로로 자동 로드)
//        /// </summary>
//        private void Form1_Shown(object sender, EventArgs e)
//        {
//            if (!string.IsNullOrEmpty(settings.LastVppPath) && System.IO.File.Exists(settings.LastVppPath))
//            {
//                AppLogger.Info("마지막 VPP 자동 로드: " + settings.LastVppPath);
//                LoadVisionProJob(settings.LastVppPath);
//            }
//            updateStatusDisplay();
//        }


//        /// <summary>
//        /// 261028@Claude [2단계 2-1] 폼 종료 시 Job Manager와 카메라(FrameGrabber) 해제
//        /// 해제하지 않으면 종료 후에도 프로세스가 남거나 카메라가 잡혀 있을 수 있다.
//        /// </summary>
//        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
//        {
//            shutdownJobManager(jobManager);
//            jobManager = null;

//            try
//            {
//                // 프로그램 종료 직전이므로 AllowRecovery = false
//                CogFrameGrabbers frameGrabbers = new CogFrameGrabbers();
//                foreach (ICogFrameGrabber frameGrabber in frameGrabbers)
//                {
//                    frameGrabber.Disconnect(false);
//                }
//            }
//            catch (Exception ex)
//            {
//                AppLogger.Error("FrameGrabber 해제 실패", ex);
//            }
//        }


//        /// <summary>
//        /// 261028@Claude [2단계 2-1, 2-2] Job Manager 정리 (null이면 아무것도 하지 않음)
//        /// </summary>
//        private void shutdownJobManager(CogJobManager manager)
//        {
//            if (manager == null)
//            {
//                return;
//            }
//            try
//            {
//                manager.Shutdown();
//            }
//            catch (Exception ex)
//            {
//                AppLogger.Error("Job Manager Shutdown 실패", ex);
//            }
//        }



//        /// <summary>
//        /// QuickBuild 응용프로그램 Job 파일 경로 지정
//        /// </summary>
//        private void btnLoadJob_Click()
//        {
//            // Open a file dialog to select the VPP file
//            try
//            {
//                using (OpenFileDialog openFileDialog = new OpenFileDialog())
//                {
//                    openFileDialog.Filter = "VisionPro Job Files (*.vpp)|*.vpp";
//                    openFileDialog.Title = "Select a VisionPro Job File";
//                    // 261028@Claude [3단계] 마지막 VPP 폴더에서 열기
//                    if (!string.IsNullOrEmpty(settings.LastVppPath))
//                    {
//                        openFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(settings.LastVppPath);
//                    }
//                    if (openFileDialog.ShowDialog() == DialogResult.OK)
//                    {
//                        jobFilePath = openFileDialog.FileName;
//                        LoadVisionProJob(jobFilePath);
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                // 261028@Claude [2단계 2-8] 로그 기록
//                AppLogger.Error("VPP 파일 선택/로드 오류", ex);
//                MessageBox.Show("Error loading job file: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);

//            }
//        }


//        /// <summary>
//        /// 261028@Claude [2단계 2-2, 2-3] VPP 로드 개선
//        /// - 새 VPP를 지역 변수에 먼저 읽고, 필요한 툴을 모두 이름으로 확인한 뒤에만 필드에 반영한다.
//        /// - 필수 툴이 하나라도 없으면 새로 읽은 것을 정리하고 기존 상태를 그대로 유지한다. (일부만 초기화된 상태 방지)
//        /// - 교체에 성공하면 이전 Job Manager를 Shutdown 한다.
//        /// - 하위 ToolGroup은 번호(Tools[2]) 대신 이름("CogToolGroup1")으로 찾고, 없을 때만 기존 방식(Tools[2])을 사용한다.
//        /// </summary>
//        private void LoadVisionProJob(string vppPath)
//        {
//            Cursor previousCursor = Cursor.Current;
//            Cursor.Current = Cursors.WaitCursor;

//            CogJobManager newJobManager = null;
//            bool isApplied = false;

//            try
//            {
//                // "QuickBuilder응용 프로그램" 으로 저장 된 Job으로 저장한 것을 읽어온다.
//                object loadedobject = CogSerializer.LoadObjectFromFile(vppPath);

//                CogToolGroup newToolGroup;
//                if (loadedobject is CogJobManager manager)
//                {
//                    newJobManager = manager;
//                    newToolGroup = manager.JobCount > 0 ? manager.Job(0).VisionTool as CogToolGroup : null;
//                }
//                else if (loadedobject is CogJob job)
//                {
//                    // Case B: 파일이 단일 Job 구조인 경우
//                    newToolGroup = job.VisionTool as CogToolGroup;
//                }
//                else if (loadedobject is CogToolGroup toolGroup)
//                {
//                    // Case C: 파일이 툴그룹(ToolGroup) 자체로 저장된 경우
//                    newToolGroup = toolGroup;
//                }
//                else
//                {
//                    MessageBox.Show($"지원하지 않는 VPP 파일 형식입니다. 타입: {loadedobject?.GetType().Name}");
//                    return;
//                }

//                // 필요한 툴을 모두 확인 (없는 툴은 목록으로 모아서 한 번에 알린다)
//                List<string> missingTools = new List<string>();

//                CogToolGroup newToolGroupGroup = null;
//                CogAcqFifoTool newAcqFifoTool = null;
//                CogIPOneImageTool newIPOneImageTool = null;
//                CogToolBlock newLocate = null;
//                CogToolBlock newLeftSide = null;
//                CogToolBlock newRightSide = null;

//                if (newToolGroup == null)
//                {
//                    missingTools.Add("Job의 ToolGroup");
//                }
//                else
//                {
//                    newAcqFifoTool = findTool(newToolGroup, "CogAcqFifoTool1") as CogAcqFifoTool;

//                    newToolGroupGroup = findTool(newToolGroup, "CogToolGroup1") as CogToolGroup;
//                    if (newToolGroupGroup == null && newToolGroup.Tools.Count > 2)
//                    {
//                        newToolGroupGroup = newToolGroup.Tools[2] as CogToolGroup;   // 기존 방식
//                    }
//                }

//                if (newToolGroup != null && newToolGroupGroup == null)
//                {
//                    missingTools.Add("CogToolGroup1 (하위 ToolGroup)");
//                }
//                else if (newToolGroupGroup != null)
//                {
//                    newIPOneImageTool = findTool(newToolGroupGroup, "CogIPOneImageTool1") as CogIPOneImageTool;
//                    newLocate = findTool(newToolGroupGroup, "Locate") as CogToolBlock;
//                    newLeftSide = findTool(newToolGroupGroup, "LEFTside") as CogToolBlock;
//                    newRightSide = findTool(newToolGroupGroup, "RIGHTside") as CogToolBlock;

//                    if (newIPOneImageTool == null) missingTools.Add("CogIPOneImageTool1");
//                    if (newLocate == null) missingTools.Add("Locate");
//                    if (newLeftSide == null) missingTools.Add("LEFTside");
//                    if (newRightSide == null) missingTools.Add("RIGHTside");
//                }

//                if (missingTools.Count > 0)
//                {
//                    string message = "다음 툴을 찾을 수 없어 VPP를 적용하지 않았습니다. (기존 설정 유지)\n- "
//                                     + string.Join("\n- ", missingTools);
//                    AppLogger.Info("VPP 로드 거부: " + vppPath + " / 없는 툴: " + string.Join(", ", missingTools));
//                    MessageBox.Show(message, "VPP 로드 실패", MessageBoxButtons.OK, MessageBoxIcon.Warning);
//                    return;
//                }

//                // 모든 필수 툴 확인 완료 → 한 번에 교체
//                CogJobManager oldJobManager = jobManager;

//                jobManager = newJobManager;
//                myToolGroup = newToolGroup;
//                myToolGroupGroup = newToolGroupGroup;
//                acqFifoTool = newAcqFifoTool;
//                IPOneImageTool = newIPOneImageTool;
//                locate = newLocate;
//                leftSide = newLeftSide;
//                rightSide = newRightSide;
//                isApplied = true;

//                // 이전 Job Manager 정리
//                if (oldJobManager != null && !ReferenceEquals(oldJobManager, newJobManager))
//                {
//                    shutdownJobManager(oldJobManager);
//                }

//                AppLogger.Info("VPP 로드 완료: " + vppPath);

//                // 261028@Claude [3단계] 마지막 VPP 경로 저장(다음 실행 시 자동 로드), 상태 표시 갱신
//                loadedVppPath = vppPath;
//                settings.LastVppPath = vppPath;
//                settings.Save();
//                updateStatusDisplay();

//                // 카메라 툴은 파일 모드만 사용할 때는 없어도 되므로 경고만 한다.
//                if (acqFifoTool == null)
//                {
//                    MessageBox.Show("툴그룹 내부에 'CogAcqFifoTool1' 툴이 존재하지 않습니다. 파일 모드만 사용할 수 있습니다.");
//                }
//            }
//            catch (Exception ex)
//            {
//                AppLogger.Error("VPP 파일 로드 실패: " + vppPath, ex);
//                MessageBox.Show($"VPP 파일 로드 실패: {AppLogger.GetFullMessage(ex)}");
//            }
//            finally
//            {
//                // 적용하지 않은 새 Job Manager는 정리한다.
//                if (!isApplied)
//                {
//                    shutdownJobManager(newJobManager);
//                }
//                Cursor.Current = previousCursor;
//            }
//        }


//        /// <summary>
//        /// 261028@Claude [2단계 2-3] ToolGroup 안에서 이름으로 툴 찾기 (없으면 null)
//        /// </summary>
//        private static ICogTool findTool(CogToolGroup group, string toolName)
//        {
//            foreach (ICogTool tool in group.Tools)
//            {
//                if (tool != null && tool.Name == toolName)
//                {
//                    return tool;
//                }
//            }
//            return null;
//        }


//        // ===================================================================================================
//        // 261028@Claude [2단계] 아래는 변경 전 LoadVisionProJob (주석 처리)
//        // - 툴을 못 찾아도 필드를 일부만 바꾼 채 계속 진행함 (2-3)
//        // - 다시 로드할 때 이전 Job Manager를 정리하지 않음 (2-2)
//        // ===================================================================================================
//        ///// <summary>
//        /////  "QuickBuilder응용 프로그램"을 지정된 경로에서 읽어온다.
//        /////  "QuickBuilder응용 프로그램" 으로 저장 된 Job이다. 
//        ///// </summary>
//        ///// <param name="vppPath"></param>
//        //private void LoadVisionProJob(string vppPath)
//        //{
//        //    // 
//        //    try
//        //    {
//        //        // "QuickBuilder응용 프로그램" 으로 저장 된 Job으로 저장한 것을 읽어온다.
//        //        // 작업 JOb 파일은 QuickBuilder에서 하단에 "CongJob1"으로 표시되는 것을 말한다.
//        //        object loadedobject = CogSerializer.LoadObjectFromFile(vppPath);

//        //        if (loadedobject is CogJobManager manager)
//        //        {
//        //            jobManager = manager;

//        //            int jobCount = (int)jobManager.JobCount;    // 1023_QB_CBoxNoSwitch.vpp의 JobCount는 1이다.

//        //            // Job 내부에 있는 ToolGroup 및 이미지 스위치 툴블록 가져오기
//        //            // (구조에 따라 Job 인덱스나 이름은 현장 환경에 맞게 수정 필요)
//        //            //[0] { Cognex.VisionPro.QuickBuild.Implementation.Internal.CogInputImageTool}
//        //            //[1] { Cognex.VisionPro.CogAcqFifoTool}
//        //            //[2] { Cognex.VisionPro.ToolGroup.CogToolGroup} <= 이것을 ToolGroup을 사용
//        //            myToolGroup = jobManager.Job(0).VisionTool as CogToolGroup;

//        //            // CogAcqFifoTool acqFifoTool = myToolGroup.Tools[1] as CogAcqFifoTool;


//        //            // 아래 2. 툴그룹 내부에서 스위치 툴블록 찾아오기 에서 진행
//        //            //if (myToolGroup != null)
//        //            //{
//        //            //    tbImageSource = myToolGroup.Tools["tbImageSource"] as CogToolBlock;
//        //            //}
//        //        }
//        //        else if (loadedobject is CogJob job)
//        //        {
//        //            // Case B: 파일이 단일 Job 구조인 경우
//        //            jobManager = null; // 필요시 관리 객체 조정
//        //            myToolGroup = job.VisionTool as CogToolGroup;
//        //        }
//        //        else if (loadedobject is CogToolGroup toolGroup)
//        //        {
//        //            // Case C: 파일이 툴그룹(ToolGroup) 자체로 저장된 경우 (이 경우가 매우 많습니다!)
//        //            jobManager = null;
//        //            myToolGroup = toolGroup;
//        //        }
//        //        else
//        //        {
//        //            MessageBox.Show($"지원하지 않는 VPP 파일 형식입니다. 타입: {loadedobject?.GetType().Name}");
//        //            return;
//        //        }

//        //        // 2. 툴그룹 내부에서 툴블록 찾아오기
//        //        if (myToolGroup != null)
//        //        {
//        //            //[0] { Cognex.VisionPro.QuickBuild.Implementation.Internal.CogInputImageTool}
//        //            //[1] { Cognex.VisionPro.CogAcqFifoTool}
//        //            //[2] { Cognex.VisionPro.ToolGroup.CogToolGroup} <= 이것을 ToolGroup을 사용
//        //            //      [0] {Cognex.VisionPro.ImageProcessing.CogIPOneImageTool}
//        //            //      [1] { Cognex.VisionPro.ToolBlock.CogToolBlock}
//        //            //      [2] { Cognex.VisionPro.ToolBlock.CogToolBlock}
//        //            //      [3] { Cognex.VisionPro.ToolBlock.CogToolBlock}
//        //            myToolGroupGroup = myToolGroup.Tools[2] as CogToolGroup;

//        //            // ====================================================================
//        //            // 261023@jdin 사용하지 않는다. 
//        //            // 툴그룹 내부에서 스위치 툴블록 찾아오기
//        //            //tbImageSource = myToolGroup.Tools["tbImageSource"] as CogToolBlock;
//        //            //if (tbImageSource == null)
//        //            //{
//        //            //    MessageBox.Show("툴그룹 내부에 'tbImageSource' 툴블록이 존재하지 않습니다. 이름을 확인해주세요.");
//        //            //}
//        //            // ====================================================================

//        //            // Tool 그룹 안에서 Tool 존재 확인
//        //            acqFifoTool = myToolGroup.Tools["CogAcqFifoTool1"] as CogAcqFifoTool;
//        //            if (acqFifoTool == null)
//        //            {
//        //                MessageBox.Show("툴그룹 내부에 'CogAcqFifoTool1' 툴이 존재하지 않습니다. 이름을 확인해주세요.");
//        //            }
//        //            IPOneImageTool = myToolGroupGroup.Tools["CogIPOneImageTool1"] as CogIPOneImageTool;
//        //            if (IPOneImageTool == null)
//        //            {
//        //                MessageBox.Show("툴그룹 내부에 'CogIPOneImageTool1' 툴이 존재하지 않습니다. 이름을 확인해주세요.");
//        //            }
//        //            locate = myToolGroupGroup.Tools["Locate"] as CogToolBlock;
//        //            if (locate == null)
//        //            {
//        //                MessageBox.Show("툴그룹 내부에 'Locate' 툴블록이 존재하지 않습니다. 이름을 확인해주세요.");
//        //            }
//        //            leftSide = myToolGroupGroup.Tools["LEFTside"] as CogToolBlock;
//        //            if (leftSide == null)
//        //            {
//        //                MessageBox.Show("툴그룹 내부에 'LEFTside' 툴블록이 존재하지 않습니다. 이름을 확인해주세요.");
//        //            }
//        //            rightSide = myToolGroupGroup.Tools["RIGHTside"] as CogToolBlock;
//        //            if (rightSide == null)
//        //            {
//        //                MessageBox.Show("툴그룹 내부에 'RIGHTside' 툴블록이 존재하지 않습니다. 이름을 확인해주세요.");
//        //            }

//        //        }
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        MessageBox.Show($"VPP 파일 로드 실패: {ex.Message}");
//        //    }
//        //}



//        /// <summary>
//        /// 검사 실행을 하기 전에 PMAlignTool의 결과를 확인하여 위치 확인 여부를 판단
//        /// Location 확인이 실패하면 검사 실행을 하지 않음
//        /// </summary>
//        /// <returns></returns>
//        private bool isCheckLocate()
//        {
//            // 261028@Claude [3단계] 이번 검사의 PMAlign 점수 (CSV 기록용)
//            lastAlignScore = null;

//            // 261028@Claude [1단계 2-4] locate, pmAlignTool, Results의 null 및 실행 에러 확인
//            if (locate == null)
//            {
//                textBox5.Text = "NG - No Locate";
//                return false;
//            }
//            if (locate.RunStatus.Result == CogToolResultConstants.Error)
//            {
//                textBox5.Text = "NG - Locate Error";
//                return false;
//            }

//            // PMAlignTool 실행 결과 확인
//            pmAlignTool = locate.Tools["CogPMAlignTool1"] as CogPMAlignTool;
//            // PMAlignTool의 결과 실패하면 결과 값이 없다. 즉 null이다.
//            // 기준 Score 값보다 작게 나오는 것과 다르다.
//            // 예외처리가 필요하다.
//            if (pmAlignTool == null || pmAlignTool.Results == null)
//            {
//                textBox5.Text = "NG - No PMAlign";
//                return false;
//            }
//            int alignCount = pmAlignTool.Results.Count;
//            if (alignCount == 0)
//            {
//                textBox5.Text = "NG - No Result";
//                return false;     // 검사실행을 하지 않음
//            }
//            else
//            {
//                double pmScore = pmAlignTool.Results[0].Score;
//                lastAlignScore = pmScore;
//                // 0~1 사이의 값으로, 1에 가까울수록 정렬이 잘 된 것으로 판단
//                // 261028@Claude [3단계 3-2] 기준 점수를 설정 파일(PMAlignMinScore, 기본 0.6)에서 읽음
//                // if (pmScore < 0.6)
//                if (pmScore < settings.PMAlignMinScore)
//                {
//                    textBox5.Text = pmScore.ToString("F2") + " - FAIL";
//                    return false; // 검사실행을 하지 않음
//                }
//                else
//                {
//                    textBox5.Text = pmScore.ToString("F2") + " - PASS";
//                    return true;
//                }
//            }
//        }


//        /// --- [1] 오프라인 파일 모드 ---
//        /// 261028@jdin 오프라인 파일 모드를 분리함.
//        /// --- [1] 오프라인 파일 모드 ---
//        /// 261028@Claude [1단계 1-2] 이미지 로드 실패·취소를 호출부에 알리도록 bool 반환으로 변경
//        // private void offlineFileMode()
//        private bool offlineFileMode()
//        {
//            // --- [1] 오프라인 파일 모드 ---
//            // 1. 파일에서 이미지 로드 (Cognex 파일 로더 또는 일반 이미지 로드 방식 활용)
//            ICogImage fileImage = LoadImageFromFile();
//            if (fileImage == null)
//            {
//                MessageBox.Show("이미지 파일을 불러오지 못했습니다.");
//                // return;
//                return false;
//            }

//            // 261028@Claude [3단계] NG 이미지 저장용으로 입력 이미지 보관 (출처는 LoadImageFromFile에서 기록)
//            currentInputImage = fileImage;

//            IPOneImageTool.InputImage = fileImage;
//            ICogImage cogImage3 = (ICogImage)IPOneImageTool.InputImage;
//            cogRecordDisplay1.Image = cogImage3;
//            cogRecordDisplay1.Fit();

//            // 3. 카메라 타임아웃을 원천 차단하기 위해 검사 툴그룹만 단독 실행
//            myToolGroupGroup.Run();

//            // Tool 실행 후 출력 이미지 확인
//            ICogImage cogImage1 = (ICogImage)IPOneImageTool.OutputImage;
//            cogRecordDisplay1.Image = cogImage1;
//            cogRecordDisplay1.Fit();

//            return true;
//        }



//        /// --- [2] 라이브 카메라 모드 ---
//        /// 261028@jdin 라이브 카메라 모드를 분리함.
//        /// --- [2] 라이브 카메라 모드 --- 
//        private bool liveCameraMode()
//        {
//            // --- [2] 라이브 카메라 모드 ---
//            // 1. IPOneImageTool.InputImage 초기화
//            IPOneImageTool.InputImage = null;


//            // 2. 카메라 취득부터 검사까지 전체 잡 매니저 실행
//            // 261028@Claude [1단계 1-1] jobManager.Run()은 비동기라 결과를 너무 일찍 읽는 문제가 있었음.
//            // 동기 실행인 myToolGroup.Run()으로 통일 (Run이 끝난 뒤 결과를 읽으므로 이전 값 표시·동시 접근 없음)
//            // jobManager.Run();
//            myToolGroup.Run();   // myToolGroup.Run();

//            // 3. 카메라 취득 결과 확인
//            if (acqFifoTool.RunStatus.Result != CogToolResultConstants.Accept)
//            {
//                MessageBox.Show("카메라 이미지 취득 실패");
//                return false;
//            }
//            else if (acqFifoTool.OutputImage == null)
//            {
//                MessageBox.Show("OutputImage가 null입니다. 이미지가 들어오지 않았습니다.");
//                return false;
//            }

//            // 261028@Claude [3단계] NG 이미지 저장용으로 입력 이미지와 출처 보관
//            currentInputImage = acqFifoTool.OutputImage;
//            currentImageSource = "Camera";

//            // 261027@jdin TEMP
//            ICogImage cogImage5 = (ICogImage)acqFifoTool.OutputImage;
//            cogRecordDisplay1.Image = cogImage5;
//            // 화면 크기에 맞게 이미지 자동 조절 (Fit)
//            cogRecordDisplay1.Fit();

//            // IPOneImageTool.OutputImage는 InputImag입력이 없어도, 기존 값을 가지고 있다.
//            // 따라서, IPOneImageTool.IntputImage = null인지 확인해야 한다.

//            ICogImage cogImage1 = (ICogImage)IPOneImageTool.OutputImage;
//            cogRecordDisplay1.Image = cogImage1;
//            // 화면 크기에 맞게 이미지 자동 조절 (Fit)
//            cogRecordDisplay1.Fit();


//            return true;
//        }   

//        /// <summary>
//        /// 
//        /// </summary>
//        private void clearDisplayAll()
//        {

//            // Display1, 2 Clear
//            cogRecordDisplay1.StaticGraphics.Clear();       // 고정 그래픽 제거
//            cogRecordDisplay1.InteractiveGraphics.Clear();  // 인터랙티브 그래픽 제거
//            cogRecordDisplay1.Record = null;                // Record로 표시했다면 제거
//            cogRecordDisplay1.Image = null;                 // 이미지 제거
//            cogRecordDisplay1.Fit();

//            //// 1단계: 강제 다시 그리기
//            //cogRecordDisplay1.Invalidate();
//            //cogRecordDisplay1.Refresh();
//            //// 2단계(테스트용): 잠깐 숨겨보기
//            //cogRecordDisplay1.Visible = false;
//            //MessageBox.Show("지금 Display1 화면에 이미지가 남아 있나요?");
//            //cogRecordDisplay1.Visible = true;


//            cogRecordDisplay2.StaticGraphics.Clear();       // 고정 그래픽 제거
//            cogRecordDisplay2.InteractiveGraphics.Clear();  // 인터랙티브 그래픽 제거
//            cogRecordDisplay2.Record = null;                // Record로 표시했다면 제거
//            cogRecordDisplay2.Image = null;                 // 이미지 제거
//            cogRecordDisplay2.Fit();
//            //// 1단계: 강제 다시 그리기
//            //cogRecordDisplay2.Invalidate();
//            //cogRecordDisplay2.Refresh();
//            //// 2단계(테스트용): 잠깐 숨겨보기
//            //cogRecordDisplay2.Visible = false;
//            //MessageBox.Show("지금 Display2 화면에 이미지가 남아 있나요?");
//            //cogRecordDisplay2.Visible = true;


//            //MessageBox.Show(
//            //        $"Image null: {cogRecordDisplay2.Image == null}\n" +
//            //        $"Record null: {cogRecordDisplay2.Record == null}\n" +
//            //        $"Live: {cogRecordDisplay2.LiveDisplayRunning}\n" +
//            //        $"Visible: {cogRecordDisplay2.Visible}, Parent: {cogRecordDisplay2.Parent?.Name}");

//        }


//        /// <summary>
//        /// 검사 실행 버튼 클릭 이벤트 (라이브 vs 오프라인 파일 모드 분기)
//        /// </summary>
//        private void button1_Click(object sender, EventArgs e)
//        {
//            // 261028@Claude [3단계 3-2] 메모장으로 바꾼 설정을 검사마다 다시 읽어 반영
//            settings = InspectionSettings.Load();

//            // 261028@Claude [1단계 1-5] jobManager가 없으면(VPP가 CogJob/ToolGroup 형식) 파일 모드까지 막히던 문제.
//            // 모드별로 필요한 객체만 확인하도록 변경
//            //if (jobManager == null || myToolGroup == null)
//            //{
//            //    MessageBox.Show("비전 시스템이 초기화되지 않았습니다.");
//            //    return;
//            //}
//            string notReadyMessage = getNotReadyMessage(chkFileMode.Checked);
//            if (notReadyMessage != null)
//            {
//                MessageBox.Show(notReadyMessage);
//                return;
//            }

//            // Clear Disply 1, 2
//            clearDisplayAll();

//            // Clear Old Inspect Result
//            clearInspectResults();

//            // 1. IPOneImageTool.InputImage 초기화
//            IPOneImageTool.InputImage = null;

//            // 2. 검사 실행 전 결과 레코드 초기화
//            ICogRecords resultRecords = myToolGroup.CreateLastRunRecord().SubRecords;
//            resultRecords.Clear();

//            //  261021@jdin 1021_VINA_BK_QB03.vpp 사용할 것
//            // UI의 체크박스 상태 등으로 모드 판별 (true: 파일 검사, false: 라이브 카메라)
//            bool isOfflineFileMode = chkFileMode.Checked;

//            // 261028@Claude [3단계 3-1, 3-7, 5-2] 검사 1회 = 실행 → 위치 확인 → 측정 판정 → OK/NG 표시 → 결과 저장
//            // - 위치 확인에 실패해도 NG로 판정해서 카운트·CSV·NG 이미지에 남긴다.
//            // - 이미지 로드 취소·카메라 취득 실패는 검사가 아니므로 판정·저장하지 않는다.
//            InspectionResult result = new InspectionResult();
//            result.IsOfflineFileMode = isOfflineFileMode;
//            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
//            currentInputImage = null;

//            try
//            {
//                // [1] 파일 모드 / [2] 라이브 카메라 모드 실행
//                bool isRunOk = isOfflineFileMode ? offlineFileMode() : liveCameraMode();
//                if (!isRunOk)
//                {
//                    if (!isOfflineFileMode)
//                    {
//                        MessageBox.Show("카메라 취득에 실패했습니다.");
//                    }
//                    return;
//                }
//                result.ImageSource = currentImageSource;

//                // 위치 확인
//                result.IsLocateOk = isCheckLocate();
//                result.AlignText = textBox5.Text;
//                result.AlignScore = lastAlignScore;

//                if (result.IsLocateOk)
//                {
//                    // 결과 디스플레이 바인딩 (툴그룹 실행 결과 레코드 표시)
//                    // 3 - LastRun.Locate.CogFixtureTool1.outputImage <= 이것으로 함
//                    ICogRecord resultRecord = myToolGroup.CreateLastRunRecord().SubRecords["CogToolGroup1.Locate.CogFixtureTool1.OutputImage"];
//                    cogRecordDisplay2.Record = resultRecord;
//                    // 화면 크기에 맞게 이미지 자동 조절 (Fit)
//                    cogRecordDisplay2.Fit();

//                    // [3-7] LEFTside/RIGHTside 측정 그래픽 표시
//                    showMeasureGraphics();
//                }

//                // [3-1, 3-4] 측정값 읽기와 공차 판정 (위치 확인 실패 시 "-" 표시)
//                evaluateMeasures(result, result.IsLocateOk);

//                stopwatch.Stop();
//                result.ElapsedMs = stopwatch.ElapsedMilliseconds;

//                // OK/NG 표시, 카운트, 결과 저장
//                completeInspection(result);

//                if (!result.IsLocateOk)
//                {
//                    MessageBox.Show("위치 확인에 실패했습니다.");
//                }
//            }
//            catch (Exception ex)
//            {
//                // 261028@Claude [2단계 2-8] 로그 기록
//                AppLogger.Error("검사 실행 오류 (FileMode=" + isOfflineFileMode + ")", ex);
//                MessageBox.Show($"검사 실행 오류 : {ex.Message}");
//            }

//            // ===================================================================================================
//            // 261028@Claude [3단계] 아래는 1단계 코드 (주석 처리)
//            // - OK/NG 판정, 결과 저장, 측정 그래픽 표시가 없었음
//            // ===================================================================================================
//            //// 261028@Claude [1단계 1-2, 1-4] 모드별 실행 → 위치 확인 → 결과 표시를 하나의 try 안에서 처리.
//            //// 이미지 취득·로드가 실패하거나 위치 확인이 실패하면 즉시 중단하고,
//            //// 측정 결과는 모든 단계가 성공했을 때만 읽는다. (이전 검사값 표시·처리되지 않은 예외 방지)
//            //try
//            //{
//            //    // [1] 파일 모드 / [2] 라이브 카메라 모드 실행
//            //    bool isRunOk = isOfflineFileMode ? offlineFileMode() : liveCameraMode();
//            //    if (!isRunOk)
//            //    {
//            //        if (!isOfflineFileMode)
//            //        {
//            //            MessageBox.Show("카메라 취득에 실패했습니다.");
//            //        }
//            //        return;
//            //    }

//            //    bool isLocate = isCheckLocate();
//            //    if (!isLocate)
//            //    {
//            //        MessageBox.Show("위치 확인에 실패했습니다.");
//            //        return;
//            //    }

//            //    // 결과 디스플레이 바인딩 (툴그룹 실행 결과 레코드 표시)
//            //    // 3 - LastRun.Locate.CogFixtureTool1.outputImage <= 이것으로 함
//            //    ICogRecord resultRecord = myToolGroup.CreateLastRunRecord().SubRecords["CogToolGroup1.Locate.CogFixtureTool1.OutputImage"];
//            //    cogRecordDisplay2.Record = resultRecord;
//            //    // 화면 크기에 맞게 이미지 자동 조절 (Fit)
//            //    cogRecordDisplay2.Fit();

//            //    // Results
//            //    getInspectResults();
//            //}
//            //catch (Exception ex)
//            //{
//            //    // 261028@Claude [2단계 2-8] 로그 기록
//            //    AppLogger.Error("검사 실행 오류 (FileMode=" + isOfflineFileMode + ")", ex);
//            //    MessageBox.Show($"검사 실행 오류 : {ex.Message}");
//            //}

//            // ===================================================================================================
//            // 261028@Claude [1단계] 아래는 변경 전 코드 (주석 처리)
//            // - 파일 로드 실패·취소 시에도 위치 확인과 결과 읽기가 계속 진행됨 (1-2)
//            // - getInspectResults()가 try 밖에서 항상 실행되어 예외 시 앱이 종료될 수 있음 (1-4)
//            // ===================================================================================================
//            //if (isOfflineFileMode)
//            //{
//            //    try
//            //    {
//            //        offlineFileMode();

//            //        bool isLocate = isCheckLocate();

//            //        if (!isLocate)
//            //        {
//            //            MessageBox.Show("위치 확인에 실패했습니다.");
//            //            return;
//            //        }

//            //        // 4. 결과 디스플레이 바인딩 (툴그룹 실행 결과 레코드 표시)
//            //        // ====================================================================================================
//            //        // fixtureTool.CreateLastRunRecord().SubRecords[0]; => SubRecords[0]가 나와야 한다.
//            //        // fixtureTool = (CogFixtureTool)locate.Tools["CogFixtureTool1"];
//            //        // ICogRecord resultRecord = fixtureTool.CreateLastRunRecord().SubRecords[0];                
//            //        // ====================================================================================================
//            //        // 0 - CogIPOneImageTool1.InputImage
//            //        // 1 - CogIPOneImageTool1.OutputImage
//            //        // 2 - CogFixtureTool1.OutputImage
//            //        // 4 - LEFTside.InputImage
//            //        // 6 - RIGHTside.InputImage
//            //        // resultRecords = myToolGroupGroup.CreateLastRunRecord().SubRecords; 위 리스트 참조할 것
//            //        // 상위 myToolGroup과 하위 myToolGroupGroup이 다른 것을 주의할 것
//            //        // ICogRecords resultRecords = myToolGroup.CreateLastRunRecord().SubRecords;

//            //        ICogRecord resultRecord = myToolGroup.CreateLastRunRecord().SubRecords["CogToolGroup1.Locate.CogFixtureTool1.OutputImage"];
//            //        cogRecordDisplay2.Record = resultRecord;
//            //        // 5. 화면 크기에 맞게 이미지 자동 조절 (Fit)
//            //        cogRecordDisplay2.Fit();
//            //    }
//            //    catch (Exception ex)
//            //    {
//            //        MessageBox.Show($"Tool Group 실행 오류 : {ex.Message}");
//            //    }
//            //}
//            //else
//            //{   // --- [2] 라이브 카메라 모드 ---
//            //    try
//            //    {
//            //        bool isCameraOk = liveCameraMode();

//            //        if(!isCameraOk)
//            //        {
//            //            MessageBox.Show("카메라 취득에 실패했습니다.");
//            //            return;
//            //        }

//            //        // CogAcqFifoTool acqFifoTool = myToolGroup.Tools[1] as CogAcqFifoTool;
//            //        // CogAcqFifoTool acqFifoTool = myToolGroup.Tools["CogAcqFifoTool1"] as CogAcqFifoTool;


//            //        // =======================================================================================
//            //        // 261028@Jdin acqFifoTool 실행 테스트 
//            //        // acqFifoTool.Run();
//            //        //if (acqFifoTool.RunStatus.Result != CogToolResultConstants.Accept)
//            //        //{
//            //        //    MessageBox.Show("카메라 이미지 취득 실패");
//            //        //    return;
//            //        //}
//            //        //else if(acqFifoTool.OutputImage == null)
//            //        //{
//            //        //    MessageBox.Show("OutputImage가 null입니다. 이미지가 들어오지 않았습니다.");
//            //        //    return;
//            //        //}
//            //        // 
//            //        //ICogImage cogImage9 = (ICogImage)acqFifoTool.OutputImage;
//            //        //cogRecordDisplay1.Image = cogImage9;
//            //        // 화면 크기에 맞게 이미지 자동 조절 (Fit)
//            //        //cogRecordDisplay1.Fit();
//            //        // =======================================================================================


//            //        // --- [2] 라이브 카메라 모드 ---
//            //        //ICogRecords resultRecords = myToolGroup.CreateLastRunRecord().SubRecords;
//            //        //resultRecords.Clear();


//            //        bool isLocate = isCheckLocate();

//            //        if (!isLocate)
//            //        {
//            //            MessageBox.Show("위치 확인에 실패했습니다.");
//            //            return;
//            //        }

//            //        // 결과 디스플레이 바인딩 (툴그룹 실행 결과 레코드 표시)
//            //        // 0 - LastRun.Image Source.OutputImage,
//            //        // 1 - LastRun.Locate.CogPMAlignTool1.InputImage,
//            //        // 2 - Locate.CogPMAlignTool1.OutputImage
//            //        // 3 - LastRun.Locate.CogFixtureTool1.outputImage <= 이것으로 함
//            //        ICogRecord resultRecord = myToolGroup.CreateLastRunRecord().SubRecords["CogToolGroup1.Locate.CogFixtureTool1.OutputImage"];
//            //        cogRecordDisplay2.Record = resultRecord;

//            //        // 5. 화면 크기에 맞게 이미지 자동 조절 (Fit)
//            //        cogRecordDisplay2.Fit();
//            //    }
//            //    catch (Exception ex)
//            //    {
//            //        MessageBox.Show($"Job Manager 실행 오류 : {ex.Message}");
//            //    }
//            //}

//            //// Reults
//            //getInspectResults();
//        }


//        /// <summary>
//        /// 검사 실행 전에 검사 결과를 초기화
//        /// </summary>
//        private void clearInspectResults()
//        {
//            textBox1.Text = "";
//            textBox2.Text = "";
//            textBox3.Text = "";
//            textBox4.Text = "";
//            textBox5.Text = "";

//            // 261028@Claude [3단계 3-1] 판정 표시와 NG 강조 색 초기화
//            lblJudge.Text = "-";
//            lblJudge.BackColor = SystemColors.Control;
//            lblJudge.ForeColor = SystemColors.ControlText;
//            foreach (TextBox box in new[] { textBox1, textBox2, textBox3, textBox4 })
//            {
//                box.BackColor = SystemColors.Control;
//            }
//        }


//        /// <summary>
//        /// 261028@Claude [3단계 3-1, 3-4] 측정값 4개를 읽어 공차로 판정하고 TextBox에 표시
//        /// - 출력은 설정의 OutputName(이름)으로 먼저 찾고, 없으면 OutputIndex(번호, 기존 방식)를 사용
//        /// - 측정 ToolBlock이 Accept가 아니면 값 대신 NG(상태) 표시 (1단계 1-3 유지)
//        /// - 공차를 벗어난 값은 TextBox를 빨간색으로 표시
//        /// - readValues가 false(위치 확인 실패)면 값을 읽지 않고 "-" 표시
//        /// </summary>
//        private void evaluateMeasures(InspectionResult result, bool readValues)
//        {
//            result.Measures.Add(evaluateMeasure(leftSide, settings.LeftDist1, readValues));
//            result.Measures.Add(evaluateMeasure(leftSide, settings.LeftDist2, readValues));
//            result.Measures.Add(evaluateMeasure(rightSide, settings.RightDist1, readValues));
//            result.Measures.Add(evaluateMeasure(rightSide, settings.RightDist2, readValues));

//            TextBox[] boxes = { textBox1, textBox2, textBox3, textBox4 };
//            for (int i = 0; i < boxes.Length; i++)
//            {
//                boxes[i].Text = result.Measures[i].Text;
//                boxes[i].BackColor = (readValues && !result.Measures[i].IsOk) ? Color.LightPink : SystemColors.Control;
//            }
//        }


//        /// <summary>
//        /// 261028@Claude [3단계 3-1] 측정값 하나 판정
//        /// </summary>
//        private MeasureResult evaluateMeasure(CogToolBlock sideBlock, MeasureSpec spec, bool readValues)
//        {
//            MeasureResult measure = new MeasureResult();
//            measure.Name = spec.DisplayName;

//            if (!readValues)
//            {
//                measure.Text = "-";
//                return measure;
//            }
//            if (sideBlock == null)
//            {
//                measure.Text = "N/A";
//                return measure;
//            }

//            CogToolResultConstants status = sideBlock.RunStatus.Result;
//            if (status != CogToolResultConstants.Accept)
//            {
//                measure.Text = "NG(" + status + ")";
//                return measure;
//            }

//            double? value = readOutputValue(sideBlock, spec);
//            if (!value.HasValue)
//            {
//                measure.Text = "N/A";
//                return measure;
//            }

//            measure.Value = value;
//            measure.Text = value.Value.ToString("F2");
//            measure.IsOk = !spec.UseTolerance || (value.Value >= spec.Min && value.Value <= spec.Max);
//            return measure;
//        }


//        /// <summary>
//        /// 261028@Claude [3단계 3-4] ToolBlock 출력값 읽기: 이름 우선, 없으면 번호. 숫자가 아니면 null
//        /// </summary>
//        private double? readOutputValue(CogToolBlock block, MeasureSpec spec)
//        {
//            object value = null;
//            bool isFound = false;

//            if (!string.IsNullOrEmpty(spec.OutputName))
//            {
//                foreach (CogToolBlockTerminal terminal in block.Outputs)
//                {
//                    if (terminal.Name == spec.OutputName)
//                    {
//                        value = terminal.Value;
//                        isFound = true;
//                        break;
//                    }
//                }
//                if (!isFound)
//                {
//                    AppLogger.Info($"{block.Name} 출력 '{spec.OutputName}'을(를) 찾지 못해 번호 {spec.OutputIndex}를 사용");
//                }
//            }

//            if (!isFound)
//            {
//                if (spec.OutputIndex < 0 || spec.OutputIndex >= block.Outputs.Count)
//                {
//                    return null;
//                }
//                value = block.Outputs[spec.OutputIndex].Value;
//            }

//            if (value is IConvertible)
//            {
//                try
//                {
//                    return Convert.ToDouble(value);
//                }
//                catch (FormatException) { }
//                catch (InvalidCastException) { }
//            }
//            return null;
//        }


//        /// <summary>
//        /// 261028@Claude [3단계 3-7] LEFTside/RIGHTside의 측정 그래픽을 결과 화면(Display2)에 겹쳐 표시
//        /// 그래픽 표시에 실패해도 검사 결과에는 영향이 없도록 로그만 남긴다.
//        /// </summary>
//        private void showMeasureGraphics()
//        {
//            try
//            {
//                if (leftSide != null)
//                {
//                    addRecordGraphics(leftSide.CreateLastRunRecord());
//                }
//                if (rightSide != null)
//                {
//                    addRecordGraphics(rightSide.CreateLastRunRecord());
//                }
//            }
//            catch (Exception ex)
//            {
//                AppLogger.Error("측정 그래픽 표시 실패", ex);
//            }
//        }


//        /// <summary>
//        /// 261028@Claude [3단계 3-7] 레코드(하위 레코드 포함)에 들어 있는 그래픽을 Display2에 추가
//        /// </summary>
//        private void addRecordGraphics(ICogRecord record)
//        {
//            if (record == null)
//            {
//                return;
//            }

//            if (record.Content is CogGraphicCollection graphics)
//            {
//                foreach (ICogGraphic graphic in graphics)
//                {
//                    cogRecordDisplay2.StaticGraphics.Add(graphic, "Measure");
//                }
//            }
//            else if (record.Content is ICogGraphic graphic)
//            {
//                cogRecordDisplay2.StaticGraphics.Add(graphic, "Measure");
//            }

//            if (record.SubRecords != null)
//            {
//                for (int i = 0; i < record.SubRecords.Count; i++)
//                {
//                    addRecordGraphics(record.SubRecords[i]);
//                }
//            }
//        }


//        /// <summary>
//        /// 261028@Claude [3단계 3-1, 5-2] 검사 마무리: OK/NG 표시, 카운트, CSV·NG 이미지 저장, 상태 표시
//        /// </summary>
//        private void completeInspection(InspectionResult result)
//        {
//            showJudge(result.IsOk);

//            if (result.IsOk)
//            {
//                okCount++;
//            }
//            else
//            {
//                ngCount++;
//            }
//            lastElapsedMs = result.ElapsedMs;

//            if (settings.SaveResultCsv)
//            {
//                ResultRecorder.AppendCsv(result);
//            }
//            if (!result.IsOk && settings.SaveNgImage)
//            {
//                ResultRecorder.SaveNgImage(currentInputImage, result.Time);
//            }

//            updateStatusDisplay();
//        }


//        /// <summary>
//        /// 261028@Claude [3단계 3-1] 최종 OK/NG를 크게 표시
//        /// </summary>
//        private void showJudge(bool isOk)
//        {
//            lblJudge.Text = isOk ? "OK" : "NG";
//            lblJudge.BackColor = isOk ? Color.LimeGreen : Color.Red;
//            lblJudge.ForeColor = Color.White;
//        }


//        /// <summary>
//        /// 261028@Claude [3단계 5-2] 제목줄과 상태 표시줄 갱신 (VPP, 모드, 검사 시간, OK/NG 카운트)
//        /// </summary>
//        private void updateStatusDisplay()
//        {
//            string vppName = string.IsNullOrEmpty(loadedVppPath) ? "(VPP 없음)" : System.IO.Path.GetFileName(loadedVppPath);
//            string mode = chkFileMode.Checked ? "파일" : "카메라";

//            this.Text = "VproManageBlack02 - " + vppName;
//            lblStatus.Text = $"VPP: {vppName}    |    모드: {mode}    |    검사 시간: {lastElapsedMs} ms    |    OK: {okCount}    NG: {ngCount}";
//        }


//        /// <summary>
//        /// 261028@Claude [3단계 3-2] 설정 파일(InspectionSettings.xml)을 메모장으로 열기
//        /// 저장 후 다음 검사부터 반영된다.
//        /// </summary>
//        private void btnSettings_Click(object sender, EventArgs e)
//        {
//            try
//            {
//                if (!System.IO.File.Exists(InspectionSettings.FilePath))
//                {
//                    settings.Save();
//                }
//                System.Diagnostics.Process.Start("notepad.exe", "\"" + InspectionSettings.FilePath + "\"");
//            }
//            catch (Exception ex)
//            {
//                AppLogger.Error("설정 파일 열기 실패", ex);
//                MessageBox.Show($"설정 파일을 열 수 없습니다: {ex.Message}\n{InspectionSettings.FilePath}");
//            }
//        }


//        // ===================================================================================================
//        // 261028@Claude [3단계] 아래는 1단계 결과 표시 코드 (주석 처리)
//        // - 공차 판정, 이름 기반 출력 읽기가 없었음 → evaluateMeasures / evaluateMeasure / readOutputValue로 대체
//        // ===================================================================================================
//        ///// <summary>
//        ///// 검사 후 측저 결과를 읽어와서 TextBox에 표시
//        ///// </summary>        
//        //private void getInspectResults()
//        //{
//        //    // 261028@Claude [1단계 1-3] 측정 ToolBlock의 RunStatus를 확인한 뒤에만 Outputs를 읽는다.
//        //    // 실패한 툴의 Outputs에는 이전 검사값이 남아 있으므로 그대로 표시하면 안 된다.
//        //    showSideResult(leftSide, textBox1, textBox2);
//        //    showSideResult(rightSide, textBox3, textBox4);

//        //    //double leftDist1 = 0.0;
//        //    //double leftDist2 = 0.0;
//        //    //double rightDist1 = 0.0;
//        //    //double rightDist2 = 0.0;

//        //    //leftDist1 = (double)leftSide.Outputs[0].Value;
//        //    //leftDist2 = (double)leftSide.Outputs[1].Value;

//        //    //rightDist1 = (double)rightSide.Outputs[0].Value;
//        //    //rightDist2 = (double)rightSide.Outputs[1].Value;

//        //    //textBox1.Text = leftDist1.ToString("F2");
//        //    //textBox2.Text = leftDist2.ToString("F2");
//        //    //textBox3.Text = rightDist1.ToString("F2");
//        //    //textBox4.Text = rightDist2.ToString("F2");
//        //}


//        ///// <summary>
//        ///// 261028@Claude [1단계 1-3] 측정 ToolBlock(LEFTside/RIGHTside) 하나의 결과를 TextBox 2개에 표시
//        ///// 툴이 없거나 Accept가 아니면 측정값 대신 NG 상태를 표시한다.
//        ///// </summary>
//        //private void showSideResult(CogToolBlock sideBlock, TextBox txtDist1, TextBox txtDist2)
//        //{
//        //    if (sideBlock == null)
//        //    {
//        //        txtDist1.Text = "N/A";
//        //        txtDist2.Text = "N/A";
//        //        return;
//        //    }

//        //    CogToolResultConstants result = sideBlock.RunStatus.Result;
//        //    if (result != CogToolResultConstants.Accept)
//        //    {
//        //        txtDist1.Text = "NG(" + result + ")";
//        //        txtDist2.Text = "NG(" + result + ")";
//        //        return;
//        //    }

//        //    txtDist1.Text = formatOutputValue(sideBlock, 0);
//        //    txtDist2.Text = formatOutputValue(sideBlock, 1);
//        //}


//        ///// <summary>
//        ///// 261028@Claude [1단계 1-4] ToolBlock 출력값을 안전하게 문자열로 변환 (없거나 숫자가 아니면 N/A)
//        ///// </summary>
//        //private string formatOutputValue(CogToolBlock block, int index)
//        //{
//        //    if (index >= block.Outputs.Count)
//        //    {
//        //        return "N/A";
//        //    }

//        //    object value = block.Outputs[index].Value;
//        //    if (value is IConvertible)
//        //    {
//        //        try
//        //        {
//        //            return Convert.ToDouble(value).ToString("F2");
//        //        }
//        //        catch (FormatException) { }
//        //        catch (InvalidCastException) { }
//        //    }
//        //    return "N/A";
//        //}


//        /// <summary>
//        /// 261028@Claude [1단계 1-5] 모드별로 검사 실행에 필요한 객체가 준비되었는지 확인
//        /// 준비되었으면 null, 아니면 사용자에게 보여줄 메시지를 반환한다.
//        /// - 공통: myToolGroup, myToolGroupGroup, IPOneImageTool
//        /// - 라이브 카메라 모드: acqFifoTool 추가 (myToolGroup.Run()으로 실행하므로 jobManager는 필요 없음)
//        /// </summary>
//        private string getNotReadyMessage(bool isOfflineFileMode)
//        {
//            if (myToolGroup == null || myToolGroupGroup == null)
//            {
//                return "비전 시스템이 초기화되지 않았습니다. Model Load를 먼저 실행하세요.";
//            }
//            if (IPOneImageTool == null)
//            {
//                return "'CogIPOneImageTool1' 툴이 없어 검사를 실행할 수 없습니다.";
//            }
//            if (!isOfflineFileMode && acqFifoTool == null)
//            {
//                return "'CogAcqFifoTool1' 툴이 없어 카메라 모드를 실행할 수 없습니다.";
//            }
//            return null;
//        }


//        /// <summary>
//        /// 이미지 파일 경로로부터 ICogImage 객체를 생성하는 헬퍼 메서드
//        /// </summary>
//        private ICogImage LoadImageFromFile()
//        {
//            // 1. 파일 열기 다이얼로그 생성
//            using (OpenFileDialog openFileDialog = new OpenFileDialog())
//            {
//                // 다이얼로그 초기 설정
//                openFileDialog.Title = "비전 검사할 이미지 선택";
//                openFileDialog.Filter = "이미지 파일 (*.bmp;*.jpg;*.png)|*.bmp;*.jpg;*.png|모든 파일 (*.*)|*.*";
//                // 261028@Claude [3단계] 마지막으로 이미지를 연 폴더에서 시작 (없으면 기존 경로)
//                // openFileDialog.InitialDirectory = @"D:\0_JDIN\JDIN_VisionProgram\jdinVPro\VPro\VproBlack";
//                openFileDialog.InitialDirectory = System.IO.Directory.Exists(settings.LastImageFolder)
//                    ? settings.LastImageFolder
//                    : @"D:\0_JDIN\JDIN_VisionProgram\jdinVPro\VPro\VproBlack";

//                // 2. 사용자가 파일을 선택하고 [열기]를 눌렀을 때
//                if (openFileDialog.ShowDialog() == DialogResult.OK)
//                {
//                    try
//                    {
//                        string selectedImagePath = openFileDialog.FileName;

//                        // 261028@Claude [3단계] 이미지 출처(CSV 기록용)와 마지막 폴더 저장
//                        currentImageSource = selectedImagePath;
//                        settings.LastImageFolder = System.IO.Path.GetDirectoryName(selectedImagePath);
//                        settings.Save();

//                        // CogImageFileTool을 사용하면 파일 열기/닫기 및 독립된 이미지 객체 관리가 안전합니다.
//                        using (CogImageFileTool fileTool = new CogImageFileTool())
//                        {
//                            fileTool.Operator.Open(selectedImagePath, CogImageFileModeConstants.Read);

//                            // 파일 내의 첫 번째 이미지 프레임 가져오기
//                            ICogImage loadedImage = fileTool.Operator[0];

//                            // [중요] 툴을 닫기 전에 이미지가 메모리에 온전히 유지되도록 
//                            // 독립된 복사본(Deep Copy)을 만들거나 파일 툴의 출력으로 할당 후 가져옵니다.
//                            // 여기서는 가장 안전하게 CogImageFile 객체를 직접 다루는 표준 방식을 적용합니다.

//                            fileTool.Operator.Close();

//                            // 불러온 이미지를 반환
//                            return loadedImage;
//                        }
//                    }
//                    catch (Exception ex)
//                    {
//                        // 261028@Claude [2단계 2-8] 로그 기록
//                        AppLogger.Error("이미지 파일 로드 오류: " + openFileDialog.FileName, ex);
//                        MessageBox.Show($"이미지 처리 중 에러 발생: {ex.Message}");
//                    }
//                }
//            } // End of using OpenFileDialog

//            // 사용자가 취소를 눌렀거나 에러가 발생한 경우 null 반환 (컴파일 에러 방지)
//            return null;
//        }

//        private void chkFileMode_CheckedChanged(object sender, EventArgs e)
//        {
//            // 261028@Claude [3단계 5-2] 모드가 바뀌면 상태 표시 갱신
//            updateStatusDisplay();
//        }

//        private void button1_Click_1(object sender, EventArgs e)
//        {
//            btnLoadJob_Click();
//        }
//    }
//}
