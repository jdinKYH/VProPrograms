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
namespace VproManageBlack02
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

        /// <summary>최종 판정: 위치 확인 성공 + 모든 측정값 OK</summary>
        public bool IsOk
        {
            get { return IsLocateOk && Measures.Count > 0 && Measures.All(m => m.IsOk); }
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
            get { return Path.Combine(Application.StartupPath, "Results"); }
        }


        /// <summary>
        /// Results\yyyyMMdd.csv 에 한 줄 추가 (파일이 없으면 헤더 먼저 기록)
        /// </summary>
        public static void AppendCsv(InspectionResult result)
        {
            try
            {
                Directory.CreateDirectory(ResultDir);
                string csvPath = Path.Combine(ResultDir, result.Time.ToString("yyyyMMdd") + ".csv");
                bool isNewFile = !File.Exists(csvPath);

                StringBuilder sb = new StringBuilder();
                if (isNewFile)
                {
                    sb.Append("Time,Mode,ImageSource,AlignScore,");
                    foreach (MeasureResult m in result.Measures)
                    {
                        sb.Append(csvEscape(m.Name)).Append(',');
                    }
                    sb.Append("Judge,ElapsedMs").AppendLine();
                }

                sb.Append(result.Time.ToString("yyyy-MM-dd HH:mm:ss.fff")).Append(',');
                sb.Append(result.IsOfflineFileMode ? "File" : "Camera").Append(',');
                sb.Append(csvEscape(result.ImageSource)).Append(',');
                sb.Append(result.AlignScore.HasValue ? result.AlignScore.Value.ToString("F3", CultureInfo.InvariantCulture) : "").Append(',');
                foreach (MeasureResult m in result.Measures)
                {
                    sb.Append(m.Value.HasValue ? m.Value.Value.ToString("F3", CultureInfo.InvariantCulture) : csvEscape(m.Text)).Append(',');
                }
                sb.Append(result.JudgeText).Append(',').Append(result.ElapsedMs).AppendLine();

                // Excel에서 한글이 깨지지 않도록 BOM 포함 UTF-8
                File.AppendAllText(csvPath, sb.ToString(), new UTF8Encoding(true));
            }
            catch (Exception ex)
            {
                AppLogger.Error("검사 결과 CSV 저장 실패", ex);
            }
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

                CogImageFile imageFile = new CogImageFile();
                imageFile.Open(imagePath, CogImageFileModeConstants.Write);
                imageFile.Append(image);
                imageFile.Close();
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
