using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using HarmonyLib;
using WeatherTweaks.Definitions;

namespace WeatherTweaks.Modules
{
  partial class Types
  {
    public abstract class CombinedWeatherType : RegisteredWeatherType
    {
      public List<LevelWeatherType> Weathers = [];

      public CombinedWeatherType(string name, List<LevelWeatherType> weathers, float weightModify = 1f)
      {
        Plugin.logger.LogDebug($"Creating CombinedWeatherType: {name}");

        Name = name;
        Weathers = weathers;
        WeightModify = weightModify;
        Type = CustomWeatherType.Combined;

        WeatherManager.AddCombinedWeather(this);
      }
    }
  }
}
