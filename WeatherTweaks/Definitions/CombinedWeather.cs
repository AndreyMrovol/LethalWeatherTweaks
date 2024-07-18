using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using HarmonyLib;
using WeatherRegistry;
using WeatherTweaks.Definitions;

namespace WeatherTweaks.Definitions
{
  public partial class Types
  {
    public class CombinedWeatherType : WeatherType
    {
      private List<LevelWeatherType> _levelweathertypes = [];
      public List<LevelWeatherType> LevelWeatherTypes
      {
        get { return _levelweathertypes; }
        private set { _levelweathertypes = value; }
      }

      private List<Weather> _weathers = [];
      public List<Weather> Weathers
      {
        get
        {
          if (_weathers.Count == 0)
          {
            _weathers = LevelWeatherTypes.Select(weatherType => WeatherRegistry.WeatherManager.GetWeather(weatherType)).ToList();
          }

          return _weathers;
        }
        private set { _weathers = value; }
      }

      public new float WeightModify = 0.15f;

      public new bool CanWeatherBeApplied(SelectableLevel level)
      {
        if (!Enabled.Value)
        {
          return false;
        }

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

      public CombinedWeatherType(string name, List<LevelWeatherType> weathers, float weightModifier = 0.15f)
        : base(name, CustomWeatherType.Combined)
      {
        if (weathers.Count == 0)
        {
          return;
        }

        Plugin.logger.LogDebug($"Creating CombinedWeatherType: {Name}");

        // Weathers = weathers.Append(baseWeather).Distinct().ToList();
        // Weathers.ForEach(weather =>
        // {
        //   Plugin.logger.LogWarning($"Adding weather effect: {weather}");
        //   Effects.Add(TimeOfDay.Instance.effects[(int)weather]);
        // });

        // WeatherType = new(Name, (LevelWeatherType)baseWeather, Weathers, CustomWeatherType.Combined) { Effects = [], };

        LevelWeatherTypes = weathers.Distinct().ToList();

        Name = name;
        weatherType = weathers[0];

        WeightModify = weightModifier;

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
