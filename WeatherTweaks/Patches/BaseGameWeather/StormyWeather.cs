using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
// using Mono.Cecil.Cil;
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

    // [HarmonyTranspiler]
    //     [HarmonyPatch("OnEnable")]
    //     static public IEnumerable<CodeInstruction> EclipseOnEnablePatch(IEnumerable<CodeInstruction> instructions)
    //     {
    //       var logger = Plugin.logger;

    //       CodeMatcher codeMatcher = new(instructions);

    //       // match the following IL code:

    //       // IL_22: brfalse Label4
    //       // IL_23: ldsfld Malfunctions.Malfunction Malfunctions.State::MalfunctionNavigation
    //       // IL_24: ldfld bool Malfunctions.Malfunction::Notified
    //       // IL_25: brtrue Label5

    //       // and remove everything except IL_25: brtrue Label5
    //       codeMatcher.MatchForward(
    //         false,
    //         // new CodeMatch(OpCodes.Brfalse),
    //         new CodeMatch(OpCodes.Ldsfld),
    //         new CodeMatch(OpCodes.Ldfld),
    //         new CodeMatch(OpCodes.Brtrue)
    //       );

    //       codeMatcher.Repeat(match =>
    //       {
    //         match.RemoveInstructions(3);
    //       });

    //       // logger.LogDebug($"Patched {wherefrom} for {weatherType}");
    //       return codeMatcher.InstructionEnumeration();
    //     }
  }
}
