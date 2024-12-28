using System.Collections.Generic;
using HarmonyLib;

namespace WeatherTweaks
{
  [HarmonyPatch(typeof(EclipseWeather))]
  partial class BasegameWeatherPatch
  {
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(EclipseWeather), "OnEnable")]
    static IEnumerable<CodeInstruction> EclipseOnEnablePatch(IEnumerable<CodeInstruction> instructions)
    {
      return CurrentWeatherVariablePatch(instructions, LevelWeatherType.Eclipsed, "EclipseWeather.OnEnable");
    }
  }
}
