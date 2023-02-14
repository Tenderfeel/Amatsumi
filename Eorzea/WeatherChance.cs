using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eorzea
{
    /// <summary>
    /// エリア毎の天気を判別する
    /// </summary>
    /// <see cref="https://github.com/eorzea-weather/node-eorzea-weather"/>
    internal class WeatherChance
    {
        /// <summary>
        /// リムサロミンサの天気
        /// </summary>
        /// <param name="chance">CalculateForecastTargetの戻り値</param>
        /// <returns>天気のID文字列</returns>
        public static string LimsaLominsa(int chance)
        {
            if (chance < 20)
            {
                return Constants.WEATHER_CLOUDS;
            }
            if (chance < 50)
            {
                return Constants.WEATHER_CLEAR_SKIES;
            }
            if (chance < 80)
            {
                return Constants.WEATHER_FAIR_SKIES;
            }
            if (chance < 90)
            {
                return Constants.WEATHER_FOG;
            }
            return Constants.WEATHER_RAIN;
        }

        /// <summary>
        /// イディルシャイアの天気
        /// </summary>
        /// <param name="chance">CalculateForecastTargetの戻り値</param>
        /// <returns>天気のID文字列</returns>
        public static string Idyllshire(int chance)
        {
            if (chance < 10)
            {
                return Constants.WEATHER_CLOUDS;
            }
            if (chance < 20)
            {
                return Constants.WEATHER_FOG;
            }
            if (chance < 30)
            {
                return Constants.WEATHER_RAIN;
            }
            if (chance < 40)
            {
                return Constants.WEATHER_SHOWERS;
            }
            if (chance < 70)
            {
                return Constants.WEATHER_CLEAR_SKIES;
            }
            return Constants.WEATHER_FAIR_SKIES;
        }
    }
}
