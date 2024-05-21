using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HarmonyLib;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using WeatherTweaks.Definitions;

namespace WeatherTweaks
{
  [HarmonyPatch(typeof(StartOfRound))]
  public static class SetMapScreenInfoToCurrentLevelPatch
  {
    [HarmonyPatch("SetMapScreenInfoToCurrentLevel")]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.HigherThanNormal)]
    internal static void GameMethodPatch(ref TextMeshProUGUI ___screenLevelDescription, ref SelectableLevel ___currentLevel)
    {
      if (!ConfigManager.MapScreenPatch.Value)
      {
        return;
      }

      if (Variables.CurrentWeathers == null || Variables.CurrentWeathers.Count == 0)
      {
        Plugin.logger.LogWarning("CurrentWeathers is null, cannot display weather");
        return;
      }

      string weatherCondition = Variables.GetPlanetCurrentWeather(___currentLevel);

      StringBuilder stringBuilder = new();
      stringBuilder.Append("ORBITING: " + ___currentLevel.PlanetName + "\n");
      stringBuilder.Append($"WEATHER: {GetColoredString(___currentLevel)}\n");
      stringBuilder.Append(___currentLevel.LevelDescription ?? "");

      ___screenLevelDescription.fontWeight = FontWeight.Bold;
      ___screenLevelDescription.text = stringBuilder.ToString();
    }

    private static string GetColoredString(SelectableLevel level)
    {
      Weather currentWeather = Variables.GetPlanetCurrentWeatherType(level).Weather;
      string currentWeatherString = Variables.GetPlanetCurrentWeather(level);

      Color weatherColor = currentWeather.Color;
      string weatherColorString = ColorUtility.ToHtmlStringRGB(weatherColor);

      bool uncertain = currentWeather.Name == currentWeatherString;

      if (currentWeatherString == "[UNKNOWN]")
      {
        return "<color=#4a4a4a>[UNKNOWN]</color>";
      }

      if (!ConfigManager.ColoredWeathers.Value)
      {
        return currentWeatherString;
      }

      // string outputString = currentWeatherString;
      string outputString = "";
      Regex split = new(@"(\/)|(\?)|(>)|(\+)");

      split
        .Split(currentWeatherString)
        .ToList()
        .ForEach(word =>
        {
          string newWord = word.Trim();

          // Plugin.logger.LogDebug($"newWord: {newWord}");

          // create a method to resolve each individual word to a weather color
          // without using LevelWeatherType *because* it's not gonna resolve when the word is not a weather

          string pickedColor = newWord switch
          {
            "Testing" => "BA089C",
            "+" => "FFFFFF",
            ">" => "FFFFFF",
            "/" => "FFFFFF",
            "?" => "FFFFFF",
            _ => "000000",
          };

          if (newWord == currentWeather.Name)
          {
            pickedColor = weatherColorString;
          }

          outputString += pickedColor != "000000" ? $"<color=#{pickedColor}>{word}</color>" : $"{newWord}";
        });

      Plugin.logger.LogWarning($"Output string: {outputString}");

      return outputString;
    }
  }
}
