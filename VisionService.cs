using Cognex.VisionPro;
using Cognex.VisionPro.ImageFile;
using Cognex.VisionPro.ImageProcessing;
using Cognex.VisionPro.PMAlign;
using Cognex.VisionPro.QuickBuild;
using Cognex.VisionPro.ToolBlock;
using Cognex.VisionPro.ToolGroup;
using System;
using System.Collections.Generic;

// ===============================================================================
//  261028@Claude [4단계 6] VisionPro 처리(로드·실행·결과 추출)를 UI(Form1)에서 분리
//  Form1은 화면 표시와 사용자 입력만 담당하고, VisionPro 객체는 이 클래스만 다룬다.
//
//  VPP 구조 ("1023_QB_CBoxNoSwitch.vpp" 기준)
//    Job(0).VisionTool = CogToolGroup
//      [0] CogInputImageTool
//      [1] CogAcqFifoTool1          (카메라 모드에서만 필요)
//      [2] CogToolGroup1            (검사 그룹)
//            CogIPOneImageTool1     (입력 이미지를 직접 주입하는 툴)
//            Locate                 (CogPMAlignTool1, CogFixtureTool1)
//            LEFTside, RIGHTside    (측정 ToolBlock, 출력 0/1 = Dist1/Dist2)
// ===============================================================================
namespace VproManageBlack02
{
    internal class VisionService
    {
        // 툴 이름 (VPP에서 이름을 바꾸면 여기만 고치면 된다)
        private const string AcqFifoToolName = "CogAcqFifoTool1";
        private const string InspectionGroupName = "CogToolGroup1";
        private const string IPOneImageToolName = "CogIPOneImageTool1";
        private const string LocateName = "Locate";
        private const string LeftSideName = "LEFTside";
        private const string RightSideName = "RIGHTside";
        private const string PMAlignToolName = "CogPMAlignTool1";
        private const string ResultRecordKey = "CogToolGroup1.Locate.CogFixtureTool1.OutputImage";

        public CogJobManager JobManager { get; private set; }
        public CogToolGroup ToolGroup { get; private set; }             // 상위 그룹 (기존 myToolGroup)
        public CogToolGroup InspectionGroup { get; private set; }       // 검사 그룹 (기존 myToolGroupGroup)
        public CogAcqFifoTool AcqFifoTool { get; private set; }
        public CogIPOneImageTool IPOneImageTool { get; private set; }
        public CogToolBlock Locate { get; private set; }
        public CogToolBlock LeftSide { get; private set; }
        public CogToolBlock RightSide { get; private set; }

        /// <summary>로드에 성공한 VPP 경로 (없으면 빈 문자열)</summary>
        public string LoadedVppPath { get; private set; } = "";

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
        /// VPP 로드 (2단계 2-2, 2-3 동작 유지)
        /// - 새 VPP를 지역 변수에 먼저 읽고, 필요한 툴을 모두 이름으로 확인한 뒤에만 반영한다.
        /// - 필수 툴이 하나라도 없으면 새로 읽은 것을 정리하고 기존 상태를 그대로 유지한다.
        /// - 교체에 성공하면 이전 Job Manager를 Shutdown 한다.
        /// - 파일 읽기 중 예외는 호출부로 전달한다. (새로 읽은 Job Manager는 정리한 뒤)
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

                CogToolGroup newInspectionGroup = null;
                CogAcqFifoTool newAcqFifoTool = null;
                CogIPOneImageTool newIPOneImageTool = null;
                CogToolBlock newLocate = null;
                CogToolBlock newLeftSide = null;
                CogToolBlock newRightSide = null;

                if (newToolGroup == null)
                {
                    missingTools.Add("Job의 ToolGroup");
                }
                else
                {
                    newAcqFifoTool = FindTool(newToolGroup, AcqFifoToolName) as CogAcqFifoTool;

                    newInspectionGroup = FindTool(newToolGroup, InspectionGroupName) as CogToolGroup;
                    if (newInspectionGroup == null && newToolGroup.Tools.Count > 2)
                    {
                        newInspectionGroup = newToolGroup.Tools[2] as CogToolGroup;   // 기존 방식
                    }
                }

                if (newToolGroup != null && newInspectionGroup == null)
                {
                    missingTools.Add(InspectionGroupName + " (하위 ToolGroup)");
                }
                else if (newInspectionGroup != null)
                {
                    newIPOneImageTool = FindTool(newInspectionGroup, IPOneImageToolName) as CogIPOneImageTool;
                    newLocate = FindTool(newInspectionGroup, LocateName) as CogToolBlock;
                    newLeftSide = FindTool(newInspectionGroup, LeftSideName) as CogToolBlock;
                    newRightSide = FindTool(newInspectionGroup, RightSideName) as CogToolBlock;

                    if (newIPOneImageTool == null) missingTools.Add(IPOneImageToolName);
                    if (newLocate == null) missingTools.Add(LocateName);
                    if (newLeftSide == null) missingTools.Add(LeftSideName);
                    if (newRightSide == null) missingTools.Add(RightSideName);
                }

                if (missingTools.Count > 0)
                {
                    AppLogger.Info("VPP 로드 거부: " + vppPath + " / 없는 툴: " + string.Join(", ", missingTools));
                    return new LoadResult
                    {
                        Message = "다음 툴을 찾을 수 없어 VPP를 적용하지 않았습니다. (기존 설정 유지)\n- " + string.Join("\n- ", missingTools)
                    };
                }

                // 모든 필수 툴 확인 완료 → 한 번에 교체
                CogJobManager oldJobManager = JobManager;

                JobManager = newJobManager;
                ToolGroup = newToolGroup;
                InspectionGroup = newInspectionGroup;
                AcqFifoTool = newAcqFifoTool;
                IPOneImageTool = newIPOneImageTool;
                Locate = newLocate;
                LeftSide = newLeftSide;
                RightSide = newRightSide;
                LoadedVppPath = vppPath;
                isApplied = true;

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
            if (ToolGroup == null || InspectionGroup == null)
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
        /// 파일 모드: 이미지를 CogIPOneImageTool1에 직접 주입하고 검사 그룹만 실행
        /// (카메라 타임아웃을 원천 차단하기 위해 검사 툴그룹만 단독 실행)
        /// </summary>
        public void RunWithImage(ICogImage image)
        {
            IPOneImageTool.InputImage = image;
            InspectionGroup.Run();
        }


        /// <summary>
        /// 카메라 모드: 카메라 취득부터 검사까지 상위 ToolGroup을 동기 실행 (1단계 1-1)
        /// 취득에 실패하면 메시지를, 성공하면 null을 반환한다.
        /// </summary>
        public string RunWithCamera(out ICogImage acquiredImage)
        {
            acquiredImage = null;

            // IPOneImageTool.OutputImage는 InputImage 입력이 없어도 기존 값을 가지고 있으므로 초기화
            IPOneImageTool.InputImage = null;

            ToolGroup.Run();

            if (AcqFifoTool.RunStatus.Result != CogToolResultConstants.Accept)
            {
                return "카메라 이미지 취득 실패";
            }
            if (AcqFifoTool.OutputImage == null)
            {
                return "OutputImage가 null입니다. 이미지가 들어오지 않았습니다.";
            }

            acquiredImage = AcqFifoTool.OutputImage;
            return null;
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
            return ToolGroup.CreateLastRunRecord().SubRecords[ResultRecordKey];
        }


        /// <summary>
        /// 측정 그래픽을 담고 있는 LEFTside/RIGHTside의 LastRun 레코드 (3단계 3-7)
        /// </summary>
        public List<ICogRecord> CreateMeasureRecords()
        {
            List<ICogRecord> records = new List<ICogRecord>();
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
                foreach (CogToolBlockTerminal terminal in block.Outputs)
                {
                    if (terminal.Name == spec.OutputName)
                    {
                        value = terminal.Value;
                        isFound = true;
                        break;
                    }
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
    }
}
