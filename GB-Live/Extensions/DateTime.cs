using System;

namespace GB_Live.Extensions
{
    public static class DateTimeExt
    {
        public static bool Between(this DateTime dateTime, DateTime low, DateTime high)
        {
            if (dateTime == null) { throw new ArgumentNullException(nameof(dateTime)); }

            return dateTime >= low && dateTime <= high;
        }
    }
}
