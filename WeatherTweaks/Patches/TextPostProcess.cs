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
    internal static MrovLib.Logger logger = new("WeatherTweaks Terminal", ConfigManager.LogLogs);

    [HarmonyPatch("TextPostProcess")]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.VeryHigh)]
    private static bool PatchGameMethod(ref string modifiedDisplayText, TerminalNode node)
    {
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

      return true;
    }
  }
}
