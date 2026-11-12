using Cognex.VisionPro.QuickBuild;
using Cognex.VisionPro.ToolBlock;
using Cognex.VisionPro.ToolGroup;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace VProEditAll
{
    // =================================================================================================
    // 이 파일이 하는 일: vpp 파일을 "VisionPro 로 열기 전에" 바이트로만 읽어서
    //   1) 어떤 형식인지 (QuickBuild / Job / ToolGroup / ToolBlock)
    //   2) 카메라·I/O 설정이 들어 있는지
    // 를 알아낸다.
    //
    // [안전] 왜 열기 전에 바이트로 읽는가?
    //   VisionPro 로 파일을 열면 그 순간 카메라·I/O 연결을 시도한다.
    //   그래서 연결되기 전에 사용자에게 경고하려면, 파일을 글자(바이트)로만 먼저 훑어봐야 한다.
    //   → 이 파일의 코드는 VisionPro 로 파일을 열지 않는다. (이 성질을 바꾸지 말 것)
    // =================================================================================================

    /// <summary>
    /// vpp 파일 형식
    /// </summary>
    public enum VppKind
    {
        Unknown,        // 판정 불가 (불러온 뒤 객체로 다시 판정)
        QuickBuild,     // CogJobManager (QuickBuild 전체)
        Job,            // CogJob (QuickBuild Job 하나만 저장한 파일)
        ToolGroup,      // CogToolGroup
        ToolBlock,      // CogToolBlock
        Unsupported     // 툴 하나만 저장한 파일 등 (예: CogAcqFifoTool)
    }

    /// <summary>
    /// 파일을 바이트로 읽은 결과
    /// </summary>
    public class VppFileInfo
    {
        public string RootTypeName = string.Empty;  // 파일 최상위 객체 형식 이름 (예: Cognex.VisionPro.QuickBuild.CogJobManager)
        public VppKind Kind = VppKind.Unknown;
        public bool IsBinary;                       // BinaryFormatter 형식 여부 (false 이면 XML/SOAP 등)
        public bool HasAcqFifo;                     // 영상 취득(AcqFifo) 설정 포함
        public bool HasGigE;                        // GigE 카메라 설정 포함
        public bool HasIOSettings;                  // I/O·통신(Discrete, TCP/IP) 설정 포함
    }

    /// <summary>
    /// vpp 파일 형식/위험 요소(카메라, I/O) 판정
    /// - 파일 앞부분에서 최상위 객체 형식 이름을 찾는다.
    /// - 파일 전체에서 AcqFifo / GigE / I/O 설정 문자열을 찾는다.
    /// </summary>
    public static class VppInspector
    {
        private const int HeadLength = 8192;   // 형식 이름을 찾을 앞부분 길이 (바이트)

        public static VppFileInfo Inspect(string filePath)
        {
            VppFileInfo info = new VppFileInfo();
            byte[] data = File.ReadAllBytes(filePath);
            if (data.Length == 0) return info;

            // BinaryFormatter 파일은 첫 바이트가 0 이다.
            info.IsBinary = data[0] == 0;

            // 앞부분을 글자로 바꿔서, 정규식(글자 패턴 찾기)으로 형식 이름을 찾는다.
            int headLength = Math.Min(data.Length, HeadLength);
            string head = Encoding.ASCII.GetString(data, 0, headLength);

            if (info.IsBinary)
            {
                // 바이너리: 처음 나오는 Cognex 클래스 이름이 최상위 객체
                //   패턴 뜻: "Cognex.VisionPro" + (글자/점 여러 개) + ".Cog" + 글자 여러 개
                Match match = Regex.Match(head, @"Cognex\.VisionPro[\w\.]*\.Cog\w+");
                if (match.Success) info.RootTypeName = match.Value;
            }
            else
            {
                // XML(SOAP): <SOAP-ENV:Body> 다음 첫 요소가 최상위 객체 (예: <a1:CogJobManager ...>)
                // ※ 시험해 보니 VisionPro 객체는 XML 로 저장 자체가 안 되므로 실제로는 거의 없는 경우다.
                int bodyIndex = head.IndexOf("SOAP-ENV:Body", StringComparison.Ordinal);
                if (bodyIndex >= 0)
                {
                    //   패턴 뜻: "<a" + 숫자 + ":" + (Cog 로 시작하는 이름)  → 괄호 안 부분이 Groups[1]
                    Regex soapPattern = new Regex(@"<a\d+:(Cog\w+)");
                    Match match = soapPattern.Match(head, bodyIndex);
                    if (match.Success) info.RootTypeName = match.Groups[1].Value;
                }
            }

            info.Kind = KindFromTypeName(info.RootTypeName);
            info.HasAcqFifo = ContainsAscii(data, "CogAcqFifo");
            info.HasGigE = ContainsAscii(data, "GigE");
            info.HasIOSettings = ContainsAscii(data, "CogIODiscreteInfoList") || ContainsAscii(data, "CogIOTCPIPInfoList");
            return info;
        }

        /// <summary>
        /// 형식 이름 → VppKind (전체 이름/짧은 이름 모두 허용)
        /// </summary>
        public static VppKind KindFromTypeName(string typeName)
        {
            if (string.IsNullOrEmpty(typeName)) return VppKind.Unknown;
            string shortName = typeName.Substring(typeName.LastIndexOf('.') + 1);
            switch (shortName)
            {
                case "CogJobManager": return VppKind.QuickBuild;
                case "CogJob": return VppKind.Job;
                case "CogToolBlock": return VppKind.ToolBlock;
                case "CogToolGroup": return VppKind.ToolGroup;
                default: return VppKind.Unsupported;
            }
        }

        /// <summary>
        /// 불러온 객체 → VppKind
        /// [안전] CogToolBlock 은 CogToolGroup 을 상속한다 (ToolBlock 도 "ToolGroup 의 한 종류").
        ///        그래서 ToolGroup 을 먼저 검사하면 ToolBlock 도 ToolGroup 으로 잘못 판정된다.
        ///        → 반드시 ToolBlock 을 먼저 검사할 것.
        /// </summary>
        public static VppKind KindFromObject(object obj)
        {
            if (obj == null) return VppKind.Unknown;
            if (obj is CogJobManager) return VppKind.QuickBuild;
            if (obj is CogJob) return VppKind.Job;
            if (obj is CogToolBlock) return VppKind.ToolBlock;     // ToolGroup 보다 먼저
            if (obj is CogToolGroup) return VppKind.ToolGroup;
            return VppKind.Unsupported;
        }

        /// <summary>
        /// 화면에 보여 줄 형식 이름
        /// </summary>
        public static string DisplayName(VppKind kind)
        {
            switch (kind)
            {
                case VppKind.QuickBuild: return "QuickBuild";
                case VppKind.Job: return "Job";
                case VppKind.ToolGroup: return "ToolGroup";
                case VppKind.ToolBlock: return "ToolBlock";
                case VppKind.Unsupported: return "미지원";
                default: return "알 수 없음";
            }
        }

        /// <summary>
        /// 바이트 배열 안에 text(영문) 가 들어 있는지 찾는다.
        /// 방법: 첫 글자가 나오는 위치를 빠르게 찾고(Array.IndexOf), 그 자리부터 나머지 글자가 같은지 비교한다.
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
