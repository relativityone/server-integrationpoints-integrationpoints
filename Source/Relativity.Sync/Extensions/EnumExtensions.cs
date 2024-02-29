using System;
using System.Linq;

namespace Relativity.Sync.Extensions
{
    internal static class EnumExtensions
    {
        public static bool IsIn<T>(this T value, params T[] inclusions) where T : Enum
        {
            if (value == null || inclusions == null)
            {
                return false;
            }

            return inclusions.Contains(value);
        }
    }
}
