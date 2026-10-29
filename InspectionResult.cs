using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Cognex.VisionPro;
using Cognex.VisionPro.ImageFile;

// ===============================================================================
//  261028@Claude [3단계 3-1] 검사 결과 (OK/NG 판정)
//  261028@Claude [3단계] 결과 저장: Results\yyyyMMdd.csv, NG 이미지 Results\NG\yyyyMMdd\
// ===============================================================================
namespace VProManager01
{
    /// <summary>
    /// 측정값 하나의 결과
    /// </summary>
    public class MeasureResult
    {
        public string Name;
        public double? Value;       // 읽지 못하면 null
        public string Text;         // 화면 표시 문자열 (값, NG(상태), N/A)
        public bool IsOk;           // 값을 읽었고, 공차를 쓰면 공차 안에 있음
    }


    /// <summary>
    /// 검사 1회의 결과
    /// </summary>
    public class InspectionResult
    {
        public DateTime Time = DateTime.Now;
        public bool IsOfflineFileMode;
        public string ImageSource = "";       // 파일 경로 또는 "Camera"
        public string AlignText = "";         // 화면에 표시한 위치 확인 결과
        public double? AlignScore;
        public bool IsLocateOk;
        public List<MeasureResult> Measures = new List<MeasureResult>();
        public long ElapsedMs;

        // 261029@Claude [구조 변경 2] CogResultsAnalysisTool1 판정
        public string AnalysisText = "";      // 화면·CSV 표시 (Accept, Reject, NotRun, Error, "-"=툴 없음)
        /// <summary>true/false: 판정에 반영, null: 툴이 없거나 설정에서 끔 (판정에 쓰지 않음)</summary>
        public bool? IsAnalysisOk;

        // 261029@Claude [구조 변경 2] 최종 판정에 결과 분석 Accept 조건 추가 (판정에 쓰는 경우만) (기존 코드)
        //public bool IsOk
        //{
        //    get { return IsLocateOk && Measures.Count > 0 && Measures.All(m => m.IsOk); }
        //}
        /// <summary>최종 판정: 위치 확인 성공 + 모든 측정값 OK + 결과 분석 Accept(IsAnalysisOk가 null이면 조건 없음)</summary>
        public bool IsOk
        {
            get { return IsLocateOk && Measures.Count > 0 && Measures.All(m => m.IsOk) && IsAnalysisOk != false; }
        }

        public string JudgeText
        {
            get { return IsOk ? "OK" : "NG"; }
        }
    }


    /// <summary>
    /// 검사 결과를 파일로 저장 (실패해도 검사는 계속되도록 예외는 로그만 남긴다)
    /// </summary>
    internal static class ResultRecorder
    {
        private static string ResultDir
        {
            // 261029@Claude [검토 보완 8] 실행 폴더에 쓸 수 없으면 ProgramData 사용
            // get { return Path.Combine(Application.StartupPath, "Results"); }
            get { return AppPaths.ResultDirectory; }
        }


        /// <summary>
        /// 261029@Claude [검토 보완 6] Results\yyyyMMdd.csv 에 한 줄 추가
        /// 파일이 Excel 등에서 열려 있어 쓸 수 없으면 yyyyMMdd_locked.csv 에 대신 기록한다.
        /// 정상이면 null, 아니면 화면에 보여줄 경고를 반환한다. (기존: 실패해도 로그에만 남음)
        /// </summary>
        public static string AppendCsv(InspectionResult result)
        {
            string date = result.Time.ToString("yyyyMMdd");
            try
            {
                Directory.CreateDirectory(ResultDir);
                appendCsvLine(Path.Combine(ResultDir, date + ".csv"), result);
                return null;
            }
            catch (IOException ex)
            {
                AppLogger.Error("검사 결과 CSV 저장 실패 (파일 사용 중?) → 대체 파일에 저장 시도", ex);
                try
                {
                    appendCsvLine(Path.Combine(ResultDir, date + "_locked.csv"), result);
                    return $"CSV({date}.csv)가 열려 있어 {date}_locked.csv에 저장";
                }
                catch (Exception ex2)
                {
                    AppLogger.Error("검사 결과 대체 CSV 저장 실패", ex2);
                    return "CSV 저장 실패 (로그 확인)";
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error("검사 결과 CSV 저장 실패", ex);
                return "CSV 저장 실패 (로그 확인)";
            }
        }


        /// <summary>
        /// CSV 파일에 검사 결과 한 줄 추가 (파일이 없으면 헤더 먼저 기록). 실패하면 예외
        /// </summary>
        private static void appendCsvLine(string csvPath, InspectionResult result)
        {
            {
                bool isNewFile = !File.Exists(csvPath);

                StringBuilder sb = new StringBuilder();
                if (isNewFile)
                {
                    sb.Append("Time,Mode,ImageSource,AlignScore,");
                    foreach (MeasureResult m in result.Measures)
                    {
                        sb.Append(csvEscape(m.Name)).Append(',');
                    }
                    // 261029@Claude [구조 변경 2] 결과 분석 열 추가 (기존: Judge,ElapsedMs)
                    sb.Append("Analysis,Judge,ElapsedMs").AppendLine();
                }

                sb.Append(result.Time.ToString("yyyy-MM-dd HH:mm:ss.fff")).Append(',');
                sb.Append(result.IsOfflineFileMode ? "File" : "Camera").Append(',');
                sb.Append(csvEscape(result.ImageSource)).Append(',');
                sb.Append(result.AlignScore.HasValue ? result.AlignScore.Value.ToString("F3", CultureInfo.InvariantCulture) : "").Append(',');
                foreach (MeasureResult m in result.Measures)
                {
                    sb.Append(m.Value.HasValue ? m.Value.Value.ToString("F3", CultureInfo.InvariantCulture) : csvEscape(m.Text)).Append(',');
                }
                sb.Append(csvEscape(result.AnalysisText)).Append(',');     // 261029@Claude [구조 변경 2]
                sb.Append(result.JudgeText).Append(',').Append(result.ElapsedMs).AppendLine();

                // Excel에서 한글이 깨지지 않도록 BOM 포함 UTF-8
                File.AppendAllText(csvPath, sb.ToString(), new UTF8Encoding(true));
            }
            // 261029@Claude [검토 보완 6] 예외 처리는 AppendCsv에서 (기존 코드)
            //catch (Exception ex)
            //{
            //    AppLogger.Error("검사 결과 CSV 저장 실패", ex);
            //}
        }


        /// <summary>
        /// NG 이미지를 Results\NG\yyyyMMdd\HHmmssfff.bmp 로 저장. 저장한 경로를 반환 (실패 시 null)
        /// </summary>
        public static string SaveNgImage(ICogImage image, DateTime time)
        {
            if (image == null)
            {
                return null;
            }
            try
            {
                string ngDir = Path.Combine(ResultDir, "NG", time.ToString("yyyyMMdd"));
                Directory.CreateDirectory(ngDir);
                string imagePath = Path.Combine(ngDir, time.ToString("HHmmssfff") + ".bmp");

                // 261029@Claude [검토 보완] 저장 중 예외가 나도 파일이 닫히도록 finally에서 Close
                //CogImageFile imageFile = new CogImageFile();
                //imageFile.Open(imagePath, CogImageFileModeConstants.Write);
                //imageFile.Append(image);
                //imageFile.Close();
                CogImageFile imageFile = new CogImageFile();
                try
                {
                    imageFile.Open(imagePath, CogImageFileModeConstants.Write);
                    imageFile.Append(image);
                }
                finally
                {
                    try { imageFile.Close(); } catch { }
                }
                return imagePath;
            }
            catch (Exception ex)
            {
                AppLogger.Error("NG 이미지 저장 실패", ex);
                return null;
            }
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
