using Cognex.VisionPro;
using Cognex.VisionPro.ImageFile;
using Cognex.VisionPro.QuickBuild;
using Cognex.VisionPro.ResultsAnalysis;
using Cognex.VisionPro.ToolBlock;
using Cognex.VisionPro.ToolGroup;
using System;
using System.Collections.Generic;

// ===============================================================================
//  261029@Claude [VProManager02] VisionPro 처리 (단순 구조)
//  VProManager01의 VisionService(툴을 하나씩 실행하며 값을 코드로 넘기던 방식)를 대신한다.
//  이전 구현은 D:\10_InProcess\VINA\VProManager01\VisionService.cs 에 있다.
//
//  원칙
//    - VPP(Job)는 QuickBuild에서 따로 검증한다. 프로그램은 툴·터미널이 있는지 하나하나 확인하지 않는다.
//      (없으면 검사 중 예외가 나고, Form1이 메시지로 보여 준다)
//    - 툴 사이의 값 전달은 VPP 링크가 한다. 프로그램은 ToolGroup.Run()을 한 번 호출할 뿐이다.
//
//  VPP 구조 (tree3.png 기준)
//    Job(0).VisionTool = CogToolGroup
//      Image Source             사용 안 함
//      tbImageSource (ToolBlock)  in  IsFileMode (int, 1 = 파일 모드), FileInputImage
//                                 out OutputImage
//                                 블록 안 CogAcqFifoTool1 + 스크립트:
//                                   파일 모드 → FileInputImage를 그대로 내보냄 (AcqFifo 실행 안 함)
//                                   라이브   → AcqFifo로 취득해서 내보냄
//                                   이미지가 없으면 블록 결과 Error
//      Locate (ToolBlock)         out OutputImage (Fixture 이미지), AlignScore
//      LEFTside / RIGHTside       out leftDist01, leftDist02 / rightDist01, rightDist02
//      CogResultsAnalysisTool1    in  LeftD1, LeftD2, RightD1, RightD2 (판정식 없음, 화면 Grid 표시용)
//
//  검사 순서 (Form1.runInspection)
//    ① Run(파일 모드 여부, 파일 이미지)      → tbImageSource 입력 설정 후 ToolGroup.Run()
//    ② GetImageSourceError()                  → 이미지 입력 실패면 메시지 (판정·기록 없이 종료)
//    ③ CheckLocate(기준 점수)                 → AlignScore로 위치 판정 (실패면 NG)
//    ④ EvaluateMeasure(...)                   → 측정값 읽기 (공차 판정은 설정의 Min/Max)
//    ⑤ GetAnalysisColumns/Values()            → 결과 분석 툴 항목·값 (화면 Grid)
// ===============================================================================
namespace VProManager02
{
    internal class VisionService
    {
        // 툴 이름 (VPP에서 이름을 바꾸면 여기만 고친다)
        private const string ImageSourceBlockName = "tbImageSource";
        private const string LocateName = "Locate";
        private const string LeftSideName = "LEFTside";
        private const string RightSideName = "RIGHTside";
        // 결과 분석 툴은 이름이 아니라 형식(CogResultsAnalysisTool)으로 찾는다. (이름을 바꿔도 동작)

        // 터미널 이름 (트리에 보이는 이름 그대로, 대소문자 구분)
        private const string IsFileModeTerminal = "IsFileMode";          // tbImageSource 입력 (int: 1 = 파일, 0 = 라이브)
        private const string FileInputImageTerminal = "FileInputImage";  // tbImageSource 입력
        private const string OutputImageTerminal = "OutputImage";        // tbImageSource·Locate 출력
        private const string AlignScoreTerminal = "AlignScore";          // Locate 출력 (못 찾으면 0이 나오도록 VPP에서 설정)

        public CogJobManager JobManager { get; private set; }
        public CogToolGroup ToolGroup { get; private set; }
        public CogToolBlock ImageSourceBlock { get; private set; }
        public CogToolBlock Locate { get; private set; }
        public CogToolBlock LeftSide { get; private set; }
        public CogToolBlock RightSide { get; private set; }
        public CogResultsAnalysisTool ResultsAnalysisTool { get; private set; }   // 없으면 null (Grid에 항목 열 없음)

        /// <summary>로드에 성공한 VPP 경로 (없으면 빈 문자열)</summary>
        public string LoadedVppPath { get; private set; } = "";


        public class LoadResult
        {
            public bool IsSuccess;
            public string Message = "";
        }


        public class LocateResult
        {
            public bool IsOk;
            public string Text = "";     // 화면 표시 문자열 (예: "0.85 - PASS", "NG - No Score")
            public double? Score;
        }


        /// <summary>
        /// VPP 로드
        /// - QuickBuild Job Manager / 단일 Job / ToolGroup 형식을 받는다.
        /// - 툴은 이름으로 찾기만 하고, 없어도 로드를 거부하지 않는다. (VPP 검증은 QuickBuild에서)
        /// - 새 VPP를 다 읽은 뒤에 교체하고, 이전 Job Manager는 Shutdown 한다.
        /// - 파일 읽기 예외는 호출부로 전달한다. (새로 읽은 Job Manager는 정리한 뒤)
        /// </summary>
        public LoadResult LoadJob(string vppPath)
        {
            CogJobManager newJobManager = null;
            bool isApplied = false;

            try
            {
                object loadedObject = CogSerializer.LoadObjectFromFile(vppPath);

                CogToolGroup newToolGroup = null;
                if (loadedObject is CogJobManager manager)
                {
                    // QuickBuild에서 저장한 Job Manager (일반적인 경우). 첫 번째 Job만 사용한다.
                    newJobManager = manager;
                    newToolGroup = manager.JobCount > 0 ? manager.Job(0).VisionTool as CogToolGroup : null;
                }
                else if (loadedObject is CogJob job)
                {
                    newToolGroup = job.VisionTool as CogToolGroup;
                }
                else if (loadedObject is CogToolGroup toolGroup)
                {
                    newToolGroup = toolGroup;
                }

                // 실행 대상(ToolGroup)이 없으면 아무것도 할 수 없으므로 이것만 확인한다.
                if (newToolGroup == null)
                {
                    return new LoadResult { Message = $"VPP에서 ToolGroup을 찾을 수 없습니다. (타입: {loadedObject?.GetType().Name})" };
                }

                // 교체 (이전 Job Manager는 정리)
                CogJobManager oldJobManager = JobManager;

                JobManager = newJobManager;
                ToolGroup = newToolGroup;
                ImageSourceBlock = FindTool(newToolGroup, ImageSourceBlockName) as CogToolBlock;
                Locate = FindTool(newToolGroup, LocateName) as CogToolBlock;
                LeftSide = FindTool(newToolGroup, LeftSideName) as CogToolBlock;
                RightSide = FindTool(newToolGroup, RightSideName) as CogToolBlock;
                ResultsAnalysisTool = FindFirst<CogResultsAnalysisTool>(newToolGroup);
                LoadedVppPath = vppPath;
                isApplied = true;

                if (oldJobManager != null && !ReferenceEquals(oldJobManager, newJobManager))
                {
                    ShutdownJobManager(oldJobManager);
                }

                AppLogger.Info("VPP 로드 완료: " + vppPath
                               + (ResultsAnalysisTool == null ? " (결과 분석 툴 없음)" : " (결과 분석: " + ResultsAnalysisTool.Name + ")"));
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
        /// 검사를 실행할 수 있는지 (VPP를 로드했는지)만 확인. 준비되었으면 null, 아니면 메시지.
        /// </summary>
        public string GetNotReadyMessage()
        {
            return ToolGroup == null ? "비전 시스템이 초기화되지 않았습니다. Model Load를 먼저 실행하세요." : null;
        }


        /// <summary>
        /// ① 검사 실행: tbImageSource 입력을 설정하고 ToolGroup을 한 번 실행한다. (동기 실행)
        /// - 파일 모드: IsFileMode = 1, FileInputImage = 파일 이미지 → 스크립트가 AcqFifo 없이 그대로 내보냄
        /// - 라이브   : IsFileMode = 0, FileInputImage = null → 스크립트가 블록 안 AcqFifo로 취득
        /// FileInputImage는 VPP 링크가 없으므로 ToolGroup이 실행되어도 덮어쓰이지 않는다.
        /// </summary>
        public void Run(bool isFileMode, ICogImage fileImage)
        {
            ImageSourceBlock.Inputs[IsFileModeTerminal].Value = isFileMode ? 1 : 0;
            ImageSourceBlock.Inputs[FileInputImageTerminal].Value = isFileMode ? fileImage : null;   // 라이브 모드는 이전 파일 이미지를 비움
            ToolGroup.Run();
        }


        /// <summary>
        /// ② 이미지 입력 결과: tbImageSource가 Accept가 아니면 메시지(스크립트가 남긴 RunStatus.Message), 아니면 null
        /// 카메라 취득 실패, 파일 이미지 없음, 블록 안 AcqFifo 없음이 모두 여기서 걸린다.
        /// </summary>
        public string GetImageSourceError()
        {
            ICogRunStatus status = ImageSourceBlock.RunStatus;
            if (status.Result == CogToolResultConstants.Accept)
            {
                return null;
            }
            return string.IsNullOrEmpty(status.Message) ? "이미지 입력 실패 (" + status.Result + ")" : status.Message;
        }


        /// <summary>이번 검사 입력 이미지 (tbImageSource.OutputImage). 카메라 모드 이미지 저장·화면 표시에 사용</summary>
        public ICogImage InputImage
        {
            get { return ImageSourceBlock.Outputs[OutputImageTerminal].Value as ICogImage; }
        }


        /// <summary>
        /// ③ 위치 확인: Locate 출력 AlignScore로 판정
        /// - Locate 결과 Error              → "NG - Locate Error"
        /// - AlignScore가 없거나 숫자가 아님 → "NG - No Score"
        /// - AlignScore < 기준 점수          → "0.45 - FAIL"
        /// 못 찾았을 때 AlignScore를 0으로 내보내는 것은 VPP(Locate) 쪽에서 맡는다.
        /// </summary>
        public LocateResult CheckLocate(double minScore)
        {
            if (Locate.RunStatus.Result == CogToolResultConstants.Error)
            {
                return new LocateResult { Text = "NG - Locate Error" };
            }

            double? score = ToDouble(Locate.Outputs[AlignScoreTerminal].Value);
            if (!score.HasValue)
            {
                return new LocateResult { Text = "NG - No Score" };
            }

            bool isOk = score.Value >= minScore;
            return new LocateResult
            {
                IsOk = isOk,
                Score = score,
                Text = score.Value.ToString("F2") + (isOk ? " - PASS" : " - FAIL")
            };
        }


        /// <summary>
        /// ④ 측정값 하나 판정
        /// - readValues가 false(위치 확인 실패)면 값을 읽지 않고 "-"
        /// - 측정 ToolBlock이 Accept가 아니면 NG(상태) (실패한 툴의 출력에는 이전 값이 남아 있을 수 있음)
        /// - 출력은 이름(OutputName) 우선, 없으면 번호(OutputIndex)
        /// - 공차 판정은 설정(Settings 창)의 Min/Max
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


        /// <summary>결과 화면에 표시할 이미지: Locate 출력 OutputImage (Fixture 좌표계 이미지)</summary>
        public ICogImage LocatedImage
        {
            get { return Locate.Outputs[OutputImageTerminal].Value as ICogImage; }
        }


        /// <summary>
        /// 결과 화면에 겹쳐 그릴 그래픽을 담은 LastRun 레코드 (Locate, LEFTside, RIGHTside)
        /// 레코드 키(툴 이름)에 의존하지 않도록 블록 전체 레코드를 넘기고, Form1이 그래픽만 골라 그린다.
        /// </summary>
        public List<ICogRecord> CreateGraphicRecords()
        {
            return new List<ICogRecord>
            {
                Locate.CreateLastRunRecord(),
                LeftSide.CreateLastRunRecord(),
                RightSide.CreateLastRunRecord()
            };
        }


        /// <summary>
        /// ⑤ 결과 분석 툴 항목 이름 (툴에 정의된 순서). Grid 열 머리글로 쓴다. 툴이 없으면 빈 목록.
        /// RunParams는 "이름 → 식" 사전이며, QuickBuild에서 만든 입력(LeftD1 …)과 계산식이 모두 들어 있다.
        /// </summary>
        public List<string> GetAnalysisColumns()
        {
            List<string> names = new List<string>();
            if (ResultsAnalysisTool == null || ResultsAnalysisTool.RunParams == null)
            {
                return names;
            }
            for (int i = 0; i < ResultsAnalysisTool.RunParams.Count; i++)
            {
                names.Add(ResultsAnalysisTool.RunParams.GetKey(i));
            }
            return names;
        }


        /// <summary>
        /// ⑤ 결과 분석 툴 항목 값 (GetAnalysisColumns와 같은 순서, 화면 표시용 문자열)
        /// - 계산 결과(Result.EvaluatedExpressions)가 있으면 그 값, 계산 실패면 "Err"
        /// - 계산 결과에 없으면 입력 식의 값(VPP 링크로 들어온 값)
        /// - 숫자는 소수 3자리
        /// </summary>
        public List<string> GetAnalysisValues()
        {
            List<string> values = new List<string>();
            foreach (string name in GetAnalysisColumns())
            {
                object value = null;
                bool isError = false;
                try
                {
                    CogResultsAnalysisEvaluationInfo info = ResultsAnalysisTool.Result?.EvaluatedExpressions[name];
                    if (info != null)
                    {
                        isError = !info.Success;
                        value = info.Value;
                    }
                    else
                    {
                        value = (ResultsAnalysisTool.RunParams[name] as CogResultsAnalysisInputExpression)?.Value;
                    }
                }
                catch (Exception)
                {
                    // 계산 결과 목록에 그 이름이 없으면 예외 → 입력 식의 값으로 대신한다.
                    value = (ResultsAnalysisTool.RunParams[name] as CogResultsAnalysisInputExpression)?.Value;
                }

                values.Add(isError ? "Err" : FormatValue(value));
            }
            return values;
        }


        /// <summary>
        /// 종료 시 Job Manager와 카메라(FrameGrabber) 해제
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
            using (CogImageFileTool fileTool = new CogImageFileTool())
            {
                fileTool.Operator.Open(imagePath, CogImageFileModeConstants.Read);
                ICogImage loadedImage = fileTool.Operator[0];
                fileTool.Operator.Close();
                return loadedImage;
            }
        }


        /// <summary>
        /// ToolBlock 출력값 읽기: 이름 우선, 없으면 번호. 숫자가 아니면 null
        /// </summary>
        private static double? ReadOutputValue(CogToolBlock block, MeasureSpec spec)
        {
            if (!string.IsNullOrEmpty(spec.OutputName))
            {
                foreach (CogToolBlockTerminal terminal in block.Outputs)
                {
                    if (terminal.Name == spec.OutputName)
                    {
                        return ToDouble(terminal.Value);
                    }
                }
                AppLogger.Info($"{block.Name} 출력 '{spec.OutputName}'을(를) 찾지 못해 번호 {spec.OutputIndex}를 사용");
            }

            if (spec.OutputIndex < 0 || spec.OutputIndex >= block.Outputs.Count)
            {
                return null;
            }
            return ToDouble(block.Outputs[spec.OutputIndex].Value);
        }


        /// <summary>
        /// 숫자로 바꾸기. 숫자가 아니면 null (true/false는 측정값이 아니므로 1/0으로 바꾸지 않음)
        /// </summary>
        private static double? ToDouble(object value)
        {
            if (value == null || value is bool || !(value is IConvertible))
            {
                return null;
            }
            try
            {
                return Convert.ToDouble(value);
            }
            catch (FormatException) { }
            catch (InvalidCastException) { }
            return null;
        }


        /// <summary>Grid 표시용 문자열: 숫자는 소수 3자리, 없으면 빈칸</summary>
        private static string FormatValue(object value)
        {
            double? number = ToDouble(value);
            if (number.HasValue)
            {
                return number.Value.ToString("F3");
            }
            return value == null ? "" : value.ToString();
        }


        /// <summary>
        /// Job Manager 정리 (null이면 아무것도 하지 않음)
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
        /// ToolGroup 바로 아래에서 이름으로 툴 찾기 (없으면 null)
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
        /// ToolGroup 바로 아래에서 형식으로 첫 번째 툴 찾기 (없으면 null)
        /// </summary>
        private static T FindFirst<T>(CogToolGroup group) where T : class, ICogTool
        {
            foreach (ICogTool tool in group.Tools)
            {
                if (tool is T found)
                {
                    return found;
                }
            }
            return null;
        }
    }
}
