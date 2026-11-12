using Cognex.VisionPro;
using Cognex.VisionPro.QuickBuild;
using Cognex.VisionPro.ToolGroup;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace VProEditAll
{
    /// <summary>
    /// 불러온 vpp 파일 하나 (불러오기 / 편집 대상 꺼내기 / 저장 / 정리)
    /// - QuickBuild(CogJobManager) : Job 을 골라 그 Job 의 ToolGroup 을 편집. 저장은 JobManager 전체
    /// - Job(CogJob)               : Job 의 ToolGroup 을 편집. 저장은 CogJob 그대로
    /// - ToolGroup / ToolBlock     : 객체 자체를 편집/저장
    ///
    /// [보안] vpp 파일은 프로그램 코드(스크립트)를 포함할 수 있고, 불러올 때 그 코드가 컴파일된다.
    ///        또 파일을 객체로 되살리는 방식(BinaryFormatter) 자체가 코드 실행에 악용될 수 있다.
    ///        → 출처를 모르는 vpp 파일은 열지 않는다. (README.md 운영 규칙 참고)
    /// </summary>
    public class VppDocument : IDisposable
    {
        public string FilePath { get; private set; }
        public object Root { get; private set; }     // 파일에서 불러온 최상위 객체 (형식은 Kind 로 구분)
        public VppKind Kind { get; private set; }

        // Root 를 형식에 맞게 꺼내 쓰기 위한 도우미.
        // 형식이 다르면 null 이 된다. (예: ToolGroup 파일이면 JobManager 는 null)
        private CogJobManager JobManager
        {
            get { return Root as CogJobManager; }
        }

        private CogJob Job
        {
            get { return Root as CogJob; }
        }

        // 외부에서 new 로 만들지 못하게 막음. 대신 Load() 를 사용한다.
        private VppDocument() { }

        /// <summary>
        /// 파일 불러오기. 지원하지 않는 형식이면 NotSupportedException.
        /// </summary>
        public static VppDocument Load(string filePath)
        {
            object obj = CogSerializer.LoadObjectFromFile(filePath);
            VppKind kind = VppInspector.KindFromObject(obj);

            if (kind == VppKind.Unsupported || kind == VppKind.Unknown)
            {
                string typeName = "null";
                if (obj != null) typeName = obj.GetType().FullName;

                ReleaseObject(obj);
                throw new NotSupportedException($"지원하지 않는 형식입니다: {typeName}\n(QuickBuild / Job / ToolGroup / ToolBlock 파일만 편집할 수 있습니다.)");
            }

            VppDocument doc = new VppDocument();
            doc.FilePath = filePath;
            doc.Root = obj;
            doc.Kind = kind;
            doc.StopRunning();  // 실행 중인 상태로 저장된 파일일 수 있으므로 정지
            return doc;
        }

        /// <summary>
        /// 편집 가능한 Job 개수 (QuickBuild: Job 수, Job 파일: 1, ToolGroup/ToolBlock: 0)
        /// </summary>
        public int JobCount
        {
            get
            {
                if (Kind == VppKind.QuickBuild) return JobManager.JobCount;
                if (Kind == VppKind.Job) return 1;
                return 0;
            }
        }

        public string GetJobName(int index)
        {
            if (Kind == VppKind.QuickBuild) return JobManager.Job(index).Name;
            if (Kind == VppKind.Job) return Job.Name;
            return string.Empty;
        }

        /// <summary>
        /// 편집 화면에 연결할 대상.
        /// ToolBlock 파일이면 CogToolBlock 객체가 그대로 돌아온다 (CogToolBlock 은 CogToolGroup 의 한 종류).
        /// </summary>
        public CogToolGroup GetEditSubject(int jobIndex)
        {
            if (Kind == VppKind.QuickBuild) return JobManager.Job(jobIndex).VisionTool as CogToolGroup;
            if (Kind == VppKind.Job) return Job.VisionTool as CogToolGroup;
            if (Kind == VppKind.ToolGroup || Kind == VppKind.ToolBlock) return Root as CogToolGroup;
            return null;
        }

        /// <summary>
        /// 스크립트 컴파일 오류 목록
        /// - VisionPro 는 스크립트에 오류가 있어도 불러오기 오류를 내지 않고 ScriptError 에만 남긴다.
        /// - 모든 Job 의 ToolGroup 과, 그 안에 들어 있는 ToolGroup/ToolBlock 까지 모두 검사한다.
        /// </summary>
        public List<string> GetScriptErrors()
        {
            List<string> errors = new List<string>();

            // ToolGroup/ToolBlock 파일은 JobCount 가 0 이지만 검사할 대상은 1개 있다.
            int count = JobCount;
            if (count == 0) count = 1;

            for (int j = 0; j < count; j++)
            {
                string jobName = string.Empty;
                if (Kind == VppKind.QuickBuild || Kind == VppKind.Job) jobName = GetJobName(j);

                CollectScriptErrors(GetEditSubject(j), jobName, errors);
            }
            return errors;
        }

        /// <summary>
        /// group 의 스크립트 오류를 errors 에 추가한다.
        /// group 안에 또 ToolGroup 이 있으면 이 함수를 다시 불러서 그 안도 검사한다 (재귀 호출).
        /// parentPath: 오류 위치 표시용 경로 (예: "Job1 > ToolGroup > Locate")
        /// </summary>
        private static void CollectScriptErrors(CogToolGroup group, string parentPath, List<string> errors)
        {
            if (group == null) return;

            string path = group.Name;
            if (!string.IsNullOrEmpty(parentPath)) path = parentPath + " > " + group.Name;

            // ScriptError 는 오류가 없어도 컴파일 기록을 담고 있다 (시험으로 확인).
            //   정상 : "-Referenced Assembly: ..." / "-Found possible Script Class: ..." (모두 '-' 로 시작)
            //   오류 : "Unable to load script code in Locate." / "Compile Error in Script: ..."
            // → '-' 로 시작하지 않는 줄이 있으면 오류로 판단한다.
            string scriptLog = group.ScriptError;
            if (scriptLog == null) scriptLog = string.Empty;

            List<string> errorLines = new List<string>();
            foreach (string raw in scriptLog.Split('\n'))
            {
                string line = raw.Trim();
                if (line.Length == 0) continue;
                if (line.StartsWith("-")) continue;
                errorLines.Add(line);
            }

            if (errorLines.Count > 0)
            {
                // "Compile Error" 가 들어 있는 줄을 대표로 보여 준다. 없으면 첫 줄.
                string summary = errorLines[0];
                foreach (string line in errorLines)
                {
                    if (line.IndexOf("Compile Error", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        summary = line;
                        break;
                    }
                }
                errors.Add(path + " : " + summary);
            }

            // 안에 들어 있는 ToolGroup/ToolBlock 도 검사
            foreach (ICogTool tool in group.Tools)
            {
                CogToolGroup subGroup = tool as CogToolGroup;   // ToolBlock 도 여기 포함됨
                if (subGroup != null) CollectScriptErrors(subGroup, path, errors);
            }
        }

        /// <summary>
        /// QuickBuild 의 I/O 사용 설정.
        /// bool? 는 true / false / null 세 가지 값을 가질 수 있다. QuickBuild 가 아니면 null (해당 없음).
        /// </summary>
        public bool? IOEnabled
        {
            get
            {
                if (Kind == VppKind.QuickBuild) return JobManager.IOEnable;
                return null;
            }
        }

        /// <summary>
        /// 저장.
        /// [안전] 원본을 지키기 위해 3단계로 저장한다. (단순하게 바로 덮어쓰지 말 것)
        ///   1) 임시 파일(.saving.tmp)에 먼저 저장
        ///   2) 임시 파일과 원본을 교체 (원본은 .bak 으로 남김)
        ///   3) 중간에 실패하면 임시 파일만 지움 → 원본은 그대로
        /// ※ .bak 은 1개만 남는다 (저장할 때마다 이전 .bak 을 덮어씀).
        /// ※ ExcludeDataBindings 옵션은 툴 간 연결이 빠지므로 사용하지 않는다.
        /// </summary>
        /// <returns>만들어진 백업 파일 경로 (새 파일이면 null)</returns>
        public string Save(string filePath, CogSerializationOptionsConstants options)
        {
            StopRunning();   // 실행 중에 저장하지 않도록 정지

            string fullPath = Path.GetFullPath(filePath);
            string tempPath = fullPath + ".saving.tmp";
            string backupPath = null;

            try
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);

                // 1) 임시 파일에 저장
                CogSerializer.SaveObjectToFile(Root, tempPath, typeof(BinaryFormatter), options);

                // 2) 원본과 교체
                if (File.Exists(fullPath))
                {
                    backupPath = fullPath + ".bak";
                    File.Replace(tempPath, fullPath, backupPath, true);   // 원본 → .bak (기존 .bak 은 덮어씀)
                }
                else
                {
                    File.Move(tempPath, fullPath);                         // 새 파일이면 이름만 바꿈
                }

                FilePath = fullPath;
                return backupPath;
            }
            finally
            {
                // 3) 저장에 실패했으면 임시 파일이 남아 있으므로 지운다 (원본은 그대로 남아 있음)
                try
                {
                    if (File.Exists(tempPath)) File.Delete(tempPath);
                }
                catch
                {
                    // 임시 파일을 못 지워도 원본에는 영향 없음
                }
            }
        }

        /// <summary>
        /// Job/JobManager 실행 정지 (실행 중이 아니어도 안전하게 호출할 수 있음)
        /// </summary>
        public void StopRunning()
        {
            try
            {
                if (Kind == VppKind.QuickBuild) JobManager.Stop();
                else if (Kind == VppKind.Job) Job.Stop();
            }
            catch
            {
                // [안전] 정리 중 오류는 무시 (뒤의 정리 단계가 계속 실행되도록)
            }
        }

        /// <summary>
        /// 정리: 정지 → Shutdown(JobManager/Job) 또는 Dispose(ToolGroup/ToolBlock)
        /// [안전] 이 함수를 부르기 전에 편집 화면의 Subject 를 먼저 null 로 끊어야 한다.
        ///        (편집 화면이 아직 연결된 객체를 정리하면 오류가 날 수 있음)
        /// </summary>
        public void Dispose()
        {
            StopRunning();
            ReleaseObject(Root);
            Root = null;
        }

        /// <summary>
        /// 불러온 객체를 형식에 맞는 방법으로 정리한다.
        /// </summary>
        private static void ReleaseObject(object obj)
        {
            try
            {
                CogJobManager jobManager = obj as CogJobManager;
                CogJob job = obj as CogJob;
                IDisposable disposable = obj as IDisposable;

                if (jobManager != null) jobManager.Shutdown();
                else if (job != null) job.Shutdown();
                else if (disposable != null) disposable.Dispose();
            }
            catch
            {
                // [안전] 정리 중 오류는 무시
            }
        }
    }
}
