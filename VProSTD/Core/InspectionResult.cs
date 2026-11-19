using Cognex.VisionPro;
using System.Collections.Generic;

namespace VProSTD
{
    /// <summary>
    /// 검사 1회의 결과
    /// </summary>
    public class InspectionResult
    {
        public bool Ok;                 // 최종 판정 (CogResultsAnalysisTool, 없으면 Outputs["Result"]). 오류이면 false (NG 로 집계)
        public bool IsError;            // ToolBlock 실행 오류 / 이미지 오류
        public string Message = "";     // 오류 내용 또는 참고 메시지
        public double TimeMs;           // ToolBlock 실행 시간 (ms)

        public string Source = "";      // 이미지 출처 (파일 이름 또는 "CAMERA")
        public ICogImage Image;         // 검사한 이미지 (원본 저장용)
        public ICogRecord Record;       // 화면에 보여 줄 Record

        // CogResultsAnalysisTool 의 Input / Output 항목 이름과 값 (결과 표, CSV 용)
        public List<string> OutputNames = new List<string>();
        public List<string> OutputValues = new List<string>();

        public string JudgeText
        {
            get
            {
                if (IsError) return "ERROR";
                if (Ok) return "OK";
                return "NG";
            }
        }
    }
}
