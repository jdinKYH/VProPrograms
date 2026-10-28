// Cognex
using Cognex.VisionPro;
using Cognex.VisionPro.CalibFix;
using Cognex.VisionPro.Display;
using Cognex.VisionPro.ImageFile;
using Cognex.VisionPro.ImageProcessing;
using Cognex.VisionPro.PMAlign;
using Cognex.VisionPro.QuickBuild;
using Cognex.VisionPro.QuickBuild.Implementation.Internal;
using Cognex.VisionPro.ToolBlock;
using Cognex.VisionPro.ToolGroup;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;



// ===============================================================================
//  1023@jdin "1023_QB_CBoxNoSwitch.vpp" 사용할 것
//  tbImageSource를 사용하지 않고
//  CogIPOneImageTool1을 사용하여 InputImage를 직접 주입하는 방식으로 변경
//  입력은 CogAcqFifoTool로 입력이 기본, CheckBox를 통해 파일 입력 모드로 변경 
// ===============================================================================
namespace VproManageBlack02
{
    public partial class Form1 : Form
    {
        CogJobManager jobManager;
        CogToolGroup myToolGroup;
        CogToolGroup myToolGroupGroup;         // 261023@jdin 추가
        // CogToolBlock tbImageSource;
        CogToolBlock locate;
        CogToolBlock leftSide;
        CogToolBlock rightSide;
        CogFixtureTool fixtureTool;
        CogPMAlignTool pmAlignTool;
        CogIPOneImageTool IPOneImageTool;
        CogAcqFifoTool acqFifoTool; 

        // 261019@jdin 폼 클래스 내부에 선언
        // 참조에 Cognex.VisionPro.Controls가 있어야 된다.
        private CogRecordDisplay cogRecordDisplay1;
        private CogRecordDisplay cogRecordDisplay2;


        // 261019@jdin CogDisplay 선언
        private CogDisplay cogDisplayOut;


        //List<CogJob> job = new List<CogJob>();
        //List<CogJobIndependent> jobIndependent = new List<CogJobIndependent>();

        string jobFilePath;

        public Form1()
        {
            InitializeComponent();


            // 코드로 CogRecordDisplay 생성 및 설정
            cogRecordDisplay1 = new CogRecordDisplay();
            cogRecordDisplay1.Location = new System.Drawing.Point(30, 50); // (가로, 세로)
            cogRecordDisplay1.Size = new System.Drawing.Size(600, 600);


            // 코드로 CogRecordDisplay 생성 및 설정
            cogRecordDisplay2 = new CogRecordDisplay();
            cogRecordDisplay2.Location = new System.Drawing.Point(750, 50); // (가로, 세로)
            cogRecordDisplay2.Size = new System.Drawing.Size(600, 600);

            // 폼에 컨트롤 추가
            this.Controls.Add(cogRecordDisplay1);   // input image
            this.Controls.Add(cogRecordDisplay2);   // result image


            //// 261019@jdin CogDisplay
            //cogDisplayOut = new CogDisplay();
            //cogDisplayOut.Location = new System.Drawing.Point(1500, 700); // (가로, 세로)
            //cogDisplayOut.Size = new System.Drawing.Size(100, 100);
            //cogDisplayOut.Dock = DockStyle.Fill;
            //this.Controls.Add(cogDisplayOut);            
        }



        /// <summary>
        /// QuickBuild 응용프로그램 Job 파일 경로 지정
        /// </summary>
        private void btnLoadJob_Click()
        {
            // Open a file dialog to select the VPP file
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "VisionPro Job Files (*.vpp)|*.vpp";
                    openFileDialog.Title = "Select a VisionPro Job File";
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        jobFilePath = openFileDialog.FileName;
                        LoadVisionProJob(jobFilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading job file: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }


        /// <summary>
        ///  "QuickBuilder응용 프로그램"을 지정된 경로에서 읽어온다.
        ///  "QuickBuilder응용 프로그램" 으로 저장 된 Job이다. 
        /// </summary>
        /// <param name="vppPath"></param>
        private void LoadVisionProJob(string vppPath)
        {
            // 
            try
            {
                // "QuickBuilder응용 프로그램" 으로 저장 된 Job으로 저장한 것을 읽어온다.
                // 작업 JOb 파일은 QuickBuilder에서 하단에 "CongJob1"으로 표시되는 것을 말한다.
                object loadedobject = CogSerializer.LoadObjectFromFile(vppPath);

                if (loadedobject is CogJobManager manager)
                {
                    jobManager = manager;

                    int jobCount = (int)jobManager.JobCount;    // 1023_QB_CBoxNoSwitch.vpp의 JobCount는 1이다.

                    // Job 내부에 있는 ToolGroup 및 이미지 스위치 툴블록 가져오기
                    // (구조에 따라 Job 인덱스나 이름은 현장 환경에 맞게 수정 필요)
                    //[0] { Cognex.VisionPro.QuickBuild.Implementation.Internal.CogInputImageTool}
                    //[1] { Cognex.VisionPro.CogAcqFifoTool}
                    //[2] { Cognex.VisionPro.ToolGroup.CogToolGroup} <= 이것을 ToolGroup을 사용
                    myToolGroup = jobManager.Job(0).VisionTool as CogToolGroup;

                    // CogAcqFifoTool acqFifoTool = myToolGroup.Tools[1] as CogAcqFifoTool;


                    // 아래 2. 툴그룹 내부에서 스위치 툴블록 찾아오기 에서 진행
                    //if (myToolGroup != null)
                    //{
                    //    tbImageSource = myToolGroup.Tools["tbImageSource"] as CogToolBlock;
                    //}
                }
                else if (loadedobject is CogJob job)
                {
                    // Case B: 파일이 단일 Job 구조인 경우
                    jobManager = null; // 필요시 관리 객체 조정
                    myToolGroup = job.VisionTool as CogToolGroup;
                }
                else if (loadedobject is CogToolGroup toolGroup)
                {
                    // Case C: 파일이 툴그룹(ToolGroup) 자체로 저장된 경우 (이 경우가 매우 많습니다!)
                    jobManager = null;
                    myToolGroup = toolGroup;
                }
                else
                {
                    MessageBox.Show($"지원하지 않는 VPP 파일 형식입니다. 타입: {loadedobject?.GetType().Name}");
                    return;
                }

                // 2. 툴그룹 내부에서 툴블록 찾아오기
                if (myToolGroup != null)
                {
                    //[0] { Cognex.VisionPro.QuickBuild.Implementation.Internal.CogInputImageTool}
                    //[1] { Cognex.VisionPro.CogAcqFifoTool}
                    //[2] { Cognex.VisionPro.ToolGroup.CogToolGroup} <= 이것을 ToolGroup을 사용
                    //      [0] {Cognex.VisionPro.ImageProcessing.CogIPOneImageTool}
                    //      [1] { Cognex.VisionPro.ToolBlock.CogToolBlock}
                    //      [2] { Cognex.VisionPro.ToolBlock.CogToolBlock}
                    //      [3] { Cognex.VisionPro.ToolBlock.CogToolBlock}
                    myToolGroupGroup = myToolGroup.Tools[2] as CogToolGroup;

                    // ====================================================================
                    // 261023@jdin 사용하지 않는다. 
                    // 툴그룹 내부에서 스위치 툴블록 찾아오기
                    //tbImageSource = myToolGroup.Tools["tbImageSource"] as CogToolBlock;
                    //if (tbImageSource == null)
                    //{
                    //    MessageBox.Show("툴그룹 내부에 'tbImageSource' 툴블록이 존재하지 않습니다. 이름을 확인해주세요.");
                    //}
                    // ====================================================================

                    // Tool 그룹 안에서 Tool 존재 확인
                    acqFifoTool = myToolGroup.Tools["CogAcqFifoTool1"] as CogAcqFifoTool;
                    if (acqFifoTool == null)
                    {
                        MessageBox.Show("툴그룹 내부에 'CogAcqFifoTool1' 툴이 존재하지 않습니다. 이름을 확인해주세요.");
                    }
                    IPOneImageTool = myToolGroupGroup.Tools["CogIPOneImageTool1"] as CogIPOneImageTool;
                    if (IPOneImageTool == null)
                    {
                        MessageBox.Show("툴그룹 내부에 'CogIPOneImageTool1' 툴이 존재하지 않습니다. 이름을 확인해주세요.");
                    }
                    locate = myToolGroupGroup.Tools["Locate"] as CogToolBlock;
                    if (locate == null)
                    {
                        MessageBox.Show("툴그룹 내부에 'Locate' 툴블록이 존재하지 않습니다. 이름을 확인해주세요.");
                    }
                    leftSide = myToolGroupGroup.Tools["LEFTside"] as CogToolBlock;
                    if (leftSide == null)
                    {
                        MessageBox.Show("툴그룹 내부에 'LEFTside' 툴블록이 존재하지 않습니다. 이름을 확인해주세요.");
                    }
                    rightSide = myToolGroupGroup.Tools["RIGHTside"] as CogToolBlock;
                    if (rightSide == null)
                    {
                        MessageBox.Show("툴그룹 내부에 'RIGHTside' 툴블록이 존재하지 않습니다. 이름을 확인해주세요.");
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"VPP 파일 로드 실패: {ex.Message}");
            }
        }



        /// <summary>
        /// 검사 실행을 하기 전에 PMAlignTool의 결과를 확인하여 위치 확인 여부를 판단
        /// Location 확인이 실패하면 검사 실행을 하지 않음
        /// </summary>
        /// <returns></returns>
        private bool isCheckLocate()
        {
            // 261028@Claude [1단계 2-4] locate, pmAlignTool, Results의 null 및 실행 에러 확인
            if (locate == null)
            {
                textBox5.Text = "NG - No Locate";
                return false;
            }
            if (locate.RunStatus.Result == CogToolResultConstants.Error)
            {
                textBox5.Text = "NG - Locate Error";
                return false;
            }

            // PMAlignTool 실행 결과 확인
            pmAlignTool = locate.Tools["CogPMAlignTool1"] as CogPMAlignTool;
            // PMAlignTool의 결과 실패하면 결과 값이 없다. 즉 null이다.
            // 기준 Score 값보다 작게 나오는 것과 다르다.
            // 예외처리가 필요하다.
            if (pmAlignTool == null || pmAlignTool.Results == null)
            {
                textBox5.Text = "NG - No PMAlign";
                return false;
            }
            int alignCount = pmAlignTool.Results.Count;
            if (alignCount == 0)
            {
                textBox5.Text = "NG - No Result";
                return false;     // 검사실행을 하지 않음
            }
            else
            {
                double pmScore = pmAlignTool.Results[0].Score;
                // 0~1 사이의 값으로, 1에 가까울수록 정렬이 잘 된 것으로 판단                
                if (pmScore < 0.6)
                {
                    textBox5.Text = pmScore.ToString("F2") + " - FAIL";
                    return false; // 검사실행을 하지 않음
                }
                else
                {
                    textBox5.Text = pmScore.ToString("F2") + " - PASS";
                    return true;
                }
            }
        }


        /// --- [1] 오프라인 파일 모드 ---
        /// 261028@jdin 오프라인 파일 모드를 분리함.
        /// --- [1] 오프라인 파일 모드 ---
        /// 261028@Claude [1단계 1-2] 이미지 로드 실패·취소를 호출부에 알리도록 bool 반환으로 변경
        // private void offlineFileMode()
        private bool offlineFileMode()
        {
            // --- [1] 오프라인 파일 모드 ---
            // 1. 파일에서 이미지 로드 (Cognex 파일 로더 또는 일반 이미지 로드 방식 활용)
            ICogImage fileImage = LoadImageFromFile();
            if (fileImage == null)
            {
                MessageBox.Show("이미지 파일을 불러오지 못했습니다.");
                // return;
                return false;
            }

            IPOneImageTool.InputImage = fileImage;
            ICogImage cogImage3 = (ICogImage)IPOneImageTool.InputImage;
            cogRecordDisplay1.Image = cogImage3;
            cogRecordDisplay1.Fit();

            // 3. 카메라 타임아웃을 원천 차단하기 위해 검사 툴그룹만 단독 실행
            myToolGroupGroup.Run();

            // Tool 실행 후 출력 이미지 확인
            ICogImage cogImage1 = (ICogImage)IPOneImageTool.OutputImage;
            cogRecordDisplay1.Image = cogImage1;
            cogRecordDisplay1.Fit();

            return true;
        }



        /// --- [2] 라이브 카메라 모드 ---
        /// 261028@jdin 라이브 카메라 모드를 분리함.
        /// --- [2] 라이브 카메라 모드 --- 
        private bool liveCameraMode()
        {
            // --- [2] 라이브 카메라 모드 ---
            // 1. IPOneImageTool.InputImage 초기화
            IPOneImageTool.InputImage = null;
            

            // 2. 카메라 취득부터 검사까지 전체 잡 매니저 실행
            // 261028@Claude [1단계 1-1] jobManager.Run()은 비동기라 결과를 너무 일찍 읽는 문제가 있었음.
            // 동기 실행인 myToolGroup.Run()으로 통일 (Run이 끝난 뒤 결과를 읽으므로 이전 값 표시·동시 접근 없음)
            // jobManager.Run();
            myToolGroup.Run();   // myToolGroup.Run();

            // 3. 카메라 취득 결과 확인
            if (acqFifoTool.RunStatus.Result != CogToolResultConstants.Accept)
            {
                MessageBox.Show("카메라 이미지 취득 실패");
                return false;
            }
            else if (acqFifoTool.OutputImage == null)
            {
                MessageBox.Show("OutputImage가 null입니다. 이미지가 들어오지 않았습니다.");
                return false;
            }

            // 261027@jdin TEMP
            ICogImage cogImage5 = (ICogImage)acqFifoTool.OutputImage;
            cogRecordDisplay1.Image = cogImage5;
            // 화면 크기에 맞게 이미지 자동 조절 (Fit)
            cogRecordDisplay1.Fit();

            // IPOneImageTool.OutputImage는 InputImag입력이 없어도, 기존 값을 가지고 있다.
            // 따라서, IPOneImageTool.IntputImage = null인지 확인해야 한다.

            ICogImage cogImage1 = (ICogImage)IPOneImageTool.OutputImage;
            cogRecordDisplay1.Image = cogImage1;
            // 화면 크기에 맞게 이미지 자동 조절 (Fit)
            cogRecordDisplay1.Fit();


            return true;
        }   

        /// <summary>
        /// 
        /// </summary>
        private void clearDisplayAll()
        {

            // Display1, 2 Clear
            cogRecordDisplay1.StaticGraphics.Clear();       // 고정 그래픽 제거
            cogRecordDisplay1.InteractiveGraphics.Clear();  // 인터랙티브 그래픽 제거
            cogRecordDisplay1.Record = null;                // Record로 표시했다면 제거
            cogRecordDisplay1.Image = null;                 // 이미지 제거
            cogRecordDisplay1.Fit();

            //// 1단계: 강제 다시 그리기
            //cogRecordDisplay1.Invalidate();
            //cogRecordDisplay1.Refresh();
            //// 2단계(테스트용): 잠깐 숨겨보기
            //cogRecordDisplay1.Visible = false;
            //MessageBox.Show("지금 Display1 화면에 이미지가 남아 있나요?");
            //cogRecordDisplay1.Visible = true;


            cogRecordDisplay2.StaticGraphics.Clear();       // 고정 그래픽 제거
            cogRecordDisplay2.InteractiveGraphics.Clear();  // 인터랙티브 그래픽 제거
            cogRecordDisplay2.Record = null;                // Record로 표시했다면 제거
            cogRecordDisplay2.Image = null;                 // 이미지 제거
            cogRecordDisplay2.Fit();
            //// 1단계: 강제 다시 그리기
            //cogRecordDisplay2.Invalidate();
            //cogRecordDisplay2.Refresh();
            //// 2단계(테스트용): 잠깐 숨겨보기
            //cogRecordDisplay2.Visible = false;
            //MessageBox.Show("지금 Display2 화면에 이미지가 남아 있나요?");
            //cogRecordDisplay2.Visible = true;


            //MessageBox.Show(
            //        $"Image null: {cogRecordDisplay2.Image == null}\n" +
            //        $"Record null: {cogRecordDisplay2.Record == null}\n" +
            //        $"Live: {cogRecordDisplay2.LiveDisplayRunning}\n" +
            //        $"Visible: {cogRecordDisplay2.Visible}, Parent: {cogRecordDisplay2.Parent?.Name}");

        }


        /// <summary>
        /// 검사 실행 버튼 클릭 이벤트 (라이브 vs 오프라인 파일 모드 분기)
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            // 261028@Claude [1단계 1-5] jobManager가 없으면(VPP가 CogJob/ToolGroup 형식) 파일 모드까지 막히던 문제.
            // 모드별로 필요한 객체만 확인하도록 변경
            //if (jobManager == null || myToolGroup == null)
            //{
            //    MessageBox.Show("비전 시스템이 초기화되지 않았습니다.");
            //    return;
            //}
            string notReadyMessage = getNotReadyMessage(chkFileMode.Checked);
            if (notReadyMessage != null)
            {
                MessageBox.Show(notReadyMessage);
                return;
            }

            // Clear Disply 1, 2
            clearDisplayAll();

            // Clear Old Inspect Result
            clearInspectResults();

            // 1. IPOneImageTool.InputImage 초기화
            IPOneImageTool.InputImage = null;

            // 2. 검사 실행 전 결과 레코드 초기화
            ICogRecords resultRecords = myToolGroup.CreateLastRunRecord().SubRecords;
            resultRecords.Clear();

            //  261021@jdin 1021_VINA_BK_QB03.vpp 사용할 것
            // UI의 체크박스 상태 등으로 모드 판별 (true: 파일 검사, false: 라이브 카메라)
            bool isOfflineFileMode = chkFileMode.Checked;

            // 261028@Claude [1단계 1-2, 1-4] 모드별 실행 → 위치 확인 → 결과 표시를 하나의 try 안에서 처리.
            // 이미지 취득·로드가 실패하거나 위치 확인이 실패하면 즉시 중단하고,
            // 측정 결과는 모든 단계가 성공했을 때만 읽는다. (이전 검사값 표시·처리되지 않은 예외 방지)
            try
            {
                // [1] 파일 모드 / [2] 라이브 카메라 모드 실행
                bool isRunOk = isOfflineFileMode ? offlineFileMode() : liveCameraMode();
                if (!isRunOk)
                {
                    if (!isOfflineFileMode)
                    {
                        MessageBox.Show("카메라 취득에 실패했습니다.");
                    }
                    return;
                }

                bool isLocate = isCheckLocate();
                if (!isLocate)
                {
                    MessageBox.Show("위치 확인에 실패했습니다.");
                    return;
                }

                // 결과 디스플레이 바인딩 (툴그룹 실행 결과 레코드 표시)
                // 3 - LastRun.Locate.CogFixtureTool1.outputImage <= 이것으로 함
                ICogRecord resultRecord = myToolGroup.CreateLastRunRecord().SubRecords["CogToolGroup1.Locate.CogFixtureTool1.OutputImage"];
                cogRecordDisplay2.Record = resultRecord;
                // 화면 크기에 맞게 이미지 자동 조절 (Fit)
                cogRecordDisplay2.Fit();

                // Results
                getInspectResults();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"검사 실행 오류 : {ex.Message}");
            }

            // ===================================================================================================
            // 261028@Claude [1단계] 아래는 변경 전 코드 (주석 처리)
            // - 파일 로드 실패·취소 시에도 위치 확인과 결과 읽기가 계속 진행됨 (1-2)
            // - getInspectResults()가 try 밖에서 항상 실행되어 예외 시 앱이 종료될 수 있음 (1-4)
            // ===================================================================================================
            //if (isOfflineFileMode)
            //{
            //    try
            //    {
            //        offlineFileMode();

            //        bool isLocate = isCheckLocate();

            //        if (!isLocate)
            //        {
            //            MessageBox.Show("위치 확인에 실패했습니다.");
            //            return;
            //        }

            //        // 4. 결과 디스플레이 바인딩 (툴그룹 실행 결과 레코드 표시)
            //        // ====================================================================================================
            //        // fixtureTool.CreateLastRunRecord().SubRecords[0]; => SubRecords[0]가 나와야 한다.
            //        // fixtureTool = (CogFixtureTool)locate.Tools["CogFixtureTool1"];
            //        // ICogRecord resultRecord = fixtureTool.CreateLastRunRecord().SubRecords[0];                
            //        // ====================================================================================================
            //        // 0 - CogIPOneImageTool1.InputImage
            //        // 1 - CogIPOneImageTool1.OutputImage
            //        // 2 - CogFixtureTool1.OutputImage
            //        // 4 - LEFTside.InputImage
            //        // 6 - RIGHTside.InputImage
            //        // resultRecords = myToolGroupGroup.CreateLastRunRecord().SubRecords; 위 리스트 참조할 것
            //        // 상위 myToolGroup과 하위 myToolGroupGroup이 다른 것을 주의할 것
            //        // ICogRecords resultRecords = myToolGroup.CreateLastRunRecord().SubRecords;

            //        ICogRecord resultRecord = myToolGroup.CreateLastRunRecord().SubRecords["CogToolGroup1.Locate.CogFixtureTool1.OutputImage"];
            //        cogRecordDisplay2.Record = resultRecord;
            //        // 5. 화면 크기에 맞게 이미지 자동 조절 (Fit)
            //        cogRecordDisplay2.Fit();
            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show($"Tool Group 실행 오류 : {ex.Message}");
            //    }
            //}
            //else
            //{   // --- [2] 라이브 카메라 모드 ---
            //    try
            //    {
            //        bool isCameraOk = liveCameraMode();

            //        if(!isCameraOk)
            //        {
            //            MessageBox.Show("카메라 취득에 실패했습니다.");
            //            return;
            //        }

            //        // CogAcqFifoTool acqFifoTool = myToolGroup.Tools[1] as CogAcqFifoTool;
            //        // CogAcqFifoTool acqFifoTool = myToolGroup.Tools["CogAcqFifoTool1"] as CogAcqFifoTool;


            //        // =======================================================================================
            //        // 261028@Jdin acqFifoTool 실행 테스트 
            //        // acqFifoTool.Run();
            //        //if (acqFifoTool.RunStatus.Result != CogToolResultConstants.Accept)
            //        //{
            //        //    MessageBox.Show("카메라 이미지 취득 실패");
            //        //    return;
            //        //}
            //        //else if(acqFifoTool.OutputImage == null)
            //        //{
            //        //    MessageBox.Show("OutputImage가 null입니다. 이미지가 들어오지 않았습니다.");
            //        //    return;
            //        //}
            //        // 
            //        //ICogImage cogImage9 = (ICogImage)acqFifoTool.OutputImage;
            //        //cogRecordDisplay1.Image = cogImage9;
            //        // 화면 크기에 맞게 이미지 자동 조절 (Fit)
            //        //cogRecordDisplay1.Fit();
            //        // =======================================================================================


            //        // --- [2] 라이브 카메라 모드 ---
            //        //ICogRecords resultRecords = myToolGroup.CreateLastRunRecord().SubRecords;
            //        //resultRecords.Clear();


            //        bool isLocate = isCheckLocate();

            //        if (!isLocate)
            //        {
            //            MessageBox.Show("위치 확인에 실패했습니다.");
            //            return;
            //        }

            //        // 결과 디스플레이 바인딩 (툴그룹 실행 결과 레코드 표시)
            //        // 0 - LastRun.Image Source.OutputImage,
            //        // 1 - LastRun.Locate.CogPMAlignTool1.InputImage,
            //        // 2 - Locate.CogPMAlignTool1.OutputImage
            //        // 3 - LastRun.Locate.CogFixtureTool1.outputImage <= 이것으로 함
            //        ICogRecord resultRecord = myToolGroup.CreateLastRunRecord().SubRecords["CogToolGroup1.Locate.CogFixtureTool1.OutputImage"];
            //        cogRecordDisplay2.Record = resultRecord;

            //        // 5. 화면 크기에 맞게 이미지 자동 조절 (Fit)
            //        cogRecordDisplay2.Fit();
            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show($"Job Manager 실행 오류 : {ex.Message}");
            //    }
            //}

            //// Reults
            //getInspectResults();
        }


        /// <summary>
        /// 검사 실행 전에 검사 결과를 초기화
        /// </summary>
        private void clearInspectResults()
        {
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";
            textBox5.Text = "";
        }


        /// <summary>
        /// 검사 후 측저 결과를 읽어와서 TextBox에 표시
        /// </summary>        
        private void getInspectResults()
        {
            // 261028@Claude [1단계 1-3] 측정 ToolBlock의 RunStatus를 확인한 뒤에만 Outputs를 읽는다.
            // 실패한 툴의 Outputs에는 이전 검사값이 남아 있으므로 그대로 표시하면 안 된다.
            showSideResult(leftSide, textBox1, textBox2);
            showSideResult(rightSide, textBox3, textBox4);

            //double leftDist1 = 0.0;
            //double leftDist2 = 0.0;
            //double rightDist1 = 0.0;
            //double rightDist2 = 0.0;

            //leftDist1 = (double)leftSide.Outputs[0].Value;
            //leftDist2 = (double)leftSide.Outputs[1].Value;

            //rightDist1 = (double)rightSide.Outputs[0].Value;
            //rightDist2 = (double)rightSide.Outputs[1].Value;

            //textBox1.Text = leftDist1.ToString("F2");
            //textBox2.Text = leftDist2.ToString("F2");
            //textBox3.Text = rightDist1.ToString("F2");
            //textBox4.Text = rightDist2.ToString("F2");
        }


        /// <summary>
        /// 261028@Claude [1단계 1-3] 측정 ToolBlock(LEFTside/RIGHTside) 하나의 결과를 TextBox 2개에 표시
        /// 툴이 없거나 Accept가 아니면 측정값 대신 NG 상태를 표시한다.
        /// </summary>
        private void showSideResult(CogToolBlock sideBlock, TextBox txtDist1, TextBox txtDist2)
        {
            if (sideBlock == null)
            {
                txtDist1.Text = "N/A";
                txtDist2.Text = "N/A";
                return;
            }

            CogToolResultConstants result = sideBlock.RunStatus.Result;
            if (result != CogToolResultConstants.Accept)
            {
                txtDist1.Text = "NG(" + result + ")";
                txtDist2.Text = "NG(" + result + ")";
                return;
            }

            txtDist1.Text = formatOutputValue(sideBlock, 0);
            txtDist2.Text = formatOutputValue(sideBlock, 1);
        }


        /// <summary>
        /// 261028@Claude [1단계 1-4] ToolBlock 출력값을 안전하게 문자열로 변환 (없거나 숫자가 아니면 N/A)
        /// </summary>
        private string formatOutputValue(CogToolBlock block, int index)
        {
            if (index >= block.Outputs.Count)
            {
                return "N/A";
            }

            object value = block.Outputs[index].Value;
            if (value is IConvertible)
            {
                try
                {
                    return Convert.ToDouble(value).ToString("F2");
                }
                catch (FormatException) { }
                catch (InvalidCastException) { }
            }
            return "N/A";
        }


        /// <summary>
        /// 261028@Claude [1단계 1-5] 모드별로 검사 실행에 필요한 객체가 준비되었는지 확인
        /// 준비되었으면 null, 아니면 사용자에게 보여줄 메시지를 반환한다.
        /// - 공통: myToolGroup, myToolGroupGroup, IPOneImageTool
        /// - 라이브 카메라 모드: acqFifoTool 추가 (myToolGroup.Run()으로 실행하므로 jobManager는 필요 없음)
        /// </summary>
        private string getNotReadyMessage(bool isOfflineFileMode)
        {
            if (myToolGroup == null || myToolGroupGroup == null)
            {
                return "비전 시스템이 초기화되지 않았습니다. Model Load를 먼저 실행하세요.";
            }
            if (IPOneImageTool == null)
            {
                return "'CogIPOneImageTool1' 툴이 없어 검사를 실행할 수 없습니다.";
            }
            if (!isOfflineFileMode && acqFifoTool == null)
            {
                return "'CogAcqFifoTool1' 툴이 없어 카메라 모드를 실행할 수 없습니다.";
            }
            return null;
        }


        /// <summary>
        /// 이미지 파일 경로로부터 ICogImage 객체를 생성하는 헬퍼 메서드
        /// </summary>
        private ICogImage LoadImageFromFile()
        {
            // 1. 파일 열기 다이얼로그 생성
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                // 다이얼로그 초기 설정
                openFileDialog.Title = "비전 검사할 이미지 선택";
                openFileDialog.Filter = "이미지 파일 (*.bmp;*.jpg;*.png)|*.bmp;*.jpg;*.png|모든 파일 (*.*)|*.*";
                openFileDialog.InitialDirectory = @"D:\0_JDIN\JDIN_VisionProgram\jdinVPro\VPro\VproBlack";

                // 2. 사용자가 파일을 선택하고 [열기]를 눌렀을 때
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string selectedImagePath = openFileDialog.FileName;

                        // CogImageFileTool을 사용하면 파일 열기/닫기 및 독립된 이미지 객체 관리가 안전합니다.
                        using (CogImageFileTool fileTool = new CogImageFileTool())
                        {
                            fileTool.Operator.Open(selectedImagePath, CogImageFileModeConstants.Read);

                            // 파일 내의 첫 번째 이미지 프레임 가져오기
                            ICogImage loadedImage = fileTool.Operator[0];

                            // [중요] 툴을 닫기 전에 이미지가 메모리에 온전히 유지되도록 
                            // 독립된 복사본(Deep Copy)을 만들거나 파일 툴의 출력으로 할당 후 가져옵니다.
                            // 여기서는 가장 안전하게 CogImageFile 객체를 직접 다루는 표준 방식을 적용합니다.

                            fileTool.Operator.Close();

                            // 불러온 이미지를 반환
                            return loadedImage;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"이미지 처리 중 에러 발생: {ex.Message}");
                    }
                }
            } // End of using OpenFileDialog

            // 사용자가 취소를 눌렀거나 에러가 발생한 경우 null 반환 (컴파일 에러 방지)
            return null;
        }

        private void chkFileMode_CheckedChanged(object sender, EventArgs e)
        {


        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            btnLoadJob_Click();
        }
    }
}
