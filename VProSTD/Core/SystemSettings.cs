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

        public CameraSettings Camera = new CameraSettings();   // CONNECT CAMERA 탭 SAVE 로 저장

        /// <summary>
        /// 설정 파일 위치: 실행 파일 옆 system.xml
        /// </summary>
        public static string FilePath
        {
            get { return Path.Combine(Application.StartupPath, "system.xml"); }
        }

        /// <summary>
        /// 실행 파일 옆 system.xml 을 읽는다. 파일이 없으면 기본값(실행 파일 옆 Data 폴더)을 만든다.
        /// </summary>
        public static SystemSettings Load()
        {
            return LoadFrom(FilePath);
        }

        /// <summary>
        /// 지정한 설정 파일을 읽는다 (SYSTEM 탭 LOAD). 파일이 없으면 기본값.
        /// </summary>
        public static SystemSettings LoadFrom(string path)
        {
            SystemSettings settings = null;

            if (File.Exists(path))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(SystemSettings));
                using (FileStream stream = File.OpenRead(path))
                {
                    settings = (SystemSettings)serializer.Deserialize(stream);
                }
            }

            if (settings == null) settings = new SystemSettings();
            if (settings.Camera == null) settings.Camera = new CameraSettings();
            settings.FillEmptyPaths();
            return settings;
        }

        /// <summary>
        /// 실행 파일 옆 system.xml 에 저장 (다음 실행 때 이 값을 씀)
        /// </summary>
        public void Save()
        {
            SaveTo(FilePath);
        }

        /// <summary>
        /// 지정한 파일에 저장 (SYSTEM 탭 SAVE 에서 고른 파일)
        /// </summary>
        public void SaveTo(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SystemSettings));
            using (FileStream stream = File.Create(path))
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

    /// <summary>
    /// 카메라 연결 설정 (레시피와 무관, 장비마다 하나).
    /// .vpp(CogAcqFifoTool) 를 쓰지 않고 이 값으로 코드에서 직접 카메라를 연다.
    /// 노출은 제품마다 다르므로 여기 두지 않고 레시피 설정(RecipeSettings.ExposureMs)에 둔다.
    /// </summary>
    public class CameraSettings
    {
        public string Type = CameraTypes.CognexGigE;   // 카메라 종류 (나중에 Euresys CameraLink 추가)
        public string Serial = "";                     // 카메라 시리얼. 비우면 첫 번째 카메라
        public string VideoFormat = "";                // 비우면 카메라의 첫 번째 영상 형식
        public int TimeoutMs = 2000;                   // 1장 취득 제한 시간
    }

    public static class CameraTypes
    {
        public const string CognexGigE = "CognexGigE";
    }
}
