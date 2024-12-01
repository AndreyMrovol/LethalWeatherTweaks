using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx.Logging;
using HarmonyLib;

namespace WeatherTweaks
{
  [HarmonyPatch(typeof(RoundManager))]
  partial class BasegameWeatherPatch
  {
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(RoundManager), "SpawnOutsideHazards")]
    static IEnumerable<CodeInstruction> SpawnOutsideHazardsPatch(IEnumerable<CodeInstruction> instructions)
    {
      CodeMatcher codeMatcher = new(instructions);

      codeMatcher = codeMatcher.MatchForward(
        false,
        new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(TimeOfDay), "Instance")),
        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(TimeOfDay), "currentLevelWeather"))
      );
      logger.LogDebug($"Matched Ldfld for RoundManager.SpawnOutsideHazards");

      codeMatcher.Repeat(match =>
      {
        // Remove original instruction
        codeMatcher.RemoveInstruction(); // removes  call class TimeOfDay  TimeOfDay::get_Instance()
        codeMatcher.RemoveInstruction(); // removes  ldfld float32 TimeOfDay::currentWeatherVariable

        // Get the current weather variable
        // Variables.GetLevelWeatherVariable method takes 2 arguments: int and bool - set them
        codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, (int)LevelWeatherType.Rainy));
        codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Variables), "LevelHasWeather")));
      });

      return codeMatcher.InstructionEnumeration();
    }
  }
}
