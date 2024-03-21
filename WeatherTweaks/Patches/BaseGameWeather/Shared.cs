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
  partial class BasegameWeatherPatch
  {
    // internal static int patchIndex = 1;

    internal static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("WeatherTweaks BGW");

    internal static IEnumerable<CodeInstruction> CurrentWeatherVariablePatch(
      IEnumerable<CodeInstruction> instructions,
      LevelWeatherType weatherType,
      string wherefrom
    )
    {
      logger.LogInfo($"Patching {wherefrom} for {weatherType}");
      CodeMatcher codeMatcher = new CodeMatcher(instructions);

      codeMatcher = codeMatcher.MatchForward(
        false,
        new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(TimeOfDay), "Instance")),
        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(TimeOfDay), "currentWeatherVariable"))
      );
      logger.LogDebug($"Matched Ldfld for {wherefrom} for {weatherType}");

      logger.LogWarning($"Matches found: {codeMatcher.Length}");
      logger.LogWarning($"Is valid: {codeMatcher.IsValid}");

      codeMatcher.Repeat(match =>
      {
        logger.LogWarning($"Matched Ldfld for {wherefrom} for {weatherType}");

        // Remove original instruction
        codeMatcher.RemoveInstruction(); // removes  call class TimeOfDay  TimeOfDay::get_Instance()
        codeMatcher.RemoveInstruction(); // removes  ldfld float32 TimeOfDay::currentWeatherVariable

        // Get the current weather variable
        // Variables.GetLevelWeatherVariable method takes 2 arguments: int and bool - set them
        codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, (int)weatherType));
        codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, 0));

        codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Variables), "GetLevelWeatherVariable")));
      });

      logger.LogDebug($"Patched {wherefrom} for {weatherType}");
      return codeMatcher.InstructionEnumeration();
    }

    internal static IEnumerable<CodeInstruction> CurrentWeatherVariable2Patch(
      IEnumerable<CodeInstruction> instructions,
      LevelWeatherType weatherType,
      string wherefrom
    )
    {
      logger.LogInfo($"Patching {wherefrom} for {weatherType}");
      CodeMatcher codeMatcher = new CodeMatcher(instructions);

      codeMatcher = codeMatcher.MatchForward(
        false,
        new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(TimeOfDay), "Instance")),
        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(TimeOfDay), "currentWeatherVariable2"))
      );
      logger.LogDebug($"Matched Ldfld for {wherefrom} for {weatherType}");

      logger.LogWarning($"Matches found: {codeMatcher.Length}");
      logger.LogWarning($"Is valid: {codeMatcher.IsValid}");

      codeMatcher.Repeat(match =>
      {
        logger.LogWarning($"Matched Ldfld for {wherefrom} for {weatherType}");

        // Remove original instruction
        codeMatcher.RemoveInstruction(); // removes  call class TimeOfDay  TimeOfDay::get_Instance()
        codeMatcher.RemoveInstruction(); // removes  ldfld float32 TimeOfDay::currentWeatherVariable

        // Get the current weather variable
        // Variables.GetLevelWeatherVariable method takes 2 arguments: int and bool - set them
        codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, (int)weatherType));
        codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, 1));

        codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Variables), "GetLevelWeatherVariable")));
      });

      logger.LogDebug($"Patched {wherefrom} for {weatherType}");
      return codeMatcher.InstructionEnumeration();
    }
  }
}
