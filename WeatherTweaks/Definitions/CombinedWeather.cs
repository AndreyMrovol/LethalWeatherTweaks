using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using HarmonyLib;

namespace WeatherTweaks.Modules
{
  partial class Types
  {
    public abstract class CombinedWeatherType
    {
      public string Name;

      public List<LevelWeatherType> Weathers = [];
      public List<WeatherEffect> Effects = [];
      public WeatherType WeatherType;

      public float weightModify = 0.3f;

      public List<RandomWeatherWithVariables> GetWeatherVariables(SelectableLevel level)
      {
        var weathers = new List<RandomWeatherWithVariables>();
        level
          .randomWeathers.ToList()
          .ForEach(weather =>
          {
            if (this.Weathers.Contains(weather.weatherType))
            {
              weathers.Add(weather);
            }
          });

        return weathers.ToList();
      }

      public bool CanCombinedWeatherBeApplied(SelectableLevel level)
      {
        var randomWeathers = level.randomWeathers;
        List<LevelWeatherType> remainingWeathers = Weathers.ToList();

        foreach (RandomWeatherWithVariables weather in randomWeathers)
        {
          if (remainingWeathers.Contains(weather.weatherType))
          {
            remainingWeathers.Remove(weather.weatherType);
          }
        }

        return remainingWeathers.Count == 0;
      }

      public ConfigEntry<bool> Enabled;

      public CombinedWeatherType(string name, List<LevelWeatherType> weathers, LevelWeatherType baseWeather)
      {
        Name = name;

        Plugin.logger.LogDebug($"Creating CombinedWeatherType: {Name}");

        Weathers = weathers.Append(baseWeather).Distinct().ToList();
        // Weathers.ForEach(weather =>
        // {
        //   Plugin.logger.LogWarning($"Adding weather effect: {weather}");
        //   Effects.Add(TimeOfDay.Instance.effects[(int)weather]);
        // });

        WeatherType = new()
        {
          Name = Name,
          Effects = [],
          Weathers = Weathers,
          weatherType = (LevelWeatherType)baseWeather,
          Type = CustomWeatherType.Combined
        };

        // TODO
        // create configFile bindings
        Enabled = ConfigManager.Instance.configFile.Bind("1b> Combined mechanics", $"{Name} Enabled", true, $"Enable {Name} combined weather");

        Variables.CombinedWeatherTypes.Add(this);
      }
    }
  }
}
