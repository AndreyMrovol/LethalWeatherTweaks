using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using WeatherTweaks.Definitions;

namespace WeatherTweaks
{
  public enum CustomWeatherType
  {
    Normal,
    Combined,
    Progressing
  }

  [JsonObject(MemberSerialization.OptIn)]
  public class WeatherType
  {
    [JsonProperty]
    public string Name;

    [JsonProperty]
    public List<Weather> Weathers;

    [JsonProperty]
    public Weather Weather;

    public bool IsOneWeather = false;

    public LevelWeatherType weatherType;

    [JsonProperty]
    public CustomWeatherType Type;

    public bool CanCombinedWeatherBeApplied(SelectableLevel level)
    {
      var possibleWeathers = Variables.LevelWeathers.Where(weather => weather.Level == level).ToList();

      foreach (Weather weather in Weathers)
      {
        if (possibleWeathers.Any(possibleWeather => possibleWeather.Weather == weather))
        {
          possibleWeathers.Remove(possibleWeathers.First(possibleWeather => possibleWeather.Weather == weather));
        }
      }

      return possibleWeathers.Count == 0;
    }

    public WeatherType(string name, List<Weather> weathers, CustomWeatherType type)
    {
      Name = name;
      // Weathers = weathers;
      Type = type;

      // if (weathers.Count == 1)
      // {
      // Weather = weathers[0];
      //   IsOneWeather = true;
      // }else{

      // }

      Weathers = weathers;

      // weatherType = Weather.VanillaWeatherType;

      // Variables.WeatherTypes.Add(this);
    }
  }
}
