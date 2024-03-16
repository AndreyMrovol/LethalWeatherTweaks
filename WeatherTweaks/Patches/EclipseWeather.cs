using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using MonoMod.Cil;

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
