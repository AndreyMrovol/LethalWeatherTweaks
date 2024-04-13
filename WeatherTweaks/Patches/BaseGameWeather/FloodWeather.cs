using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using MonoMod.Cil;
using UnityEngine;
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

    [HarmonyPostfix]
    [HarmonyPatch(typeof(FloodWeather), "OnEnable")]
    static void FloodedOnEnablePostfix(FloodWeather __instance)
    {
      __instance.floodLevelOffset =
        Mathf.Clamp(TimeOfDay.Instance.globalTime / 1080f, 0.0f, 100f) * Variables.GetLevelWeatherVariable(LevelWeatherType.Flooded, true);

      logger.LogWarning($"Enabling FloodWeather with level offset {__instance.floodLevelOffset}");
    }
  }
}
