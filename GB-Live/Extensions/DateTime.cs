using System;

namespace GB_Live.Extensions
{
    public static class DateTimeExt
    {
        public static bool Between(this DateTime dt, DateTime low, DateTime high)
        {
            if (dt == null) { throw new ArgumentNullException(nameof(dt)); }

            return dt >= low && dt <= high;
        }
    }
}
