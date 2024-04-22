using System.Linq;
using BepInEx.Configuration;

namespace WeatherTweaks
{
  class UncertainTypes
  {
    public class Uncertain : Modules.Types.UncertainWeatherType
    {
      public Uncertain()
        : base("Uncertain") { }

      public override string CreateUncertaintyString(SelectableLevel level, System.Random random)
      {
        var weather = level.currentWeather;
        var randomWeathers = level.randomWeathers.Where(w => w.weatherType != weather).ToList();

        if (randomWeathers.Count == 0)
        {
          return weather.ToString();
        }

        var randomWeather = randomWeathers[random.Next(randomWeathers.Count)];

        if (random.Next(0, 3) == 0)
        {
          return $"{randomWeather.weatherType}?";
        }
        else
        {
          return $"{weather}?";
        }
      }
    }

    public class Uncertain5050 : Modules.Types.UncertainWeatherType
    {
      public Uncertain5050()
        : base("Uncertain5050") { }

      public override string CreateUncertaintyString(SelectableLevel level, System.Random random)
      {
        var weather = level.currentWeather;
        var randomWeathers = level.randomWeathers.Where(w => w.weatherType != weather).ToList();

        if (randomWeathers.Count == 0)
        {
          return weather.ToString();
        }

        var randomWeather = randomWeathers[random.Next(randomWeathers.Count)];

        if (random.Next(0, 1) == 0)
        {
          return $"{weather}/{randomWeather.weatherType}";
        }
        else
        {
          return $"{randomWeather.weatherType}/{weather}";
        }
      }
    }

    public class Unknown : Modules.Types.UncertainWeatherType
    {
      public Unknown()
        : base("Unknown") { }

      public override string CreateUncertaintyString(SelectableLevel level, System.Random random)
      {
        return "[UNKNOWN]";
      }
    }
  }
}
