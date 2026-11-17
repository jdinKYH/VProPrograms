using Cognex.VisionPro;
using Cognex.VisionPro.ResultsAnalysis;
using Cognex.VisionPro.ToolGroup;
using System;
using System.Collections;
using System.Collections.Generic;

namespace VProSTD
{
    /// <summary>
    /// ToolBlock 안의 CogResultsAnalysisTool 에서 "출력(Output)" 항목의 이름과 값을 읽는다.
    ///
    /// CogResultsAnalysisTool 의 항목 종류 (편집 화면의 Type 열)
    ///   Input      : 다른 툴 결과를 가져오는 항목
    ///   Expression : 계산식
    ///   Output     : 최종 판단 항목  ← 이것만 읽는다 (CogResultsAnalysisResultExpression)
    /// </summary>
    public static class ResultsAnalysisReader
    {
        /// <summary>
        /// group 안에서 CogResultsAnalysisTool 을 찾는다 (안쪽 ToolGroup/ToolBlock 까지).
        /// name 이 비어 있으면 처음 찾은 것, 아니면 이름이 같은 것.
        /// </summary>
        public static CogResultsAnalysisTool Find(CogToolGroup group, string name)
        {
            if (group == null) return null;

            foreach (ICogTool tool in group.Tools)
            {
                CogResultsAnalysisTool analysis = tool as CogResultsAnalysisTool;
                if (analysis != null)
                {
                    if (string.IsNullOrEmpty(name) || analysis.Name == name) return analysis;
                }

                CogToolGroup subGroup = tool as CogToolGroup;   // ToolBlock 도 여기 포함됨
                if (subGroup != null)
                {
                    CogResultsAnalysisTool found = Find(subGroup, name);
                    if (found != null) return found;
                }
            }
            return null;
        }

        /// <summary>
        /// 출력 항목의 이름/값을 names, values 에 채운다. 결과가 없으면 값은 빈 칸.
        /// </summary>
        public static void ReadOutputs(CogResultsAnalysisTool tool, List<string> names, List<string> values)
        {
            if (tool == null) return;

            CogResultsAnalysisResult result = tool.Result;

            foreach (DictionaryEntry entry in tool.RunParams)
            {
                if (!(entry.Value is CogResultsAnalysisResultExpression)) continue;   // 출력 항목만

                string name = entry.Key.ToString();
                names.Add(name);
                values.Add(ReadValue(result, name));
            }
        }

        private static string ReadValue(CogResultsAnalysisResult result, string name)
        {
            try
            {
                if (result == null) return "";
                if (!result.EvaluatedExpressions.Contains(name)) return "";

                CogResultsAnalysisEvaluationInfo info = result.EvaluatedExpressions[name];
                if (!info.Success) return "ERR";
                return FormatValue(info.Value);
            }
            catch (Exception)
            {
                return "ERR";
            }
        }

        /// <summary>
        /// 값 → 글자. 실수는 소수점 아래 3자리까지.
        /// </summary>
        public static string FormatValue(object value)
        {
            if (value == null) return "";
            if (value is double) return ((double)value).ToString("0.###");
            if (value is float) return ((float)value).ToString("0.###");
            return value.ToString();
        }
    }
}
