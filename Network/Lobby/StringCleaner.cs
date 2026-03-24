using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// 문자열 정리 유틸 클래스
/// </summary>
public static class StringCleaner
{
    /// <summary>
    /// 입력 문자열을 정리하여 반환
    /// </summary>
    public static string Clean(string s)
    {
        // null 또는 빈 문자열이면 그대로 반환
        if (string.IsNullOrEmpty(s)) return s;

        // Unicode 정규화
        s = s.Normalize(NormalizationForm.FormC);

        // 공백 제거 ex)스페이스, 탭, 줄바꿈 등
        s = Regex.Replace(s, @"\s+", "");

        // 제어 문자 / Zero-Width 문자 제거
        s = Regex.Replace(s, @"\p{C}", "");

        return s;
    }
}
