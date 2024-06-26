using System.Text;
using System.Text.RegularExpressions;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace WeatherTweaks
{
  [HarmonyPatch(typeof(Terminal))]
  public static class TextPostProcessPatch
  {
    internal static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("WeatherTweaks Terminal");

    [HarmonyPatch("TextPostProcess")]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.VeryHigh)]
    private static bool PatchGameMethod(ref string modifiedDisplayText, TerminalNode node)
    {
      if (ConfigManager.TerminalForcePatch.Value)
      {
        logger.LogInfo("Removing terminal weather formatting");
        if (node.buyRerouteToMoon == -2)
        {
          // Re-route dialog

          logger.LogDebug("buyRerouteToMoon == -2");
          Regex regex = new(@"\ It is (\n)*currently.+\[currentPlanetTime].+");

          if (regex.IsMatch(modifiedDisplayText))
          {
            modifiedDisplayText = regex.Replace(modifiedDisplayText, "");
          }
        }

        if (node.name == "MoonsCatalogue")
        {
          Regex regex = new(@"\[planetTime\]");
          modifiedDisplayText = regex.Replace(modifiedDisplayText, "");
        }
      }

      return true;
    }
  }
}
