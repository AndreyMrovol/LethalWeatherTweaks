using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;

namespace WeatherTweaks
{
  internal class Variables
  {
    public static List<SelectableLevel> GameLevels = [];

    internal static List<SelectableLevel> GetGameLevels(StartOfRound startOfRound)
    {
      GameLevels = startOfRound.levels.Where(level => level.PlanetName != "71 Gordion").ToList();
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

    internal static List<LevelWeatherType> GetPlanetWeightedList(SelectableLevel level, Dictionary<LevelWeatherType, int> weights)
    {
      var weatherList = new List<LevelWeatherType>();

      foreach (var weather in GetPlanetPossibleWeathers(level))
      {
        var weatherType = weather;
        var weatherWeight = weights[weatherType];
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
