using System;

namespace BookMeMobi2.Helpers.Extensions
{
    public static class DateTimeExtension
    {
        public static long ToUnixTimeStamp(this DateTime date){
            DateTime sTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (long)(date - sTime).TotalSeconds;
        }
    }
}