using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HarmonyLib;
using MrovLib;
using Newtonsoft.Json;
using UnityEngine;
using WeatherRegistry;
using WeatherTweaks.Definitions;
using WeatherTweaks.Patches;

namespace WeatherTweaks
{
  internal class Variables
  {
    internal static List<SelectableLevel> GameLevels = [];

    internal static bool IsSetupFinished = false;

    internal static Weather NoneWeather => WeatherRegistry.WeatherManager.NoneWeather;
    internal static Weather WeatherTweaksWeather;
    internal static LevelWeatherType WeatherTweaksLevelWeatherType => WeatherTweaksWeather.VanillaWeatherType;

    public static List<Definitions.Types.CombinedWeatherType> CombinedWeatherTypes = [];
    public static List<Definitions.Types.ProgressingWeatherType> ProgressingWeatherTypes = [];

    public static List<Weather> WeatherTypes => WeatherRegistry.WeatherManager.Weathers;

    private static List<WeatherTweaksWeather> _weatherTweaksTypes = [];
    public static List<WeatherTweaksWeather> WeatherTweaksTypes
    {
      get
      {
        // cache the weather types on first call

        if (_weatherTweaksTypes.Count == 0)
        {
          foreach (Weather weather in WeatherTypes)
          {
            _weatherTweaksTypes.Add(new WeatherTweaksWeather(weather));
          }
        }

        return _weatherTweaksTypes;
      }
    }

    public static List<WeatherTweaksWeather> SpecialWeathers =>
      CombinedWeatherTypes.Cast<WeatherTweaksWeather>().Concat(ProgressingWeatherTypes).ToList();
    public static List<LevelWeatherType> WeatherTweaksWeathers => SpecialWeathers.Select(x => x.VanillaWeatherType).ToList();

    public static List<ImprovedWeatherEffect> CurrentEffects = [];

    public static WeatherTweaksWeather CurrentLevelWeather => GetFullWeatherType(StartOfRound.Instance.currentLevel.currentWeather);

    public static WeatherTweaksWeather GetCurrentWeather()
    {
      if (CurrentLevelWeather.CustomType == CustomWeatherType.Progressing)
      {
        if (ChangeMidDay.currentEntry == null)
        {
          Plugin.logger.LogWarning("Current entry is null");
          return (WeatherTweaksWeather)CurrentLevelWeather;
        }

        return (WeatherTweaksWeather)ChangeMidDay.currentEntry.GetWeather();
      }

      return (WeatherTweaksWeather)CurrentLevelWeather;
    }

    internal static Dictionary<int, LevelWeatherType> GetWeatherData(string weatherData)
    {
      Dictionary<int, LevelWeatherType> weatherDataDict = JsonConvert.DeserializeObject<Dictionary<int, LevelWeatherType>>(weatherData);
      return weatherDataDict;
    }

    internal static List<SelectableLevel> GetGameLevels(bool includeCompanyMoon = false)
    {
      Plugin.logger.LogDebug($"Getting game levels, {includeCompanyMoon}");
      List<SelectableLevel> GameLevels = MrovLib.SharedMethods.GetGameLevels();

      if (!includeCompanyMoon)
      {
        GameLevels = GameLevels.Where(level => level.PlanetName != "71 Gordion").ToList();
      }

      Variables.GameLevels = GameLevels;
      return GameLevels;
    }

    internal static List<WeatherTweaksWeather> GetPlanetWeatherTypes(SelectableLevel level)
    {
      List<LevelWeatherType> randomWeathers = WeatherRegistry.WeatherManager.GetPlanetPossibleWeathers(level);

      if (randomWeathers.Count() == 0)
      {
        Plugin.logger.LogError("Random weathers are empty");
        return [];
      }

      List<WeatherTweaksWeather> possibleTypes = [];

      foreach (WeatherTweaksWeather weather in WeatherTweaksTypes)
      {
        if (randomWeathers.Contains(weather.VanillaWeatherType))
        {
          possibleTypes.Add(weather);
        }

        switch (weather.CustomType)
        {
          case CustomWeatherType.Combined:
            Definitions.Types.CombinedWeatherType combinedWeather = CombinedWeatherTypes.Find(x => x.Name == weather.Name);
            if (combinedWeather.CanWeatherBeApplied(level))
            {
              possibleTypes.Add(weather);
            }
            break;

          case CustomWeatherType.Progressing:
            Definitions.Types.ProgressingWeatherType progressingWeather = ProgressingWeatherTypes.Find(x => x.Name == weather.Name);
            if (progressingWeather.CanWeatherBeApplied(level))
            {
              possibleTypes.Add(weather);
            }
            break;
        }
      }

      // Plugin.logger.LogDebug($"Possible types: {string.Join("; ", possibleTypes.Select(x => x.Name))}");
      return possibleTypes.Distinct().ToList();
    }

    public static string GetPlanetCurrentWeather(SelectableLevel level, bool uncertain = true)
    {
      bool isUncertainWeather = UncertainWeather.uncertainWeathers.ContainsKey(level.PlanetName);

      if (isUncertainWeather && uncertain)
      {
        Plugin.DebugLogger.LogDebug($"Getting uncertain weather for {level.PlanetName}");
        return UncertainWeather.uncertainWeathers[level.PlanetName];
      }
      else
      {
        Plugin.DebugLogger.LogDebug($"Getting current weather for {level.PlanetName}");

        Weather weather = WeatherManager.GetWeather(level.currentWeather);

        Plugin.logger.LogWarning($"Current weather: {weather}");
        return weather.Name;
      }
    }

    public static WeatherTweaksWeather GetPlanetCurrentWeatherType(SelectableLevel level)
    {
      return GetFullWeatherType(level.currentWeather);
    }

    public static float GetLevelWeatherVariable(LevelWeatherType weatherType, bool variable2 = false)
    {
      MrovLib.Logger logger = new("WeatherTweaks Variables", ConfigManager.LogWeatherVariables);

      if (StartOfRound.Instance == null)
      {
        Plugin.logger.LogError("StartOfRound is null");
        return 0;
      }

      SelectableLevel level = StartOfRound.Instance.currentLevel;
      RandomWeatherWithVariables randomWeather = level.randomWeathers.First(x => x.weatherType == weatherType);

      if (randomWeather == null || StartOfRound.Instance == null || level == null)
      {
        logger.LogError($"Failed to get weather variables for {level.PlanetName}:{weatherType}");
        return 0;
      }

      logger.LogDebug(
        $"Got weather variables for {level.PlanetName}:{weatherType} with variables {randomWeather.weatherVariable} {randomWeather.weatherVariable2}"
      );

      if (variable2)
      {
        return randomWeather.weatherVariable2;
      }

      return randomWeather.weatherVariable;
    }

    // public static LevelWeatherType LevelHasWeather(LevelWeatherType weatherType)
    // {
    //   Weather currentWeather = WeatherManager.GetWeather(weatherType);
    //   SelectableLevel level = StartOfRound.Instance.currentLevel;

    //   if (StartOfRound.Instance == null || level == null)
    //   {
    //     Plugin.logger.LogError($"Failed to get weather variables for {level.PlanetName}:{weatherType}");
    //     return LevelWeatherType.None;
    //   }

    //   LevelWeatherVariables weatherVariables = currentWeather.WeatherVariables.GetValueOrDefault(level);

    //   if (weatherVariables != null)
    //   {
    //     Plugin.logger.LogDebug($"Weather variable: {weatherVariables}");
    //     return weatherType;
    //   }

    //   return LevelWeatherType.None;
    // }

    public static LevelWeatherType LevelHasWeather(LevelWeatherType weatherType)
    {
      SelectableLevel level = StartOfRound.Instance.currentLevel;
      if (StartOfRound.Instance == null || level == null)
      {
        Plugin.logger.LogError($"Failed to get weather variables for {level.PlanetName}:{weatherType}");
        return LevelWeatherType.None;
      }

      WeatherTweaksWeather currentWeather = GetPlanetCurrentWeatherType(level);

      Plugin.logger.LogDebug(currentWeather.Type);

      switch (currentWeather.CustomType)
      {
        case CustomWeatherType.Combined:
          Definitions.Types.CombinedWeatherType combinedWeather = (Definitions.Types.CombinedWeatherType)currentWeather;
          if (combinedWeather.WeatherTypes.Any(x => x == weatherType))
          {
            Plugin.logger.LogWarning($"Level {level.PlanetName} has weather {weatherType}");
            return weatherType;
          }
          break;
        case CustomWeatherType.Progressing:
          Definitions.Types.ProgressingWeatherType progressingWeather = (Definitions.Types.ProgressingWeatherType)currentWeather;

          if (progressingWeather.DoesHaveWeatherHappening(weatherType))
          {
            Plugin.logger.LogWarning($"Level {level.PlanetName} has weather {weatherType}");
            return weatherType;
          }
          break;

        default:
          if (currentWeather.VanillaWeatherType == weatherType)
          {
            Plugin.logger.LogWarning($"Level {level.PlanetName} has weather {weatherType}");
            return weatherType;
          }
          break;
      }
      return LevelWeatherType.None;
    }

    internal static Weather GetVanillaWeatherType(LevelWeatherType weatherType)
    {
      return WeatherTypes.Find(x => x.VanillaWeatherType == weatherType);
    }

    internal static WeatherTweaksWeather GetFullWeatherType(Weather weatherType)
    {
      return GetFullWeatherType(weatherType.VanillaWeatherType);
    }

    internal static WeatherTweaksWeather GetFullWeatherType(LevelWeatherType weatherType)
    {
      Plugin.logger.LogDebug($"Getting full weather type for {weatherType}");

      return WeatherTweaksTypes.Find(x => x.VanillaWeatherType == weatherType);
    }

    internal static int GetWeatherLevelWeight(SelectableLevel level, LevelWeatherType weatherType)
    {
      return WeatherRegistry.WeatherManager.GetWeather(weatherType).GetWeight(level);
    }

    internal static MrovLib.WeightHandler<Weather> GetPlanetWeightedList(SelectableLevel level, float difficulty = 0)
    {
      MrovLib.Logger Logger = new("WeatherTweaks WeatherSelection", ConfigManager.LogWeatherSelection);

      WeightHandler<Weather> weights = new();
      WeightHandler<LevelWeatherType> weatherTypeWeights = new();

      difficulty = Math.Clamp(difficulty, 0, ConfigManager.MaxMultiplier.Value);
      int possibleWeathersWeightSum = 0;

      List<WeatherTweaksWeather> weatherTypes = GetPlanetWeatherTypes(level);

      Variables
        .WeatherTweaksTypes.Where(weatherType => weatherType.CustomType == CustomWeatherType.Normal)
        .ToList()
        .ForEach(weatherType =>
        {
          int localWeight = GetWeatherLevelWeight(level, weatherType.VanillaWeatherType);
          weatherTypeWeights.Add(weatherType.VanillaWeatherType, localWeight);

          if (weatherTypes.Contains(weatherType))
          {
            possibleWeathersWeightSum += localWeight;
          }
        });

      weatherTypeWeights.Add(LevelWeatherType.None, GetWeatherLevelWeight(level, LevelWeatherType.None));

      foreach (var weatherType in weatherTypes)
      {
        var weatherWeight = weatherTypeWeights.Get(weatherType.VanillaWeatherType);
        if (ConfigManager.ScaleDownClearWeather.Value && weatherType.VanillaWeatherType == LevelWeatherType.None)
        {
          int clearWeatherWeight = NoneWeather.GetWeight(level);
          int fullWeightSum = weatherTypeWeights.Sum;

          // proportion from clearWeatherWeight / fullWeightsSum
          double noWeatherFinalWeight = (double)(clearWeatherWeight * Math.Max(possibleWeathersWeightSum, 1) / Math.Max(fullWeightSum, 1));
          weatherWeight = Convert.ToInt32(noWeatherFinalWeight);

          Logger.LogDebug(
            $"Scaling down clear weather weight from {clearWeatherWeight} to {weatherWeight} : ({clearWeatherWeight} * {possibleWeathersWeightSum} / {fullWeightSum}) == {weatherWeight}"
          );
        }

        if (weatherType.CustomType == CustomWeatherType.Combined)
        {
          var combinedWeather = CombinedWeatherTypes.Find(x => x.Name == weatherType.Name);

          if (combinedWeather.CanWeatherBeApplied(level))
          {
            weatherWeight = Mathf.RoundToInt(weatherTypeWeights.Get(combinedWeather.VanillaWeatherType) * combinedWeather.WeightModify);
            Logger.LogDebug($"Weight of combined weather: {combinedWeather.Name} is {weatherWeight}");
          }
          else
          {
            Logger.LogDebug($"Combined weather: {combinedWeather.Name} can't be applied");
            continue;
          }
        }
        else if (weatherType.CustomType == CustomWeatherType.Progressing)
        {
          var progressingWeather = ProgressingWeatherTypes.Find(x => x.Name == weatherType.Name);
          if (progressingWeather.CanWeatherBeApplied(level))
          {
            weatherWeight = Mathf.RoundToInt(weatherTypeWeights.Get(weatherType.VanillaWeatherType) * progressingWeather.WeightModify);

            Logger.LogDebug($"Weight of progressing weather: {progressingWeather.Name} is {weatherWeight}");
          }
          else
          {
            Logger.LogDebug($"Progressing weather: {progressingWeather.Name} can't be applied");
            continue;
          }
        }

        if (difficulty != 0 && weatherType.VanillaWeatherType == LevelWeatherType.None)
        {
          weatherWeight = (int)(weatherWeight * (1 - difficulty));
        }

        Logger.LogDebug($"{weatherType.Name} has weight {weatherWeight}");

        weights.Add(weatherType, weatherWeight);
      }

      return weights;
    }
  }
}
