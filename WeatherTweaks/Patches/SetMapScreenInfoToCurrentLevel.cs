using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

namespace WeatherTweaks
{
  [HarmonyPatch(typeof(StartOfRound))]
  public static class SetMapScreenInfoToCurrentLevelPatch
  {
    [HarmonyPatch("SetMapScreenInfoToCurrentLevel")]
    [HarmonyPostfix]
    internal static void GameMethodPatch(ref TextMeshProUGUI ___screenLevelDescription, ref SelectableLevel ___currentLevel)
    {
      bool isUncertainWeather = UncertainWeather.uncertainWeathers.ContainsKey(___currentLevel.PlanetName);
      Plugin.logger.LogDebug($"Is uncertain weather: {isUncertainWeather}");
      string weatherCondition;

      if (isUncertainWeather)
      {
        weatherCondition = UncertainWeather.uncertainWeathers[___currentLevel.PlanetName];
      }
      else
      {
        weatherCondition = ___currentLevel.currentWeather.ToString();
      }

      Plugin.logger.LogDebug($"Weather condition: {weatherCondition}");

      StringBuilder stringBuilder = new();
      stringBuilder.Append("ORBITING: " + ___currentLevel.PlanetName + "\n");
      stringBuilder.Append("WEATHER: " + weatherCondition + "\n");
      stringBuilder.Append(___currentLevel.LevelDescription ?? "");
      ___screenLevelDescription.text = stringBuilder.ToString();
    }
  }
}
