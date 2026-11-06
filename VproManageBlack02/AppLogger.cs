using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

// ===============================================================================
//  261028@Claude [2단계 2-8] 파일 로그
//  실행 파일 폴더의 Logs\yyyyMMdd.log 에 기록한다.
//  예외는 메시지뿐 아니라 InnerException과 스택까지 남긴다.
// ===============================================================================
namespace VproManageBlack02
{
    internal static class AppLogger
    {
        private static readonly object lockObject = new object();

        /// <summary>
        /// 일반 정보 기록
        /// </summary>
        public static void Info(string message)
        {
            write("INFO", message);
        }

        /// <summary>
        /// 예외 기록 (InnerException, 스택 포함)
        /// </summary>
        public static void Error(string context, Exception ex)
        {
            write("ERROR", context + Environment.NewLine + ex);
        }

        /// <summary>
        /// 사용자에게 보여줄 예외 메시지 (가장 안쪽 InnerException까지 포함)
        /// </summary>
        public static string GetFullMessage(Exception ex)
        {
            StringBuilder sb = new StringBuilder(ex.Message);
            Exception inner = ex.InnerException;
            while (inner != null)
            {
                sb.Append(Environment.NewLine).Append("→ ").Append(inner.Message);
                inner = inner.InnerException;
            }
            return sb.ToString();
        }

        private static void write(string level, string message)
        {
            try
            {
                // 261029@Claude [검토 보완 8] 실행 폴더에 쓸 수 없으면 ProgramData 사용
                // string logDir = Path.Combine(Application.StartupPath, "Logs");
                string logDir = AppPaths.LogDirectory;
                Directory.CreateDirectory(logDir);
                string logPath = Path.Combine(logDir, DateTime.Now.ToString("yyyyMMdd") + ".log");
                string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}{Environment.NewLine}";

                lock (lockObject)
                {
                    File.AppendAllText(logPath, line, Encoding.UTF8);
                }
            }
            catch
            {
                // 로그 기록 실패가 검사 프로그램을 멈추게 해서는 안 된다.
            }
        }
    }
}
