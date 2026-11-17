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
    ///   출력 : Result     (bool, true = OK)  ← 필수
    ///   입력 : PixelX, PixelY (double)       ← 있으면 SYSTEM 탭 값을 넣어 줌
    /// </summary>
    public class JobRunner : IDisposable
    {
        public const string InputImageName = "InputImage";
        public const string ResultName = "Result";

        public CogToolBlock ToolBlock { get; private set; }
        public string FilePath { get; private set; }

        public JobRunner(CogToolBlock toolBlock, string filePath)
        {
            ToolBlock = toolBlock;
            FilePath = filePath;
        }

        /// <summary>
        /// 약속(InputImage / Result)이 지켜졌는지 확인한다. 문제가 있으면 목록으로 돌려준다.
        /// </summary>
        public List<string> CheckContract()
        {
            List<string> problems = new List<string>();
            if (!ToolBlock.Inputs.Contains(InputImageName)) problems.Add("Inputs 에 \"" + InputImageName + "\" 이 없습니다.");
            if (!ToolBlock.Outputs.Contains(ResultName)) problems.Add("Outputs 에 \"" + ResultName + "\" 이 없습니다.");
            return problems;
        }

        /// <summary>
        /// 검사 1회.
        /// </summary>
        public InspectionResult Run(ICogImage image, SystemSettings system, RecipeSettings recipe)
        {
            InspectionResult result = new InspectionResult();
            result.Image = image;

            // 1) 이미지 넣기 (입력 형식이 흑백인데 컬러 이미지가 오면 흑백으로 바꿔서 넣음)
            try
            {
                CogToolBlockTerminal input = ToolBlock.Inputs[InputImageName];
                input.Value = ConvertForInput(image, input.ValueType);
                SetInputIfExists("PixelX", system.PixelX);
                SetInputIfExists("PixelY", system.PixelY);
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

            // 3) 판정: Outputs["Result"] 가 true 이면 OK
            object value = ToolBlock.Outputs[ResultName].Value;
            result.Ok = !result.IsError && value is bool && (bool)value;

            // 4) 화면에 보여 줄 Record
            result.Record = SelectRecord(ToolBlock.CreateLastRunRecord(), recipe.DisplayRecordName);

            // 5) 결과 표 값 (CogResultsAnalysisTool 의 출력 항목)
            CogResultsAnalysisTool analysis = ResultsAnalysisReader.Find(ToolBlock, recipe.ResultsAnalysisToolName);
            ResultsAnalysisReader.ReadOutputs(analysis, result.OutputNames, result.OutputValues);

            return result;
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
        /// 화면에 보여 줄 Record 고르기
        ///   1) recipe.xml 의 DisplayRecordName 과 이름이 같은 Record
        ///   2) 없으면 이미지가 들어 있는 첫 번째 Record
        ///   3) 이미지가 있는 Record 가 하나도 없으면 null → 화면에는 입력 이미지를 그대로 보여 준다
        /// </summary>
        private static ICogRecord SelectRecord(ICogRecord record, string name)
        {
            if (record == null) return null;

            if (!string.IsNullOrEmpty(name))
            {
                ICogRecord named = FindRecord(record, name, false);
                if (named != null) return named;
            }
            return FindRecord(record, null, true);
        }

        /// <summary>
        /// record 와 그 안쪽 Record 를 차례로 찾는다 (재귀 호출).
        /// name 이 있으면 이름(RecordKey)이 같은 것, imageOnly 이면 이미지가 들어 있는 것.
        /// </summary>
        private static ICogRecord FindRecord(ICogRecord record, string name, bool imageOnly)
        {
            bool nameOk = name == null || record.RecordKey == name;
            bool imageOk = !imageOnly || record.Content is ICogImage;
            if (nameOk && imageOk) return record;

            if (record.SubRecords == null) return null;
            foreach (ICogRecord sub in record.SubRecords)
            {
                ICogRecord found = FindRecord(sub, name, imageOnly);
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
