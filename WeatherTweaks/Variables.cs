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

    internal static List<SelectableLevel> GetGameLevels(StartOfRound __instance)
    {
      GameLevels = __instance.levels.Where(level => level.PlanetName != "71 Gordion").ToList();
      return GameLevels;
    }

    internal static List<LevelWeatherType> GetPlanetWeightedList(SelectableLevel level, Dictionary<LevelWeatherType, int> weights)
    {
      var weatherList = new List<LevelWeatherType>();

      foreach (var weather in level.randomWeathers.ToList())
      {
        var weatherType = weather.weatherType;
        var weatherWeight = weights[weatherType];
        Plugin.logger.LogDebug($"weather: {weatherType} has weight {weatherWeight}");

        for (var i = 0; i < weatherWeight; i++)
        {
          weatherList.Add(weatherType);
        }
      }

      return weatherList;
    }

    internal static List<bool> GetConditionsWeightedList(string condition)
    {
      var list = new List<bool>();

      // switch: based on condition (none/weather/eclipse) get corresponding weights from ConfigManager
      // then add to list
      switch (condition)
      {
        case "none":
          for (var i = 0; i < ConfigManager.NoneToWeatherBaseWeight.Value; i++)
          {
            list.Add(true);
          }

          for (var i = 0; i < ConfigManager.NoneToNoneBaseWeight.Value; i++)
          {
            list.Add(false);
          }
          break;
        case "weather":
          for (var i = 0; i < ConfigManager.WeatherToWeatherBaseWeight.Value; i++)
          {
            list.Add(true);
          }

          for (var i = 0; i < ConfigManager.WeatherToNoneBaseWeight.Value; i++)
          {
            list.Add(false);
          }
          break;
        case "eclipse":
          for (var i = 0; i < ConfigManager.EclipsedToWeatherBaseWeight.Value; i++)
          {
            list.Add(true);
          }

          for (var i = 0; i < ConfigManager.EclipsedToNoneBaseWeight.Value; i++)
          {
            list.Add(false);
          }
          break;
      }

      return list;
    }
  }
}
