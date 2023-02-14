using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eorzea
{
    /// <summary>
    /// エオルゼア時間計算クラス  
    /// </summary>
    /// <see cref="http://blog.livedoor.jp/eomap/archives/1005236796.html"/>

    public class Clock
    {
        public int MONTHS_PER_YEAR { get; set; }
        public int DATES_PER_MONTH { get; set; }
        public int HOURS_PER_DATE { get; set; }
        public int MINUTES_PER_HOUR { get; set; }
        public int SECONDS_PER_MINUTE { get; set; }
        public int MILLISECONDS_PER_SECONDS { get; set; }
        public double EORZEA_PER_LOCAL { get; set; }
        public long EORZEA_MILLISECONDS { get; set; }
        public long MILLISECONDS_PER_HOUR { get; set; }
        public long MILLISECONDS_PER_MINUTE { get; set; }
        public long UNIX { get; set; }
        public int Version { get; set; }


        public Clock()
        {
            Version = 1;

            MONTHS_PER_YEAR = 12;
            DATES_PER_MONTH = 32;
            HOURS_PER_DATE = 24;
            MINUTES_PER_HOUR = 60;
            SECONDS_PER_MINUTE = 60;
            MILLISECONDS_PER_SECONDS = 1000;

            // エオルゼア時間の1日（60分*24時間 = 1440分） = 地球時間70分
            EORZEA_PER_LOCAL =  (1440.0 / 70.0);

            EORZEA_MILLISECONDS = 0;

            UNIX = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            EORZEA_MILLISECONDS = (long)(UNIX * EORZEA_PER_LOCAL);

            MILLISECONDS_PER_MINUTE = MILLISECONDS_PER_SECONDS * SECONDS_PER_MINUTE;

            MILLISECONDS_PER_HOUR = MILLISECONDS_PER_MINUTE * MINUTES_PER_HOUR;
        }

        /// <summary>
        /// クラス内で保持しているローカル時間を基準にして計算する
        /// </summary>
        /// <returns>hour:min</returns>
        public string GetLocalbaseET()
        {
            long hour = (EORZEA_MILLISECONDS / MILLISECONDS_PER_HOUR) % HOURS_PER_DATE;
            long min = (EORZEA_MILLISECONDS / MILLISECONDS_PER_MINUTE) % MINUTES_PER_HOUR;

            return "[ET]" + hour + ":" + min;
        }

        /// <summary>
        /// パラメーターで渡されたDateTimeを基準にして計算する
        /// </summary>
        /// <param name="currentLocalTime">基準にする時間</param>
        /// <returns>hour:min</returns>
        public string GetCurrentET(DateTime currentLocalTime)
        {

            // @see https://yukimemo.hatenadiary.jp/entry/2014/05/24/005417
            DateTime UNIX_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            double nowTicks = (currentLocalTime.ToUniversalTime() - UNIX_EPOCH).TotalMilliseconds;
            double unixTimestamp = nowTicks * EORZEA_PER_LOCAL;

            double hour2 = Math.Floor(unixTimestamp / MILLISECONDS_PER_HOUR) % HOURS_PER_DATE;
            double min2 = Math.Floor(unixTimestamp / MILLISECONDS_PER_MINUTE) % MINUTES_PER_HOUR;

            return "[ET]" + hour2 + ":" + min2; 
        }
    }
}
