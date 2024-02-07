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
    [HarmonyPriority(Priority.Last)]
    internal static void GameMethodPatch(ref TextMeshProUGUI ___screenLevelDescription, ref SelectableLevel ___currentLevel)
    {
      if (!ConfigManager.MapScreenPatch.Value)
      {
        return;
      }

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
      stringBuilder.Append("WEATHER: " + $"{GetHexColor(weatherCondition)}{weatherCondition}</color>" + "\n");
      stringBuilder.Append(___currentLevel.LevelDescription ?? "");
      ___screenLevelDescription.text = stringBuilder.ToString();
    }

    private static LevelWeatherType ResolveWeatherStringToType(string inputWeather)
    {
      // This is a simple switch statement that resolves the weather string to a LevelWeatherType
      // This needs to account for uncertain weather mechanic (e.g foggy/eclipsed as weather string)
      // in that case pick the most severe weather type for color to be correct
      // eclipse > flooded > stormy > foggy > rainy > dustclouds > none
      // do it in switch, but using Contains

      if (inputWeather.Contains("Eclipsed"))
      {
        return LevelWeatherType.Eclipsed;
      }
      else if (inputWeather.Contains("Flooded"))
      {
        return LevelWeatherType.Flooded;
      }
      else if (inputWeather.Contains("Stormy"))
      {
        return LevelWeatherType.Stormy;
      }
      else if (inputWeather.Contains("Foggy"))
      {
        return LevelWeatherType.Foggy;
      }
      else if (inputWeather.Contains("Rainy"))
      {
        return LevelWeatherType.Rainy;
      }
      else if (inputWeather.Contains("DustClouds"))
      {
        return LevelWeatherType.DustClouds;
      }
      else
      {
        return LevelWeatherType.None;
      }
    }

    private static string GetHexColor(string currentWeather)
    {
      if (currentWeather == "[UNKNOWN]")
      {
        return "<color=#4a4a4a>";
      }

      LevelWeatherType weatherType = ResolveWeatherStringToType(currentWeather);

      string pickedColor = weatherType switch
      {
        LevelWeatherType.None or LevelWeatherType.DustClouds => "69FF6B",
        LevelWeatherType.Rainy or LevelWeatherType.Foggy => "FFDC00",
        LevelWeatherType.Stormy or LevelWeatherType.Flooded => "FF9300",
        LevelWeatherType.Eclipsed => "FF0000",
        _ => "FFFFFF",
      };

      return "<color=#" + pickedColor + ">";
    }
  }
}
