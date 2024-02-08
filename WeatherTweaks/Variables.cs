using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;

namespace WeatherTweaks
{
  internal class Variables
  {
    public static List<SelectableLevel> GameLevels = [];
    public static Dictionary<string, SelectableLevel> PlanetNames = [];

    internal static List<SelectableLevel> GetGameLevels(StartOfRound startOfRound)
    {
      GameLevels = startOfRound.levels.Where(level => level.PlanetName != "71 Gordion").ToList();

      GameLevels.ForEach(level =>
      {
        if (!PlanetNames.ContainsKey(level.PlanetName))
        {
          string replacedPlanetName = Regex.Replace(level.PlanetName, @"\d", "").Trim();
          string splitPlanetName = replacedPlanetName.Split(' ')[0];

          PlanetNames.Add(level.PlanetName, level);
          Plugin.logger.LogDebug($"Added {level.PlanetName} to PlanetNames");

          if (level.PlanetName != replacedPlanetName)
          {
            PlanetNames.Add(replacedPlanetName, level);
            Plugin.logger.LogDebug($"Added {replacedPlanetName} to PlanetNames");
          }

          if (splitPlanetName != replacedPlanetName)
          {
            PlanetNames.Add(splitPlanetName, level);
            Plugin.logger.LogDebug($"Added {splitPlanetName} to PlanetNames");
          }
        }
      });

      return GameLevels;
    }

    internal static List<LevelWeatherType> GetPlanetPossibleWeathers(SelectableLevel level)
    {
      List<LevelWeatherType> weathersToChooseFrom = level
        .randomWeathers.Where(randomWeather =>
          randomWeather.weatherType != LevelWeatherType.None && randomWeather.weatherType != LevelWeatherType.DustClouds
        )
        .ToList()
        .Select(x => x.weatherType)
        .Append(LevelWeatherType.None)
        .ToList();

      return weathersToChooseFrom;
    }

    internal static List<LevelWeatherType> GetPlanetWeightedList(
      SelectableLevel level,
      Dictionary<LevelWeatherType, int> weights,
      float difficulty = 0
    )
    {
      var weatherList = new List<LevelWeatherType>();

      difficulty = Math.Clamp(difficulty, 0, ConfigManager.MaxMultiplier.Value);

      foreach (var weather in GetPlanetPossibleWeathers(level))
      {
        var weatherType = weather;
        var weatherWeight = weights[weatherType];

        if (ConfigManager.ScaleDownClearWeather.Value && weatherType == LevelWeatherType.None)
        {
          int clearWeatherWeight = weights[LevelWeatherType.None];
          int fullWeightSum = weights.Sum(x => x.Value);

          int possibleWeathersWeightSum = 0;
          level
            .randomWeathers.ToList()
            .Where(randomWeather => randomWeather.weatherType != LevelWeatherType.DustClouds)
            .ToList()
            .ForEach(randomWeather =>
            {
              possibleWeathersWeightSum = possibleWeathersWeightSum + weights[randomWeather.weatherType];
            });
          // proportion from clearWeatherWeight / fullWeightsSum

          double noWetherFinalWeight = (double)(clearWeatherWeight * possibleWeathersWeightSum / fullWeightSum);
          weatherWeight = Convert.ToInt32(noWetherFinalWeight);

          Plugin.logger.LogDebug($"{clearWeatherWeight} * {possibleWeathersWeightSum} / {fullWeightSum} == {weatherWeight}");
          Plugin.logger.LogDebug($"Scaling down clear weather weight from {clearWeatherWeight} to {weatherWeight}");
        }

        if (difficulty != 0 && weatherType == LevelWeatherType.None)
        {
          weatherWeight = (int)(weatherWeight * (1 - difficulty));
        }

        Plugin.logger.LogDebug($"{weatherType} has weight {weatherWeight}");

        for (var i = 0; i < weatherWeight; i++)
        {
          weatherList.Add(weatherType);
        }
      }

      return weatherList;
    }
  }
}
