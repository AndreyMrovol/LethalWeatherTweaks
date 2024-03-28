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
    internal static List<SelectableLevel> GameLevels = [];

    internal static WeatherType NoneWeather;
    public static List<WeatherType> WeatherTypes = [];

    public static List<CombinedWeatherType> CombinedWeatherTypes = [];
    public static List<ProgressingWeatherType> ProgressingWeatherTypes = [];

    public static Dictionary<SelectableLevel, WeatherType> CurrentWeathers = [];
    public static List<WeatherEffect> CurrentEffects = [];

    public static WeatherType CurrentLevelWeather;

    internal static WeatherType GetCurrentWeather()
    {
      if (CurrentLevelWeather.Type == CustomWeatherType.Progressing)
      {
        return ChangeMidDay.currentEntry.GetWeatherType();
      }

      return CurrentLevelWeather;
    }

    internal static Dictionary<int, LevelWeatherType> GetWeatherData(string weatherData)
    {
      Dictionary<int, LevelWeatherType> weatherDataDict = JsonConvert.DeserializeObject<Dictionary<int, LevelWeatherType>>(weatherData);
      return weatherDataDict;
    }

    internal static List<SelectableLevel> GetGameLevels(StartOfRound startOfRound, bool includeCompanyMoon = false)
    {
      GameLevels = LethalLevelLoader.PatchedContent.SeletectableLevels.Where(level => level.PlanetName != "71 Gordion").ToList();

      if (includeCompanyMoon)
      {
        GameLevels.Add(LethalLevelLoader.PatchedContent.SeletectableLevels.First(level => level.PlanetName == "71 Gordion"));
      }

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
        if (randomWeathers.Contains(weather.weatherType) && weather != NoneWeather)
        {
          possibleTypes.Add(weather);
        }
      }

      // Plugin.logger.LogDebug($"Possible types: {string.Join("; ", possibleTypes.Select(x => x.Name))}");
      return possibleTypes.Distinct().ToList();
    }

    internal static Dictionary<string, WeatherType> GetAllPlanetWeathersDictionary()
    {
      Dictionary<string, WeatherType> weathers = new();

      CurrentWeathers
        .ToList()
        .ForEach(weather =>
        {
          weathers.Add(weather.Key.PlanetName, weather.Value);
        });

      return weathers;
    }

    internal static void PopulateWeathers(StartOfRound startOfRound)
    {
      Plugin.logger.LogDebug("Populating weathers");
      WeatherEffect[] effects = TimeOfDay.Instance.effects;

      if (effects == null || effects.Count() == 0)
      {
        Plugin.logger.LogWarning("Effects are null");
      }

      NoneWeather = new("None", LevelWeatherType.None, [LevelWeatherType.None], CustomWeatherType.Vanilla) { Effects = [] };
      WeatherTypes.Add(NoneWeather);

      for (int i = 0; i < effects.Length; i++)
      {
        WeatherEffect effect = effects[i];

        LevelWeatherType weatherType = (LevelWeatherType)i;
        WeatherType newWeather = new(weatherType.ToString(), weatherType, [weatherType], CustomWeatherType.Vanilla) { Effects = [effect] };

        WeatherTypes.Add(newWeather);
      }

      CombinedWeatherTypes.ForEach(combinedWeather =>
      {
        if (combinedWeather.Enabled.Value == false)
        {
          Plugin.logger.LogDebug($"Combined weather: {combinedWeather.Name} is disabled");
          return;
        }
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

      ProgressingWeatherTypes.ForEach(progressingWeather =>
      {
        if (progressingWeather.Enabled.Value == false)
        {
          Plugin.logger.LogDebug($"Progressing weather: {progressingWeather.Name} is disabled");
          return;
        }

        Plugin.logger.LogDebug($"Adding progressing weather: {progressingWeather.Name}");

        WeatherTypes.Add(progressingWeather.WeatherType);
      });
    }

    public static string GetPlanetCurrentWeather(SelectableLevel level, bool uncertain = true)
    {
      bool isUncertainWeather = UncertainWeather.uncertainWeathers.ContainsKey(level.PlanetName);

      if (isUncertainWeather && uncertain)
      {
        return UncertainWeather.uncertainWeathers[level.PlanetName];
      }
      else
      {
        if (CurrentWeathers.ContainsKey(level) == false)
        {
          return level.currentWeather.ToString();
        }

        return CurrentWeathers[level].Name;
      }
    }

    public static WeatherType GetPlanetCurrentWeatherType(SelectableLevel level)
    {
      return CurrentWeathers[level];
    }

    public static float GetLevelWeatherVariable(LevelWeatherType weatherType, bool variable2 = false)
    {
      if (StartOfRound.Instance == null)
      {
        Plugin.logger.LogError("StartOfRound is null");
        return 0;
      }

      SelectableLevel level = StartOfRound.Instance.currentLevel;
      RandomWeatherWithVariables randomWeather = level.randomWeathers.First(x => x.weatherType == weatherType);

      if (randomWeather == null || StartOfRound.Instance == null || level == null)
      {
        Plugin.logger.LogError($"Failed to get weather variables for {level.PlanetName}:{weatherType}");
        return 0;
      }

      Plugin.logger.LogDebug(
        $"Got weather variables for {level.PlanetName}:{weatherType} with variables {randomWeather.weatherVariable} {randomWeather.weatherVariable2}"
      );

      if (variable2)
      {
        return randomWeather.weatherVariable2;
      }

      return randomWeather.weatherVariable;
    }

    public static LevelWeatherType LevelHasWeather(LevelWeatherType weatherType)
    {
      SelectableLevel level = StartOfRound.Instance.currentLevel;
      if (StartOfRound.Instance == null || level == null)
      {
        Plugin.logger.LogError($"Failed to get weather variables for {level.PlanetName}:{weatherType}");
        return LevelWeatherType.None;
      }

      if (CurrentWeathers[level].Weathers.Contains(weatherType))
      {
        Plugin.logger.LogWarning($"Level {level.PlanetName} has weather {weatherType}");
        return weatherType;
      }

      return LevelWeatherType.None;
    }

    internal static WeatherType GetVanillaWeatherType(LevelWeatherType weatherType)
    {
      return WeatherTypes.Find(x => x.weatherType == weatherType && x.Type == CustomWeatherType.Vanilla);
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
            continue;
          }
        }

        if (weatherType.Type == CustomWeatherType.Progressing)
        {
          var progressingWeather = ProgressingWeatherTypes.Find(x => x.Name == weatherType.Name);
          if (progressingWeather.Enabled.Value == false)
          {
            Plugin.logger.LogDebug($"Progressing weather: {progressingWeather.Name} is disabled");
            continue;
          }

          if (progressingWeather.CanWeatherBeApplied(level) == false)
          {
            Plugin.logger.LogDebug($"Progressing weather: {progressingWeather.Name} can't be applied");
            continue;
          }

          weatherWeight = Mathf.RoundToInt(weights[weather.weatherType] * 2);
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
