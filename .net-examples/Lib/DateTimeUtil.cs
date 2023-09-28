using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeExamples.Lib {
    /**
     * This class converts between .net dates and java millisecond system dates.
     */
    public class DateTimeUtil {

        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /**
         * get the current time in unix-friendly milliseconds.
         */
        public static long GetCurrentUnixTimestampMillis() {
            return (long)(DateTime.UtcNow - UnixEpoch).TotalMilliseconds;
        }

        /**
         * get the current date for the given unix-friendly time in millizeconds
         */
        public static DateTime DateTimeFromUnixTimestampMillis(long millis) {
            return UnixEpoch.AddMilliseconds(millis);
        }

        /**
         * get the current time in unix-friendly seconds.
         */
        public static long GetCurrentUnixTimestampSeconds() {
            return (long)(DateTime.UtcNow - UnixEpoch).TotalSeconds;
        }

         
        /**
         * get tthe current date for the given unix-friendly time in seconds.
         */
        public static DateTime DateTimeFromUnixTimestampSeconds(long seconds) {
            return UnixEpoch.AddSeconds(seconds);
        }

        /**
         * return true if the time is after midnight, and false, otherwise.
         */
        public static bool isAfterMidnight() {
            return DateTime.Now.Hour == 0;
        }

        public static long getEpochMilliseconds() {
            return (long) DateTime.Now.Subtract(DateTime.MinValue.AddYears(1969)).TotalMilliseconds;
        }
    }
}
