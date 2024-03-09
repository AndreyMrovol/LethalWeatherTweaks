using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;
using static WeatherTweaks.Modules.Types;

namespace WeatherTweaks
{
  internal class Variables
  {
    public static List<SelectableLevel> GameLevels = [];

    // public static Dictionary<string, SelectableLevel> PlanetNames = [];

    public static List<WeatherType> WeatherTypes = [];
    public static WeatherType NoneWeather;
    public static List<CombinedWeatherType> CombinedWeatherTypes = [];

    public static Dictionary<SelectableLevel, WeatherType> CurrentWeathers = [];
    public static List<WeatherEffect> CurrentEffects = [];

    internal static Dictionary<int, LevelWeatherType> GetWeatherData(string weatherData)
    {
      Dictionary<int, LevelWeatherType> weatherDataDict = JsonConvert.DeserializeObject<Dictionary<int, LevelWeatherType>>(weatherData);
      return weatherDataDict;
    }

    internal static List<SelectableLevel> GetGameLevels(StartOfRound startOfRound)
    {
      GameLevels = LethalLevelLoader.PatchedContent.SeletectableLevels.Where(level => level.PlanetName != "71 Gordion").ToList();

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

    internal static List<WeatherType> GetPlanetWeatherTypes(SelectableLevel level)
    {
      List<LevelWeatherType> randomWeathers = GetPlanetPossibleWeathers(level);

      List<WeatherType> possibleTypes = new();

      foreach (WeatherType weather in WeatherTypes)
      {
        if (randomWeathers.Contains(weather.weatherType))
        {
          possibleTypes.Add(weather);
        }
      }

      // Plugin.logger.LogDebug($"Possible types: {string.Join("; ", possibleTypes.Select(x => x.Name))}");
      return possibleTypes.Distinct().ToList();
    }

    internal static void PopulateWeathers(StartOfRound startOfRound)
    {
      Plugin.logger.LogDebug("Populating weathers");
      WeatherEffect[] effects = TimeOfDay.Instance.effects;

      if (effects == null || effects.Count() == 0)
      {
        Plugin.logger.LogWarning("Effects are null");
      }

      NoneWeather = new()
      {
        Name = "None",
        Effects = [],
        weatherType = LevelWeatherType.None,
        Type = CustomWeatherType.Vanilla
      };

      for (int i = 0; i < effects.Length; i++)
      {
        WeatherEffect effect = effects[i];

        LevelWeatherType weatherType = (LevelWeatherType)i;
        WeatherType newWeather =
          new()
          {
            Name = weatherType.ToString(),
            Effects = [effect],
            weatherType = weatherType,
          };

        WeatherTypes.Add(newWeather);
      }

      CombinedWeatherTypes.ForEach(combinedWeather =>
      {
        Plugin.logger.LogDebug($"Adding combined weather: {combinedWeather.Name}");

        combinedWeather.Effects.Clear();
        combinedWeather.Weathers.ForEach(weather =>
        {
          Plugin.logger.LogWarning($"Adding weather effect: {weather}");
          combinedWeather.Effects.Add(TimeOfDay.Instance.effects[(int)weather]);
        });
        combinedWeather.WeatherType.Effects = combinedWeather.Effects;
        combinedWeather.WeatherType.Weathers = combinedWeather.Weathers;

        WeatherTypes.Add(combinedWeather.WeatherType);
      });
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
        if (CurrentWeathers.ContainsKey(level) == false)
        {
          Plugin.logger.LogWarning($"CurrentWeathers doesn't contain key {level.PlanetName}");

          Plugin.logger.LogDebug("CurrentWeathers count: " + CurrentWeathers.Count);

          CurrentWeathers.Do(x =>
          {
            Plugin.logger.LogDebug($"Key: {x.Key.PlanetName}");
            Plugin.logger.LogDebug($"Value: {(x.Value == null ? "null" : x.Value.Name)}");
          });

          return level.currentWeather.ToString();
        }

        return CurrentWeathers[level].Name;
      }
    }

    internal static WeatherType GetFullWeatherType(WeatherType weatherType)
    {
      return WeatherTypes.Find(x => x.Name == weatherType.Name);
    }

    internal static List<WeatherType> GetPlanetWeightedList(
      SelectableLevel level,
      Dictionary<LevelWeatherType, int> weights,
      float difficulty = 0
    )
    {
      var weatherList = new List<WeatherType>();

      difficulty = Math.Clamp(difficulty, 0, ConfigManager.MaxMultiplier.Value);

      foreach (var weather in GetPlanetWeatherTypes(level))
      {
        var weatherType = weather;
        var weatherWeight = weights[weather.weatherType];

        // Plugin.logger.LogDebug($"Weather: {weatherType.Name} has weight {weatherWeight}");

        if (ConfigManager.ScaleDownClearWeather.Value && weather.weatherType == LevelWeatherType.None)
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

        if (weatherType.Type == CustomWeatherType.Combined)
        {
          var combinedWeather = CombinedWeatherTypes.Find(x => x.Name == weatherType.Name);

          if (combinedWeather.CanCombinedWeatherBeApplied(level))
          {
            weatherWeight = Mathf.RoundToInt(weights[weather.weatherType] * combinedWeather.weightModify);
          }
          else
          {
            Plugin.logger.LogDebug($"Combined weather: {combinedWeather.Name} can't be applied");
            weatherWeight = 0;
          }
        }

        if (difficulty != 0 && weatherType.weatherType == LevelWeatherType.None)
        {
          weatherWeight = (int)(weatherWeight * (1 - difficulty));
        }

        Plugin.logger.LogDebug($"{weatherType.Name} has weight {weatherWeight}");

        for (var i = 0; i < weatherWeight; i++)
        {
          weatherList.Add(weather);
        }
      }

      return weatherList;
    }
  }
}
