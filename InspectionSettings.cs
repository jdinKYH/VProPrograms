using System;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

// ===============================================================================
//  261028@Claude [3단계 3-1, 3-2, 3-4] 검사 설정
//  실행 파일 폴더의 InspectionSettings.xml 에 저장한다. (없으면 기본값으로 새로 만든다)
//  - PMAlign 최소 점수 (기존 하드코딩 0.6)
//  - 측정값 4개의 ToolBlock 출력 이름/번호와 공차(Min/Max)
//  - 마지막 VPP 경로, 마지막 이미지 폴더, 결과 저장 여부
//  메모장으로 수정한 뒤 다음 검사부터 반영된다. (검사할 때마다 다시 읽음)
// ===============================================================================
namespace VproManageBlack02
{
    /// <summary>
    /// 측정값 하나의 출력 위치와 공차
    /// </summary>
    public class MeasureSpec
    {
        /// <summary>화면·CSV에 표시할 이름</summary>
        public string DisplayName = "";

        /// <summary>ToolBlock 출력 이름. 비어 있거나 찾지 못하면 OutputIndex를 사용</summary>
        public string OutputName = "";

        /// <summary>ToolBlock 출력 번호 (기존 방식)</summary>
        public int OutputIndex = 0;

        /// <summary>true일 때만 Min/Max로 OK/NG 판정</summary>
        public bool UseTolerance = false;

        public double Min = 0.0;
        public double Max = 0.0;

        public MeasureSpec() { }

        public MeasureSpec(string displayName, int outputIndex)
        {
            DisplayName = displayName;
            OutputIndex = outputIndex;
        }
    }


    public class InspectionSettings
    {
        /// <summary>PMAlign 결과 점수가 이 값보다 작으면 위치 확인 실패 (기존 0.6)</summary>
        public double PMAlignMinScore = 0.6;

        public MeasureSpec LeftDist1 = new MeasureSpec("Left Dist1", 0);
        public MeasureSpec LeftDist2 = new MeasureSpec("Left Dist2", 1);
        public MeasureSpec RightDist1 = new MeasureSpec("Right Dist1", 0);
        public MeasureSpec RightDist2 = new MeasureSpec("Right Dist2", 1);

        /// <summary>마지막으로 로드에 성공한 VPP (시작 시 자동 로드)</summary>
        public string LastVppPath = "";

        /// <summary>마지막으로 이미지를 연 폴더</summary>
        public string LastImageFolder = "";

        /// <summary>검사 결과를 Results\yyyyMMdd.csv 에 저장</summary>
        public bool SaveResultCsv = true;

        /// <summary>NG 이미지를 Results\NG\yyyyMMdd\ 에 저장</summary>
        public bool SaveNgImage = true;

        /// <summary>261028@Claude [4단계] 회귀 테스트(expected.csv)에서 측정값 비교 허용 오차</summary>
        public double RegressionValueTolerance = 0.01;


        [XmlIgnore]
        public static string FilePath
        {
            get { return Path.Combine(Application.StartupPath, "InspectionSettings.xml"); }
        }


        /// <summary>
        /// 설정 파일을 읽는다. 없으면 기본값으로 만들고 저장한다. 읽기에 실패하면 기본값을 사용한다.
        /// </summary>
        public static InspectionSettings Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(InspectionSettings));
                    using (FileStream stream = File.OpenRead(FilePath))
                    {
                        return (InspectionSettings)serializer.Deserialize(stream);
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error("설정 파일 읽기 실패 (기본값 사용): " + FilePath, ex);
                return new InspectionSettings();
            }

            InspectionSettings defaults = new InspectionSettings();
            defaults.Save();
            return defaults;
        }


        public void Save()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(InspectionSettings));
                using (FileStream stream = File.Create(FilePath))
                {
                    serializer.Serialize(stream, this);
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error("설정 파일 저장 실패: " + FilePath, ex);
            }
        }
    }
}
