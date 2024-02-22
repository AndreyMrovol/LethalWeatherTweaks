using System.Text;
using System.Text.RegularExpressions;
using BepInEx.Logging;
using HarmonyLib;
using LethalLevelLoader;
using UnityEngine;

namespace WeatherTweaks
{
  [HarmonyPatch(typeof(TerminalManager))]
  public static class LLLTerminalPatch
  {
    internal static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("WeatherTweaks Terminal");

    [HarmonyPatch("GetWeatherConditions")]
    [HarmonyPostfix]
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
  }
}
