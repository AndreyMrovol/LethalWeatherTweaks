using System.Collections.Generic;
using WeatherTweaks.Definitions;
using static WeatherTweaks.Modules.Types;

namespace WeatherTweaks
{
  class WeatherManager
  {
    public static List<Modules.Types.ProgressingWeatherType> RegisteredProgressingWeathers = [];
    public static List<Modules.Types.CombinedWeatherType> RegisteredCombinedWeathers = [];

    public static List<RegisteredWeatherType> RegisteredCustomWeatherTypes = [];

    public static void AddProgressingWeather(ProgressingWeatherType weather)
    {
      RegisteredProgressingWeathers.Add(weather);
      RegisteredCustomWeatherTypes.Add(weather);
    }

    public static void AddCombinedWeather(CombinedWeatherType weather)
    {
      RegisteredCombinedWeathers.Add(weather);
      RegisteredCustomWeatherTypes.Add(weather);
    }
  }
}
