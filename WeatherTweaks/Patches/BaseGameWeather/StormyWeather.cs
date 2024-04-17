using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace WeatherTweaks
{
  [HarmonyPatch(typeof(StormyWeather))]
  partial class BasegameWeatherPatch
  {
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(StormyWeather), "DetermineNextStrikeInterval")]
    static IEnumerable<CodeInstruction> StormyDetermineNextStrikePatch(IEnumerable<CodeInstruction> instructions)
    {
      return CurrentWeatherVariablePatch(instructions, LevelWeatherType.Stormy, "StormyWeather.DetermineNextStrikeInterval");
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(StormyWeather), "LightningStrikeRandom")]
    static IEnumerable<CodeInstruction> StormyLightningStrikeRandomPatch(IEnumerable<CodeInstruction> instructions)
    {
      return CurrentWeatherVariablePatch(instructions, LevelWeatherType.Stormy, "StormyWeather.StormyLightningStrikeRandomPatch");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(StormyWeather), "OnEnable")]
    static void StormyOnEnablePostfix(StormyWeather __instance)
    {
      __instance.timeAtLastStrike = TimeOfDay.Instance.globalTime + 25f;

      logger.LogWarning($"StormyWeather.Enable: {__instance.randomThunderTime} {__instance.timeAtLastStrike}");
    }
  }
}
