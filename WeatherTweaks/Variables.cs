using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;
using WeatherTweaks.Definitions;
using WeatherTweaks.Patches;
using static WeatherTweaks.Modules.Types;

namespace WeatherTweaks
{
  internal class Variables
  {
    internal static List<SelectableLevel> GameLevels = [];
    internal static bool IsSetupFinished = false;

    internal static WeatherType NoneWeather;
    public static List<Weather> Weathers = [];
    public static List<WeatherType> WeatherTypes = [];

    public static List<Weather> RegisteredWeathers = [];

    public static List<LevelWeather> LevelWeathers = [];

    // public static List<VanillaWeatherType> VanillaWeathers = [];
    public static List<Definitions.Types.CombinedWeatherType> CombinedWeatherTypes = [];
    public static List<Definitions.Types.ProgressingWeatherType> ProgressingWeatherTypes = [];

    public static Dictionary<SelectableLevel, WeatherType> CurrentWeathers = [];

    public static List<Definitions.WeatherEffect> CurrentEffects = [];
    public static WeatherType CurrentLevelWeather;

    internal static WeatherType GetCurrentWeather()
    {
      if (CurrentLevelWeather.Type == CustomWeatherType.Progressing)
      {
        if (ChangeMidDay.currentEntry == null)
        {
          Plugin.logger.LogWarning("Current entry is null");
          return CurrentLevelWeather;
        }

        return ChangeMidDay.currentEntry.GetWeatherType();
      }

      return CurrentLevelWeather;
    }

    internal static Dictionary<int, LevelWeatherType> GetWeatherData(string weatherData)
    {
      Dictionary<int, LevelWeatherType> weatherDataDict = JsonConvert.DeserializeObject<Dictionary<int, LevelWeatherType>>(weatherData);
      return weatherDataDict;
    }

    internal static List<SelectableLevel> GetGameLevels(bool includeCompanyMoon = false)
    {
      Plugin.logger.LogDebug($"Getting game levels, {includeCompanyMoon}");
      List<SelectableLevel> GameLevels = MrovLib.API.SharedMethods.GetGameLevels();

      // for (int i = 0; i < GameLevels.Count; i++)
      // {
      //   if (GameLevels[i] == null)
      //   {
      //     Plugin.logger.LogDebug($"Level {i} is null");
      //     // continue;
      //     return null;
      //   }

      //   Plugin.logger.LogDebug($"Level {i}: {GameLevels[i].PlanetName}");
      // }

      if (!includeCompanyMoon)
      {
        GameLevels = GameLevels.Where(level => level.PlanetName != "71 Gordion").ToList();
      }

      Variables.GameLevels = GameLevels;
      return GameLevels;
    }

    internal static List<Weather> GetPlanetPossibleWeathers(SelectableLevel level)
    {
      Plugin.logger.LogDebug($"Getting possible weathers for {level.PlanetName}");

      List<LevelWeather> possibleWeathers = LevelWeathers.Where(x => x.Level == level).ToList();

      Plugin.logger.LogInfo($"Possible weathers: {string.Join("; ", possibleWeathers.Select(x => x.Weather.Name))}");

      if (possibleWeathers == null || possibleWeathers.Count() == 0)
      {
        Plugin.logger.LogError("Random weathers are null");
        return [];
      }

      List<Weather> weathersToChooseFrom = possibleWeathers.Select(x => x.Weather).ToList();

      // List<LevelWeatherType> weathersToChooseFrom = level
      //   .randomWeathers.Where(randomWeather =>
      //     randomWeather.weatherType != LevelWeatherType.None && randomWeather.weatherType != LevelWeatherType.DustClouds
      //   )
      //   .ToList()
      //   .Select(x => x.weatherType)
      //   .Append(LevelWeatherType.None)
      //   .ToList();

      return weathersToChooseFrom;
    }

    internal static List<WeatherType> GetPlanetWeatherTypes(SelectableLevel level)
    {
      Plugin.logger.LogDebug($"Getting weather types for {level.PlanetName}");
      List<Weather> randomWeathers = GetPlanetPossibleWeathers(level);

      if (randomWeathers.Count() == 0)
      {
        Plugin.logger.LogError("Random weathers are empty");
        return [];
      }

      Plugin.logger.LogMessage($"Got {randomWeathers.Count()} random weathers for {level.PlanetName}");

      List<WeatherType> possibleTypes = [];

      Plugin.logger.LogWarning($"Weather Types: {WeatherTypes.Count()}");

      foreach (Weather randomWeather in randomWeathers)
      {
        foreach (WeatherType weatherType in WeatherTypes)
        {
          if (weatherType == null)
          {
            Plugin.logger.LogWarning($"Weather Type is null");
            continue;
          }

          Plugin.logger.LogWarning($"Weather Type: {weatherType.Name}");

          if (weatherType.Weather == randomWeather)
          {
            Plugin.logger.LogWarning($"Weather Type: {weatherType.Name} contains {randomWeather.Name}");
            possibleTypes.Add(weatherType);
          }
        }
      }

      // foreach (Weather weather in randomWeathers)
      // {
      //   if (weather.VanillaWeatherType == LevelWeatherType.DustClouds)
      //   {
      //     continue;
      //   }

      //   WeatherType weatherType = WeatherTypes.Find(x => x.Weathers.SequenceEqual([weather]));

      //   if (weatherType == null)
      //   {
      //     Plugin.logger.LogWarning($"No weather type found for {weather.Name}");
      //     continue;
      //   }

      //   weatherType.weatherType = weather.VanillaWeatherType;
      //   possibleTypes.Add(weatherType);
      // }

      Plugin.logger.LogDebug($"Possible types: {string.Join("; ", possibleTypes.Select(x => x.Name))}");

      // foreach (Weather weather in Weathers)
      // {
      //   LevelWeatherVariables variables = weather.WeatherVariables.Find(x => x.Level == level);

      //   if (variables == null)
      //   {
      //     Plugin.logger.LogWarning($"No weather variables found for {level.PlanetName}");
      //     continue;
      //   }

      //   List<WeatherType> types = WeatherTypes.Where(x => x.Weathers.SequenceEqual([weather])).ToList();

      //   possibleTypes.Add(types.First());
      // }

      // foreach (WeatherType weather in WeatherTypes)
      // {
      //   if (randomWeathers.Contains(weather.weatherType) && weather.Type == CustomWeatherType.Vanilla)
      //   {
      //     possibleTypes.Add(weather);
      //   }

      //   switch (weather.Type)
      //   {
      //     case CustomWeatherType.Combined:
      //       CombinedWeatherType combinedWeather = CombinedWeatherTypes.Find(x => x.Name == weather.Name);
      //       if (combinedWeather.CanCombinedWeatherBeApplied(level))
      //       {
      //         possibleTypes.Add(weather);
      //       }
      //       break;
      //     case CustomWeatherType.Progressing:
      //       ProgressingWeatherType progressingWeather = ProgressingWeatherTypes.Find(x => x.Name == weather.Name);
      //       if (progressingWeather.Enabled.Value == false)
      //       {
      //         Plugin.logger.LogDebug($"Progressing weather: {progressingWeather.Name} is disabled");
      //         continue;
      //       }

      //       if (progressingWeather.CanWeatherBeApplied(level))
      //       {
      //         possibleTypes.Add(weather);
      //       }
      //       break;
      //   }
      // }

      // Plugin.logger.LogDebug($"Possible types: {string.Join("; ", possibleTypes.Select(x => x.Name))}");
      return possibleTypes.Distinct().ToList();
    }

    internal static Dictionary<string, WeatherType> GetAllPlanetWeathersDictionary()
    {
      Dictionary<string, WeatherType> weathers = [];

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

      if (TimeOfDay.Instance == null)
      {
        Plugin.logger.LogError("TimeOfDay is null");
        return;
      }

      WeatherTypes.Clear();
      // WeatherEffect[] effects = TimeOfDay.Instance.effects;

      // if (effects == null || effects.Count() == 0)
      // {
      //   Plugin.logger.LogWarning("Effects are null");
      // }

      // NoneWeather = new("None", LevelWeatherType.None, [LevelWeatherType.None], CustomWeatherType.Vanilla) { Effects = [] };
      // WeatherTypes.Add(NoneWeather);

      for (int i = 0; i < Weathers.Count; i++)
      {
        Weather weather = Weathers[i];

        Plugin.logger.LogMessage("Creating weather: " + weather.Name);

        WeatherType weatherType = new(weather.Name, CustomWeatherType.Normal) { weatherType = weather.VanillaWeatherType, Weather = weather };

        WeatherTypes.Add(weatherType);
      }

      CombinedWeatherTypes.ForEach(combinedWeather =>
      {
        Plugin.logger.LogMessage($"Creating combined weather: {combinedWeather.Name}");

        if (combinedWeather.Enabled.Value == false)
        {
          Plugin.logger.LogDebug($"Combined weather: {combinedWeather.Name} is disabled");
          return;
        }
        Plugin.logger.LogDebug($"Adding combined weather: {combinedWeather.Name}");

        Definitions.Types.CombinedWeatherType newCombinedWeather =
          new(combinedWeather.Name, combinedWeather.Weathers) { weatherType = LevelWeatherType.None };

        WeatherTypes.Add(newCombinedWeather);
      });

      ProgressingWeatherTypes.ForEach(progressingWeather =>
      {
        Plugin.logger.LogMessage($"Creating progressing weather: {progressingWeather.Name}");

        if (progressingWeather.Enabled.Value == false)
        {
          Plugin.logger.LogDebug($"Progressing weather: {progressingWeather.Name} is disabled");
          return;
        }

        Plugin.logger.LogDebug($"Adding progressing weather: {progressingWeather.Name}");

        Weather startingWeather = Weathers.Find(x => x.VanillaWeatherType == progressingWeather.StartingWeather);
        Definitions.Types.ProgressingWeatherType newProgressingWeather =
          new(progressingWeather.Name, progressingWeather.StartingWeather, progressingWeather.WeatherEntries)
          {
            weatherType = LevelWeatherType.None,
          };

        WeatherTypes.Add(newProgressingWeather);
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
      return GetFullWeatherType(CurrentWeathers.TryGetValue(level, out WeatherType weather) ? weather : NoneWeather);
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

      Plugin.logger.LogInfo($"Current level: {level.PlanetName}");
      Plugin.logger.LogInfo($"Is current level in Levels: {GameLevels.Contains(level)}");
      Plugin.logger.LogInfo($"Current level weather: {level.currentWeather}");
      Plugin.logger.LogInfo($"Is current level weather in CurrentWeathers: {CurrentWeathers.ContainsKey(level)}");

      if (!CurrentWeathers.ContainsKey(level))
      {
        Plugin.logger.LogWarning($"Level {level.PlanetName} has no defined weather");

        GameLevels.ForEach(level =>
        {
          Plugin.logger.LogDebug($"Level: {level.PlanetName} has weather {CurrentWeathers.TryGetValue(level, out WeatherType weather)}");
        });

        // return LevelWeatherType.None;
      }
      else
      {
        Plugin.logger.LogWarning($"Level {level.PlanetName} has a defined weather: {CurrentWeathers[level].Name}");
      }

      if (CurrentWeathers[level].Weather.VanillaWeatherType == weatherType)
      {
        Plugin.logger.LogWarning($"Level {level.PlanetName} has weather {weatherType}");
        return weatherType;
      }

      return LevelWeatherType.None;
    }

    internal static WeatherType GetVanillaWeatherType(LevelWeatherType weatherType)
    {
      return WeatherTypes.Find(x => x.Weather.VanillaWeatherType == weatherType && x.Type == CustomWeatherType.Normal);
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

      List<WeatherType> weatherTypes = GetPlanetWeatherTypes(level);

      if (weatherTypes.Count == 0)
      {
        return [];
      }

      foreach (var weather in weatherTypes)
      {
        // clone the object
        Weather typeOfWeather = weather.Weather;

        if (typeOfWeather.VanillaWeatherType == LevelWeatherType.DustClouds)
        {
          typeOfWeather.VanillaWeatherType = LevelWeatherType.None;
        }

        var weatherWeight = weights[typeOfWeather.VanillaWeatherType];

        if (ConfigManager.ScaleDownClearWeather.Value && typeOfWeather.VanillaWeatherType == LevelWeatherType.None)
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
              possibleWeathersWeightSum += weights[randomWeather.weatherType];
            });
          // proportion from clearWeatherWeight / fullWeightsSum

          double noWetherFinalWeight = (double)(clearWeatherWeight * Math.Max(possibleWeathersWeightSum, 1) / fullWeightSum);
          weatherWeight = Convert.ToInt32(noWetherFinalWeight);

          Plugin.logger.LogDebug(
            $"Scaling down clear weather weight from {clearWeatherWeight} to {weatherWeight} : ({clearWeatherWeight} * {possibleWeathersWeightSum} / {fullWeightSum}) == {weatherWeight}"
          );
        }

        if (weather.Type == CustomWeatherType.Combined)
        {
          var combinedWeather = CombinedWeatherTypes.Find(x => x.Name == weather.Name);

          if (combinedWeather.CanWeatherBeApplied(level))
          {
            weatherWeight = Mathf.RoundToInt(weights[typeOfWeather.VanillaWeatherType] * combinedWeather.weightModify);
          }
          else
          {
            Plugin.logger.LogDebug($"Combined weather: {combinedWeather.Name} can't be applied");
            continue;
          }
        }

        if (weather.Type == CustomWeatherType.Progressing)
        {
          var progressingWeather = ProgressingWeatherTypes.Find(x => x.Name == weather.Name);
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

          weatherWeight = Mathf.RoundToInt(weights[typeOfWeather.VanillaWeatherType] * progressingWeather.weightModify);
        }

        if (difficulty != 0 && typeOfWeather.VanillaWeatherType == LevelWeatherType.None)
        {
          weatherWeight = (int)(weatherWeight * (1 - difficulty));
        }

        Plugin.logger.LogDebug($"{weather.Name} has weight {weatherWeight}");

        for (var i = 0; i < weatherWeight; i++)
        {
          weatherList.Add(weather);
        }
      }

      return weatherList;
    }
  }
}
