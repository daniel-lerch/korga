using System;

namespace Korga.Server.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime? NullIfDefault(this DateTime dateTime)
        {
            return dateTime == default ? null : dateTime;
        }
    }
}
