using System.Linq;
using WeatherRegistry;

namespace WeatherTweaks
{
  class UncertainTypes
  {
    public class Uncertain : Modules.UncertainWeatherType
    {
      public Uncertain()
        : base("Uncertain") { }

      public override string CreateUncertaintyString(SelectableLevel level, System.Random random)
      {
        Weather weather = WeatherManager.GetWeather(level.currentWeather);
        var randomWeathers = level.randomWeathers.Where(w => w.weatherType != weather.VanillaWeatherType).ToList();

        if (randomWeathers.Count == 0)
        {
          return weather.name;
        }

        Weather randomWeather = WeatherManager.GetWeather(randomWeathers[random.Next(randomWeathers.Count)].weatherType);

        if (random.Next(0, 3) == 0)
        {
          return $"{randomWeather.name}?";
        }
        else
        {
          return $"{weather.name}?";
        }
      }
    }

    public class Uncertain5050 : Modules.UncertainWeatherType
    {
      public Uncertain5050()
        : base("Uncertain5050") { }

      public override string CreateUncertaintyString(SelectableLevel level, System.Random random)
      {
        Weather weather = WeatherManager.GetWeather(level.currentWeather);
        var randomWeathers = level.randomWeathers.Where(w => w.weatherType != weather.VanillaWeatherType).ToList();

        if (randomWeathers.Count == 0)
        {
          return weather.ToString();
        }

        Weather randomWeather = WeatherManager.GetWeather(randomWeathers[random.Next(randomWeathers.Count)].weatherType);

        if (random.Next(0, 2) == 0)
        {
          return $"{weather.name}/{randomWeather.name}";
        }
        else
        {
          return $"{randomWeather.name}/{weather.name}";
        }
      }
    }

    public class Unknown : Modules.UncertainWeatherType
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
