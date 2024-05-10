using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using HarmonyLib;
using WeatherTweaks.Definitions;

namespace WeatherTweaks.Modules
{
  partial class Types
  {
    public abstract class CombinedWeatherType
    {
      public CombinedWeatherType(string name, List<LevelWeatherType> weathers, float weightModify = 1f)
      {
        Plugin.logger.LogDebug($"Creating CombinedWeatherType: {name}");

        // Weathers = weathers.Append(baseWeather).Distinct().ToList();
        // Weathers.ForEach(weather =>
        // {
        //   Plugin.logger.LogWarning($"Adding weather effect: {weather}");
        //   Effects.Add(TimeOfDay.Instance.effects[(int)weather]);
        // });

        // WeatherType = new(Name, (LevelWeatherType)baseWeather, Weathers, CustomWeatherType.Combined) { Effects = [], }

        // List<Weather> weathersToCombine = Variables
        //   .Weathers.Where(weather => weathers.Contains(weather.VanillaWeatherType) && weather.WeatherType == Type.Vanilla)
        //   .ToList();

        new Definitions.Types.CombinedWeatherType(name, weathers) { weightModify = weightModify };
      }
    }
  }
}
