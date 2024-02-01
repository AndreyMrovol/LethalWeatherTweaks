using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;

namespace WeatherTweaks
{
  internal class WeatherCalculation
  {
    internal static Dictionary<string, LevelWeatherType> previousDayWeather = [];

    internal static Dictionary<string, LevelWeatherType> NewWeathers(StartOfRound startOfRound)
    {
      Plugin.logger.LogMessage("SetWeathers called.");

      if (!StartOfRound.Instance.IsHost)
      {
        Plugin.logger.LogMessage("Not a host, cannot generate weather!");
        return null;
      }

      previousDayWeather.Clear();

      int seed = startOfRound.randomMapSeed + 31;
      System.Random random = new System.Random(seed);

      Dictionary<string, LevelWeatherType> vanillaSelectedWeather = VanillaWeathers(0, startOfRound);
      Dictionary<string, LevelWeatherType> currentWeather = new Dictionary<string, LevelWeatherType>();

      List<SelectableLevel> levels = Variables.GetGameLevels(startOfRound);
      int day = startOfRound.gameStats.daysSpent;
      int dayInQuota = day % 3;

      if (day == 0)
      {
        List<string> noWeatherOnStartPlanets = ["41 Experimentation", "56 Vow"];
        List<SelectableLevel> planetsToPickFrom = levels.Where(level => !noWeatherOnStartPlanets.Contains(level.PlanetName)).ToList();

        if (levels.Count > 9)
        {
          // pick another random planet
          noWeatherOnStartPlanets.Add(levels[random.Next(0, levels.Count)].PlanetName);
        }

        return FirstDayWeathers(planetsToPickFrom, noWeatherOnStartPlanets, random);
      }

      foreach (SelectableLevel level in levels)
      {
        previousDayWeather[level.PlanetName] = level.currentWeather;

        LevelWeatherType vanillaWeather = vanillaSelectedWeather.ContainsKey(level.PlanetName)
          ? vanillaSelectedWeather[level.PlanetName]
          : LevelWeatherType.None;

        // the weather should be more random by making it less random:

        // possible weathers taken from level.randomWeathers
        // use random for seeded randomness

        Plugin.logger.LogDebug("-------------");
        Plugin.logger.LogDebug($"{level.PlanetName}");
        Plugin.logger.LogDebug($"previousDayWeather: {previousDayWeather[level.PlanetName]}");

        currentWeather[level.PlanetName] = LevelWeatherType.None;

        if (level.overrideWeather)
        {
          Plugin.logger.LogDebug($"Override weather present, changing weather to {level.overrideWeatherType}");
          currentWeather[level.PlanetName] = level.overrideWeatherType;
          continue;
        }

        // and now the fun part

        // rework mechanic to use weighted lists

        var possibleWeathers = level
          .randomWeathers.Where(randomWeather =>
            randomWeather.weatherType != LevelWeatherType.None && randomWeather.weatherType != LevelWeatherType.DustClouds
          )
          .ToList();

        possibleWeathers.Do(x => Plugin.logger.LogDebug($"possibleWeathers: {x.weatherType}"));

        if (possibleWeathers.Count == 0)
        {
          Plugin.logger.LogDebug("No possible weathers, setting to None");
          currentWeather[level.PlanetName] = LevelWeatherType.None;
          continue;
        }

        List<LevelWeatherType> weathersToChooseFrom = possibleWeathers
          .ToList()
          .Select(x => x.weatherType)
          .Append(LevelWeatherType.None)
          .ToList();

        var weatherWeights = Variables.GetPlanetWeightedList(level, ConfigManager.Weights[previousDayWeather[level.PlanetName]]);
        var weather = weatherWeights[random.Next(0, weatherWeights.Count)];

        currentWeather[level.PlanetName] = weather;

        Plugin.logger.LogDebug($"currentWeather: {currentWeather[level.PlanetName]}");
        currentWeather[level.PlanetName] = currentWeather[level.PlanetName];
      }
      Plugin.logger.LogDebug("-------------");

      return currentWeather;
    }

    private static Dictionary<string, LevelWeatherType> FirstDayWeathers(
      StartOfRound startOfRound,
      List<string> planetsWithoutWeather,
      System.Random random
    )
    {
      Plugin.logger.LogInfo("First day, setting predefined weather conditions");

      var possibleWeathersTable = new ConsoleTables.ConsoleTable("planet", "randomWeathers");

      // from all levels, 2 cannot have a weather condition (41 Experimentation and 56 Vow)
      // if there are more than 9 levels (vanilla amount), make it 3 without weather

      Dictionary<string, LevelWeatherType> selectedWeathers = new Dictionary<string, LevelWeatherType>();

      SelectableLevel[] levels = startOfRound.levels;
      foreach (SelectableLevel level in levels)
      {
        string planetName = level.PlanetName;
        Plugin.logger.LogDebug($"planet: {planetName}");

        var randomWeathers = level
          .randomWeathers.Where(randomWeather =>
            randomWeather.weatherType != LevelWeatherType.None && randomWeather.weatherType != LevelWeatherType.DustClouds
          )
          .ToList();

        // var randomWeathers = level.randomWeathers.ToList();
        Plugin.logger.LogDebug($"randomWeathers count: {randomWeathers.Count}");
        randomWeathers.Do(x => Plugin.logger.LogDebug($"randomWeathers: {x.weatherType}"));

        var stringifiedRandomWeathers = JsonConvert.SerializeObject(randomWeathers.Select(x => x.weatherType.ToString()).ToList());

        if (randomWeathers.Count == 0 || randomWeathers == null)
        {
          Plugin.logger.LogDebug($"No random weathers for {planetName}, skipping");
          possibleWeathersTable.AddRow(level.PlanetName, stringifiedRandomWeathers);
          continue;
        }

        if (planetsWithoutWeather.Contains(planetName))
        {
          selectedWeathers[planetName] = LevelWeatherType.None;
          Plugin.logger.LogDebug($"Skipping {planetName} (predefined)");
          continue;
        }

        // 5% chance for eclipsed
        bool shouldBeEclipsed = random.Next(0, 100) < 5;
        var selectedRandom = randomWeathers[random.Next(0, randomWeathers.Count)];

        if (shouldBeEclipsed)
        {
          Plugin.logger.LogDebug($"Setting eclipsed for {planetName}");
          // check if eclipsed is possible in randomWeathers
          if (!randomWeathers.Any(x => x.weatherType == LevelWeatherType.Eclipsed))
          {
            Plugin.logger.LogDebug($"Eclipsed not possible for {planetName}, skipping");
            possibleWeathersTable.AddRow(level.PlanetName, stringifiedRandomWeathers);
            continue;
          }
          else
          {
            selectedRandom = randomWeathers.First(x => x.weatherType == LevelWeatherType.Eclipsed);
          }
        }

        Plugin.logger.LogDebug($"Set weather for {planetName}: {selectedRandom.weatherType}");
        selectedWeathers[planetName] = randomWeathers[random.Next(0, randomWeathers.Count)].weatherType;

        possibleWeathersTable.AddRow(level.PlanetName, stringifiedRandomWeathers);
      }

      Plugin.logger.LogInfo("Possible weathers:\n" + possibleWeathersTable.ToMinimalString());
      return selectedWeathers;
    }

    //
    //
    //

    private static Dictionary<string, LevelWeatherType> VanillaWeathers(int connectedPlayersOnServer, StartOfRound startOfRound)
    {
      Dictionary<string, LevelWeatherType> vanillaSelectedWeather = new Dictionary<string, LevelWeatherType>();

      System.Random random = new System.Random(startOfRound.randomMapSeed + 31);
      List<SelectableLevel> list = ((IEnumerable<SelectableLevel>)startOfRound.levels).ToList<SelectableLevel>();
      float num1 = 1f;
      if (connectedPlayersOnServer + 1 > 1 && startOfRound.daysPlayersSurvivedInARow > 2 && startOfRound.daysPlayersSurvivedInARow % 3 == 0)
        num1 = (float)random.Next(15, 25) / 10f;
      int num2 = Mathf.Clamp(
        (int)(
          (double)Mathf.Clamp(startOfRound.planetsWeatherRandomCurve.Evaluate((float)random.NextDouble()) * num1, 0.0f, 1f)
          * (double)startOfRound.levels.Length
        ),
        0,
        startOfRound.levels.Length
      );
      for (int index = 0; index < num2; ++index)
      {
        SelectableLevel selectableLevel = list[random.Next(0, list.Count)];
        if (selectableLevel.randomWeathers != null && selectableLevel.randomWeathers.Length != 0)
          vanillaSelectedWeather[selectableLevel.PlanetName] = selectableLevel
            .randomWeathers[random.Next(0, selectableLevel.randomWeathers.Length)]
            .weatherType;
        list.Remove(selectableLevel);
      }

      return vanillaSelectedWeather;
    }
  }
}
