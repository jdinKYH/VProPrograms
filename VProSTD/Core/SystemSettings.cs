using System;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace VProSTD
{
    /// <summary>
    /// 프로그램 공통 설정 (SYSTEM 탭).
    /// 파일 위치는 실행 파일 옆 system.xml 로 고정한다.
    /// (System Path 안에 두면, 경로를 읽기 전에 그 경로를 알아야 하는 문제가 생김)
    /// </summary>
    public class SystemSettings
    {
        public double PixelX = 1.0;          // mm/pixel (ToolBlock 에 PixelX 입력이 있으면 넣어 줌)
        public double PixelY = 1.0;

        public string BasePath = "";
        public string SystemPath = "";
        public string RecipePath = "";
        public string LogPath = "";
        public string ResultPath = "";

        public bool SaveAsJpg = false;       // false = BMP, true = JPG
        public bool OriginSave = true;       // 원본 이미지 저장
        public bool ResultSave = true;       // 검사 그래픽이 그려진 화면 저장

        public string LastRecipe = "";       // 마지막으로 불러온 레시피 (다음 실행 때 자동으로 불러옴)

        /// <summary>
        /// 설정 파일 위치: 실행 파일 옆 system.xml
        /// </summary>
        public static string FilePath
        {
            get { return Path.Combine(Application.StartupPath, "system.xml"); }
        }

        /// <summary>
        /// 파일에서 읽는다. 파일이 없으면 기본값(실행 파일 옆 Data 폴더)을 만든다.
        /// </summary>
        public static SystemSettings Load()
        {
            SystemSettings settings = null;

            if (File.Exists(FilePath))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(SystemSettings));
                using (FileStream stream = File.OpenRead(FilePath))
                {
                    settings = (SystemSettings)serializer.Deserialize(stream);
                }
            }

            if (settings == null) settings = new SystemSettings();
            settings.FillEmptyPaths();
            return settings;
        }

        public void Save()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SystemSettings));
            using (FileStream stream = File.Create(FilePath))
            {
                serializer.Serialize(stream, this);
            }
        }

        /// <summary>
        /// 비어 있는 경로를 기본값으로 채우고, 폴더가 없으면 만든다.
        /// </summary>
        public void FillEmptyPaths()
        {
            if (string.IsNullOrWhiteSpace(BasePath)) BasePath = Path.Combine(Application.StartupPath, "Data");
            if (string.IsNullOrWhiteSpace(SystemPath)) SystemPath = Path.Combine(BasePath, "System");
            if (string.IsNullOrWhiteSpace(RecipePath)) RecipePath = Path.Combine(BasePath, "Recipe");
            if (string.IsNullOrWhiteSpace(LogPath)) LogPath = Path.Combine(BasePath, "Log");
            if (string.IsNullOrWhiteSpace(ResultPath)) ResultPath = Path.Combine(BasePath, "Result");

            CreateFolder(SystemPath);
            CreateFolder(RecipePath);
            CreateFolder(LogPath);
            CreateFolder(ResultPath);
        }

        private static void CreateFolder(string path)
        {
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception)
            {
                // 만들 수 없는 경로(없는 드라이브 등)는 사용할 때 오류로 알린다
            }
        }
    }
}
