using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using BepInEx.Logging;
using HarmonyLib;
using LethalLevelLoader;
using MrovLib;
using UnityEngine;

namespace WeatherTweaks.Patches
{
  public static class LLL
  {
    internal static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("WeatherTweaks LLL");

    internal static void Init()
    {
      var harmony = new Harmony("WeatherTweaks.LLL");

      if (MrovLib.Plugin.LLL.IsModPresent)
      {
        logger.LogWarning("Patching LethalLevelLoader");
        // Patch the ExtendedLevel GetWeatherConditions method
        harmony.Patch(
          AccessTools.Method(typeof(LethalLevelLoader.TerminalManager), "GetWeatherConditions"),
          postfix: new HarmonyMethod(typeof(Patches.LLL), "PatchNewLLL")
        );
      }
      else
      {
        logger.LogWarning("Patching Old LethalLevelLoader");
        // Patch the Selectable GetWeatherConditions method
        harmony.Patch(
          AccessTools.Method(typeof(LethalLevelLoader.TerminalManager), "GetWeatherConditions"),
          postfix: new HarmonyMethod(typeof(Patches.LLL), "PatchOldLLL")
        );
      }

      Plugin.IsLLLPresent = true;
    }

    private static void PatchNewLLL(ExtendedLevel extendedLevel, ref string __result)
    {
      __result = PatchLLL(extendedLevel.SelectableLevel);
    }

    private static void PatchOldLLL(SelectableLevel selectableLevel, ref string __result)
    {
      __result = PatchLLL(selectableLevel);
    }

    private static string PatchLLL(SelectableLevel selectableLevel)
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

      return currentWeather;
    }
  }
}
