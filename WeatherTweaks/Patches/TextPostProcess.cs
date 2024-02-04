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

      if (node.name == "MoonsCatalogue")
      {
        // Moon catalogue

        logger.LogDebug("Moon catalogue");
        Regex regex = new Regex(@"(?:\[companyBuyingPercent\]\.)(.|\n)*$");
        Regex planetnameRegex = new Regex(@"\d+\ *");

        if (regex.IsMatch(modifiedDisplayText))
        {
          modifiedDisplayText = regex.Replace(modifiedDisplayText, "");
        }
        else
        {
          return true;
        }

        StringBuilder stringBuilder = new();
        stringBuilder.Append(modifiedDisplayText);
        stringBuilder.Append($"{Mathf.RoundToInt(StartOfRound.Instance.companyBuyingRate * 100f)}%");
        stringBuilder.Append("\n\n");

        var levels = Variables.GameLevels;
        foreach (var level in levels)
        {
          string currentWeather = UncertainWeather.uncertainWeathers.ContainsKey(level.PlanetName)
            ? UncertainWeather.uncertainWeathers[level.PlanetName]
            : level.currentWeather.ToString();

          if (currentWeather == "None")
          {
            currentWeather = "";
          }
          else if (currentWeather.Contains("[") || currentWeather.Contains("]"))
          {
            //
          }
          else
          {
            currentWeather = $"({currentWeather})";
          }

          stringBuilder.Append($"* {planetnameRegex.Replace(level.PlanetName, "")} {currentWeather}");
          stringBuilder.Append("\r\n");
        }

        modifiedDisplayText = stringBuilder.ToString();
      }

      return true;
    }
  }
}
