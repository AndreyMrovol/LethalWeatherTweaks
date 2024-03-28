using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using MonoMod.Cil;
using WeatherTweaks;

namespace WeatherTweaks
{
  [HarmonyPatch(typeof(FloodWeather))]
  partial class BasegameWeatherPatch
  {
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(FloodWeather), "OnEnable")]
    static IEnumerable<CodeInstruction> FloodedOnEnablePatch(IEnumerable<CodeInstruction> instructions)
    {
      return CurrentWeatherVariablePatch(instructions, LevelWeatherType.Flooded, "FloodWeather.OnEnable");
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(FloodWeather), "OnGlobalTimeSync")]
    static IEnumerable<CodeInstruction> FloodedOnGlobalTimeSyncPatch(IEnumerable<CodeInstruction> instructions)
    {
      return CurrentWeatherVariable2Patch(instructions, LevelWeatherType.Flooded, "FloodWeather.OnGlobalTimeSync");
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(FloodWeather), "Update")]
    static IEnumerable<CodeInstruction> FloodedUpdatePatch(IEnumerable<CodeInstruction> instructions)
    {
      return CurrentWeatherVariablePatch(instructions, LevelWeatherType.Flooded, "FloodWeather.Update");
    }
  }
}
