using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace VProSTD
{
    /// <summary>
    /// 기록 파일 쓰기
    ///   <LogPath>/yyyyMMdd_log.txt     : 화면 Log 와 같은 내용
    ///   <LogPath>/yyyyMMdd_result.csv  : 검사 결과 1회 = 1줄
    ///   <ResultPath>/yyyyMMdd/OK|NG/HHmmss_fff_Origin.bmp / _Result.bmp : 이미지
    /// 기록 실패(디스크 부족 등)는 검사를 멈추지 않도록 오류 메시지만 돌려준다.
    /// </summary>
    public class ResultLogger
    {
        private readonly SystemSettings _settings;
        private string _lastCsvHeader = "";   // 항목이 바뀌면 머리글을 다시 쓰기 위해 기억

        public ResultLogger(SystemSettings settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// Log 한 줄을 파일에 추가. 실패하면 오류 메시지, 성공하면 null.
        /// </summary>
        public string WriteLog(string line)
        {
            try
            {
                string path = Path.Combine(_settings.LogPath, DateTime.Now.ToString("yyyyMMdd") + "_log.txt");
                File.AppendAllText(path, line + Environment.NewLine, Encoding.UTF8);
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// 검사 결과 1줄을 CSV 에 추가. 실패하면 오류 메시지, 성공하면 null.
        /// </summary>
        public string WriteResult(InspectionResult result)
        {
            try
            {
                string path = Path.Combine(_settings.LogPath, DateTime.Now.ToString("yyyyMMdd") + "_result.csv");

                string header = "Time,Source,Judge,TimeMs," + string.Join(",", result.OutputNames);
                StringBuilder text = new StringBuilder();

                // 오류 결과(출력 값 없음)는 머리글을 바꾸지 않고 앞 4칸만 쓴다
                bool noOutputs = result.OutputNames.Count == 0 && _lastCsvHeader.Length > 0;

                // 파일이 새로 생기거나 항목이 바뀌면 머리글을 한 줄 쓴다
                if (!File.Exists(path) || (!noOutputs && header != _lastCsvHeader))
                {
                    text.AppendLine(header);
                    _lastCsvHeader = header;
                }

                text.Append(DateTime.Now.ToString("HH:mm:ss.fff")).Append(',');
                text.Append(Csv(result.Source)).Append(',');
                text.Append(result.JudgeText).Append(',');
                text.Append(result.TimeMs.ToString("0.0"));
                foreach (string value in result.OutputValues)
                {
                    text.Append(',').Append(Csv(value));
                }
                text.AppendLine();

                File.AppendAllText(path, text.ToString(), Encoding.UTF8);
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// 이미지 저장 (SYSTEM 탭의 Origin Save / Result Save / BMP·JPG 설정을 따름).
        /// resultBitmap: 검사 그래픽이 그려진 화면 (없으면 null).
        /// 실패하면 오류 메시지, 성공(또는 저장 안 함)이면 null.
        /// </summary>
        public string SaveImages(InspectionResult result, Image resultBitmap)
        {
            if (!_settings.OriginSave && !_settings.ResultSave) return null;

            try
            {
                string judgeFolder = "NG";
                if (result.Ok) judgeFolder = "OK";

                string folder = Path.Combine(_settings.ResultPath, DateTime.Now.ToString("yyyyMMdd"), judgeFolder);
                Directory.CreateDirectory(folder);

                string baseName = DateTime.Now.ToString("HHmmss_fff");
                string ext = ".bmp";
                ImageFormat format = ImageFormat.Bmp;
                if (_settings.SaveAsJpg)
                {
                    ext = ".jpg";
                    format = ImageFormat.Jpeg;
                }

                if (_settings.OriginSave && result.Image != null)
                {
                    using (Bitmap origin = result.Image.ToBitmap())
                    {
                        origin.Save(Path.Combine(folder, baseName + "_Origin" + ext), format);
                    }
                }

                if (_settings.ResultSave && resultBitmap != null)
                {
                    resultBitmap.Save(Path.Combine(folder, baseName + "_Result" + ext), format);
                }
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// CSV 칸 안에 쉼표/따옴표가 있으면 따옴표로 감싼다.
        /// </summary>
        private static string Csv(string value)
        {
            if (value == null) return "";
            if (value.IndexOfAny(new[] { ',', '"', '\n' }) < 0) return value;
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
    }
}
