using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BookMeMobi2.MobiMetadata.Utilities
{
    public static class StringExtension
    {
        public static string ReplaceSpecialChars(this string text)
        {
            var s = text;
            s = s.Replace("&#8211;", "-");
            //// smart double quotes
            //s = Regex.Replace(s, "[\u201C\u201D\u201E]", "\"");
            //// ellipsis
            //s = Regex.Replace(s, "\u2026", "...");
            //// dashes
            //s = Regex.Replace(s, "[\u2013\u2014]", "-");
            //// circumflex
            //s = Regex.Replace(s, "\u02C6", "^");
            //// open angle bracket
            //s = Regex.Replace(s, "\u2039", "<");
            //// close angle bracket
            //s = Regex.Replace(s, "\u203A", ">");
            //// spaces
            //s = Regex.Replace(s, "[\u02DC\u00A0]", " ");

            return s;
        }
        public static string ReplaceNullTerminatorWithEmpty(this string source)
        {
            return source.Replace("\0", "");
        }
    }
}
