using Cognex.VisionPro;
using Cognex.VisionPro.CalibFix;
using Cognex.VisionPro.Caliper;
using Cognex.VisionPro.ImageFile;
using Cognex.VisionPro.ImageProcessing;
using Cognex.VisionPro.PMAlign;
using Cognex.VisionPro.QuickBuild;
using Cognex.VisionPro.QuickBuild.Implementation.Internal;
using Cognex.VisionPro.ToolGroup;
// using Cognex.VisionPro.Controls;
using Cognex.VisionPro.ToolBlock;


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


// =====================================================
// 261019@jdin 
// =====================================================

namespace VproBlack
{
    public partial class Form1 : Form
    {
        private CogToolGroup toolGroup;
        private string jobFilePath;

        CogToolBlock mToolBlock;

        // 261019@jdin 폼 클래스 내부에 선언
        private CogRecordDisplay cogRecordDisplay1;


        public Form1()
        {
            InitializeComponent();


            // 코드로 CogRecordDisplay 생성 및 설정
            cogRecordDisplay1 = new CogRecordDisplay();
            cogRecordDisplay1.Location = new System.Drawing.Point(250, 155); // (가로, 세로)
            cogRecordDisplay1.Size = new System.Drawing.Size(1500, 1000);

            // 폼에 컨트롤 추가
            this.Controls.Add(cogRecordDisplay1);

        }

        private void btnLoadJob_Click(object sender, EventArgs e)
        {
            // Open a file dialog to select the VPP file
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    // QuickBuilder에서 작성한 CogToolGroup Job파일 선택합니다.
                    openFileDialog.Filter = "VisionPro ToolGroup Job Files (*.vpp)|*.vpp";
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
                MessageBox.Show("Error loading ToolGroup job file: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Error loading ToolGroup job file: ";
            }
        }


        private void LoadVisionProJob(string FilePath)
        {
            try
            {
                object loadObject = CogSerializer.LoadObjectFromFile(FilePath);

                if (loadObject is CogToolGroup)
                {
                    toolGroup = (CogToolGroup)loadObject;
                    lblStatus.Text = "CogToolGroup Job 파일이 정상 로드됨";
                }
                else
                {
                    throw new Exception("로드된 객체가 CogToolGroup Job 파일 아닙니다.");
                }

                // Job 로드 후 초기화 작업
                PrepareJobForExecution();

            }
            catch (Exception ex)
            {
                MessageBox.Show("CogToolGroup Job 파일 로드 에러 : " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Error : CogToolGroup Job 파일 로드 에러";
            }
        }

        private void PrepareJobForExecution()
        {            
            // 261019@jdin 수정
            // 예시: ToolGroup에서 툴을 가져와 안전하게 캐스팅
            // ICogTool tool = toolGroup.Tools["ToolName"];
            ICogTool tool = toolGroup.Tools["CogIPOneImageTool1"];

            // 261020@jdin ToolGroup안에 ToolBlock이 있다.
            // ToolBlock은 1.Locate, 2.LEFT, 3.RIGHT가 있다.
            mToolBlock = (CogToolBlock)toolGroup.Tools["Locate"];


            // 강제 캐스팅 대신 'as' 사용
            CogIPOneImageTool ipOneTool = tool as CogIPOneImageTool;
            if (ipOneTool != null)
            {
                // CogIPOneImageTool일 때의 처리 로직
            }
            else
            {
                // 타입이 다를 경우 예외 처리를 하거나 로깅
                MessageBox.Show("툴 타입이 일치하지 않습니다.");
            }
                        

            // 1. 툴 그룹 실행 또는 개별 툴 실행 (예: 툴 그룹 전체를 실행하는 경우)
            toolGroup.Run();

            // 2. Fixture 툴에서 결과 객체 가져오기 (필요한 경우)
            // CogFixtureTool fixtureTool = (CogFixtureTool)toolGroup.Tools["CogFixtureTool1"];

            // 3. 툴의 가장 최근 실행 결과 기록(Record) 생성
            // CreateLastResultRecord()는 이미지뿐만 아니라 Fixture 좌표계 아이콘 등 모든 그래픽 요소를 포함합니다.
            // fixtureTool.CreateLastRunRecord().SubRecords[0]; => SubRecords[0]가 나와야 한다.
            // ICogRecord resultRecord = fixtureTool.CreateLastRunRecord().SubRecords[0];


            ICogRecord resultRecord = ipOneTool.CreateLastRunRecord().SubRecords[0]; 
            

            // 4. CogRecordDisplay에 Record 연결
            cogRecordDisplay1.Record = resultRecord;

            // 5. 화면 크기에 맞게 이미지 자동 조절 (Fit)
            cogRecordDisplay1.Fit();


        }

        private void btnRunJob_Click(object sender, EventArgs e)
        {
            try
            {
                // jdinVproManager 참조할 것
                lblStatus.Text = "Job 실행 중";
                toolGroup.Run();

                // 2. Fixture 툴에서 결과 객체 가져오기 (필요한 경우)
                CogFixtureTool fixtureTool = (CogFixtureTool)toolGroup.Tools["CogFixtureTool1"];

                // 3. 툴의 가장 최근 실행 결과 기록(Record) 생성
                // CreateLastResultRecord()는 이미지뿐만 아니라 Fixture 좌표계 아이콘 등 모든 그래픽 요소를 포함합니다.
                // fixtureTool.CreateLastRunRecord().SubRecords[0]; => SubRecords[0]가 나와야 한다.
                ICogRecord resultRecord = fixtureTool.CreateLastRunRecord().SubRecords[0];

                // 4. CogRecordDisplay에 Record 연결
                cogRecordDisplay1.Record = resultRecord;

                // 5. 화면 크기에 맞게 이미지 자동 조절 (Fit)
                cogRecordDisplay1.Fit();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error running job: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "QuickBuilder 응용Job 파일 실행 실패";
            }
        }



        //
        // 261019@jdin 추가
        //
        private void btnSetInput_Click(object sender, EventArgs e)
        {
            // 1. 파일 열기 다이얼로그 생성
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                // 다이얼로그 초기 설정
                openFileDialog.Title = "비전 검사할 이미지 선택";
                openFileDialog.Filter = "이미지 파일 (*.bmp;*.jpg;*.png)|*.bmp;*.jpg;*.png|모든 파일 (*.*)|*.*";
                openFileDialog.InitialDirectory = @"D:\0_JDIN\JDIN_VisionProgram\jdinVPro\VPro\VproBlack"; // 기본 열릴 폴더 지정 (선택사항)


                // 261019@jdin 수정
                // 예시: ToolGroup에서 툴을 가져와 안전하게 캐스팅
                // ICogTool tool = toolGroup.Tools["ToolName"];

                ICogTool tool = toolGroup.Tools["CogIPOneImageTool1"];
                // 강제 캐스팅 대신 'as' 사용
                CogIPOneImageTool ipOneTool = tool as CogIPOneImageTool;


                // 2. 사용자가 파일을 선택하고 [열기]를 눌렀을 때
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string selectedImagePath = openFileDialog.FileName;

                        // 3. CogImageFile을 이용해 선택된 이미지 파일 열기
                        using (CogImageFile imageFile = new CogImageFile())
                        {
                            imageFile.Open(selectedImagePath, CogImageFileModeConstants.Read);

                            // 첫 번째 프레임의 이미지를 ICogImage 객체로 가져오기
                            ICogImage cogImage = imageFile[0];

                            // 4. ipOneTool의 InputImage에 할당
                            ipOneTool.InputImage = cogImage;

                            // 2019@ㅓ야ㅜ
                            cogRecordDisplay1.Image = ipOneTool.InputImage;


                            // 261019@jdin 추가
                            pBox.Image = ipOneTool.InputImage.ToBitmap();

                            imageFile.Close();
                        }

                        // 5. 툴 실행
                        ipOneTool.Run();
                        MessageBox.Show($"이미지 로드 및 실행 완료: {System.IO.Path.GetFileName(selectedImagePath)}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"이미지 처리 중 에러 발생: {ex.Message}");
                    }
                }
            }
        }
    }
}
