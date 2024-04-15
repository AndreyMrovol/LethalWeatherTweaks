using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using BepInEx.Logging;
using HarmonyLib;
using LethalLevelLoader;
using UnityEngine;
using UnityEngine.UI;

namespace WeatherTweaks.Patches
{
  public static class LLL
  {
    internal static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("WeatherTweaks LLL");

    internal static void Init()
    {
      var harmony = new Harmony("WeatherTweaks.LLL");

      // Patch the GetWeatherConditions method
      harmony.Patch(
        AccessTools.Method(typeof(TerminalManager), "GetWeatherConditions"),
        postfix: new HarmonyMethod(typeof(Patches.LLL), "PatchLLL")
      );

      Plugin.IsLLLPresent = true;
    }

    private static void PatchLLL(SelectableLevel selectableLevel, ref string __result)
    {
      string currentWeather = Variables.GetPlanetCurrentWeather(selectableLevel);

      logger.LogDebug($"GetMoonConditions {selectableLevel.PlanetName}::{currentWeather}");

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

      __result = currentWeather;
    }

    internal static List<SelectableLevel> GetSelectableLevels()
    {
      return LethalLevelLoader.PatchedContent.SeletectableLevels;
    }
  }
}
