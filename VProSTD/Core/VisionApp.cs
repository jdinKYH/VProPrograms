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
    ///   → Outputs["Result"] 로 OK/NG 판정, 카운트, Log, CSV 기록
    /// </summary>
    public class VisionApp
    {
        public SystemSettings System { get; private set; }
        public ResultLogger Logger { get; private set; }

        // 현재 레시피 (불러오지 않았으면 RecipeName 이 null)
        public string RecipeName { get; private set; }
        public string RecipeFolder { get; private set; }
        public RecipeSettings Recipe { get; private set; }
        public JobRunner Job { get; private set; }

        // 이미지 입력
        public AcqImageSource Acq { get; private set; }               // CONNECT 전에는 null
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

        // 예/아니오 확인 창 (MainForm 이 MessageBox 로 연결해 준다)
        public Func<string, bool> Confirm = message => true;

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
            get { return Acq != null && Acq.IsConnected; }
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
            string folder = RecipeManager.FolderOf(System.RecipePath, name);
            string jobPath = Path.Combine(folder, RecipeManager.JobFileName);

            // ① 바이트로만 확인 (VisionPro 로 열지 않음)
            VppFileInfo info = VppInspector.Inspect(jobPath);
            if (info.Kind != VppKind.ToolBlock && info.Kind != VppKind.Unknown)
            {
                WriteLog("Job.vpp 가 ToolBlock 파일이 아닙니다: " + info.RootTypeName, LogKind.Ng);
                return false;
            }

            // ② 카메라 / I/O 설정이 들어 있으면 경고 (설계상 Job.vpp 에는 취득이 없어야 함)
            if (info.HasAcqFifo || info.HasIOSettings)
            {
                string message = name + " 의 Job.vpp 에 카메라(AcqFifo) 또는 I/O 설정이 들어 있습니다.\n" +
                                 "불러오면 카메라·I/O 에 연결을 시도합니다.\n" +
                                 "(VProSTD 는 취득을 Acq.vpp 로 분리하는 것이 원칙입니다)\n\n계속 불러올까요?";
                if (!Confirm(message)) return false;
                _cameraUsed = true;
            }

            // ③ 이전 레시피 닫기 (카메라 설정도 레시피 것이므로 함께 닫는다)
            UnloadRecipe();
            DisconnectCamera();

            // ④ 불러오기
            object obj = VppFile.Load(jobPath);
            CogToolBlock toolBlock = obj as CogToolBlock;
            if (toolBlock == null)
            {
                VppFile.Release(obj);
                WriteLog("Job.vpp 를 ToolBlock 으로 불러올 수 없습니다.", LogKind.Ng);
                return false;
            }

            Job = new JobRunner(toolBlock, jobPath);
            RecipeName = name;
            RecipeFolder = folder;
            Recipe = RecipeManager.LoadSettings(folder);
            RestoreImageSource();

            System.LastRecipe = name;
            SaveSystemQuietly();

            WriteLog("RECIPE: '" + name + "' 불러옴");
            foreach (string problem in Job.CheckContract())
            {
                WriteLog("RECIPE: " + problem, LogKind.Warning);
            }
            foreach (string error in GetScriptErrors(toolBlock))
            {
                WriteLog("RECIPE: 스크립트 오류 - " + error, LogKind.Ng);
            }

            if (RecipeChanged != null) RecipeChanged();
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
            RecipeFolder = null;
            Recipe = new RecipeSettings();

            if (RecipeChanged != null) RecipeChanged();
        }

        /// <summary>
        /// 현재 레시피 저장: Job.vpp + (CONNECT 했으면) Acq.vpp + recipe.xml
        /// 저장할 때마다 이전 파일이 .bak 으로 1개 남는다.
        /// </summary>
        public void SaveRecipe()
        {
            if (Job == null) throw new InvalidOperationException("불러온 레시피가 없습니다.");

            VppFile.Save(Job.ToolBlock, Job.FilePath, false);
            if (Acq != null) Acq.Save(Path.Combine(RecipeFolder, RecipeManager.AcqFileName));

            Recipe.LastImagePath = Files.SourcePath ?? "";
            RecipeManager.SaveSettings(RecipeFolder, Recipe);

            WriteLog("RECIPE: '" + RecipeName + "' 저장");
        }

        /// <summary>
        /// 레시피에 기억된 마지막 이미지 파일/폴더를 다시 고른다 (없어졌으면 무시).
        /// </summary>
        private void RestoreImageSource()
        {
            Files = new FileImageSource();
            string path = Recipe.LastImagePath;
            if (string.IsNullOrEmpty(path)) return;

            if (Directory.Exists(path)) Files.SetFolder(path);
            else if (File.Exists(path)) Files.SetSingleFile(path);
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
        /// 카메라 연결: 현재 레시피의 Acq.vpp 를 불러온다. 없으면 첫 번째 카메라로 새로 만든다.
        /// </summary>
        public void ConnectCamera()
        {
            if (RecipeName == null) throw new InvalidOperationException("레시피를 먼저 불러오세요.");

            string message = "카메라에 연결합니다.\n\n" +
                             "장비 PC 에서는 QuickBuild / 생산 프로그램을 먼저 종료하세요.\n" +
                             "(같은 카메라를 두 프로그램이 동시에 쓰면 연결이 끊어질 수 있습니다)\n" +
                             "※ 카메라는 이 프로그램을 종료해야 해제됩니다.\n\n계속할까요?";
            if (!Confirm(message)) return;

            DisconnectCamera();

            string acqPath = Path.Combine(RecipeFolder, RecipeManager.AcqFileName);
            _cameraUsed = true;   // 불러오는 순간 연결될 수 있으므로 먼저 표시 (종료 시 해제)

            if (File.Exists(acqPath))
            {
                VppFileInfo info = VppInspector.Inspect(acqPath);
                if (info.Kind != VppKind.AcqFifo && info.Kind != VppKind.Unknown)
                {
                    throw new InvalidDataException("Acq.vpp 가 CogAcqFifoTool 파일이 아닙니다: " + info.RootTypeName);
                }
                Acq = AcqImageSource.Load(acqPath);
                WriteLog("CAMERA: Acq.vpp 불러옴 - " + Acq.CameraName);
            }
            else
            {
                if (!Confirm("이 레시피에 Acq.vpp 가 없습니다.\n연결된 첫 번째 카메라로 새로 만들까요?\n(RECIPE 탭에서 SAVE 하면 Acq.vpp 로 저장됩니다)")) return;
                Acq = AcqImageSource.CreateFromFirstCamera();
                WriteLog("CAMERA: 새로 만듦 - " + Acq.CameraName);
            }

            if (!Acq.IsConnected) WriteLog("CAMERA: 카메라를 찾을 수 없습니다 (File Mode 로만 검사할 수 있습니다).", LogKind.Warning);
            if (CameraChanged != null) CameraChanged();
        }

        /// <summary>
        /// 카메라 설정(AcqFifoTool) 정리. 카메라 자체는 프로그램 종료 때 해제된다.
        /// </summary>
        public void DisconnectCamera()
        {
            if (Acq == null) return;
            Acq.Dispose();
            Acq = null;
            if (CameraChanged != null) CameraChanged();
        }

        /// <summary>
        /// TRIGGER: 1장만 취득 (검사는 하지 않음)
        /// </summary>
        public ICogImage GrabOnly()
        {
            if (Acq == null) throw new InvalidOperationException("CONNECT 를 먼저 누르세요.");
            return Acq.Grab();
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
                    if (Acq == null) throw new InvalidOperationException("카메라가 연결되어 있지 않습니다. CONNECT CAMERA 탭에서 CONNECT 하거나 File Mode 를 켜세요.");
                    source = "CAMERA";
                    image = Acq.Grab();
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
