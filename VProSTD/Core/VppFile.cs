using Cognex.VisionPro;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace VProSTD
{
    /// <summary>
    /// vpp 파일 불러오기 / 저장 / 정리
    ///
    /// [보안] vpp 파일은 프로그램 코드(스크립트)를 포함할 수 있다. 출처를 모르는 vpp 파일은 쓰지 않는다.
    /// </summary>
    public static class VppFile
    {
        /// <summary>
        /// 파일을 불러온다. (VisionPro 가 파일을 객체로 되살림)
        /// </summary>
        public static object Load(string path)
        {
            return CogSerializer.LoadObjectFromFile(path);
        }

        /// <summary>
        /// 저장.
        /// [안전] 원본을 지키기 위해 3단계로 저장한다. (단순하게 바로 덮어쓰지 말 것)
        ///   1) 임시 파일(.saving.tmp)에 먼저 저장
        ///   2) 임시 파일과 원본을 교체 (원본은 .bak 으로 남김, .bak 은 1개만 유지)
        ///   3) 중간에 실패하면 임시 파일만 지움 → 원본은 그대로
        /// minimum = true 이면 입력 이미지·마지막 결과를 빼고 저장한다 (파일이 작아짐).
        /// </summary>
        public static void Save(object obj, string path, bool minimum)
        {
            CogSerializationOptionsConstants options = CogSerializationOptionsConstants.All;
            if (minimum) options = CogSerializationOptionsConstants.Minimum;

            string fullPath = Path.GetFullPath(path);
            string tempPath = fullPath + ".saving.tmp";

            try
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);

                CogSerializer.SaveObjectToFile(obj, tempPath, typeof(BinaryFormatter), options);

                if (File.Exists(fullPath))
                {
                    File.Replace(tempPath, fullPath, fullPath + ".bak", true);
                }
                else
                {
                    File.Move(tempPath, fullPath);
                }
            }
            finally
            {
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
        /// 불러온 객체 정리 (오류는 무시 - 정리 중 오류로 다음 정리 단계가 건너뛰어지지 않게)
        /// </summary>
        public static void Release(object obj)
        {
            try
            {
                IDisposable disposable = obj as IDisposable;
                if (disposable != null) disposable.Dispose();
            }
            catch
            {
                // [안전] 정리 중 오류는 무시
            }
        }
    }
}
