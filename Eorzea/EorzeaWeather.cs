using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Eorzea
{
    /// <summary>
    /// エオルゼア天気
    /// </summary>
    /// <see cref="https://github.com/eorzea-weather/node-eorzea-weather"/>
    internal class Weather
    {
        public Weather()
        {

        }

        public string GetWeather(string zoneName, DateTime localDate)
        {
 
            // ACTのZoneNameをZoneIDに変換
            Constants.ZoneId.TryGetValue(zoneName, out string zoneId);

            if (zoneId == null)
            {
                return "Zone not found.";
            }


            Type type = typeof(WeatherChance);
            MethodInfo methodInfo = type.GetMethod(zoneId);

            if (methodInfo != null)
            {
                // 天気を算出
                int chance = CalculateForecastTarget(localDate);
                object[] parameters = new object[] { chance };
                return (string)methodInfo.Invoke(null, parameters);
            }

            return "Zone not found.";
        }


        public int CalculateForecastTarget(DateTime localDate)
        {
            int unixtime = (int)Math.Floor(localDate.ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
           
            // Get Eorzea hour for weather start
            double bell = (double)unixtime / 175;

            // Do the magic 'cause for calculations 16:00 is 0, 00:00 is 8 and 08:00 is 16
            int increment = (int)((bell + 8 - (bell % 8)) % 24);

            // Take Eorzea days since unix epoch
            double totalDays = (double)unixtime / 4200;
            int calcBase = (int)(totalDays * 100 + increment);

            int step1 = (int)(((calcBase << 11) ^ calcBase) & 0xffffffff);
            int step2 = (int)(((step1 >> 8) ^ step1) & 0xffffffff);

            return step2 % 100;

        }
    }

}
