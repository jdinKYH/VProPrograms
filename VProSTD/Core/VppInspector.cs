using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace VProSTD
{
    // =================================================================================================
    // 이 파일이 하는 일: vpp 파일을 "VisionPro 로 열기 전에" 바이트로만 읽어서
    //   1) 어떤 형식인지 (ToolBlock / AcqFifoTool)
    //   2) 카메라·I/O 설정이 들어 있는지
    // 를 알아낸다. (VProEditAll 의 VppInspector 를 VProSTD 용으로 줄인 것)
    //
    // [안전] VisionPro 로 파일을 열면 그 순간 카메라·I/O 연결을 시도한다.
    //        그래서 열기 전에 바이트로만 훑어보고 경고한다. → 이 파일은 VisionPro 로 파일을 열지 않는다.
    // =================================================================================================

    public enum VppKind
    {
        Unknown,        // 판정 불가
        ToolBlock,      // CogToolBlock   → 레시피 .vpp
        AcqFifo,        // CogAcqFifoTool → 카메라 설정 파일 (VProSTD 는 쓰지 않음, 레시피 목록에서 뺌)
        Unsupported     // 그 밖의 형식 (QuickBuild, Job, ToolGroup 등)
    }

    public class VppFileInfo
    {
        public string RootTypeName = string.Empty;   // 최상위 객체 형식 이름
        public VppKind Kind = VppKind.Unknown;
        public bool HasAcqFifo;                      // 영상 취득(AcqFifo) 설정 포함
        public bool HasIOSettings;                   // I/O·통신 설정 포함
    }

    public static class VppInspector
    {
        private const int HeadLength = 8192;   // 형식 이름을 찾을 앞부분 길이 (바이트)

        public static VppFileInfo Inspect(string filePath)
        {
            VppFileInfo info = new VppFileInfo();
            byte[] data = File.ReadAllBytes(filePath);
            if (data.Length == 0) return info;

            // 앞부분에서 처음 나오는 Cognex 클래스 이름이 최상위 객체
            int headLength = Math.Min(data.Length, HeadLength);
            string head = Encoding.ASCII.GetString(data, 0, headLength);
            Match match = Regex.Match(head, @"Cognex\.VisionPro[\w\.]*\.Cog\w+");
            if (match.Success) info.RootTypeName = match.Value;

            info.Kind = KindFromTypeName(info.RootTypeName);
            info.HasAcqFifo = ContainsAscii(data, "CogAcqFifo");
            info.HasIOSettings = ContainsAscii(data, "CogIODiscreteInfoList") || ContainsAscii(data, "CogIOTCPIPInfoList");
            return info;
        }

        /// <summary>
        /// 앞부분(8KB)만 읽어서 형식만 알아낸다. 레시피 목록처럼 파일이 여러 개일 때 쓴다.
        /// </summary>
        public static VppKind InspectKind(string filePath)
        {
            try
            {
                byte[] data = new byte[HeadLength];
                int length;
                using (FileStream stream = File.OpenRead(filePath))
                {
                    length = stream.Read(data, 0, data.Length);
                }
                string head = Encoding.ASCII.GetString(data, 0, length);
                Match match = Regex.Match(head, @"Cognex\.VisionPro[\w\.]*\.Cog\w+");
                return KindFromTypeName(match.Success ? match.Value : "");
            }
            catch (IOException)
            {
                return VppKind.Unknown;
            }
            catch (UnauthorizedAccessException)
            {
                return VppKind.Unknown;
            }
        }

        public static VppKind KindFromTypeName(string typeName)
        {
            if (string.IsNullOrEmpty(typeName)) return VppKind.Unknown;
            string shortName = typeName.Substring(typeName.LastIndexOf('.') + 1);
            if (shortName == "CogToolBlock") return VppKind.ToolBlock;
            if (shortName == "CogAcqFifoTool") return VppKind.AcqFifo;
            return VppKind.Unsupported;
        }

        /// <summary>
        /// 바이트 배열 안에 text(영문) 가 들어 있는지 찾는다.
        /// </summary>
        private static bool ContainsAscii(byte[] data, string text)
        {
            byte[] pattern = Encoding.ASCII.GetBytes(text);
            int last = data.Length - pattern.Length;
            int i = 0;
            while (i <= last)
            {
                i = Array.IndexOf(data, pattern[0], i, last - i + 1);
                if (i < 0) return false;
                int k = 1;
                while (k < pattern.Length && data[i + k] == pattern[k]) k++;
                if (k == pattern.Length) return true;
                i++;
            }
            return false;
        }
    }
}
