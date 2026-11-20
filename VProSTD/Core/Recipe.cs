using Cognex.VisionPro;
using Cognex.VisionPro.ToolBlock;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace VProSTD
{
    /// <summary>
    /// 레시피 하나의 설정 (<레시피 이름>.recipe.xml)
    /// </summary>
    public class RecipeSettings
    {
        public string ResultsAnalysisToolName = "";  // 결과 표에 쓸 CogResultsAnalysisTool 이름. 비우면 첫 번째
        public bool FileMode = false;                // 마지막 File Mode 상태
        public string LastImagePath = "";            // 마지막으로 고른 이미지 파일 또는 폴더
        public double ExposureMs = 0;                // 이 레시피의 카메라 노출 (ms). 0 이하면 카메라 값을 그대로 씀
    }

    /// <summary>
    /// 레시피 파일 관리. 레시피 = Recipe Path 안의 .vpp 파일 하나 (이름은 사용자가 정함)
    ///   <RecipePath>/<레시피 이름>.vpp          검사 ToolBlock
    ///   <RecipePath>/<레시피 이름>.recipe.xml   위 RecipeSettings (없으면 기본값)
    /// 카메라(이미지 취득) 설정은 레시피에 넣지 않는다 → system.xml 의 Camera (CONNECT CAMERA 탭)
    /// </summary>
    public static class RecipeManager
    {
        public const string Extension = ".vpp";
        public const string SettingsExtension = ".recipe.xml";

        public static string FileOf(string recipePath, string name)
        {
            return Path.Combine(recipePath, name + Extension);
        }

        public static string SettingsFileOf(string recipePath, string name)
        {
            return Path.Combine(recipePath, name + SettingsExtension);
        }

        /// <summary>
        /// 사용자가 입력한 이름에서 앞뒤 공백과 .vpp 를 뗀다.
        /// </summary>
        public static string CleanName(string name)
        {
            name = (name ?? "").Trim();
            if (name.EndsWith(Extension, StringComparison.OrdinalIgnoreCase))
            {
                name = name.Substring(0, name.Length - Extension.Length).Trim();
            }
            return name;
        }

        /// <summary>
        /// 레시피 이름 목록: Recipe Path 의 .vpp 파일 (확장자를 뗀 이름).
        /// 카메라 설정 파일(CogAcqFifoTool)처럼 ToolBlock 이 아닌 것으로 확인된 파일은 뺀다.
        /// </summary>
        public static List<string> GetNames(string recipePath)
        {
            List<string> names = new List<string>();
            if (!Directory.Exists(recipePath)) return names;

            foreach (string file in Directory.GetFiles(recipePath, "*" + Extension))
            {
                // "*.vpp" 는 ".vppx" 같은 파일도 찾으므로 확장자를 다시 확인
                if (!string.Equals(Path.GetExtension(file), Extension, StringComparison.OrdinalIgnoreCase)) continue;

                VppKind kind = VppInspector.InspectKind(file);
                if (kind == VppKind.AcqFifo || kind == VppKind.Unsupported) continue;

                names.Add(Path.GetFileNameWithoutExtension(file));
            }
            names.Sort(StringComparer.OrdinalIgnoreCase);
            return names;
        }

        /// <summary>
        /// 새 이름 확인. 문제가 있으면 오류 메시지, 문제없으면 null
        /// </summary>
        public static string CheckName(string recipePath, string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "이름을 입력하세요.";
            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) return "이름에 쓸 수 없는 글자가 있습니다.";
            if (File.Exists(FileOf(recipePath, name))) return "같은 이름의 레시피가 이미 있습니다.";
            return null;
        }

        /// <summary>
        /// 새 레시피: 빈 ToolBlock 에 InputImage 입력과 Result 출력을 미리 만들어 저장한다.
        /// </summary>
        public static void Create(string recipePath, string name)
        {
            Directory.CreateDirectory(recipePath);

            CogToolBlock toolBlock = new CogToolBlock();
            toolBlock.Name = name;
            toolBlock.Inputs.Add(new CogToolBlockTerminal(JobRunner.InputImageName, typeof(ICogImage)));
            toolBlock.Outputs.Add(new CogToolBlockTerminal(JobRunner.ResultName, false));

            VppFile.Save(toolBlock, FileOf(recipePath, name), false);
            VppFile.Release(toolBlock);

            SaveSettings(SettingsFileOf(recipePath, name), new RecipeSettings());
        }

        /// <summary>
        /// 레시피 복사: .vpp 와 설정 파일(있으면)을 새 이름으로 복사한다.
        /// </summary>
        public static void Copy(string recipePath, string sourceName, string newName)
        {
            File.Copy(FileOf(recipePath, sourceName), FileOf(recipePath, newName));

            string settings = SettingsFileOf(recipePath, sourceName);
            if (File.Exists(settings)) File.Copy(settings, SettingsFileOf(recipePath, newName));
        }

        /// <summary>
        /// 레시피 삭제: .vpp, .vpp.bak, 설정 파일만 지운다 (같은 폴더의 다른 파일은 건드리지 않음).
        /// </summary>
        public static void Delete(string recipePath, string name)
        {
            string file = FileOf(recipePath, name);
            File.Delete(file);
            DeleteIfExists(file + ".bak");
            DeleteIfExists(SettingsFileOf(recipePath, name));
        }

        private static void DeleteIfExists(string path)
        {
            if (File.Exists(path)) File.Delete(path);
        }

        public static RecipeSettings LoadSettings(string path)
        {
            if (!File.Exists(path)) return new RecipeSettings();

            XmlSerializer serializer = new XmlSerializer(typeof(RecipeSettings));
            using (FileStream stream = File.OpenRead(path))
            {
                return (RecipeSettings)serializer.Deserialize(stream);
            }
        }

        public static void SaveSettings(string path, RecipeSettings settings)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(RecipeSettings));
            using (FileStream stream = File.Create(path))
            {
                serializer.Serialize(stream, settings);
            }
        }
    }
}
