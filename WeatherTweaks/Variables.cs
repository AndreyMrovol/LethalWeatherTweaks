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
    public static Dictionary<SelectableLevel, string> CurrentWeathers = [];

    internal static Dictionary<int, LevelWeatherType> GetWeatherData(string weatherData)
    {
      Dictionary<int, LevelWeatherType> weatherDataDict = JsonConvert.DeserializeObject<Dictionary<int, LevelWeatherType>>(weatherData);
      return weatherDataDict;
    }

    internal static List<SelectableLevel> GetGameLevels()
    {
      GameLevels = MrovLib.API.SharedMethods.GetGameLevels();

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

    internal static string GetPlanetCurrentWeather(SelectableLevel level)
    {
      bool isUncertainWeather = UncertainWeather.uncertainWeathers.ContainsKey(level.PlanetName);

      if (isUncertainWeather)
      {
        return UncertainWeather.uncertainWeathers[level.PlanetName];
      }
      else
      {
        return level.currentWeather.ToString();
      }
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
