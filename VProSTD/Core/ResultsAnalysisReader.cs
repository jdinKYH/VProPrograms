using Cognex.VisionPro;
using Cognex.VisionPro.ResultsAnalysis;
using Cognex.VisionPro.ToolGroup;
using System;
using System.Collections;
using System.Collections.Generic;

namespace VProSTD
{
    /// <summary>
    /// ToolBlock 안의 CogResultsAnalysisTool 에서 Input / Output 항목의 이름과 값을 읽는다 (결과 표, CSV).
    ///
    /// CogResultsAnalysisTool 의 항목 종류 (편집 화면의 Type 열)
    ///   Input      : 다른 툴 결과를 가져오는 항목 (측정값)  ← 읽음 (CogResultsAnalysisInputExpression)
    ///   Expression : 중간 계산식                            ← 읽지 않음
    ///   Output     : 최종 판단 항목                          ← 읽음 (CogResultsAnalysisResultExpression)
    /// </summary>
    public static class ResultsAnalysisReader
    {
        /// <summary>
        /// group 안에서 CogResultsAnalysisTool 을 찾는다.
        /// 먼저 group 바로 안의 툴에서 찾고, 없으면 안쪽 ToolGroup/ToolBlock 으로 들어가서 찾는다
        /// (안쪽 ToolBlock 의 툴이 바깥 판정 툴보다 먼저 잡히지 않게).
        /// name 이 비어 있으면 처음 찾은 것, 아니면 이름이 같은 것.
        /// </summary>
        public static CogResultsAnalysisTool Find(CogToolGroup group, string name)
        {
            if (group == null) return null;

            // 1) 바로 안의 툴
            foreach (ICogTool tool in group.Tools)
            {
                CogResultsAnalysisTool analysis = tool as CogResultsAnalysisTool;
                if (analysis == null) continue;
                if (string.IsNullOrEmpty(name) || analysis.Name == name) return analysis;
            }

            // 2) 안쪽 ToolGroup / ToolBlock
            foreach (ICogTool tool in group.Tools)
            {
                CogToolGroup subGroup = tool as CogToolGroup;   // ToolBlock 도 여기 포함됨
                if (subGroup == null) continue;
                CogResultsAnalysisTool found = Find(subGroup, name);
                if (found != null) return found;
            }
            return null;
        }

        /// <summary>
        /// Input / Output 항목의 이름/값을 툴에 들어 있는 순서대로 names, values 에 채운다. 결과가 없으면 값은 빈 칸.
        /// </summary>
        public static void ReadItems(CogResultsAnalysisTool tool, List<string> names, List<string> values)
        {
            if (tool == null) return;

            CogResultsAnalysisResult result = tool.Result;

            foreach (DictionaryEntry entry in tool.RunParams)
            {
                bool isInput = entry.Value is CogResultsAnalysisInputExpression;
                bool isOutput = entry.Value is CogResultsAnalysisResultExpression;
                if (!isInput && !isOutput) continue;   // 중간 계산식(Expression)은 빼고

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
