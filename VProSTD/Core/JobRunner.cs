using Cognex.VisionPro;
using Cognex.VisionPro.ResultsAnalysis;
using Cognex.VisionPro.ToolBlock;
using System;
using System.Collections.Generic;

namespace VProSTD
{
    /// <summary>
    /// 검사 실행: 이미지를 ToolBlock.Inputs["InputImage"] 에 넣고 Run() 한 뒤 결과를 읽는다.
    ///
    /// Job.vpp(ToolBlock) 약속
    ///   입력 : InputImage (ICogImage)        ← 필수
    ///   판정 : CogResultsAnalysisTool 의 판정(Decision). Accept = OK, Error = ERROR, 나머지(Warning/Reject) = NG
    ///          CogResultsAnalysisTool 이 없으면 Outputs 의 Result (bool, true = OK) 로 판정 (예전 방식 레시피)
    ///   입력 : PixelX, PixelY (double)       ← 있으면 SYSTEM 탭 값을 넣어 줌
    /// </summary>
    public class JobRunner : IDisposable
    {
        public const string InputImageName = "InputImage";
        public const string ResultName = "Result";

        // RUN 화면에 보여 줄 검사 결과 이미지 (ToolBlock 편집 화면의 LastRun.Locate.CogFixtureTool1.OutputImage)
        public const string DisplayRecordName = "Locate.CogFixtureTool1.OutputImage";

        public CogToolBlock ToolBlock { get; private set; }
        public string FilePath { get; set; }   // 다른 이름으로 저장하면 바뀜

        public JobRunner(CogToolBlock toolBlock, string filePath)
        {
            ToolBlock = toolBlock;
            FilePath = filePath;
        }

        /// <summary>
        /// 약속(InputImage / 판정할 곳)이 지켜졌는지 확인한다. 문제가 있으면 목록으로 돌려준다.
        /// analysisToolName: recipe.xml 의 ResultsAnalysisToolName (비우면 처음 찾은 것)
        /// </summary>
        public List<string> CheckContract(string analysisToolName)
        {
            List<string> problems = new List<string>();
            if (!ToolBlock.Inputs.Contains(InputImageName)) problems.Add("Inputs 에 \"" + InputImageName + "\" 이 없습니다.");

            bool hasAnalysis = ResultsAnalysisReader.Find(ToolBlock, analysisToolName) != null;
            if (!hasAnalysis && !ToolBlock.Outputs.Contains(ResultName))
            {
                problems.Add("판정할 CogResultsAnalysisTool 이 없습니다 (Outputs 에 \"" + ResultName + "\" 도 없습니다).");
            }
            return problems;
        }

        /// <summary>
        /// ToolBlock 입력에 이미지와 PixelX / PixelY 를 넣는다 (실행은 하지 않음).
        /// 입력 형식이 흑백인데 컬러 이미지가 오면 흑백으로 바꿔서 넣는다.
        /// 검사 때, 그리고 RECIPE 탭 편집 화면에서 바로 실행해 볼 수 있도록 이미지를 고르거나 TRIGGER 할 때 쓴다.
        /// </summary>
        public void SetInputImage(ICogImage image, SystemSettings system)
        {
            CogToolBlockTerminal input = ToolBlock.Inputs[InputImageName];
            input.Value = ConvertForInput(image, input.ValueType);
            SetInputIfExists("PixelX", system.PixelX);
            SetInputIfExists("PixelY", system.PixelY);
        }

        /// <summary>
        /// 검사 1회.
        /// </summary>
        public InspectionResult Run(ICogImage image, SystemSettings system, RecipeSettings recipe)
        {
            InspectionResult result = new InspectionResult();
            result.Image = image;

            // 1) 이미지 넣기
            try
            {
                SetInputImage(image, system);
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Message = "이미지를 입력할 수 없습니다: " + ex.Message;
                return result;
            }

            // 2) 실행
            ToolBlock.Run();

            ICogRunStatus status = ToolBlock.RunStatus;
            result.TimeMs = status.TotalTime;

            if (status.Result == CogToolResultConstants.Error)
            {
                result.IsError = true;
                result.Message = status.Message;
            }

            // 3) 판정. 판정할 곳이 없어도 ERROR 로 두고, 아래 화면 표시·결과 표는 계속 만든다 (이미지는 보여 주기 위해)
            CogResultsAnalysisTool analysis = ResultsAnalysisReader.Find(ToolBlock, recipe.ResultsAnalysisToolName);
            if (analysis != null) JudgeByAnalysis(analysis, result);
            else JudgeByResultOutput(result);

            // 3-1) 판정 툴 앞의 툴이 실패(Reject/Error)했으면 NG.
            //      예: PMAlign 이 못 찾으면 Locate 가 Reject 인데, Fixture 출력에는 이전 이미지가 남아서
            //      뒤의 측정 툴이 이전 이미지로 측정하고 판정 툴은 Accept 를 낼 수 있다.
            string failedTools = FindFailedTools(analysis);
            bool upstreamFailed = failedTools.Length > 0;
            if (upstreamFailed || IsFailed(status.Result))
            {
                result.Ok = false;
                if (upstreamFailed && result.Message.Length == 0) result.Message = "실패한 툴: " + failedTools;
            }

            // 4) 화면에 보여 줄 Record
            result.Record = SelectRecord();

            // 5) 결과 표 값 (CogResultsAnalysisTool 의 Input / Output 항목)
            ResultsAnalysisReader.ReadItems(analysis, result.OutputNames, result.OutputValues);

            // ERROR 이거나 앞의 툴이 실패했으면 값은 비운다: 실패한 툴의 출력에는 이전 검사 값이 남아 있어서 믿을 수 없다 (열 이름은 유지).
            // 판정 툴만 Reject 인 경우(측정값이 기준을 벗어남)는 값이 이번 검사 값이므로 그대로 보여 준다.
            if (result.IsError || upstreamFailed)
            {
                for (int i = 0; i < result.OutputValues.Count; i++) result.OutputValues[i] = "";
            }

            return result;
        }

        /// <summary>
        /// CogResultsAnalysisTool 의 판정(Decision)으로 판정한다. Accept = OK, Error = ERROR, 나머지 = NG.
        /// </summary>
        private static void JudgeByAnalysis(CogResultsAnalysisTool analysis, InspectionResult result)
        {
            result.Ok = false;
            if (result.IsError) return;   // ToolBlock 실행 오류가 먼저

            CogResultsAnalysisResult analysisResult = analysis.Result;
            if (analysisResult == null)
            {
                result.IsError = true;
                result.Message = analysis.Name + " 결과가 없습니다: " + analysis.RunStatus.Message;
                return;
            }

            CogToolResultConstants decision = analysisResult.Decision;
            if (decision == CogToolResultConstants.Accept)
            {
                result.Ok = true;
            }
            else if (decision == CogToolResultConstants.Error)
            {
                result.IsError = true;
                result.Message = analysis.Name + " 판정 오류: " + analysis.RunStatus.Message;
            }
        }

        /// <summary>
        /// 예전 방식: ToolBlock Outputs["Result"] 가 true 이면 OK. Result 도 없으면 ERROR.
        /// </summary>
        private void JudgeByResultOutput(InspectionResult result)
        {
            result.Ok = false;
            if (ToolBlock.Outputs.Contains(ResultName))
            {
                object value = ToolBlock.Outputs[ResultName].Value;
                result.Ok = !result.IsError && value is bool && (bool)value;
            }
            else if (!result.IsError)
            {
                result.IsError = true;
                result.Message = "판정할 CogResultsAnalysisTool 이 없습니다 (Outputs 에 \"" + ResultName + "\" 도 없습니다).";
            }
        }

        /// <summary>
        /// 입력 형식이 흑백(CogImage8Grey)이고 이미지가 컬러이면 흑백으로 바꾼다.
        /// </summary>
        private static ICogImage ConvertForInput(ICogImage image, Type inputType)
        {
            if (inputType == typeof(CogImage8Grey) && !(image is CogImage8Grey))
            {
                return CogImageConvert.GetIntensityImage(image, 0, 0, image.Width, image.Height);
            }
            return image;
        }

        private void SetInputIfExists(string name, double value)
        {
            if (ToolBlock.Inputs.Contains(name)) ToolBlock.Inputs[name].Value = value;
        }

        /// <summary>
        /// 화면에 보여 줄 Record: 검사 결과 이미지 DisplayRecordName.
        /// 이 Record 가 없는 ToolBlock 이면 null → 화면에는 입력 이미지를 그대로 보여 준다.
        /// </summary>
        private ICogRecord SelectRecord()
        {
            ICogRecord record = ToolBlock.CreateLastRunRecord();
            if (record == null) return null;

            ICogRecord found = FindRecord(record, DisplayRecordName);
            if (found == null || !(found.Content is ICogImage)) return null;
            return found;
        }

        /// <summary>
        /// ToolBlock 바로 안의 툴 중 Reject/Error 인 툴 (판정 툴은 뺌). 예: "Locate Reject, LEFTside Error". 없으면 "".
        /// </summary>
        private string FindFailedTools(CogResultsAnalysisTool analysis)
        {
            List<string> failed = new List<string>();
            foreach (ICogTool tool in ToolBlock.Tools)
            {
                if (tool == analysis) continue;
                if (tool.RunStatus != null && IsFailed(tool.RunStatus.Result)) failed.Add(tool.Name + " " + tool.RunStatus.Result);
            }
            return string.Join(", ", failed.ToArray());
        }

        private static bool IsFailed(CogToolResultConstants value)
        {
            return value == CogToolResultConstants.Reject || value == CogToolResultConstants.Error;
        }

        /// <summary>
        /// record 와 그 안쪽 Record 를 차례로 보면서 이름(RecordKey)이 같은 것을 찾는다 (재귀 호출).
        /// </summary>
        private static ICogRecord FindRecord(ICogRecord record, string name)
        {
            if (record.RecordKey == name) return record;

            if (record.SubRecords == null) return null;
            foreach (ICogRecord sub in record.SubRecords)
            {
                ICogRecord found = FindRecord(sub, name);
                if (found != null) return found;
            }
            return null;
        }

        /// <summary>
        /// [안전] 편집 화면의 Subject 를 먼저 끊은 뒤에 부를 것.
        /// </summary>
        public void Dispose()
        {
            VppFile.Release(ToolBlock);
            ToolBlock = null;
        }
    }
}
