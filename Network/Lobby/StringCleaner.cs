using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public static class StringCleaner
{
    public static string Clean(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        s = s.Normalize(NormalizationForm.FormC);
        s = Regex.Replace(s, @"\s+", "");      // 공백류 제거
        s = Regex.Replace(s, @"\p{C}", "");    // 제어/Zero-Width 제거 (U+200B 포함)
        return s;
    }
}
