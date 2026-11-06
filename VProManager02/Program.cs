using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VProManager02
{
    internal static class Program
    {
        /// <summary>
        /// 해당 애플리케이션의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 261028@Claude [2단계 2-8] 전역 예외 처리
            // 처리되지 않은 예외가 나도 앱이 바로 종료되지 않도록 로그를 남기고 사용자에게 안내한다.
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppLogger.Info("프로그램 시작");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

            AppLogger.Info("프로그램 종료");
        }

        /// <summary>
        /// 261028@Claude [2단계 2-8] UI 스레드에서 처리되지 않은 예외
        /// </summary>
        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            AppLogger.Error("처리되지 않은 예외 (UI 스레드)", e.Exception);
            MessageBox.Show($"예상하지 못한 오류가 발생했습니다.\n{AppLogger.GetFullMessage(e.Exception)}\n\n자세한 내용은 Logs 폴더의 로그를 확인하세요.",
                "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// 261028@Claude [2단계 2-8] UI 외 스레드에서 처리되지 않은 예외 (이 경우 앱 종료는 막을 수 없으므로 기록만 한다)
        /// </summary>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            if (ex != null)
            {
                AppLogger.Error("처리되지 않은 예외 (백그라운드 스레드)", ex);
            }
            else
            {
                AppLogger.Info("처리되지 않은 예외 (백그라운드 스레드): " + e.ExceptionObject);
            }
        }
    }
}
