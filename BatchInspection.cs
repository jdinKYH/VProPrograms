using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

// ===============================================================================
//  261028@Claude [4단계] 폴더 일괄 검사와 회귀 테스트
//  - 폴더의 이미지를 모두 파일 모드로 검사하고 Results\Batch_yyyyMMdd_HHmmss.csv 로 보고서를 만든다.
//  - 폴더에 expected.csv 가 있으면 기대값과 비교한다. (회귀 테스트)
//
//  expected.csv 형식 (첫 줄은 헤더, FileName과 Judge는 필수, 측정값 열은 선택)
//      FileName,Judge,Left Dist1,Left Dist2,Right Dist1,Right Dist2
//      sample01.bmp,OK,12.34,12.40,11.98,12.01
//      sample02.bmp,NG,,,,
//  - 측정값 열 이름은 설정의 DisplayName과 같아야 한다.
//  - 비어 있는 칸은 비교하지 않는다. 값은 RegressionValueTolerance(기본 0.01) 이내면 통과.
//  - 일괄 검사 보고서의 앞 열이 같은 형식이므로, 기준이 되는 보고서를 expected.csv 로 복사해서 쓰면 된다.
// ===============================================================================
namespace VProManager01
{
    /// <summary>
    /// 일괄 검사 이미지 한 장의 결과
    /// </summary>
    internal class BatchItem
    {
        public string FilePath;
        public InspectionResult Result;     // 검사 오류면 null
        public string Error = "";
        public string RegressionStatus = "-";   // PASS / FAIL / - (기대값 없음)
        public string RegressionDetail = "";

        public string FileName
        {
            get { return Path.GetFileName(FilePath); }
        }

        public string JudgeText
        {
            get { return Result == null ? "ERROR" : Result.JudgeText; }
        }
    }


    internal static class BatchInspection
    {
        public const string ExpectedFileName = "expected.csv";

        /// <summary>일괄 검사 대상 이미지 확장자</summary>
        private static readonly string[] ImageExtensions = { ".bmp", ".jpg", ".jpeg", ".png", ".tif", ".tiff" };


        /// <summary>
        /// 폴더 안의 이미지 파일 목록 (이름순)
        /// </summary>
        public static List<string> GetImageFiles(string folder)
        {
            return Directory.GetFiles(folder)
                .Where(f => ImageExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }


        /// <summary>
        /// 폴더의 expected.csv 를 읽는다. 없으면 null. 키는 파일 이름(대소문자 무시), 값은 열 이름 → 값
        /// </summary>
        public static Dictionary<string, Dictionary<string, string>> LoadExpected(string folder)
        {
            string expectedPath = Path.Combine(folder, ExpectedFileName);
            if (!File.Exists(expectedPath))
            {
                return null;
            }

            // 261029@Claude [검토 보완 7] Excel 기본 저장(ANSI, CP949)도 읽을 수 있도록 인코딩 자동 판별
            // string[] lines = File.ReadAllLines(expectedPath, Encoding.UTF8);
            string[] lines = readAllLinesAutoEncoding(expectedPath);
            if (lines.Length == 0)
            {
                return null;
            }

            List<string> header = splitCsvLine(lines[0]);
            int fileNameIndex = header.FindIndex(h => h.Equals("FileName", StringComparison.OrdinalIgnoreCase));
            if (fileNameIndex < 0)
            {
                throw new InvalidDataException(ExpectedFileName + " 첫 줄에 FileName 열이 없습니다.");
            }

            var expected = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                {
                    continue;
                }
                List<string> cells = splitCsvLine(lines[i]);
                if (fileNameIndex >= cells.Count || string.IsNullOrWhiteSpace(cells[fileNameIndex]))
                {
                    continue;
                }

                var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                for (int c = 0; c < header.Count && c < cells.Count; c++)
                {
                    row[header[c].Trim()] = cells[c].Trim();
                }
                expected[cells[fileNameIndex].Trim()] = row;
            }
            return expected;
        }


        /// <summary>
        /// 기대값과 비교해서 item.RegressionStatus / RegressionDetail 을 채운다.
        /// </summary>
        public static void Compare(BatchItem item, Dictionary<string, Dictionary<string, string>> expected, double valueTolerance)
        {
            if (expected == null)
            {
                return;
            }
            Dictionary<string, string> row;
            if (!expected.TryGetValue(item.FileName, out row))
            {
                item.RegressionStatus = "-";
                item.RegressionDetail = "기대값 없음";
                return;
            }

            List<string> failures = new List<string>();

            string expectedJudge;
            if (row.TryGetValue("Judge", out expectedJudge) && expectedJudge.Length > 0
                && !expectedJudge.Equals(item.JudgeText, StringComparison.OrdinalIgnoreCase))
            {
                failures.Add($"Judge {expectedJudge}→{item.JudgeText}");
            }

            if (item.Result != null)
            {
                foreach (MeasureResult measure in item.Result.Measures)
                {
                    string expectedText;
                    if (!row.TryGetValue(measure.Name, out expectedText) || expectedText.Length == 0)
                    {
                        continue;   // 비어 있는 칸은 비교하지 않음
                    }

                    double expectedValue;
                    if (!double.TryParse(expectedText, NumberStyles.Float, CultureInfo.InvariantCulture, out expectedValue))
                    {
                        failures.Add($"{measure.Name} 기대값 형식 오류({expectedText})");
                    }
                    else if (!measure.Value.HasValue)
                    {
                        failures.Add($"{measure.Name} {expectedText}→{measure.Text}");
                    }
                    else if (Math.Abs(measure.Value.Value - expectedValue) > valueTolerance)
                    {
                        failures.Add($"{measure.Name} {expectedText}→{measure.Value.Value.ToString("F3", CultureInfo.InvariantCulture)}");
                    }
                }
            }

            item.RegressionStatus = failures.Count == 0 ? "PASS" : "FAIL";
            item.RegressionDetail = string.Join("; ", failures);
        }


        /// <summary>
        /// 일괄 검사 보고서를 Results\Batch_yyyyMMdd_HHmmss.csv 로 저장하고 경로를 반환 (실패 시 null)
        /// </summary>
        public static string WriteReport(string folder, List<BatchItem> items, List<string> measureNames)
        {
            try
            {
                // 261029@Claude [검토 보완 8] 실행 폴더에 쓸 수 없으면 ProgramData 사용
                // string resultDir = Path.Combine(Application.StartupPath, "Results");
                string resultDir = AppPaths.ResultDirectory;
                Directory.CreateDirectory(resultDir);
                string reportPath = Path.Combine(resultDir, "Batch_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv");

                StringBuilder sb = new StringBuilder();
                sb.Append("FileName,Judge,");
                foreach (string name in measureNames)
                {
                    sb.Append(csvEscape(name)).Append(',');
                }
                // 261029@Claude [구조 변경 3] AlignScore 뒤에 결과 분석(Analysis) 열 추가 (기존 코드)
                // FileName,Judge,측정값 열의 순서는 그대로라 보고서를 expected.csv로 복사해 쓰는 방식은 그대로 동작한다.
                //sb.Append("AlignScore,ElapsedMs,Error,Regression,RegressionDetail,Folder").AppendLine();
                sb.Append("AlignScore,Analysis,ElapsedMs,Error,Regression,RegressionDetail,Folder").AppendLine();

                foreach (BatchItem item in items)
                {
                    sb.Append(csvEscape(item.FileName)).Append(',');
                    sb.Append(item.JudgeText).Append(',');
                    for (int i = 0; i < measureNames.Count; i++)
                    {
                        MeasureResult measure = (item.Result != null && i < item.Result.Measures.Count) ? item.Result.Measures[i] : null;
                        if (measure != null && measure.Value.HasValue)
                        {
                            sb.Append(measure.Value.Value.ToString("F3", CultureInfo.InvariantCulture));
                        }
                        sb.Append(',');
                    }
                    if (item.Result != null && item.Result.AlignScore.HasValue)
                    {
                        sb.Append(item.Result.AlignScore.Value.ToString("F3", CultureInfo.InvariantCulture));
                    }
                    sb.Append(',');
                    // 261029@Claude [구조 변경 3] 결과 분석 판정 (Accept, Reject, NotRun, "(미반영)" 표시 포함. 검사 오류면 빈칸)
                    sb.Append(item.Result != null ? csvEscape(item.Result.AnalysisText) : "").Append(',');
                    sb.Append(item.Result != null ? item.Result.ElapsedMs.ToString() : "").Append(',');
                    sb.Append(csvEscape(item.Error)).Append(',');
                    sb.Append(item.RegressionStatus).Append(',');
                    sb.Append(csvEscape(item.RegressionDetail)).Append(',');
                    sb.Append(csvEscape(folder)).AppendLine();
                }

                File.WriteAllText(reportPath, sb.ToString(), new UTF8Encoding(true));
                return reportPath;
            }
            catch (Exception ex)
            {
                AppLogger.Error("일괄 검사 보고서 저장 실패", ex);
                return null;
            }
        }


        /// <summary>
        /// 261029@Claude [검토 보완 7] 텍스트 파일 읽기: BOM이 있거나 올바른 UTF-8이면 UTF-8,
        /// 아니면 시스템 기본 인코딩(한국어 Windows는 CP949, Excel의 "CSV(쉼표로 분리)" 저장 형식)
        /// </summary>
        private static string[] readAllLinesAutoEncoding(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            string text;
            try
            {
                // BOM은 자동으로 건너뛰고, 잘못된 UTF-8 바이트가 있으면 예외
                text = new UTF8Encoding(false, true).GetString(bytes);
                if (text.Length > 0 && text[0] == '﻿')
                {
                    text = text.Substring(1);
                }
            }
            catch (DecoderFallbackException)
            {
                text = Encoding.Default.GetString(bytes);
            }
            return text.Replace("\r\n", "\n").Split('\n');
        }


        /// <summary>
        /// CSV 한 줄 나누기 (큰따옴표로 감싼 칸과 "" 이스케이프 지원)
        /// </summary>
        private static List<string> splitCsvLine(string line)
        {
            List<string> cells = new List<string>();
            StringBuilder cell = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char ch = line[i];
                if (inQuotes)
                {
                    if (ch == '"' && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        cell.Append('"');
                        i++;
                    }
                    else if (ch == '"')
                    {
                        inQuotes = false;
                    }
                    else
                    {
                        cell.Append(ch);
                    }
                }
                else if (ch == '"')
                {
                    inQuotes = true;
                }
                else if (ch == ',')
                {
                    cells.Add(cell.ToString());
                    cell.Clear();
                }
                else
                {
                    cell.Append(ch);
                }
            }
            cells.Add(cell.ToString());
            return cells;
        }


        private static string csvEscape(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "";
            }
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }
            return value;
        }
    }
}
