// Cognex
using Cognex.VisionPro;
using Cognex.VisionPro.ImageFile;
using Cognex.VisionPro.ImageProcessing;
using Cognex.VisionPro.CalibFix;
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



// =============================================
//  261021@jdin 1021_VINA_BK_QB03.vpp 사용할 것
// =============================================
namespace VproManageBlack01
{
    public partial class Form1 : Form
    {
        CogJobManager jobManager;
        CogToolGroup myToolGroup;
        CogToolBlock tbImageSource;
        CogToolBlock locate;
        CogToolBlock leftSide;
        CogToolBlock rightSide;
        CogFixtureTool fixtureTool;
        CogIPOneImageTool IPOneImageTool;

        // 261019@jdin 폼 클래스 내부에 선언
        // 참조에 Cognex.VisionPro.Controls가 있어야 된다.
        private CogRecordDisplay cogRecordDisplay1;
        private CogRecordDisplay cogRecordDisplay2;



        List<CogJob> job = new List<CogJob>();
        List<CogJobIndependent> jobIndependent = new List<CogJobIndependent>();

        string jobFilePath;

        public Form1()
        {
            InitializeComponent();


            // 코드로 CogRecordDisplay 생성 및 설정
            cogRecordDisplay1 = new CogRecordDisplay();
            cogRecordDisplay1.Location = new System.Drawing.Point(10, 50); // (가로, 세로)
            cogRecordDisplay1.Size = new System.Drawing.Size(600, 600);
            

            // 코드로 CogRecordDisplay 생성 및 설정
            cogRecordDisplay2 = new CogRecordDisplay();
            cogRecordDisplay2.Location = new System.Drawing.Point(750, 50); // (가로, 세로)
            cogRecordDisplay2.Size = new System.Drawing.Size(600, 600);

            // 폼에 컨트롤 추가
            this.Controls.Add(cogRecordDisplay1);   // input image
            this.Controls.Add(cogRecordDisplay2);   // result image

            btnLoadJob_Click();
        }



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

        private void LoadVisionProJob(string vppPath)
        {
            //  261021@jdin 1021_VINA_BK_QB03.vpp 사용할 것
            try
            {
                // "QuickBuilder응용 프로그램" 으로 저장 된 Job으로 저장한 것을 읽어온다.
                // 작업 JOb 파일은 QuickBuilder에서 하단에 "CongJob1"으로 표시되는 것을 말한다.
                object loadedobject = CogSerializer.LoadObjectFromFile(vppPath);

                if (loadedobject is CogJobManager manager)
                {
                    jobManager = manager;

                    // Job 내부에 있는 ToolGroup 및 이미지 스위치 툴블록 가져오기
                    // (구조에 따라 Job 인덱스나 이름은 현장 환경에 맞게 수정 필요)
                    myToolGroup = jobManager.Job(0).VisionTool as CogToolGroup;

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
                    // 툴그룹 내부에서 스위치 툴블록 찾아오기
                    tbImageSource = myToolGroup.Tools["tbImageSource"] as CogToolBlock;
                    if (tbImageSource == null)
                    {
                        MessageBox.Show("툴그룹 내부에 'tbImageSource' 툴블록이 존재하지 않습니다. 이름을 확인해주세요.");
                    }

                    // 261023@jdin 추가 = 이미지 선택 테스트
                    IPOneImageTool = myToolGroup.Tools["CogIPOneImageTool1"] as CogIPOneImageTool;
                    if (IPOneImageTool == null)
                    {
                        MessageBox.Show("툴그룹 내부에 'CogIPOneImageTool1' 툴이 존재하지 않습니다. 이름을 확인해주세요.");
                    }
                    locate = myToolGroup.Tools["Locate"] as CogToolBlock;
                    if (tbImageSource == null)
                    {
                        MessageBox.Show("툴그룹 내부에 'Locate' 툴블록이 존재하지 않습니다. 이름을 확인해주세요.");
                    }
                    leftSide = myToolGroup.Tools["LEFTside"] as CogToolBlock;
                    if (tbImageSource == null)
                    {
                        MessageBox.Show("툴그룹 내부에 'LEFTside' 툴블록이 존재하지 않습니다. 이름을 확인해주세요.");
                    }
                    rightSide = myToolGroup.Tools["RIGHTside"] as CogToolBlock;
                    if (tbImageSource == null)
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
        /// 검사 실행 버튼 클릭 이벤트 (라이브 vs 오프라인 파일 모드 분기)
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            if (jobManager == null || myToolGroup == null || tbImageSource == null)
            {
                MessageBox.Show("비전 시스템이 초기화되지 않았습니다.");
                return;
            }

            //  261021@jdin 1021_VINA_BK_QB03.vpp 사용할 것
            // UI의 체크박스 상태 등으로 모드 판별 (true: 파일 검사, false: 라이브 카메라)
            bool isOfflineFileMode = chkFileMode.Checked;

            if (isOfflineFileMode)
            {
                // --- [1] 오프라인 파일 모드 ---
                // 1. 파일에서 이미지 로드 (Cognex 파일 로더 또는 일반 이미지 로드 방식 활용)
                // ICogImage fileImage = LoadImageFromFile(@"C:\Images\sample.bmp");
                ICogImage fileImage = LoadImageFromFile();

                if (fileImage == null)
                {
                    MessageBox.Show("이미지 파일을 불러오지 못했습니다.");
                    return;
                }

                // 2. 툴블록 터미널에 모드값(1)과 파일 이미지 주입
                tbImageSource.Inputs["IsFileMode"].Value = 1; // 1 = 파일 모드
                tbImageSource.Inputs["FileInputImage"].Value = fileImage;

               
                IPOneImageTool.InputImage = fileImage;
                //ICogImage cogImage3 = (ICogImage)tbImageSource.Inputs["FileInputImage"].Value;
                ICogImage cogImage3 = (ICogImage)IPOneImageTool.InputImage;
                cogRecordDisplay1.Image = cogImage3;
                cogRecordDisplay1.Fit();

                // input image : 261023 <= 정상적으로 입력이미지가 표시됨
                ICogImage cogImage1 = (ICogImage)IPOneImageTool.OutputImage;
                cogRecordDisplay1.Image = cogImage1;
                cogRecordDisplay1.Fit();


                ICogRecords resultRecords = myToolGroup.CreateLastRunRecord().SubRecords;
                resultRecords.Clear();

                // 3. 카메라 타임아웃을 원천 차단하기 위해 검사 툴그룹만 단독 실행
                myToolGroup.Run();


                // => 261023@jdin PMAlign Tool 결과 확인 추가
                // CogToolBlock locate;
                // locate = myToolGroup.Tools["Locate"] as CogToolBlock;
                double pmScore = (double)locate.Outputs["PMScore"].Value;
                if (pmScore < 0.5)
                {
                    textBox5.Text = "Align NG;";
                }
                else
                {
                    textBox5.Text = "Align OK";
                }


                // <= 261023@jdin PMAlign Tool 결과 확인 추가


                // input image 261023@jdin -> 위치 변경
                //ICogImage cogImage1 = (ICogImage)tbImageSource.Outputs["OutputImage"].Value;
                //cogRecordDisplay1.Image = cogImage1;
                //cogRecordDisplay1.Fit();



                //ICogRecords resultrecords = myToolGroup.CreateLastRunRecord().SubRecords;
                //resultrecords.Clear();

                // 4. 결과 디스플레이 바인딩 (툴그룹 실행 결과 레코드 표시)
                // ====================================================================================================
                // fixtureTool.CreateLastRunRecord().SubRecords[0]; => SubRecords[0]가 나와야 한다.
                // fixtureTool = (CogFixtureTool)locate.Tools["CogFixtureTool1"];
                // ICogRecord resultRecord = fixtureTool.CreateLastRunRecord().SubRecords[0];
                // ====================================================================================================
                // 0 - LastRun.Image Source.OutputImage,
                // 1 - LastRun.Locate.CogPMAlignTool1.InputImage,
                // 2 - Locate.CogPMAlignTool1.OutputImage
                // 3 - LastRun.Locate.CogFixtureTool1.outputImage <= 이것으로 함
                resultRecords = myToolGroup.CreateLastRunRecord().SubRecords;

                ICogRecord resultRecord = resultRecords[3];
                cogRecordDisplay2.Record = resultRecord;


                // 5. 화면 크기에 맞게 이미지 자동 조절 (Fit)
                cogRecordDisplay2.Fit();

                // ICogRecord resultRecord = fixtureTool.CreateLastRunRecord(); -> 아무 이미지도 디스플레이 되지 않는다.
                // cogRecordDisplay1.Image = fixtureTool.OutputImage; -> 그래픽이 없는 출력이미지 


                resultRecords.Clear();
            }
            else
            {
                try
                {
                    // --- [2] 라이브 카메라 모드 ---
                    // 1. 툴블록 터미널에 모드값(0) 설정
                    tbImageSource.Inputs["IsFileMode"].Value = 0; // 0 = 라이브 모드
                    tbImageSource.Inputs["FileInputImage"].Value = null;
                    


                    // 2. 카메라 취득부터 검사까지 전체 잡 매니저 실행
                    // Tool Group을 실행하는 것이 맞다. 
                    
                    jobManager.Run();
                    
                    //myToolGroup.Run();

                    int mode = (int)tbImageSource.Inputs["IsFileMode"].Value;

                    // Input Image
                    // ICogImage cogImage = (ICogImage)tbImageSource.Outputs["OutputImage"].Value;
                    ICogImage cogImage = (ICogImage)tbImageSource.Inputs["LiveInputImage"].Value;   // null이다. 왜?
                    cogRecordDisplay1.Image = cogImage;
                    // 화면 크기에 맞게 이미지 자동 조절 (Fit)
                    cogRecordDisplay1.Fit();


                    // 결과 디스플레이 바인딩 (툴그룹 실행 결과 레코드 표시)
                    // 0 - LastRun.Image Source.OutputImage,
                    // 1 - LastRun.Locate.CogPMAlignTool1.InputImage,
                    // 2 - Locate.CogPMAlignTool1.OutputImage
                    // 3 - LastRun.Locate.CogFixtureTool1.outputImage <= 이것으로 함
                    ICogRecord resultRecord = myToolGroup.CreateLastRunRecord().SubRecords[3];
                    cogRecordDisplay2.Record = resultRecord;
                    // 5. 화면 크기에 맞게 이미지 자동 조절 (Fit)
                    cogRecordDisplay2.Fit();


                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Job Manager 실행 오류 : {ex.Message}");
                }                
            }

            // Reults
            getResults();
        }




        private void liveMode()
        {

        }


               // 측정 결과를 읽어옴
        private void getResults()
        {
            double leftDist1 = 0.0;
            double leftDist2 = 0.0;
            double rightDist1 = 0.0;
            double rightDist2 = 0.0;

            leftDist1 = (double)leftSide.Outputs[0].Value;
            leftDist2 = (double)leftSide.Outputs[1].Value;

            rightDist1 = (double)rightSide.Outputs[0].Value;
            rightDist2 = (double)rightSide.Outputs[1].Value;

            textBox1.Text = leftDist1.ToString("F2");
            textBox2.Text = leftDist2.ToString("F2");
            textBox3.Text = rightDist1.ToString("F2");
            textBox4.Text = rightDist2.ToString("F2");
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
    }
}
