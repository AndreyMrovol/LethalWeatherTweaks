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
    internal static MrovLib.Logger logger = new("WeatherTweaks BaseGameWeatherPatches", ConfigManager.LogLogs);
    internal static Harmony harmony = new("WeatherTweaks.BaseGame");

    internal static IEnumerable<CodeInstruction> VariablePatch(
      IEnumerable<CodeInstruction> instructions,
      LevelWeatherType weatherType,
      string wherefrom,
      bool variable1 = true
    )
    {
      logger.LogInfo($"Patching {wherefrom} for {weatherType}");
      CodeMatcher codeMatcher = new CodeMatcher(instructions);

      CodeMatch var1 = new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(TimeOfDay), "currentWeatherVariable"));
      CodeMatch var2 = new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(TimeOfDay), "currentWeatherVariable2"));

      codeMatcher = codeMatcher.MatchForward(
        false,
        new CodeMatch(OpCodes.Call, AccessTools.Field(typeof(TimeOfDay), "Instance")),
        variable1 ? var1 : var2 // var1 for variable1, var2 for variable2
      );
      logger.LogDebug($"Matched Ldfld for {wherefrom} for {weatherType}");

      codeMatcher.Repeat(match =>
      {
        logger.LogInfo($"Matched Ldfld for {wherefrom} for {weatherType}");

        // Remove original instruction
        codeMatcher.RemoveInstruction(); // removes  call class TimeOfDay  TimeOfDay::get_Instance()
        codeMatcher.RemoveInstruction(); // removes  ldfld float32 TimeOfDay::currentWeatherVariable

        // Get the current weather variable
        // Variables.GetLevelWeatherVariable method takes 2 arguments: int and bool - set them
        codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, (int)weatherType));
        codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, variable1 ? 0 : 1)); // 0 for var1, 1 for var2

        codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Variables), "GetLevelWeatherVariable")));
      });

      logger.LogDebug($"Patched {wherefrom} for {weatherType}");
      return codeMatcher.InstructionEnumeration();
    }

    internal static IEnumerable<CodeInstruction> CurrentWeatherVariablePatch(
      IEnumerable<CodeInstruction> instructions,
      LevelWeatherType weatherType,
      string wherefrom
    )
    {
      return VariablePatch(instructions, weatherType, wherefrom, true);
    }

    internal static IEnumerable<CodeInstruction> CurrentWeatherVariable2Patch(
      IEnumerable<CodeInstruction> instructions,
      LevelWeatherType weatherType,
      string wherefrom
    )
    {
      return VariablePatch(instructions, weatherType, wherefrom, false);
    }
  }
}
