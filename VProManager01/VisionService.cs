using Cognex.VisionPro;
using Cognex.VisionPro.ImageFile;
using Cognex.VisionPro.ImageProcessing;
using Cognex.VisionPro.PMAlign;
using Cognex.VisionPro.QuickBuild;
using Cognex.VisionPro.ResultsAnalysis;     // 261029@Claude [구조 변경 2] CogResultsAnalysisTool
using Cognex.VisionPro.ToolBlock;
using Cognex.VisionPro.ToolGroup;
using System;
using System.Collections.Generic;

// ===============================================================================
//  261028@Claude [4단계 6] VisionPro 처리(로드·실행·결과 추출)를 UI(Form1)에서 분리
//  Form1은 화면 표시와 사용자 입력만 담당하고, VisionPro 객체는 이 클래스만 다룬다.
//
//  (기존) VPP 구조 ("1023_QB_CBoxNoSwitch.vpp" 기준)
//    Job(0).VisionTool = CogToolGroup
//      [0] CogInputImageTool
//      [1] CogAcqFifoTool1          (카메라 모드에서만 필요)
//      [2] CogToolGroup1            (검사 그룹)
//            CogIPOneImageTool1     (입력 이미지를 직접 주입하는 툴)
//            Locate                 (CogPMAlignTool1, CogFixtureTool1)
//            LEFTside, RIGHTside    (측정 ToolBlock, 출력 0/1 = Dist1/Dist2)
//
//  261029@Claude [트리 구조 적용] 하위 그룹(CogToolGroup1) 없이 모든 툴이 한 단계에 있는 구조
//    Job(0).VisionTool = CogToolGroup  ("터미널 추가..." 그룹)
//      Image Source                       Job 이미지 소스 (이 프로그램에서는 사용하지 않음)
//      CogAcqFifoTool1      → OutputImage                       카메라 모드에서만 필요
//      CogIPOneImageTool1   InputImage  ← CogAcqFifoTool1.OutputImage
//                           → OutputImage
//      Locate (ToolBlock)   in  OutputImage ← CogIPOneImageTool1.OutputImage
//                           out OutputImage (Fixture 좌표계가 적용된 이미지)
//      LEFTside (ToolBlock) in  OutputImage ← Locate.OutputImage
//                           out leftDist01, leftDist02
//      RIGHTside (ToolBlock) in OutputImage ← Locate.OutputImage
//                           out rightDist01, rightDist02
//      (261029@Claude [구조 변경 2] CogToolBlock1 → CogResultsAnalysisTool1 로 교체)
//      CogResultsAnalysisTool1  입력 이름이 측정 출력 이름과 다르다. (VPP의 이름 그대로)
//                           LeftD1  ← LEFTside.leftDist01     LeftD2  ← LEFTside.leftDist02
//                           RightD1 ← RIGHTside.rightDist01   RightD2 ← RIGHTside.rightDist02
//                           (261029@Claude VPP의 RighrD2 → RightD2 수정 반영. 입력 이름을 바꾸면 AnalysisInputMap도 고칠 것)
//                           결과: Result.Decision (Accept/Reject)
//                           261029@Claude [구조 변경 3] VPP에 판정식을 두지 않고 프로그램의 기준값(Settings 창)으로 판정한다.
//                           Decision은 기본적으로 최종 OK/NG에 쓰지 않고 기록만 한다. (UseResultsAnalysisDecision=false)
//
//  실행 방식
//    VPP 안의 링크(화살표)는 ToolGroup 전체를 Run 할 때만 전달된다.
//    이 프로그램은 ToolGroup 전체를 돌리지 않고 아래 순서로 툴을 하나씩 실행하면서
//    VPP의 링크와 같은 방향으로 값을 코드에서 직접 넘겨준다. (RunPipeline 참고)
//      ① 이미지 주입 → ② CogIPOneImageTool1 → ③ Locate → ④ LEFTside, RIGHTside → ⑤ CogResultsAnalysisTool1
//    이렇게 하면
//    - 파일 모드와 카메라 모드가 같은 실행 경로를 쓴다. (카메라 모드는 ① 앞에 취득만 추가)
//    - 파일 모드에서 CogAcqFifoTool1이 실행되지 않으므로 카메라 타임아웃이 생기지 않는다.
//    - Locate가 실패하면 측정 툴을 돌리지 않는다. (검토 3-8)
// ===============================================================================
namespace VProManager01
{
    internal class VisionService
    {
        // 툴 이름 (VPP에서 이름을 바꾸면 여기만 고치면 된다)
        private const string AcqFifoToolName = "CogAcqFifoTool1";
        // 261029@Claude [트리 구조 적용] 하위 검사 그룹(CogToolGroup1)이 없어져 사용하지 않음
        //private const string InspectionGroupName = "CogToolGroup1";
        private const string IPOneImageToolName = "CogIPOneImageTool1";
        private const string LocateName = "Locate";
        private const string LeftSideName = "LEFTside";
        private const string RightSideName = "RIGHTside";
        private const string PMAlignToolName = "CogPMAlignTool1";
        // 261029@Claude [트리 구조 적용] 상위 그룹 레코드 대신 Locate 레코드에서 Fixture 출력 이미지를 찾는다.
        //private const string ResultRecordKey = "CogToolGroup1.Locate.CogFixtureTool1.OutputImage";
        private const string ResultRecordKey = "CogFixtureTool1.OutputImage";

        // 261029@Claude [구조 변경 2] 결과 분석 툴 이름 (LEFTside/RIGHTside 측정값 4개를 입력으로 받아 Accept/Reject 판정)
        // 없으면 ⑤ 단계를 건너뛴다. (필수 툴 아님)
        private const string ResultsAnalysisToolName = "CogResultsAnalysisTool1";

        /// <summary>
        /// 261029@Claude [구조 변경 2] 결과 분석 툴 입력 ← 측정 ToolBlock 출력 대응표 (VPP 링크와 같음)
        /// 입력 이름과 출력 이름이 달라 이름이 같은 것끼리 자동으로 맞출 수 없으므로 표로 정한다.
        /// 측정 항목을 늘리면 VPP 링크와 함께 이 표에 한 줄 추가한다.
        /// </summary>
        private static readonly AnalysisLink[] AnalysisInputMap =
        {
            //               분석 입력   측정 툴      측정 출력
            new AnalysisLink("LeftD1",  true,  "leftDist01"),
            new AnalysisLink("LeftD2",  true,  "leftDist02"),
            new AnalysisLink("RightD1", false, "rightDist01"),
            new AnalysisLink("RightD2", false, "rightDist02"),   // 261029@Claude VPP 수정 반영 (기존: "RighrD2")
        };

        /// <summary>결과 분석 입력 하나와 그 값을 가져올 측정 출력</summary>
        private class AnalysisLink
        {
            public readonly string InputName;       // CogResultsAnalysisTool1 입력 이름
            public readonly bool IsLeftSide;        // true: LEFTside, false: RIGHTside
            public readonly string OutputName;      // 측정 ToolBlock 출력 터미널 이름

            public AnalysisLink(string inputName, bool isLeftSide, string outputName)
            {
                InputName = inputName;
                IsLeftSide = isLeftSide;
                OutputName = outputName;
            }

            public string SideName
            {
                get { return IsLeftSide ? LeftSideName : RightSideName; }
            }
        }

        // 261029@Claude [트리 구조 적용] ToolBlock 터미널 이름 (트리에 보이는 이름 그대로)
        // Locate는 입력과 출력의 이름이 둘 다 "OutputImage"이다. (Inputs/Outputs가 따로라 충돌하지 않음)
        private const string LocateInputImageName = "OutputImage";     // Locate 입력  ← CogIPOneImageTool1.OutputImage
        private const string LocateOutputImageName = "OutputImage";    // Locate 출력  → LEFTside/RIGHTside 입력
        private const string SideInputImageName = "OutputImage";       // LEFTside/RIGHTside 입력 ← Locate.OutputImage

        public CogJobManager JobManager { get; private set; }
        public CogToolGroup ToolGroup { get; private set; }             // 상위 그룹 (기존 myToolGroup)
        // 261029@Claude [트리 구조 적용] 하위 검사 그룹이 없어져 사용하지 않음
        //public CogToolGroup InspectionGroup { get; private set; }       // 검사 그룹 (기존 myToolGroupGroup)
        public CogAcqFifoTool AcqFifoTool { get; private set; }
        public CogIPOneImageTool IPOneImageTool { get; private set; }
        public CogToolBlock Locate { get; private set; }
        public CogToolBlock LeftSide { get; private set; }
        public CogToolBlock RightSide { get; private set; }

        /// <summary>261029@Claude [구조 변경 2] 결과 분석 툴 (CogResultsAnalysisTool1). VPP에 없으면 null</summary>
        public CogResultsAnalysisTool ResultsAnalysisTool { get; private set; }

        /// <summary>로드에 성공한 VPP 경로 (없으면 빈 문자열)</summary>
        public string LoadedVppPath { get; private set; } = "";

        // 261029@Claude [트리 구조 적용] 이번 검사에서 실제로 실행한 단계
        // 툴의 RunStatus·Outputs는 실행하지 않으면 이전 검사 값이 그대로 남아 있다.
        // 그래서 이번 검사에서 실행했는지를 따로 기록하고, 실행하지 않은 툴의 결과는 읽지 않는다.
        private bool isLocateRun;
        private bool isMeasureRun;
        private bool isAnalysisRun;     // 261029@Claude [구조 변경 2] ⑤ 결과 분석 실행 여부

        /// <summary>카메라 모드 사용 가능 여부</summary>
        public bool HasCamera
        {
            get { return AcqFifoTool != null; }
        }

        /// <summary>마지막 카메라 취득 이미지 또는 주입한 파일 이미지가 처리된 결과</summary>
        public ICogImage ProcessedImage
        {
            get { return IPOneImageTool == null ? null : IPOneImageTool.OutputImage; }
        }


        public class LoadResult
        {
            public bool IsSuccess;
            public string Message = "";
        }


        public class LocateResult
        {
            public bool IsOk;
            public string Text = "";     // 화면 표시 문자열 (예: "0.85 - PASS", "NG - No Result")
            public double? Score;
        }


        /// <summary>
        /// 261029@Claude [구조 변경 2] 결과 분석 툴 판정
        /// </summary>
        public class AnalysisResult
        {
            /// <summary>true: Accept, false: Reject·실행 안 됨·오류, null: VPP에 결과 분석 툴 없음(판정에 쓰지 않음)</summary>
            public bool? IsOk;
            public string Text = "";     // 화면·CSV 표시 문자열 (예: "Accept", "Reject", "NotRun", "-")
        }


        /// <summary>
        /// VPP 로드 (2단계 2-2, 2-3 동작 유지)
        /// - 새 VPP를 지역 변수에 먼저 읽고, 필요한 툴을 모두 이름으로 확인한 뒤에만 반영한다.
        /// - 필수 툴이 하나라도 없으면 새로 읽은 것을 정리하고 기존 상태를 그대로 유지한다.
        /// - 교체에 성공하면 이전 Job Manager를 Shutdown 한다.
        /// - 파일 읽기 중 예외는 호출부로 전달한다. (새로 읽은 Job Manager는 정리한 뒤)
        ///
        /// 261029@Claude [트리 구조 적용]
        /// - 모든 툴을 상위 ToolGroup 바로 아래에서 이름으로 찾는다. (하위 그룹 CogToolGroup1 없음)
        /// - 코드에서 값을 넘겨줄 ToolBlock 터미널(입력·출력 이미지)도 로드할 때 함께 확인한다.
        ///   (검사 중에 "터미널 없음" 예외가 나는 것보다 로드 단계에서 거부하는 것이 원인 파악이 쉽다)
        /// - 이전 구조(CogToolGroup1 하위 그룹)의 VPP는 필수 툴을 찾지 못해 로드가 거부된다.
        /// </summary>
        public LoadResult LoadJob(string vppPath)
        {
            CogJobManager newJobManager = null;
            bool isApplied = false;

            try
            {
                // "QuickBuilder응용 프로그램" 으로 저장 된 Job으로 저장한 것을 읽어온다.
                object loadedobject = CogSerializer.LoadObjectFromFile(vppPath);

                CogToolGroup newToolGroup;
                if (loadedobject is CogJobManager manager)
                {
                    // Case A: QuickBuild에서 저장한 Job Manager (일반적인 경우). 첫 번째 Job만 사용한다.
                    newJobManager = manager;
                    newToolGroup = manager.JobCount > 0 ? manager.Job(0).VisionTool as CogToolGroup : null;
                }
                else if (loadedobject is CogJob job)
                {
                    // Case B: 파일이 단일 Job 구조인 경우
                    newToolGroup = job.VisionTool as CogToolGroup;
                }
                else if (loadedobject is CogToolGroup toolGroup)
                {
                    // Case C: 파일이 툴그룹(ToolGroup) 자체로 저장된 경우
                    newToolGroup = toolGroup;
                }
                else
                {
                    return new LoadResult { Message = $"지원하지 않는 VPP 파일 형식입니다. 타입: {loadedobject?.GetType().Name}" };
                }

                // 필요한 툴을 모두 확인 (없는 툴은 목록으로 모아서 한 번에 알린다)
                List<string> missingTools = new List<string>();

                // 261029@Claude [트리 구조 적용] 하위 그룹을 거치지 않고 상위 ToolGroup에서 바로 찾는다. (기존 코드)
                //CogToolGroup newInspectionGroup = null;
                //CogAcqFifoTool newAcqFifoTool = null;
                //CogIPOneImageTool newIPOneImageTool = null;
                //CogToolBlock newLocate = null;
                //CogToolBlock newLeftSide = null;
                //CogToolBlock newRightSide = null;
                //
                //if (newToolGroup == null)
                //{
                //    missingTools.Add("Job의 ToolGroup");
                //}
                //else
                //{
                //    newAcqFifoTool = FindTool(newToolGroup, AcqFifoToolName) as CogAcqFifoTool;
                //
                //    newInspectionGroup = FindTool(newToolGroup, InspectionGroupName) as CogToolGroup;
                //    if (newInspectionGroup == null && newToolGroup.Tools.Count > 2)
                //    {
                //        newInspectionGroup = newToolGroup.Tools[2] as CogToolGroup;   // 기존 방식
                //    }
                //}
                //
                //if (newToolGroup != null && newInspectionGroup == null)
                //{
                //    missingTools.Add(InspectionGroupName + " (하위 ToolGroup)");
                //}
                //else if (newInspectionGroup != null)
                //{
                //    newIPOneImageTool = FindTool(newInspectionGroup, IPOneImageToolName) as CogIPOneImageTool;
                //    newLocate = FindTool(newInspectionGroup, LocateName) as CogToolBlock;
                //    newLeftSide = FindTool(newInspectionGroup, LeftSideName) as CogToolBlock;
                //    newRightSide = FindTool(newInspectionGroup, RightSideName) as CogToolBlock;
                //
                //    if (newIPOneImageTool == null) missingTools.Add(IPOneImageToolName);
                //    if (newLocate == null) missingTools.Add(LocateName);
                //    if (newLeftSide == null) missingTools.Add(LeftSideName);
                //    if (newRightSide == null) missingTools.Add(RightSideName);
                //}

                CogAcqFifoTool newAcqFifoTool = null;
                CogIPOneImageTool newIPOneImageTool = null;
                CogToolBlock newLocate = null;
                CogToolBlock newLeftSide = null;
                CogToolBlock newRightSide = null;
                CogResultsAnalysisTool newResultsAnalysisTool = null;

                if (newToolGroup == null)
                {
                    missingTools.Add("Job의 ToolGroup");
                }
                else
                {
                    // 선택 툴: 없으면 카메라 모드만 막히고(HasCamera=false) 파일 모드는 동작한다.
                    newAcqFifoTool = FindTool(newToolGroup, AcqFifoToolName) as CogAcqFifoTool;
                    // 선택 툴: 없으면 ⑤ 단계를 건너뛴다.
                    newResultsAnalysisTool = FindTool(newToolGroup, ResultsAnalysisToolName) as CogResultsAnalysisTool;

                    // 필수 툴: 하나라도 없으면 로드를 거부한다.
                    newIPOneImageTool = FindTool(newToolGroup, IPOneImageToolName) as CogIPOneImageTool;
                    newLocate = FindTool(newToolGroup, LocateName) as CogToolBlock;
                    newLeftSide = FindTool(newToolGroup, LeftSideName) as CogToolBlock;
                    newRightSide = FindTool(newToolGroup, RightSideName) as CogToolBlock;

                    if (newIPOneImageTool == null) missingTools.Add(IPOneImageToolName);
                    if (newLocate == null) missingTools.Add(LocateName);
                    if (newLeftSide == null) missingTools.Add(LeftSideName);
                    if (newRightSide == null) missingTools.Add(RightSideName);

                    // 코드에서 값을 넘겨줄 터미널 확인 (툴이 있을 때만, 없는 툴은 위에서 이미 목록에 들어감)
                    if (newLocate != null)
                    {
                        if (FindTerminal(newLocate.Inputs, LocateInputImageName) == null)
                            missingTools.Add(LocateName + " 입력 터미널 '" + LocateInputImageName + "'");
                        if (FindTerminal(newLocate.Outputs, LocateOutputImageName) == null)
                            missingTools.Add(LocateName + " 출력 터미널 '" + LocateOutputImageName + "'");
                    }
                    if (newLeftSide != null && FindTerminal(newLeftSide.Inputs, SideInputImageName) == null)
                    {
                        missingTools.Add(LeftSideName + " 입력 터미널 '" + SideInputImageName + "'");
                    }
                    if (newRightSide != null && FindTerminal(newRightSide.Inputs, SideInputImageName) == null)
                    {
                        missingTools.Add(RightSideName + " 입력 터미널 '" + SideInputImageName + "'");
                    }

                    // 261029@Claude [구조 변경 2] 결과 분석 툴이 있으면 대응표의 입력·출력이 모두 있어야 한다.
                    // 하나라도 빠지면 그 입력에 이전 값(또는 기본값)이 남아 판정이 틀어지므로 로드를 거부한다.
                    if (newResultsAnalysisTool != null)
                    {
                        foreach (AnalysisLink link in AnalysisInputMap)
                        {
                            if (!(GetAnalysisInput(newResultsAnalysisTool, link.InputName) is CogResultsAnalysisInputExpression))
                            {
                                missingTools.Add(ResultsAnalysisToolName + " 입력 '" + link.InputName + "'");
                            }
                            CogToolBlock sourceBlock = link.IsLeftSide ? newLeftSide : newRightSide;
                            if (sourceBlock != null && FindTerminal(sourceBlock.Outputs, link.OutputName) == null)
                            {
                                missingTools.Add(link.SideName + " 출력 터미널 '" + link.OutputName + "'");
                            }
                        }
                    }
                }

                if (missingTools.Count > 0)
                {
                    AppLogger.Info("VPP 로드 거부: " + vppPath + " / 없는 툴: " + string.Join(", ", missingTools));
                    return new LoadResult
                    {
                        Message = "다음 툴을 찾을 수 없어 VPP를 적용하지 않았습니다. (기존 설정 유지)\n- " + string.Join("\n- ", missingTools)
                    };
                }

                if (newResultsAnalysisTool == null)
                {
                    AppLogger.Info("'" + ResultsAnalysisToolName + "' 없음: 결과 분석을 건너뜁니다. (측정 공차로만 판정)");
                }

                // 모든 필수 툴 확인 완료 → 한 번에 교체
                CogJobManager oldJobManager = JobManager;

                JobManager = newJobManager;
                ToolGroup = newToolGroup;
                //InspectionGroup = newInspectionGroup;
                AcqFifoTool = newAcqFifoTool;
                IPOneImageTool = newIPOneImageTool;
                Locate = newLocate;
                LeftSide = newLeftSide;
                RightSide = newRightSide;
                ResultsAnalysisTool = newResultsAnalysisTool;
                LoadedVppPath = vppPath;
                isApplied = true;

                // 새 VPP로 바뀌었으므로 이전 VPP 기준의 실행 기록은 무효
                ResetRunState();

                // 이전 Job Manager 정리
                if (oldJobManager != null && !ReferenceEquals(oldJobManager, newJobManager))
                {
                    ShutdownJobManager(oldJobManager);
                }

                AppLogger.Info("VPP 로드 완료: " + vppPath);
                return new LoadResult { IsSuccess = true };
            }
            finally
            {
                // 적용하지 않은 새 Job Manager는 정리한다.
                if (!isApplied)
                {
                    ShutdownJobManager(newJobManager);
                }
            }
        }


        /// <summary>
        /// 모드별로 검사 실행에 필요한 객체가 준비되었는지 확인 (1단계 1-5)
        /// 준비되었으면 null, 아니면 사용자에게 보여줄 메시지를 반환한다.
        /// </summary>
        public string GetNotReadyMessage(bool isOfflineFileMode)
        {
            // 261029@Claude [트리 구조 적용] 하위 검사 그룹 확인 제거 (기존 코드)
            //if (ToolGroup == null || InspectionGroup == null)
            if (ToolGroup == null)
            {
                return "비전 시스템이 초기화되지 않았습니다. Model Load를 먼저 실행하세요.";
            }
            if (IPOneImageTool == null)
            {
                return "'" + IPOneImageToolName + "' 툴이 없어 검사를 실행할 수 없습니다.";
            }
            if (!isOfflineFileMode && AcqFifoTool == null)
            {
                return "'" + AcqFifoToolName + "' 툴이 없어 카메라 모드를 실행할 수 없습니다.";
            }
            return null;
        }


        /// <summary>
        /// 파일 모드: 이미지를 CogIPOneImageTool1에 직접 주입하고 검사 단계만 실행
        /// (카메라 타임아웃을 원천 차단하기 위해 CogAcqFifoTool1은 실행하지 않는다)
        /// </summary>
        public void RunWithImage(ICogImage image)
        {
            // 261029@Claude [트리 구조 적용] 하위 검사 그룹이 없어 툴을 순서대로 실행 (기존 코드)
            //IPOneImageTool.InputImage = image;
            //InspectionGroup.Run();
            RunPipeline(image);
        }


        /// <summary>
        /// 카메라 모드: 카메라로 한 장 취득한 뒤 파일 모드와 같은 순서로 검사 (1단계 1-1)
        /// 취득에 실패하면 메시지를, 성공하면 null을 반환한다.
        /// </summary>
        public string RunWithCamera(out ICogImage acquiredImage)
        {
            acquiredImage = null;

            // 261029@Claude [트리 구조 적용] 상위 ToolGroup 전체 실행 대신 취득 → RunPipeline (기존 코드)
            // 상위 ToolGroup을 Run 하면 VPP 링크로 값이 넘어가지만, 파일 모드와 실행 경로가 달라지고
            // Locate 실패 시 측정 툴을 건너뛸 수 없다. 그래서 취득만 따로 하고 나머지는 같은 경로를 쓴다.
            //// IPOneImageTool.OutputImage는 InputImage 입력이 없어도 기존 값을 가지고 있으므로 초기화
            //IPOneImageTool.InputImage = null;
            //
            //ToolGroup.Run();

            // 취득 전에 이번 검사 기록을 지운다. (취득에 실패하면 이전 결과를 읽지 않도록)
            ResetRunState();

            AcqFifoTool.Run();      // 동기 실행: 반환되면 OutputImage·RunStatus가 이번 취득 결과

            if (AcqFifoTool.RunStatus.Result != CogToolResultConstants.Accept)
            {
                return "카메라 이미지 취득 실패";
            }
            if (AcqFifoTool.OutputImage == null)
            {
                return "OutputImage가 null입니다. 이미지가 들어오지 않았습니다.";
            }

            acquiredImage = AcqFifoTool.OutputImage;

            // VPP 링크: CogIPOneImageTool1.InputImage ← CogAcqFifoTool1.OutputImage
            RunPipeline(acquiredImage);
            return null;
        }


        /// <summary>
        /// 261029@Claude [트리 구조 적용] 검사 단계를 VPP 트리 순서대로 하나씩 실행한다.
        /// 각 툴을 단독으로 Run 하면 VPP 링크가 동작하지 않으므로, 링크와 같은 방향으로 값을 직접 넘긴다.
        ///
        ///   ① image                      → CogIPOneImageTool1.InputImage
        ///   ② CogIPOneImageTool1.Run()
        ///   ③ CogIPOneImageTool1.OutputImage → Locate.Inputs["OutputImage"],  Locate.Run()
        ///   ④ Locate.Outputs["OutputImage"]  → LEFTside / RIGHTside.Inputs["OutputImage"], 각각 Run()
        ///   ⑤ LEFTside / RIGHTside 출력      → CogResultsAnalysisTool1 입력 (AnalysisInputMap), Run() (툴이 있을 때만)
        ///
        /// - 앞 단계가 실패하면 뒤 단계는 실행하지 않는다. (실행하지 않은 단계는 판정에서 NG 처리)
        /// - 예외는 호출부(Form1.runInspection)로 전달한다.
        /// </summary>
        private void RunPipeline(ICogImage image)
        {
            ResetRunState();

            // ①② 전처리 (IPOneImage). InputImage를 매번 새로 넣으므로 이전 이미지가 남지 않는다.
            IPOneImageTool.InputImage = image;
            IPOneImageTool.Run();
            if (IPOneImageTool.RunStatus.Result == CogToolResultConstants.Error || IPOneImageTool.OutputImage == null)
            {
                // 전처리 실패: Locate를 실행하지 않음 → CheckLocate가 "NG - Not Run" 표시
                AppLogger.Info(IPOneImageToolName + " 실행 실패: " + IPOneImageTool.RunStatus.Message);
                return;
            }

            // ③ 위치 찾기 (Locate 안의 CogPMAlignTool1 → CogFixtureTool1)
            FindTerminal(Locate.Inputs, LocateInputImageName).Value = IPOneImageTool.OutputImage;
            Locate.Run();
            isLocateRun = true;

            // Locate가 Error면 출력 이미지(Fixture 좌표계)가 없거나 이전 값이므로 측정하지 않는다. (검토 3-8)
            // PMAlign 점수 기준(설정값) 판정은 CheckLocate에서 하고, 여기서는 실행 가능 여부만 본다.
            ICogImage locatedImage = FindTerminal(Locate.Outputs, LocateOutputImageName).Value as ICogImage;
            if (Locate.RunStatus.Result == CogToolResultConstants.Error || locatedImage == null)
            {
                AppLogger.Info(LocateName + " 실패로 측정 생략: " + Locate.RunStatus.Message);
                return;
            }

            // ④ 좌/우 측정. 두 ToolBlock 모두 Locate의 출력 이미지를 입력으로 받는다.
            FindTerminal(LeftSide.Inputs, SideInputImageName).Value = locatedImage;
            FindTerminal(RightSide.Inputs, SideInputImageName).Value = locatedImage;
            LeftSide.Run();
            RightSide.Run();
            isMeasureRun = true;

            // ⑤ 결과 분석 (선택). 판정 결과(Decision)는 CheckAnalysis에서 읽는다.
            // 측정 툴 중 하나라도 Accept가 아니면 출력에 이전 값이 남아 있을 수 있으므로 실행하지 않는다.
            // (이 경우 측정값이 이미 NG(상태)라 최종 판정은 NG)
            if (ResultsAnalysisTool != null
                && LeftSide.RunStatus.Result == CogToolResultConstants.Accept
                && RightSide.RunStatus.Result == CogToolResultConstants.Accept)
            {
                RunResultsAnalysis();
            }
        }


        /// <summary>
        /// 261029@Claude [구조 변경 2] 결과 분석 툴(CogResultsAnalysisTool1) 실행
        /// AnalysisInputMap에 따라 측정 출력값을 분석 입력(InputExpression.Value)에 넣고 Run 한다.
        ///   LeftD1 ← LEFTside.leftDist01,  RightD2 ← RIGHTside.rightDist02 ...
        /// 입력·출력이 있는지는 VPP 로드 때 확인했으므로 여기서는 바로 넣는다.
        /// </summary>
        private void RunResultsAnalysis()
        {
            foreach (AnalysisLink link in AnalysisInputMap)
            {
                CogToolBlock sourceBlock = link.IsLeftSide ? LeftSide : RightSide;
                CogResultsAnalysisInputExpression input = (CogResultsAnalysisInputExpression)GetAnalysisInput(ResultsAnalysisTool, link.InputName);
                input.Value = FindTerminal(sourceBlock.Outputs, link.OutputName).Value;
            }

            ResultsAnalysisTool.Run();
            isAnalysisRun = true;

            if (ResultsAnalysisTool.RunStatus.Result == CogToolResultConstants.Error)
            {
                AppLogger.Info(ResultsAnalysisToolName + " 실행 오류: " + ResultsAnalysisTool.RunStatus.Message);
            }
        }


        /// <summary>
        /// 261029@Claude [구조 변경 2] 결과 분석 판정 읽기
        /// - VPP에 결과 분석 툴이 없으면 IsOk = null (판정에 쓰지 않음)
        /// - 이번 검사에서 실행하지 않았으면(Locate·측정 실패) NotRun → NG
        /// - 실행 오류면 Error → NG, 아니면 Result.Decision (Accept만 OK)
        /// </summary>
        public AnalysisResult CheckAnalysis()
        {
            if (ResultsAnalysisTool == null)
            {
                return new AnalysisResult { IsOk = null, Text = "-" };
            }
            if (!isAnalysisRun)
            {
                return new AnalysisResult { IsOk = false, Text = "NotRun" };
            }
            if (ResultsAnalysisTool.RunStatus.Result == CogToolResultConstants.Error || ResultsAnalysisTool.Result == null)
            {
                return new AnalysisResult { IsOk = false, Text = "Error" };
            }

            CogToolResultConstants decision = ResultsAnalysisTool.Result.Decision;
            return new AnalysisResult
            {
                IsOk = decision == CogToolResultConstants.Accept,
                Text = decision.ToString()
            };
        }


        /// <summary>
        /// 261029@Claude [트리 구조 적용] 이번 검사 실행 기록 초기화 (검사 시작 시 호출)
        /// </summary>
        private void ResetRunState()
        {
            isLocateRun = false;
            isMeasureRun = false;
            isAnalysisRun = false;
        }


        /// <summary>
        /// PMAlign 결과로 위치 확인 (1단계 2-4, 3단계 3-2 동작 유지)
        /// PMAlign이 실패하면 결과가 없다(null/0개). 기준 점수보다 낮은 것과는 다르다.
        /// </summary>
        public LocateResult CheckLocate(double minScore)
        {
            if (Locate == null)
            {
                return new LocateResult { Text = "NG - No Locate" };
            }
            // 261029@Claude [트리 구조 적용] 이번 검사에서 Locate를 실행하지 않았으면(전처리 실패)
            // RunStatus·Results가 이전 검사 값이므로 읽지 않는다.
            if (!isLocateRun)
            {
                return new LocateResult { Text = "NG - Not Run" };
            }
            if (Locate.RunStatus.Result == CogToolResultConstants.Error)
            {
                return new LocateResult { Text = "NG - Locate Error" };
            }

            CogPMAlignTool pmAlignTool = FindTool(Locate, PMAlignToolName) as CogPMAlignTool;
            if (pmAlignTool == null || pmAlignTool.Results == null)
            {
                return new LocateResult { Text = "NG - No PMAlign" };
            }
            if (pmAlignTool.Results.Count == 0)
            {
                return new LocateResult { Text = "NG - No Result" };
            }

            // 0~1 사이의 값으로, 1에 가까울수록 정렬이 잘 된 것으로 판단
            double pmScore = pmAlignTool.Results[0].Score;
            bool isOk = pmScore >= minScore;
            return new LocateResult
            {
                IsOk = isOk,
                Score = pmScore,
                Text = pmScore.ToString("F2") + (isOk ? " - PASS" : " - FAIL")
            };
        }


        /// <summary>
        /// 측정값 하나 판정 (3단계 3-1, 3-4 동작 유지)
        /// - 측정 ToolBlock이 Accept가 아니면 값 대신 NG(상태) (실패한 툴의 Outputs에는 이전 값이 남아 있음)
        /// - 출력은 이름(OutputName) 우선, 없으면 번호(OutputIndex)
        ///   (261029@Claude 새 VPP 출력 이름: leftDist01, leftDist02, rightDist01, rightDist02)
        /// - readValues가 false(위치 확인 실패)면 값을 읽지 않고 "-"
        /// </summary>
        public MeasureResult EvaluateMeasure(bool isLeftSide, MeasureSpec spec, bool readValues)
        {
            CogToolBlock sideBlock = isLeftSide ? LeftSide : RightSide;

            MeasureResult measure = new MeasureResult();
            measure.Name = spec.DisplayName;

            if (!readValues)
            {
                measure.Text = "-";
                return measure;
            }
            if (sideBlock == null)
            {
                measure.Text = "N/A";
                return measure;
            }
            // 261029@Claude [트리 구조 적용] 이번 검사에서 측정 툴을 실행하지 않았으면 이전 값이 남아 있으므로 NG
            if (!isMeasureRun)
            {
                measure.Text = "NG(NotRun)";
                return measure;
            }

            CogToolResultConstants status = sideBlock.RunStatus.Result;
            if (status != CogToolResultConstants.Accept)
            {
                measure.Text = "NG(" + status + ")";
                return measure;
            }

            double? value = ReadOutputValue(sideBlock, spec);
            if (!value.HasValue)
            {
                measure.Text = "N/A";
                return measure;
            }

            measure.Value = value;
            measure.Text = value.Value.ToString("F2");
            measure.IsOk = !spec.UseTolerance || (value.Value >= spec.Min && value.Value <= spec.Max);
            return measure;
        }


        /// <summary>
        /// 결과 화면에 표시할 레코드 (Locate.CogFixtureTool1.OutputImage)
        /// </summary>
        public ICogRecord CreateResultRecord()
        {
            // 261029@Claude [트리 구조 적용] 상위 ToolGroup은 이제 실행하지 않으므로 그 레코드는 비어 있다. (기존 코드)
            //return ToolGroup.CreateLastRunRecord().SubRecords[ResultRecordKey];

            // 1순위: Locate LastRun 레코드 안의 Fixture 출력 이미지 (PMAlign 그래픽 포함)
            try
            {
                ICogRecord fixtureRecord = Locate.CreateLastRunRecord().SubRecords[ResultRecordKey];
                if (fixtureRecord != null)
                {
                    return fixtureRecord;
                }
            }
            catch (Exception ex)
            {
                // Locate 안의 Fixture 툴 이름이 다르면 키가 없어 예외가 난다. → 2순위로 대체
                AppLogger.Info("결과 레코드 '" + LocateName + "." + ResultRecordKey + "' 없음, Locate 출력 이미지로 대체: " + ex.Message);
            }

            // 2순위: Locate 출력 터미널 이미지만 담은 레코드 (그래픽 없음)
            ICogImage locatedImage = FindTerminal(Locate.Outputs, LocateOutputImageName)?.Value as ICogImage;
            return new Cognex.VisionPro.Implementation.CogRecord(LocateName + "." + LocateOutputImageName, typeof(ICogImage),
                                 CogRecordUsageConstants.Result, false, locatedImage, LocateName + "." + LocateOutputImageName);
        }


        /// <summary>
        /// 측정 그래픽을 담고 있는 LEFTside/RIGHTside의 LastRun 레코드 (3단계 3-7)
        /// </summary>
        public List<ICogRecord> CreateMeasureRecords()
        {
            List<ICogRecord> records = new List<ICogRecord>();
            // 261029@Claude [트리 구조 적용] 이번 검사에서 측정하지 않았으면 이전 그래픽을 그리지 않는다.
            if (!isMeasureRun)
            {
                return records;
            }
            if (LeftSide != null)
            {
                records.Add(LeftSide.CreateLastRunRecord());
            }
            if (RightSide != null)
            {
                records.Add(RightSide.CreateLastRunRecord());
            }
            return records;
        }


        /// <summary>
        /// 종료 시 Job Manager와 카메라(FrameGrabber) 해제 (2단계 2-1)
        /// </summary>
        public void Shutdown()
        {
            ShutdownJobManager(JobManager);
            JobManager = null;

            try
            {
                // 프로그램 종료 직전이므로 AllowRecovery = false
                CogFrameGrabbers frameGrabbers = new CogFrameGrabbers();
                foreach (ICogFrameGrabber frameGrabber in frameGrabbers)
                {
                    frameGrabber.Disconnect(false);
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error("FrameGrabber 해제 실패", ex);
            }
        }


        /// <summary>
        /// 이미지 파일에서 첫 번째 프레임을 읽는다. 실패하면 예외를 던진다.
        /// </summary>
        public static ICogImage LoadImageFile(string imagePath)
        {
            // CogImageFileTool을 사용하면 파일 열기/닫기 및 독립된 이미지 객체 관리가 안전합니다.
            using (CogImageFileTool fileTool = new CogImageFileTool())
            {
                fileTool.Operator.Open(imagePath, CogImageFileModeConstants.Read);

                // 파일 내의 첫 번째 이미지 프레임 가져오기
                ICogImage loadedImage = fileTool.Operator[0];

                fileTool.Operator.Close();
                return loadedImage;
            }
        }


        /// <summary>
        /// ToolBlock 출력값 읽기: 이름 우선, 없으면 번호. 숫자가 아니면 null (3단계 3-4)
        /// </summary>
        private static double? ReadOutputValue(CogToolBlock block, MeasureSpec spec)
        {
            object value = null;
            bool isFound = false;

            if (!string.IsNullOrEmpty(spec.OutputName))
            {
                // 261029@Claude [트리 구조 적용] 공통 함수 FindTerminal 사용 (기존 코드)
                //foreach (CogToolBlockTerminal terminal in block.Outputs)
                //{
                //    if (terminal.Name == spec.OutputName)
                //    {
                //        value = terminal.Value;
                //        isFound = true;
                //        break;
                //    }
                //}
                CogToolBlockTerminal terminal = FindTerminal(block.Outputs, spec.OutputName);
                if (terminal != null)
                {
                    value = terminal.Value;
                    isFound = true;
                }
                if (!isFound)
                {
                    AppLogger.Info($"{block.Name} 출력 '{spec.OutputName}'을(를) 찾지 못해 번호 {spec.OutputIndex}를 사용");
                }
            }

            if (!isFound)
            {
                if (spec.OutputIndex < 0 || spec.OutputIndex >= block.Outputs.Count)
                {
                    return null;
                }
                value = block.Outputs[spec.OutputIndex].Value;
            }

            // 261029@Claude [검토 보완] true/false 출력은 측정값이 아니므로 1.00/0.00으로 바꾸지 않는다.
            if (value is bool)
            {
                return null;
            }

            if (value is IConvertible)
            {
                try
                {
                    return Convert.ToDouble(value);
                }
                catch (FormatException) { }
                catch (InvalidCastException) { }
            }
            return null;
        }


        /// <summary>
        /// Job Manager 정리 (null이면 아무것도 하지 않음) (2단계 2-1, 2-2)
        /// </summary>
        private static void ShutdownJobManager(CogJobManager manager)
        {
            if (manager == null)
            {
                return;
            }
            try
            {
                manager.Shutdown();
            }
            catch (Exception ex)
            {
                AppLogger.Error("Job Manager Shutdown 실패", ex);
            }
        }


        /// <summary>
        /// ToolGroup 안에서 이름으로 툴 찾기 (없으면 null) (2단계 2-3)
        /// 바로 아래 단계만 찾는다. (하위 ToolGroup 안까지 찾지 않음)
        /// </summary>
        private static ICogTool FindTool(CogToolGroup group, string toolName)
        {
            foreach (ICogTool tool in group.Tools)
            {
                if (tool != null && tool.Name == toolName)
                {
                    return tool;
                }
            }
            return null;
        }


        /// <summary>
        /// ToolBlock 안에서 이름으로 툴 찾기 (없으면 null)
        /// </summary>
        private static ICogTool FindTool(CogToolBlock block, string toolName)
        {
            foreach (ICogTool tool in block.Tools)
            {
                if (tool != null && tool.Name == toolName)
                {
                    return tool;
                }
            }
            return null;
        }


        /// <summary>
        /// 261029@Claude [구조 변경 2] 결과 분석 툴의 입력(식)을 이름으로 찾기 (없으면 null)
        /// RunParams는 이름 → 식(Expression) 사전이다. QuickBuild에서 만든 "입력"은
        /// CogResultsAnalysisInputExpression이고, 계산식·결과 항목은 다른 Expression 형식이다.
        /// </summary>
        private static CogResultsAnalysisExpression GetAnalysisInput(CogResultsAnalysisTool tool, string inputName)
        {
            if (tool.RunParams == null || tool.RunParams.IndexOf(inputName) < 0)
            {
                return null;
            }
            return tool.RunParams[inputName];
        }


        /// <summary>
        /// 261029@Claude [트리 구조 적용] ToolBlock 입력·출력 터미널을 이름으로 찾기 (없으면 null)
        /// 문자열 인덱서(Inputs["이름"])는 이름이 없으면 예외가 나므로 직접 찾는다.
        /// 이름은 대소문자까지 VPP와 같아야 한다. (예: "leftDist01" ≠ "LeftDist01")
        /// </summary>
        private static CogToolBlockTerminal FindTerminal(CogToolBlockTerminalCollection terminals, string name)
        {
            foreach (CogToolBlockTerminal terminal in terminals)
            {
                if (terminal.Name == name)
                {
                    return terminal;
                }
            }
            return null;
        }
    }
}
