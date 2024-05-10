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
    public class ProgressingWeatherType
    {
      public ProgressingWeatherType(
        string name,
        LevelWeatherType baseWeather,
        List<ProgressingWeatherEntry> weatherEntries,
        float weightModify = 1f
      )
      {
        new Definitions.Types.ProgressingWeatherType(name, baseWeather, weatherEntries) { weightModify = weightModify };
      }
    }
  }
}
