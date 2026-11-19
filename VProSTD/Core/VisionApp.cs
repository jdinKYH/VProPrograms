using Cognex.VisionPro;
using Cognex.VisionPro.ToolBlock;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace VProSTD
{
    /// <summary>
    /// 프로그램의 중심: 설정 / 레시피 / 이미지 입력 / 검사 / 카운트 / 기록을 한곳에서 관리한다.
    /// 화면(Page)들은 이 객체 하나를 같이 쓴다. 이 파일에는 화면 코드가 없다.
    ///
    /// 검사 1회 흐름 (InspectOnce)
    ///   이미지 얻기 (File Mode: 파일 / 아니면: 카메라 취득)
    ///   → ToolBlock.Inputs["InputImage"] 에 넣고 Run()
    ///   → CogResultsAnalysisTool 판정 (없으면 Outputs["Result"]) 으로 OK/NG, 카운트, Log, CSV 기록
    /// </summary>
    public class VisionApp
    {
        public SystemSettings System { get; private set; }
        public ResultLogger Logger { get; private set; }

        // 현재 레시피 (불러오지 않았으면 RecipeName 이 null)
        public string RecipeName { get; private set; }
        public string RecipeFilePath { get; private set; }            // <Recipe Path>\<이름>.vpp
        public RecipeSettings Recipe { get; private set; }
        public JobRunner Job { get; private set; }

        // 이미지 입력 (카메라는 레시피와 따로 관리한다. 레시피를 바꿔도 연결은 그대로)
        public ICameraSource Camera { get; private set; }             // CONNECT 전에는 null
        public FileImageSource Files { get; private set; }

        // 카운트 (오류는 NG 로 센다)
        public int TotalCount { get; private set; }
        public int OkCount { get; private set; }
        public int NgCount { get; private set; }

        // RUN 탭에서 AUTO RUN 중인지 (중에는 탭 이동, 레시피 변경, 종료를 막는다)
        public bool IsAutoRunning;

        // 카메라 설정이 들어 있는 파일을 한 번이라도 불러왔는지 → 종료 시 카메라 해제
        private bool _cameraUsed;

        // ----- 화면에 알리는 이벤트 -----
        public event Action<string, LogKind> LogAdded;     // Log 한 줄
        public event Action RecipeChanged;                 // 레시피를 불러옴 / 닫음
        public event Action RecipeUnloading;               // 레시피를 닫기 직전 (편집 화면 연결을 먼저 끊기 위해)
        public event Action CameraChanged;                 // 카메라 연결 상태 바뀜
        public event Action SystemChanged;                 // SYSTEM 탭 설정 저장 (경로가 바뀌었을 수 있음)

        // 예/아니오 확인 창 (MainForm 이 MessageBox 로 연결해 준다)
        public Func<string, bool> Confirm = message => true;
        public Action<string> Warn = message => { };      // 알림 창 (확인 버튼만). 화면에서 MessageBox 로 연결한다

        public VisionApp()
        {
            System = SystemSettings.Load();
            Logger = new ResultLogger(System);
            Recipe = new RecipeSettings();
            Files = new FileImageSource();
        }

        public bool FileMode
        {
            get { return Recipe.FileMode; }
            set { Recipe.FileMode = value; }
        }

        public bool IsCameraConnected
        {
            get { return Camera != null && Camera.IsConnected; }
        }

        /// <summary>
        /// Log 한 줄: 화면에 보내고 파일에도 쓴다.
        /// </summary>
        public void WriteLog(string text, LogKind kind = LogKind.Info)
        {
            if (LogAdded != null) LogAdded(text, kind);

            string error = Logger.WriteLog(DateTime.Now.ToString("HH:mm:ss.fff") + "  " + text);
            if (error != null && LogAdded != null) LogAdded("Log 파일 쓰기 실패: " + error, LogKind.Warning);
        }

        #region 레시피

        /// <summary>
        /// 레시피 불러오기. 성공하면 true.
        /// [안전] 바이트 확인 → 경고 → 불러오기 순서 (경고 전에 카메라·I/O 에 연결되지 않게)
        /// </summary>
        public bool LoadRecipe(string name)
        {
            string filePath = RecipeManager.FileOf(System.RecipePath, name);
            string fileName = Path.GetFileName(filePath);

            // ① 바이트로만 확인 (VisionPro 로 열지 않음)
            VppFileInfo info = VppInspector.Inspect(filePath);
            if (info.Kind != VppKind.ToolBlock && info.Kind != VppKind.Unknown)
            {
                WriteLog(fileName + " 이(가) ToolBlock 파일이 아닙니다: " + info.RootTypeName, LogKind.Ng);
                return false;
            }

            // ② 카메라 / I/O 설정이 들어 있으면 경고 (설계상 레시피에는 취득이 없어야 함)
            if (info.HasAcqFifo || info.HasIOSettings)
            {
                string message = fileName + " 에 카메라(AcqFifo) 또는 I/O 설정이 들어 있습니다.\n" +
                                 "불러오면 카메라·I/O 에 연결을 시도합니다.\n" +
                                 "(VProSTD 는 카메라를 CONNECT CAMERA 탭에서 따로 연결하는 것이 원칙입니다.\n" +
                                 " QuickBuild 에서 ToolBlock 안의 AcqFifo 를 지우고 다시 저장하세요)\n\n계속 불러올까요?";
                if (!Confirm(message)) return false;
                _cameraUsed = true;
            }

            // ③ 이전 레시피 닫기 (카메라는 레시피와 따로이므로 연결을 유지한다)
            UnloadRecipe();

            // ④ 불러오기
            object obj = VppFile.Load(filePath);
            CogToolBlock toolBlock = obj as CogToolBlock;
            if (toolBlock == null)
            {
                VppFile.Release(obj);
                WriteLog(fileName + " 을(를) ToolBlock 으로 불러올 수 없습니다.", LogKind.Ng);
                return false;
            }

            Job = new JobRunner(toolBlock, filePath);
            RecipeName = name;
            RecipeFilePath = filePath;
            Recipe = RecipeManager.LoadSettings(RecipeManager.SettingsFileOf(System.RecipePath, name));
            RestoreImageSource();
            ApplyRecipeExposure();

            System.LastRecipe = name;
            SaveSystemQuietly();

            WriteLog("RECIPE: '" + name + "' 불러옴");
            List<string> problems = Job.CheckContract(Recipe.ResultsAnalysisToolName);
            foreach (string problem in problems)
            {
                WriteLog("RECIPE: " + problem, LogKind.Warning);
            }
            foreach (string error in GetScriptErrors(toolBlock))
            {
                WriteLog("RECIPE: 스크립트 오류 - " + error, LogKind.Ng);
            }

            if (RecipeChanged != null) RecipeChanged();

            // 레시피는 불러온 상태로 두고, 검사에 필요한 단자가 없다는 것만 알린다
            if (problems.Count > 0)
            {
                string message = "레시피 '" + name + "' 에 검사에 필요한 항목이 없습니다.\n\n" +
                                 string.Join("\n", problems.ToArray()) + "\n\n" +
                                 "이대로는 검사 결과가 ERROR(NG) 로 나옵니다.\n" +
                                 "RECIPE 탭에서 ToolBlock 에 Inputs \"" + JobRunner.InputImageName + "\" (이미지) 와 " +
                                 "판정용 CogResultsAnalysisTool 을 추가하고 SAVE 하세요.";
                Warn(message);
            }
            return true;
        }

        /// <summary>
        /// 현재 레시피 닫기
        /// [안전] 정리 순서: 편집 화면 연결 끊기(RecipeUnloading) → ToolBlock 정리
        /// </summary>
        public void UnloadRecipe()
        {
            if (Job == null) return;

            if (RecipeUnloading != null) RecipeUnloading();
            Job.Dispose();
            Job = null;
            RecipeName = null;
            RecipeFilePath = null;
            Recipe = new RecipeSettings();

            if (RecipeChanged != null) RecipeChanged();
        }

        /// <summary>
        /// 현재 레시피 저장: <이름>.vpp + <이름>.recipe.xml (노출 포함)
        /// 저장할 때마다 이전 .vpp 가 .bak 으로 1개 남는다.
        /// </summary>
        public void SaveRecipe()
        {
            if (Job == null) throw new InvalidOperationException("불러온 레시피가 없습니다.");

            VppFile.Save(Job.ToolBlock, Job.FilePath, false);

            Recipe.LastImagePath = Files.SourcePath ?? "";
            RecipeManager.SaveSettings(RecipeManager.SettingsFileOf(System.RecipePath, RecipeName), Recipe);

            WriteLog("RECIPE: '" + RecipeName + "' 저장");
        }

        /// <summary>
        /// 다른 이름으로 저장: Recipe Path 에 <name>.vpp 로 저장하고, 그 레시피를 현재 레시피로 바꾼다.
        /// 같은 이름의 파일이 있으면 .bak 을 남기고 덮어쓴다.
        /// </summary>
        public void SaveRecipeAs(string name)
        {
            if (Job == null) throw new InvalidOperationException("불러온 레시피가 없습니다.");

            Directory.CreateDirectory(System.RecipePath);

            string oldName = RecipeName;
            string oldFilePath = RecipeFilePath;
            string oldJobPath = Job.FilePath;

            Job.FilePath = RecipeManager.FileOf(System.RecipePath, name);
            RecipeName = name;
            RecipeFilePath = Job.FilePath;
            try
            {
                SaveRecipe();
            }
            catch
            {
                // 저장에 실패하면 현재 레시피를 원래 이름으로 되돌린다
                Job.FilePath = oldJobPath;
                RecipeName = oldName;
                RecipeFilePath = oldFilePath;
                throw;
            }

            System.LastRecipe = name;
            SaveSystemQuietly();

            WriteLog("RECIPE: '" + oldName + "' → '" + name + "' 다른 이름으로 저장");
            if (RecipeChanged != null) RecipeChanged();
        }

        /// <summary>
        /// 레시피에 기억된 마지막 이미지 파일/폴더를 다시 고른다.
        /// 레시피에 기억된 것이 없거나 없어졌으면, 지금 골라 둔 이미지를 그대로 둔다
        /// (레시피를 불러오기 전에 Single Image / Image Folder 를 골랐어도 지워지지 않게).
        /// </summary>
        private void RestoreImageSource()
        {
            string path = Recipe.LastImagePath;
            if (string.IsNullOrEmpty(path)) return;

            if (Directory.Exists(path))
            {
                Files = new FileImageSource();
                Files.SetFolder(path);
            }
            else if (File.Exists(path))
            {
                Files = new FileImageSource();
                Files.SetSingleFile(path);
            }
        }

        /// <summary>
        /// 스크립트 컴파일 오류 (VisionPro 는 불러오기 오류 대신 ScriptError 에만 남긴다).
        /// 정상 기록은 '-' 로 시작하므로, '-' 로 시작하지 않는 줄이 있으면 오류로 본다.
        /// </summary>
        private static List<string> GetScriptErrors(CogToolBlock toolBlock)
        {
            List<string> errors = new List<string>();
            string log = toolBlock.ScriptError ?? "";
            foreach (string raw in log.Split('\n'))
            {
                string line = raw.Trim();
                if (line.Length > 0 && !line.StartsWith("-")) errors.Add(line);
            }
            return errors;
        }

        #endregion

        #region 카메라

        /// <summary>
        /// 연결된 카메라 목록 (CONNECT CAMERA 탭 SEARCH).
        /// [안전] 카메라를 검색하는 것만으로 카메라 드라이버가 열리므로 종료 시 해제 대상으로 표시한다.
        /// </summary>
        public List<CameraInfo> FindCameras()
        {
            _cameraUsed = true;
            return CognexCameraSource.FindCameras();
        }

        /// <summary>
        /// 카메라 연결 (레시피와 무관). settings 가 null 이면 system.xml 에 저장된 카메라 설정을 쓴다.
        /// 연결되면 현재 레시피의 노출을 적용한다.
        /// </summary>
        public void ConnectCamera(CameraSettings settings)
        {
            if (settings == null) settings = System.Camera;

            string message = "카메라에 연결합니다.\n\n" +
                             "장비 PC 에서는 QuickBuild / 생산 프로그램을 먼저 종료하세요.\n" +
                             "(같은 카메라를 두 프로그램이 동시에 쓰면 연결이 끊어질 수 있습니다)\n" +
                             "※ 카메라는 이 프로그램을 종료해야 해제됩니다.\n\n계속할까요?";
            if (!Confirm(message)) return;

            DisconnectCamera();

            _cameraUsed = true;   // 여는 순간 연결되므로 먼저 표시 (종료 시 해제)
            Camera = OpenCamera(settings);
            WriteLog("CAMERA: 연결 - " + Camera.CameraName + " / " + Camera.VideoFormat);

            ApplyRecipeExposure();
            if (CameraChanged != null) CameraChanged();
        }

        /// <summary>
        /// 카메라 종류에 맞는 입력을 만든다. (Euresys CameraLink 는 나중에 여기에 추가)
        /// </summary>
        private static ICameraSource OpenCamera(CameraSettings settings)
        {
            string type = string.IsNullOrEmpty(settings.Type) ? CameraTypes.CognexGigE : settings.Type;
            if (type == CameraTypes.CognexGigE) return CognexCameraSource.Open(settings);
            throw new NotSupportedException("지원하지 않는 카메라 종류입니다: " + type);
        }

        /// <summary>
        /// 지금 연결된 카메라(시리얼, 영상 형식)를 system.xml 에 저장한다. 다음 CONNECT 때 이 카메라를 연다.
        /// </summary>
        public void SaveCameraSettings()
        {
            if (Camera == null) throw new InvalidOperationException("연결된 카메라가 없습니다. CONNECT 를 먼저 누르세요.");

            System.Camera.Serial = Camera.Serial;
            System.Camera.VideoFormat = Camera.VideoFormat;
            System.Save();
            WriteLog("CAMERA: 설정 저장 - " + Camera.CameraName + " / " + Camera.VideoFormat);
        }

        /// <summary>
        /// 카메라 정리. 카메라 자체는 프로그램 종료 때 해제된다.
        /// </summary>
        public void DisconnectCamera()
        {
            if (Camera == null) return;
            Camera.Dispose();
            Camera = null;
            if (CameraChanged != null) CameraChanged();
        }

        /// <summary>
        /// 노출 변경: 카메라에 바로 적용하고, 레시피를 불러왔으면 레시피 노출로 기억한다 (레시피 SAVE 때 저장).
        /// </summary>
        public void SetExposure(double exposureMs)
        {
            if (Camera == null) return;
            Camera.Exposure = exposureMs;
            if (Job != null) Recipe.ExposureMs = exposureMs;
        }

        /// <summary>
        /// 레시피에 저장된 노출을 카메라에 적용한다 (레시피 불러오기 / 카메라 연결 때).
        /// </summary>
        private void ApplyRecipeExposure()
        {
            if (Camera == null || Recipe.ExposureMs <= 0) return;
            if (Camera.Exposure == null)
            {
                WriteLog("CAMERA: 이 카메라는 노출을 바꿀 수 없습니다 (레시피 노출 " + Recipe.ExposureMs + " ms 무시).", LogKind.Warning);
                return;
            }

            try
            {
                Camera.Exposure = Recipe.ExposureMs;
                WriteLog("CAMERA: 레시피 노출 적용 - " + Recipe.ExposureMs + " ms");
            }
            catch (Exception ex)
            {
                WriteLog("CAMERA: 레시피 노출 적용 실패 - " + ex.Message, LogKind.Warning);
            }
            if (CameraChanged != null) CameraChanged();
        }

        /// <summary>
        /// TRIGGER: 1장만 취득 (검사는 하지 않음)
        /// </summary>
        public ICogImage GrabOnly()
        {
            if (Camera == null) throw new InvalidOperationException("CONNECT 를 먼저 누르세요.");
            return Camera.Grab();
        }

        /// <summary>
        /// 고른 이미지 / TRIGGER 이미지를 레시피 ToolBlock 입력에 넣는다 (검사는 하지 않음).
        /// RECIPE 탭 편집 화면에서 RUN 없이 바로 실행해 볼 수 있게 하기 위해.
        /// 레시피가 없거나 InputImage 입력이 없으면 아무것도 하지 않는다.
        /// </summary>
        public void SetRecipeImage(ICogImage image)
        {
            if (Job == null || image == null) return;
            if (!Job.ToolBlock.Inputs.Contains(JobRunner.InputImageName)) return;

            try
            {
                Job.SetInputImage(image, System);
            }
            catch (Exception ex)
            {
                WriteLog("RECIPE: 입력 이미지를 넣을 수 없습니다 - " + ex.Message, LogKind.Warning);
            }
        }

        #endregion

        #region 검사

        /// <summary>
        /// 검사 1회: 이미지 얻기 → ToolBlock 실행 → 카운트 / Log / CSV
        /// (이미지 저장은 화면 그림이 필요하므로 RUN 화면이 SaveImages 를 부른다)
        /// </summary>
        public InspectionResult InspectOnce()
        {
            if (Job == null) throw new InvalidOperationException("레시피를 먼저 불러오세요.");

            InspectionResult result;
            string source;
            ICogImage image = null;
            string imageError = null;

            // 1) 이미지 얻기
            try
            {
                if (FileMode)
                {
                    image = Files.Next(out source);
                }
                else
                {
                    if (Camera == null) throw new InvalidOperationException("카메라가 연결되어 있지 않습니다. CONNECT CAMERA 탭에서 CONNECT 하거나 File Mode 를 켜세요.");
                    source = "CAMERA";
                    image = Camera.Grab();
                }
            }
            catch (Exception ex)
            {
                source = FileMode ? "FILE" : "CAMERA";
                imageError = ex.Message;
            }

            // 2) 검사
            if (imageError != null)
            {
                result = new InspectionResult();
                result.IsError = true;
                result.Message = imageError;
            }
            else
            {
                result = Job.Run(image, System, Recipe);
            }
            result.Source = source;

            // 3) 카운트 (오류는 NG)
            TotalCount++;
            if (result.Ok) OkCount++;
            else NgCount++;

            // 4) Log / CSV
            string text = "RUN: " + result.JudgeText + "  " + source + "  (" + result.TimeMs.ToString("0.0") + " ms)";
            if (result.Message.Length > 0) text += "  " + result.Message;

            LogKind kind = LogKind.Ng;
            if (result.Ok) kind = LogKind.Ok;
            WriteLog(text, kind);

            string csvError = Logger.WriteResult(result);
            if (csvError != null) WriteLog("결과 기록 실패: " + csvError, LogKind.Warning);

            return result;
        }

        /// <summary>
        /// 이미지 저장 (SYSTEM 탭 설정에 따름). 실패해도 검사는 계속한다.
        /// </summary>
        public void SaveImages(InspectionResult result, Image resultBitmap)
        {
            string error = Logger.SaveImages(result, resultBitmap);
            if (error != null) WriteLog("이미지 저장 실패: " + error, LogKind.Warning);
        }

        public void ResetCounts()
        {
            TotalCount = 0;
            OkCount = 0;
            NgCount = 0;
        }

        #endregion

        #region 설정 / 종료

        /// <summary>
        /// SYSTEM 탭에서 바꾼 값을 저장한다.
        /// </summary>
        public void SaveSystem()
        {
            System.FillEmptyPaths();
            System.Save();
            if (SystemChanged != null) SystemChanged();
        }

        private void SaveSystemQuietly()
        {
            try
            {
                System.Save();
            }
            catch (Exception ex)
            {
                WriteLog("system.xml 저장 실패: " + ex.Message, LogKind.Warning);
            }
        }

        /// <summary>
        /// 프로그램 종료 시 정리: 레시피 → 카메라 설정 → 카메라 연결 해제
        /// [안전] 중간 오류로 카메라 해제가 건너뛰어지지 않게 단계마다 오류를 무시한다.
        /// </summary>
        public void Shutdown()
        {
            try { UnloadRecipe(); } catch { }
            try { DisconnectCamera(); } catch { }
            ReleaseCameras();
        }

        /// <summary>
        /// 카메라(프레임 그래버) 연결 해제 - 프로그램 종료 시에만 호출한다.
        /// [안전] 지우지 말 것: 카메라 연결을 끊지 않고 종료하면 R6025 오류창이 뜨고 멈춘다 (VProEditAll 에서 확인).
        /// 카메라를 쓴 경우에만 실행한다 (CogFrameGrabbers 를 만드는 것만으로 카메라를 검색하기 때문).
        /// </summary>
        private void ReleaseCameras()
        {
            if (!_cameraUsed) return;
            try
            {
                CogFrameGrabbers frameGrabbers = new CogFrameGrabbers();
                foreach (ICogFrameGrabber frameGrabber in frameGrabbers)
                {
                    frameGrabber.Disconnect(false);
                }
            }
            catch
            {
                // [안전] 해제 중 오류는 무시
            }
            _cameraUsed = false;
        }

        #endregion
    }
}
