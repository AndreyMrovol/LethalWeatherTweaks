using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;

namespace WeatherTweaks.Patches
{
  class Malfunctions
  {
    static Type StartOfRoundPatches;
    static Assembly assembly;
    static Harmony harmony;

    internal static void Init()
    {
      assembly = Chainloader.PluginInfos["com.zealsprince.malfunctions"].Instance.GetType().Assembly;

      StartOfRoundPatches = AccessTools.TypeByName("Malfunctions.Patches.StartOfRoundPatches");

      if (StartOfRoundPatches == null)
      {
        Plugin.logger.LogError("Could not find StartOfRoundPatches class in Malfunctions assembly");
      }
      else
      {
        Plugin.logger.LogDebug("Found StartOfRoundPatches class in Malfunctions assembly");
      }

      // patch
      harmony = new Harmony("WeatherTweaks.Malfunctions");
      HarmonyMethod transpiler = new HarmonyMethod(typeof(Malfunctions).GetMethod("EclipseOnEnablePatch"));
      harmony.Patch(AccessTools.Method(StartOfRoundPatches, "OverwriteMapScreenInfo"), transpiler: transpiler);
    }

    // [HarmonyTranspiler]
    // [HarmonyPatch(typeof(Malfunctions.Patches.StartOfRoundPatches), "OverwriteMapScreenInfo")]
    static public IEnumerable<CodeInstruction> EclipseOnEnablePatch(IEnumerable<CodeInstruction> instructions)
    {
      var logger = Plugin.logger;

      CodeMatcher codeMatcher = new(instructions);

      // match the following IL code:

      // IL_22: brfalse Label4
      // IL_23: ldsfld Malfunctions.Malfunction Malfunctions.State::MalfunctionNavigation
      // IL_24: ldfld bool Malfunctions.Malfunction::Notified
      // IL_25: brtrue Label5

      // and remove everything except IL_25: brtrue Label5
      codeMatcher.MatchForward(
        false,
        // new CodeMatch(OpCodes.Brfalse),
        new CodeMatch(OpCodes.Ldsfld),
        new CodeMatch(OpCodes.Ldfld),
        new CodeMatch(OpCodes.Brtrue)
      );

      codeMatcher.Repeat(match =>
      {
        match.RemoveInstructions(3);
      });

      // logger.LogDebug($"Patched {wherefrom} for {weatherType}");
      return codeMatcher.InstructionEnumeration();
    }
  }
}
