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
    private static bool PatchGameMethod(ref string modifiedDisplayText, TerminalNode node)
    {
      if (node.buyRerouteToMoon == -2)
      {
        // Re-route dialog

        logger.LogDebug("buyRerouteToMoon == -2");
        Regex regex = new Regex(@"\ It is (\n)*currently.+\[currentPlanetTime].+");

        if (regex.IsMatch(modifiedDisplayText))
        {
          modifiedDisplayText = regex.Replace(modifiedDisplayText, "");
        }
      }

      return true;
    }
  }
}
