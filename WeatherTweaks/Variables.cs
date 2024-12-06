using System;
using System.Collections.Generic;
using System.Linq;
using MrovLib;
using Newtonsoft.Json;
using UnityEngine;
using WeatherRegistry;
using WeatherTweaks.Definitions;

namespace WeatherTweaks
{
  internal class Variables
  {
    internal static List<SelectableLevel> GameLevels => MrovLib.LevelHelper.Levels;

    internal static bool IsSetupFinished = false;

    internal static Weather NoneWeather => WeatherRegistry.WeatherManager.NoneWeather;

    public static List<Definitions.Types.CombinedWeatherType> CombinedWeathers = [];
    public static List<LevelWeatherType> CombinedWeatherTypes => CombinedWeathers.Select(x => x.VanillaWeatherType).ToList();

    public static List<Definitions.Types.ProgressingWeatherType> ProgressingWeathers = [];
    public static List<LevelWeatherType> ProgressingWeatherTypes => ProgressingWeathers.Select(x => x.VanillaWeatherType).ToList();

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
            if (weather == null)
            {
              continue;
            }

            // check if the weather is a WeatherTweaksWeather type already
            if (weather is WeatherTweaksWeather tweaksWeather)
            {
              Plugin.logger.LogDebug($"Weather {weather.Name} is a WeatherTweaks weather");

              _weatherTweaksTypes.Add(tweaksWeather);
              continue;
            }

            _weatherTweaksTypes.Add(new WeatherTweaksWeather(weather));
          }
        }

        return _weatherTweaksTypes;
      }
      set { _weatherTweaksTypes = value; }
    }

    public static List<WeatherTweaksWeather> SpecialWeathers =>
      CombinedWeathers.Cast<WeatherTweaksWeather>().Concat(ProgressingWeathers).ToList();
    public static List<LevelWeatherType> WeatherTweaksWeathers => SpecialWeathers.Select(x => x.VanillaWeatherType).ToList();

    public static List<ImprovedWeatherEffect> CurrentEffects = [];

    public static WeatherTweaksWeather CurrentLevelWeather => GetFullWeatherType(StartOfRound.Instance.currentLevel.currentWeather);

    public static WeatherTweaksWeather GetCurrentWeather()
    {
      if (CurrentLevelWeather.CustomType == CustomWeatherType.Progressing)
      {
        if (ChangeMidDay.CurrentEntry == null)
        {
          Plugin.logger.LogWarning("Current entry is null");
          return CurrentLevelWeather;
        }

        return (WeatherTweaksWeather)ChangeMidDay.CurrentEntry.GetWeather();
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
      if (includeCompanyMoon)
      {
        return GameLevels;
      }

      return GameLevels.Where(x => x != MrovLib.LevelHelper.CompanyMoon).ToList();
    }

    internal static List<WeatherTweaksWeather> GetPlanetWeatherTypes(SelectableLevel level, bool specialWeathers = false)
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
      }

      if (specialWeathers)
      {
        foreach (WeatherTweaksWeather weather in SpecialWeathers)
        {
          switch (weather.CustomType)
          {
            case CustomWeatherType.Combined:
              Definitions.Types.CombinedWeatherType combinedWeather = CombinedWeathers.Find(x => x.Name == weather.Name);
              if (combinedWeather.CanWeatherBeApplied(level))
              {
                possibleTypes.Add(weather);
              }
              break;

            case CustomWeatherType.Progressing:
              Definitions.Types.ProgressingWeatherType progressingWeather = ProgressingWeathers.Find(x => x.Name == weather.Name);
              if (progressingWeather.CanWeatherBeApplied(level))
              {
                possibleTypes.Add(weather);
              }
              break;
          }
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

      Plugin.DebugLogger.LogDebug(
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
      Plugin.logger.LogDebug($"Checking if level has weather {weatherType}");

      SelectableLevel level = StartOfRound.Instance.currentLevel;
      if (StartOfRound.Instance == null || level == null)
      {
        Plugin.logger.LogError($"Failed to get weather variables for {level.PlanetName}:{weatherType}");
        return LevelWeatherType.None;
      }

      WeatherTweaksWeather currentWeather = GetPlanetCurrentWeatherType(level);
      Plugin.logger.LogDebug($"Level has weather {currentWeather.Type}?");

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

      List<WeatherTweaksWeather> weatherTypes = GetPlanetWeatherTypes(level, true);

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
          double noWeatherFinalWeight = clearWeatherWeight * Math.Max(possibleWeathersWeightSum, 1) / Math.Max(fullWeightSum, 1);
          weatherWeight = Convert.ToInt32(noWeatherFinalWeight);

          Logger.LogDebug(
            $"Scaling down clear weather weight from {clearWeatherWeight} to {weatherWeight} : ({clearWeatherWeight} * {possibleWeathersWeightSum} / {fullWeightSum}) == {weatherWeight}"
          );
        }

        if (weatherType.CustomType == CustomWeatherType.Combined)
        {
          var combinedWeather = CombinedWeathers.Find(x => x.Name == weatherType.Name);

          if (combinedWeather.CanWeatherBeApplied(level))
          {
            weatherWeight = combinedWeather.DefaultWeight;
          }
          else
          {
            Logger.LogDebug($"Combined weather: {combinedWeather.Name} can't be applied");
            continue;
          }
        }
        else if (weatherType.CustomType == CustomWeatherType.Progressing)
        {
          weatherWeight = weatherType.DefaultWeight;
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
