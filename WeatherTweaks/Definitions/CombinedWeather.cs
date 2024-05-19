using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using HarmonyLib;
using WeatherTweaks.Definitions;

namespace WeatherTweaks.Definitions
{
  partial class Types
  {
    public class CombinedWeatherType : WeatherType
    {
      public List<Weather> Weathers = [];

      public new float weightModify = 0.15f;

      public new bool CanWeatherBeApplied(SelectableLevel level)
      {
        var randomWeathers = level.randomWeathers;
        List<LevelWeatherType> remainingWeathers = Weathers.Select(weather => weather.VanillaWeatherType).ToList();

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

      public CombinedWeatherType(string name, List<Weather> weathers)
        : base(name, CustomWeatherType.Combined)
      {
        Plugin.logger.LogDebug($"Creating CombinedWeatherType: {Name}");

        // Weathers = weathers.Append(baseWeather).Distinct().ToList();
        // Weathers.ForEach(weather =>
        // {
        //   Plugin.logger.LogWarning($"Adding weather effect: {weather}");
        //   Effects.Add(TimeOfDay.Instance.effects[(int)weather]);
        // });

        // WeatherType = new(Name, (LevelWeatherType)baseWeather, Weathers, CustomWeatherType.Combined) { Effects = [], };

        Name = name;
        Weathers = weathers.Distinct().ToList();

        // TODO
        // create configFile bindings
        Enabled = ConfigManager.Instance.configFile.Bind("1b> Combined mechanics", $"{Name} Enabled", true, $"Enable {Name} combined weather");
        // Weight = ConfigManager.Instance.configFile.Bind(
        //   "1b> Combined mechanics",
        //   $"{Name} Weight",
        //   weightModify,
        //   $"Weight of {Name} combined weather"
        // );

        Variables.CombinedWeatherTypes.Add(this);
        // Variables.WeatherTypes.Add(this);
      }
    }
  }
}
