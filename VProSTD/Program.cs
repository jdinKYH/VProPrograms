using System;
using System.Threading;
using System.Windows.Forms;

namespace VProSTD
{
    /// <summary>
    /// 프로그램 시작점.
    /// 하는 일: 오류 처리 방법을 정한 뒤 메인 화면(MainForm)을 띄운다.
    /// </summary>
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // 예상하지 못한 오류가 나도 프로그램이 바로 꺼지지 않게, 오류를 아래 두 함수로 보낸다.
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        /// <summary>
        /// 화면(UI) 작업 중 난 오류: 메시지만 보여 주고 프로그램은 계속 실행한다.
        /// </summary>
        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show($"예기치 않은 오류가 발생했습니다:\n{e.Exception.Message}",
                "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// 그 밖의 오류: 복구할 수 없으므로 내용만 알린다 (이후 프로그램이 종료된다).
        /// </summary>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string message = string.Empty;
            Exception ex = e.ExceptionObject as Exception;
            if (ex != null) message = ex.Message;
            else if (e.ExceptionObject != null) message = e.ExceptionObject.ToString();

            MessageBox.Show($"복구할 수 없는 오류가 발생해 프로그램을 종료합니다:\n{message}",
                "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
