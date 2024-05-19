using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using HarmonyLib;
using Newtonsoft.Json;
using WeatherTweaks.Definitions;
using static WeatherTweaks.Definitions.Types;

namespace WeatherTweaks.Modules
{
  partial class Types
  {
    public class ProgressingWeatherType : RegisteredWeatherType
    {
      public List<ProgressingWeatherEntry> WeatherEntries = [];
      public LevelWeatherType StartingWeather;

      public ProgressingWeatherType(
        string name,
        LevelWeatherType baseWeather,
        List<ProgressingWeatherEntry> weatherEntries,
        float weightModify = 1f
      )
      {
        Name = name;
        StartingWeather = baseWeather;
        WeatherEntries = weatherEntries;
        WeightModify = weightModify;
        Type = CustomWeatherType.Progressing;
        weatherType = baseWeather;

        WeatherManager.AddProgressingWeather(this);
      }
    }
  }
}
