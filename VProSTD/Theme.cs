using System.Drawing;

namespace VProSTD
{
    /// <summary>
    /// 화면 색상 (다크 테마). 실행 중에 바뀌는 색(탭 선택, OK/NG, 표시등, Log)은 여기서 가져다 쓴다.
    /// ※ Designer 파일에도 같은 색이 숫자로 들어 있다. 색을 바꿀 때는 두 곳을 함께 바꾼다.
    /// </summary>
    public static class Theme
    {
        // ----- 바탕 -----
        public static readonly Color Background = Color.FromArgb(22, 25, 29);     // 폼 / 페이지 바탕
        public static readonly Color Panel = Color.FromArgb(31, 35, 40);          // 타이틀 바, 카드, 표
        public static readonly Color Input = Color.FromArgb(24, 27, 31);          // 입력칸, Log, 작은 상자
        public static readonly Color Border = Color.FromArgb(58, 64, 72);         // 테두리
        public static readonly Color Display = Color.FromArgb(14, 16, 19);        // 이미지 화면 바탕

        // ----- 글자 -----
        public static readonly Color Text = Color.FromArgb(230, 232, 235);        // 기본 글자
        public static readonly Color SubText = Color.FromArgb(154, 163, 173);     // 작은 제목 / 보조 글자

        // ----- 버튼 -----
        public static readonly Color Button = Color.FromArgb(47, 51, 56);         // 일반 버튼
        public static readonly Color ButtonHover = Color.FromArgb(60, 65, 72);
        public static readonly Color ButtonDown = Color.FromArgb(72, 78, 86);
        public static readonly Color Blue = Color.FromArgb(31, 111, 178);         // 선택된 탭, AUTO RUN
        public static readonly Color Red = Color.FromArgb(179, 38, 30);           // STOP, EXIT, NG

        // ----- 상태 -----
        public static readonly Color OkFill = Color.FromArgb(46, 139, 62);        // OK 판정 칸 (채움)
        public static readonly Color OkText = Color.FromArgb(61, 220, 151);       // OK 글자, 표시등 켜짐
        public static readonly Color NgFill = Color.FromArgb(179, 38, 30);        // NG 판정 칸 (채움)
        public static readonly Color NgText = Color.FromArgb(255, 99, 88);        // NG 글자
        public static readonly Color Amber = Color.FromArgb(240, 180, 0);         // YIELD, 경고
        public static readonly Color LampOff = Color.FromArgb(90, 96, 104);       // 표시등 꺼짐
    }
}
