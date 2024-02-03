using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HarmonyLib;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

namespace WeatherTweaks
{
  [HarmonyPatch(typeof(Terminal))]
  public static class TextPostProcessPatch
  {
    [HarmonyPatch("TextPostProcess")]
    [HarmonyPostfix]
    private static string GameMethodPatch(string __result)
    {
      if (!ConfigManager.TerminalPatchEnabled.Value)
      {
        return __result;
      }

      if (!__result.Contains("Experimentation"))
      {
        return __result;
      }

      // split string by new line
      string[] lines = __result.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

      // line format example : "* Assurance (Rainy)", "* EGypt (Eclipsed)", * March (Eclipsed)
      // we need to remove the weather type from the string, but add it's uncertain version (if available)

      for (int i = 0; i < lines.Length; i++)
      {
        string line = lines[i];
        string[] parts = line.Split(' ');

        if (parts.Length > 1)
        {
          string planetName = parts[1];
          // check which planet has the same name

          if (!Variables.PlanetNames.ContainsKey(planetName))
          {
            Plugin.logger.LogDebug($"Planet name {planetName} not found in the list of planets");
            continue;
          }

          Plugin.logger.LogDebug($"Checking weather type for {planetName}");
          var level = StartOfRound.Instance.levels.FirstOrDefault(l => l.PlanetName.Contains(parts[1]));

          Plugin.logger.LogDebug($"Checking weather type for {level.PlanetName}");

          if (UncertainWeather.uncertainWeathers.ContainsKey(level.PlanetName))
          {
            Plugin.logger.LogDebug($"Replacing weather type for {level.PlanetName} with {UncertainWeather.uncertainWeathers[level.PlanetName]}");
            string uncertainWeather = UncertainWeather.uncertainWeathers[level.PlanetName];

            // check if uncertainweather has [ or ] or < or > in it
            if (!Regex.IsMatch(uncertainWeather, @"\[|\]|\<|\>"))
            {
              uncertainWeather = $"({uncertainWeather})";
            }

            lines[i] = $"{parts[0]} {Regex.Replace(level.PlanetName, @"\d", "").Trim()} {uncertainWeather}";
          }
        }
      }
      __result = string.Join("\n", lines);

      return __result;
    }
  }
}
