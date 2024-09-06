using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinJPTextSpeaker
{
    internal static class StringHelpers
    {
        public static bool IgnoreCaseEquals(this string str1, string str2)
        {
            return str1?.ToLower() == str2?.ToLower();
        }

        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static bool OrEquals(this string target, params string[] array)
        {
            foreach (var s in array)
            {
                if (target == s)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsNullOrWhiteSpace(this string s)
        {
            return string.IsNullOrWhiteSpace(s);
        }

        public static bool IgnoreCaseStartsWith(this string x, string y)
        {
            if (x == null || y == null)
            {
                return false;
            }

            return x.ToLower().StartsWith(y.ToLower());
        }

        public static bool IgnoreCaseEndsWith(this string x, string y)
        {
            if (x == null || y == null)
            {
                return false;
            }

            return x.ToLower().EndsWith(y.ToLower());
        }

        public static bool IgnoreCaseContains(this string x, string y)
        {
            if (x == null || y == null)
            {
                return false;
            }

            return x.ToLower().Contains(y.ToLower());
        }
    }
}
