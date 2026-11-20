using System;
using System.Security.Cryptography;

namespace VProSTD
{
    /// <summary>
    /// 엔지니어 모드 비밀번호.
    /// 비밀번호 자체는 어디에도 저장하지 않는다. system.xml 에는 Salt(무작위 값) 와 Hash(비밀번호로 계산한 값) 만 저장한다.
    /// [안전] 코드에 비밀번호를 넣지 말 것 (공개 저장소).
    /// 비밀번호를 잊으면: 프로그램을 끄고 system.xml 의 EngineerPasswordHash / EngineerPasswordSalt 값을 비우면
    ///                   다음에 엔지니어 탭을 누를 때 새 비밀번호를 정한다.
    /// 이 기능은 작업자의 실수를 막는 용도이며, system.xml 을 고칠 수 있는 사람까지 막지는 못한다.
    /// </summary>
    public static class EngineerPassword
    {
        public const int MinLength = 4;
        private const int Iterations = 10000;   // 계산을 일부러 느리게 해서 무작위 대입을 어렵게 한다
        private const int SaltBytes = 16;
        private const int HashBytes = 32;

        public static bool IsSet(SystemSettings settings)
        {
            return !string.IsNullOrEmpty(settings.EngineerPasswordHash) && !string.IsNullOrEmpty(settings.EngineerPasswordSalt);
        }

        /// <summary>
        /// 새 비밀번호를 설정에 넣는다 (파일 저장은 부르는 쪽에서).
        /// </summary>
        public static void Set(SystemSettings settings, string password)
        {
            byte[] salt = new byte[SaltBytes];
            using (RNGCryptoServiceProvider random = new RNGCryptoServiceProvider())
            {
                random.GetBytes(salt);
            }
            settings.EngineerPasswordSalt = Convert.ToBase64String(salt);
            settings.EngineerPasswordHash = Convert.ToBase64String(ComputeHash(password, salt));
        }

        public static bool Check(SystemSettings settings, string password)
        {
            if (!IsSet(settings) || password == null) return false;
            try
            {
                byte[] salt = Convert.FromBase64String(settings.EngineerPasswordSalt);
                byte[] expected = Convert.FromBase64String(settings.EngineerPasswordHash);
                byte[] actual = ComputeHash(password, salt);
                return SameBytes(expected, actual);
            }
            catch (FormatException)
            {
                return false;   // system.xml 값이 깨졌으면 틀린 것으로 본다
            }
        }

        private static byte[] ComputeHash(string password, byte[] salt)
        {
            using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations))
            {
                return pbkdf2.GetBytes(HashBytes);
            }
        }

        // 길이가 같으면 모든 바이트를 끝까지 비교한다 (비교 시간으로 값을 알아내지 못하게)
        private static bool SameBytes(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}
