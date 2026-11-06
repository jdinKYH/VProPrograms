using System;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

// ===============================================================================
//  261028@Claude [3단계 3-1, 3-2, 3-4] 검사 설정
//  실행 파일 폴더의 InspectionSettings.xml 에 저장한다. (없으면 기본값으로 새로 만든다)
//  - PMAlign 최소 점수 (기존 하드코딩 0.6)
//  - 측정값 4개의 ToolBlock 출력 이름/번호와 공차(Min/Max)
//    261029@Claude [트리 구조 적용] 출력 이름 기본값: leftDist01, leftDist02, rightDist01, rightDist02
//  - 마지막 VPP 경로, 마지막 이미지 폴더, 결과 저장 여부
//  메모장으로 수정한 뒤 다음 검사부터 반영된다. (검사할 때마다 다시 읽음)
//  261029@Claude [구조 변경 3] 기준값(공차 사용·Min·Max, PMAlign 최소 점수)은 Settings 창(SettingsForm)에서
//  입력한다. 나머지 항목(출력 이름·번호 등)은 창의 "XML 파일 열기"로 메모장에서 수정한다.
// ===============================================================================
namespace VProManager02
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

        /// <summary>
        /// 261029@Claude [트리 구조 적용] 출력 이름까지 지정 (이름을 먼저 찾고, 없으면 번호 사용)
        /// </summary>
        public MeasureSpec(string displayName, string outputName, int outputIndex)
            : this(displayName, outputIndex)
        {
            OutputName = outputName;
        }
    }


    public class InspectionSettings
    {
        /// <summary>PMAlign 결과 점수가 이 값보다 작으면 위치 확인 실패 (기존 0.6)</summary>
        public double PMAlignMinScore = 0.6;

        // 261029@Claude [트리 구조 적용] 새 VPP의 LEFTside/RIGHTside 출력 이름을 기본값으로 지정 (기존 코드)
        // 번호는 이름을 찾지 못할 때만 쓰인다. (LEFTside 출력 순서: leftDist01=0, leftDist02=1)
        //public MeasureSpec LeftDist1 = new MeasureSpec("Left Dist1", 0);
        //public MeasureSpec LeftDist2 = new MeasureSpec("Left Dist2", 1);
        //public MeasureSpec RightDist1 = new MeasureSpec("Right Dist1", 0);
        //public MeasureSpec RightDist2 = new MeasureSpec("Right Dist2", 1);
        public MeasureSpec LeftDist1 = new MeasureSpec("Left Dist1", DefaultLeftDist1Output, 0);
        public MeasureSpec LeftDist2 = new MeasureSpec("Left Dist2", DefaultLeftDist2Output, 1);
        public MeasureSpec RightDist1 = new MeasureSpec("Right Dist1", DefaultRightDist1Output, 0);
        public MeasureSpec RightDist2 = new MeasureSpec("Right Dist2", DefaultRightDist2Output, 1);

        // 261029@Claude [트리 구조 적용] VPP 터미널 이름 (트리에 보이는 이름 그대로, 대소문자 구분)
        private const string DefaultLeftDist1Output = "leftDist01";
        private const string DefaultLeftDist2Output = "leftDist02";
        private const string DefaultRightDist1Output = "rightDist01";
        private const string DefaultRightDist2Output = "rightDist02";

        // 261029@Claude [VProManager02] 결과 분석 판정 반영 설정(UseResultsAnalysisDecision) 제거
        // 결과 분석 툴은 화면 Grid 표시에만 쓰고, 판정은 Settings 창의 Min/Max로만 한다.
        // (이전 설정 파일에 이 항목이 남아 있어도 XmlSerializer가 무시하므로 문제없다)

        /// <summary>마지막으로 로드에 성공한 VPP (시작 시 자동 로드)</summary>
        public string LastVppPath = "";

        /// <summary>마지막으로 이미지를 연 폴더</summary>
        public string LastImageFolder = "";

        /// <summary>검사 결과를 Results\yyyyMMdd.csv 에 저장</summary>
        public bool SaveResultCsv = true;

        /// <summary>
        /// 261029@Claude [VProManager02] 카메라 모드 검사 이미지를 OK·NG 모두 Results\Images\yyyyMMdd\ 에 저장
        /// (기존 SaveNgImage: NG만 저장. 파일 모드는 저장하지 않고 원본 경로만 기록)
        /// </summary>
        public bool SaveImage = true;

        /// <summary>261028@Claude [4단계] 회귀 테스트(expected.csv)에서 측정값 비교 허용 오차</summary>
        public double RegressionValueTolerance = 0.01;


        [XmlIgnore]
        public static string FilePath
        {
            // 261029@Claude [검토 보완 8] 실행 폴더에 쓸 수 없으면 ProgramData 사용
            // get { return Path.Combine(Application.StartupPath, "InspectionSettings.xml"); }
            get { return Path.Combine(AppPaths.DataDirectory, "InspectionSettings.xml"); }
        }


        /// <summary>
        /// 261029@Claude [검토 보완 1] 마지막 읽기 오류. 비어 있지 않으면 설정 파일이 잘못된 상태이며,
        /// 사용자가 고친 내용을 덮어쓰지 않도록 Save()가 동작하지 않는다.
        /// </summary>
        [XmlIgnore]
        public static string LastLoadError { get; private set; } = "";


        /// <summary>
        /// 261029@Claude [검토 보완 1] 설정 파일을 읽는다.
        /// - 파일이 없으면 기본값으로 만들고 저장한다.
        /// - 읽기에 실패하면 fallback(직전 설정)을 그대로 쓰고 LastLoadError에 이유를 남긴다.
        ///   (기존: 기본값으로 바꿔 공차 판정이 꺼지고, 이후 Save()로 사용자 설정을 덮어씀)
        /// </summary>
        public static InspectionSettings Load(InspectionSettings fallback = null)
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(InspectionSettings));
                    InspectionSettings loaded;
                    using (FileStream stream = File.OpenRead(FilePath))
                    {
                        loaded = (InspectionSettings)serializer.Deserialize(stream);
                    }
                    loaded.fillMissingSpecs();
                    LastLoadError = "";
                    return loaded;
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error("설정 파일 읽기 실패 (직전 설정 유지, 저장 중지): " + FilePath, ex);
                LastLoadError = AppLogger.GetFullMessage(ex);
                return fallback ?? new InspectionSettings();
            }

            LastLoadError = "";
            InspectionSettings defaults = new InspectionSettings();
            defaults.Save();
            return defaults;
        }

        //public static InspectionSettings Load()
        //{
        //    try
        //    {
        //        if (File.Exists(FilePath))
        //        {
        //            XmlSerializer serializer = new XmlSerializer(typeof(InspectionSettings));
        //            using (FileStream stream = File.OpenRead(FilePath))
        //            {
        //                return (InspectionSettings)serializer.Deserialize(stream);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        AppLogger.Error("설정 파일 읽기 실패 (기본값 사용): " + FilePath, ex);
        //        return new InspectionSettings();
        //    }

        //    InspectionSettings defaults = new InspectionSettings();
        //    defaults.Save();
        //    return defaults;
        //}


        /// <summary>
        /// 261029@Claude [검토 보완 1] 파일에서 측정 항목이 빠졌거나 비어 있으면(nil) 기본값으로 채움
        /// </summary>
        private void fillMissingSpecs()
        {
            // 빈 항목(<LeftDist1/> 또는 xsi:nil)은 null이 아니라 이름이 빈 항목으로 읽히므로 이름도 채운다.
            //if (LeftDist1 == null) LeftDist1 = new MeasureSpec("Left Dist1", 0);
            //if (LeftDist2 == null) LeftDist2 = new MeasureSpec("Left Dist2", 1);
            //if (RightDist1 == null) RightDist1 = new MeasureSpec("Right Dist1", 0);
            //if (RightDist2 == null) RightDist2 = new MeasureSpec("Right Dist2", 1);
            // 261029@Claude [트리 구조 적용] 출력 이름 기본값도 함께 채운다. (기존 코드)
            //LeftDist1 = fillSpec(LeftDist1, "Left Dist1", 0);
            //LeftDist2 = fillSpec(LeftDist2, "Left Dist2", 1);
            //RightDist1 = fillSpec(RightDist1, "Right Dist1", 0);
            //RightDist2 = fillSpec(RightDist2, "Right Dist2", 1);
            LeftDist1 = fillSpec(LeftDist1, "Left Dist1", DefaultLeftDist1Output, 0);
            LeftDist2 = fillSpec(LeftDist2, "Left Dist2", DefaultLeftDist2Output, 1);
            RightDist1 = fillSpec(RightDist1, "Right Dist1", DefaultRightDist1Output, 0);
            RightDist2 = fillSpec(RightDist2, "Right Dist2", DefaultRightDist2Output, 1);
        }


        /// <summary>
        /// 측정 항목 하나의 빈 값을 기본값으로 채움
        /// 261029@Claude [트리 구조 적용] OutputName이 비어 있으면 새 VPP 출력 이름으로 채운다.
        /// 기존 설정 파일(OutputName 빈 값, 번호 0/1 사용)도 그대로 동작하지만, 이름으로 읽는 것이
        /// VPP에서 출력 순서를 바꿨을 때 다른 값을 읽는 실수를 막아 준다.
        /// 사용자가 직접 적은 OutputName은 바꾸지 않는다.
        /// </summary>
        private static MeasureSpec fillSpec(MeasureSpec spec, string defaultName, string defaultOutputName, int defaultIndex)
        {
            if (spec == null)
            {
                return new MeasureSpec(defaultName, defaultOutputName, defaultIndex);
            }
            if (string.IsNullOrWhiteSpace(spec.DisplayName))
            {
                spec.DisplayName = defaultName;
            }
            if (string.IsNullOrWhiteSpace(spec.OutputName))
            {
                spec.OutputName = defaultOutputName;
            }
            return spec;
        }


        /// <summary>
        /// 261029@Claude [검토 보완 9] 설정 경고 (상태 표시줄에 표시)
        /// - 공차를 쓰지 않는 측정 항목 (값만 읽히면 OK 판정)
        /// - Min이 Max보다 큰 측정 항목 (항상 NG 판정)
        /// </summary>
        public string GetWarningText()
        {
            MeasureSpec[] specs = { LeftDist1, LeftDist2, RightDist1, RightDist2 };
            string noTolerance = string.Join(", ", Array.ConvertAll(Array.FindAll(specs, s => !s.UseTolerance), s => s.DisplayName));
            string invalidRange = string.Join(", ", Array.ConvertAll(Array.FindAll(specs, s => s.UseTolerance && s.Min > s.Max), s => s.DisplayName));

            string warning = "";
            if (noTolerance.Length > 0)
            {
                warning += "공차 미설정: " + noTolerance;
            }
            if (invalidRange.Length > 0)
            {
                warning += (warning.Length > 0 ? " / " : "") + "Min>Max: " + invalidRange;
            }
            return warning;
        }


        public void Save()
        {
            // 261029@Claude [검토 보완 1] 설정 파일이 잘못된 상태면 사용자가 고친 내용을 덮어쓰지 않는다.
            if (!string.IsNullOrEmpty(LastLoadError))
            {
                AppLogger.Info("설정 파일 오류 상태라 저장하지 않음: " + FilePath);
                return;
            }

            try
            {
                // 261029@Claude [검토 보완 8] 저장 폴더가 없으면 만든다 (ProgramData 사용 시)
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
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
