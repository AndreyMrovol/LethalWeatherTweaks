using BepInEx.Configuration;
using WeatherRegistry;

namespace WeatherTweaks
{
  public class ConfigHelper
  {
    public static Weather GetWeatherFromString(string weatherName)
    {
      return WeatherRegistry.ConfigHelper.ResolveStringToWeather(weatherName);
    }
  }
}
