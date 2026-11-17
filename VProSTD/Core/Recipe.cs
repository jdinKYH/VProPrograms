using Cognex.VisionPro;
using Cognex.VisionPro.ToolBlock;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace VProSTD
{
    /// <summary>
    /// 레시피 하나의 설정 (recipe.xml)
    /// </summary>
    public class RecipeSettings
    {
        public string DisplayRecordName = "";        // RUN 화면에 보여 줄 Record 이름. 비우면 첫 번째 Record
        public string ResultsAnalysisToolName = "";  // 결과 표에 쓸 CogResultsAnalysisTool 이름. 비우면 첫 번째
        public bool FileMode = false;                // 마지막 File Mode 상태
        public string LastImagePath = "";            // 마지막으로 고른 이미지 파일 또는 폴더
    }

    /// <summary>
    /// 레시피 폴더 관리
    ///   <RecipePath>/<레시피 이름>/Job.vpp      검사 ToolBlock
    ///   <RecipePath>/<레시피 이름>/Acq.vpp      이미지 취득 CogAcqFifoTool (CONNECT 후 SAVE 하면 생김)
    ///   <RecipePath>/<레시피 이름>/recipe.xml   위 RecipeSettings
    /// </summary>
    public static class RecipeManager
    {
        public const string JobFileName = "Job.vpp";
        public const string AcqFileName = "Acq.vpp";
        public const string SettingsFileName = "recipe.xml";

        public static string FolderOf(string recipePath, string name)
        {
            return Path.Combine(recipePath, name);
        }

        /// <summary>
        /// 레시피 이름 목록 (Job.vpp 가 들어 있는 폴더만)
        /// </summary>
        public static List<string> GetNames(string recipePath)
        {
            List<string> names = new List<string>();
            if (!Directory.Exists(recipePath)) return names;

            foreach (string folder in Directory.GetDirectories(recipePath))
            {
                if (File.Exists(Path.Combine(folder, JobFileName)))
                {
                    names.Add(Path.GetFileName(folder));
                }
            }
            names.Sort(StringComparer.OrdinalIgnoreCase);
            return names;
        }

        /// <summary>
        /// 폴더 이름으로 쓸 수 없는 글자가 있으면 오류 메시지, 문제없으면 null
        /// </summary>
        public static string CheckName(string recipePath, string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "이름을 입력하세요.";
            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) return "이름에 쓸 수 없는 글자가 있습니다.";
            if (Directory.Exists(FolderOf(recipePath, name))) return "같은 이름의 레시피가 이미 있습니다.";
            return null;
        }

        /// <summary>
        /// 새 레시피: 빈 ToolBlock 에 InputImage 입력과 Result 출력을 미리 만들어 저장한다.
        /// </summary>
        public static void Create(string recipePath, string name)
        {
            string folder = FolderOf(recipePath, name);
            Directory.CreateDirectory(folder);

            CogToolBlock toolBlock = new CogToolBlock();
            toolBlock.Name = name;
            toolBlock.Inputs.Add(new CogToolBlockTerminal(JobRunner.InputImageName, typeof(ICogImage)));
            toolBlock.Outputs.Add(new CogToolBlockTerminal(JobRunner.ResultName, false));

            VppFile.Save(toolBlock, Path.Combine(folder, JobFileName), false);
            VppFile.Release(toolBlock);

            SaveSettings(folder, new RecipeSettings());
        }

        /// <summary>
        /// 레시피 복사: 폴더 안의 파일을 모두 새 폴더로 복사한다 (.bak 은 제외).
        /// </summary>
        public static void Copy(string recipePath, string sourceName, string newName)
        {
            string source = FolderOf(recipePath, sourceName);
            string target = FolderOf(recipePath, newName);
            Directory.CreateDirectory(target);

            foreach (string file in Directory.GetFiles(source))
            {
                if (file.EndsWith(".bak", StringComparison.OrdinalIgnoreCase)) continue;
                File.Copy(file, Path.Combine(target, Path.GetFileName(file)));
            }
        }

        /// <summary>
        /// 레시피 삭제: 폴더 전체를 지운다.
        /// </summary>
        public static void Delete(string recipePath, string name)
        {
            Directory.Delete(FolderOf(recipePath, name), true);
        }

        public static RecipeSettings LoadSettings(string folder)
        {
            string path = Path.Combine(folder, SettingsFileName);
            if (!File.Exists(path)) return new RecipeSettings();

            XmlSerializer serializer = new XmlSerializer(typeof(RecipeSettings));
            using (FileStream stream = File.OpenRead(path))
            {
                return (RecipeSettings)serializer.Deserialize(stream);
            }
        }

        public static void SaveSettings(string folder, RecipeSettings settings)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(RecipeSettings));
            using (FileStream stream = File.Create(Path.Combine(folder, SettingsFileName)))
            {
                serializer.Serialize(stream, settings);
            }
        }
    }
}
