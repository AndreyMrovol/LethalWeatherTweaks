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

      Plugin.logger.LogWarning(
        $"Color: {___screenLevelDescription.color}, {___screenLevelDescription.colorGradient}, {___screenLevelDescription.colorGradientPreset}, {___screenLevelDescription.faceColor}, {___screenLevelDescription.outlineColor}"
      );

      string weatherCondition = Variables.GetPlanetCurrentWeather(___currentLevel);

      Plugin.logger.LogDebug($"Weather condition: {weatherCondition}");

      StringBuilder stringBuilder = new();
      stringBuilder.Append("ORBITING: " + ___currentLevel.PlanetName + "\n");
      stringBuilder.Append($"WEATHER: {GetColoredString(___currentLevel)}\n");
      stringBuilder.Append(___currentLevel.LevelDescription ?? "");

      ___screenLevelDescription.fontWeight = FontWeight.Bold;
      ___screenLevelDescription.text = stringBuilder.ToString();
    }

    private static string GetColoredString(SelectableLevel level)
    {
      WeatherType currentWeather = Variables.GetPlanetCurrentWeatherType(level);
      string currentWeatherString = Variables.GetPlanetCurrentWeather(level);

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

          // create a method to resolve each individual word to a weather color
          // without using LevelWeatherType *because* it's not gonna resolve when the word is not a weather

          string pickedColor = newWord switch
          {
            "Testing" => "BA089C",
            "Eclipsed" => "FF0000",
            "Flooded" => "FF9300",
            "Stormy" => "FF9300",
            "Foggy" => "FFDC00",
            "Rainy" => "FFDC00",
            "DustClouds" => "69FF6B",
            "None" => "69FF6B",
            "+" => "FFFFFF",
            ">" => "FFFFFF",
            "/" => "FFFFFF",
            "?" => "FFFFFF",
            _ => "000000",
          };

          // if (word == ">")
          // {
          //   newWord = "⇨";
          //   sectionToReplace = new(@">");
          // }

          // outputString = outputString.Replace(word, pickedColor != "000000" ? $"<color=#{pickedColor}>{newWord}</color>" : $"{word}");
          outputString += pickedColor != "000000" ? $"<color=#{pickedColor}>{word}</color>" : $"{newWord}";
        });

      return outputString;
    }
  }
}
