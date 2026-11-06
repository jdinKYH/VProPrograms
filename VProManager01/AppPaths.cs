using System;
using System.IO;
using System.Windows.Forms;

// ===============================================================================
//  261029@Claude [검토 보완 8] 설정·결과·로그를 저장할 폴더
//  - 실행 폴더에 쓸 수 있으면 실행 폴더 (기존과 같음)
//  - 쓸 수 없으면(예: Program Files에 설치) %ProgramData%\VProManager01
// ===============================================================================
namespace VProManager01
{
    internal static class AppPaths
    {
        private static string dataDirectory;

        /// <summary>
        /// 설정(InspectionSettings.xml), Results, Logs 가 들어가는 폴더
        /// </summary>
        public static string DataDirectory
        {
            get
            {
                if (dataDirectory == null)
                {
                    dataDirectory = isWritable(Application.StartupPath)
                        ? Application.StartupPath
                        : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "VProManager01");
                }
                return dataDirectory;
            }
        }

        public static string ResultDirectory
        {
            get { return Path.Combine(DataDirectory, "Results"); }
        }

        public static string LogDirectory
        {
            get { return Path.Combine(DataDirectory, "Logs"); }
        }


        /// <summary>
        /// 폴더에 실제로 파일을 만들어 보고 쓰기 가능 여부를 판단
        /// </summary>
        private static bool isWritable(string folder)
        {
            try
            {
                string testPath = Path.Combine(folder, ".write_test_" + Guid.NewGuid().ToString("N"));
                File.WriteAllText(testPath, "");
                File.Delete(testPath);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
