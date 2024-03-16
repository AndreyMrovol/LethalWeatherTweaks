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

    internal static Dictionary<string, WeatherType> NewWeathers(StartOfRound startOfRound)
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
      Dictionary<string, WeatherType> currentWeather = [];

      List<SelectableLevel> levels = Variables.GetGameLevels(startOfRound);
      int day = startOfRound.gameStats.daysSpent;
      int quota = TimeOfDay.Instance.timesFulfilledQuota;
      int dayInQuota = day % 3;

      if (day == 0)
      {
        seed = ConfigManager.FirstDaySeed.Value;
        random = new System.Random(seed);

        List<string> noWeatherOnStartPlanets = ["41 Experimentation", "56 Vow"];
        List<SelectableLevel> planetsToPickFrom = levels.Where(level => !noWeatherOnStartPlanets.Contains(level.PlanetName)).ToList();

        if (levels.Count > 9)
        {
          // make 1/2 (default) of them have no weather
          int planetsWithoutWeather = (int)(levels.Count * 0.5);
          Plugin.logger.LogDebug($"Planets without weather: {planetsWithoutWeather + 2}");

          for (int i = 0; i < planetsWithoutWeather; i++)
          {
            // pick a random planet
            string planetName = planetsToPickFrom[random.Next(0, planetsToPickFrom.Count)].PlanetName;

            // add it to the list of planets without weather
            noWeatherOnStartPlanets.Add(planetName);

            // remove it from the list of planets to pick from
            planetsToPickFrom.RemoveAll(level => level.PlanetName == planetName);
          }
        }

        return FirstDayWeathers(levels, noWeatherOnStartPlanets, random);
      }

      float lengthMultiplier = quota * ConfigManager.GameLengthMultiplier.Value;
      float playerMultiplier = StartOfRound.Instance.livingPlayers * ConfigManager.GamePlayersMultiplier.Value;

      float difficultyMultiplier = lengthMultiplier + playerMultiplier;
      Plugin.logger.LogDebug($"Difficulty multiplier: {difficultyMultiplier}");

      foreach (SelectableLevel level in levels)
      {
        previousDayWeather[level.PlanetName] = level.currentWeather;

        LevelWeatherType vanillaWeather = vanillaSelectedWeather.ContainsKey(level.PlanetName)
          ? vanillaSelectedWeather[level.PlanetName]
          : LevelWeatherType.None;

        if (ConfigManager.AlwaysClear.Value)
        {
          Plugin.logger.LogDebug("AlwaysClear is true, setting weather to None");
          currentWeather[level.PlanetName] = Variables.NoneWeather;
          continue;
        }

        // the weather should be more random by making it less random:

        // possible weathers taken from level.randomWeathers
        // use random for seeded randomness

        Plugin.logger.LogDebug("-------------");
        Plugin.logger.LogDebug($"{level.PlanetName}");
        Plugin.logger.LogDebug($"previousDayWeather: {previousDayWeather[level.PlanetName]}");

        // change dust clouds to none (not defined in config)
        if (previousDayWeather[level.PlanetName] == LevelWeatherType.DustClouds)
        {
          previousDayWeather[level.PlanetName] = LevelWeatherType.None;
        }

        currentWeather[level.PlanetName] = Variables.NoneWeather;

        // and now the fun part
        // rework mechanic to use weighted lists

        // get all possible random weathers

        // var possibleWeathers = level
        //   .randomWeathers.Where(randomWeather =>
        //     randomWeather.weatherType != LevelWeatherType.None && randomWeather.weatherType != LevelWeatherType.DustClouds
        //   )
        //   .ToList();

        List<WeatherType> possibleWeathers = Variables.GetPlanetWeatherTypes(level);

        bool canBeDustClouds = level.randomWeathers.Any(randomWeather => randomWeather.weatherType == LevelWeatherType.DustClouds);

        // var stringifiedPossibleWeathers = JsonConvert.SerializeObject(possibleWeathers.Select(x => x.weatherType.ToString()).ToList());
        // Plugin.logger.LogDebug($"possibleWeathers: {stringifiedPossibleWeathers}");

        if (possibleWeathers.Count == 0)
        {
          Plugin.logger.LogDebug("No possible weathers, setting to None");
          currentWeather[level.PlanetName] = Variables.NoneWeather;
          continue;
        }

        if (level.overrideWeather)
        {
          Plugin.logger.LogDebug($"Override weather present, changing weather to {level.overrideWeatherType}");
          currentWeather[level.PlanetName] = Variables.WeatherTypes.Find(x =>
            x.weatherType == level.overrideWeatherType && x.Type == CustomWeatherType.Vanilla
          );
          continue;
        }

        // add None to the list of possible weathers
        List<LevelWeatherType> weathersToChooseFrom = possibleWeathers.Select(x => x.weatherType).Append(LevelWeatherType.None).ToList();

        // get the weighted list of weathers from config
        var weatherWeights = Variables.GetPlanetWeightedList(
          level,
          ConfigManager.Weights[previousDayWeather[level.PlanetName]],
          difficultyMultiplier
        );
        var weather = weatherWeights[random.Next(0, weatherWeights.Count)];

        if (weather == Variables.NoneWeather && canBeDustClouds)
        {
          // fixed 25% chance for dust clouds (replacing None as closest non-weather weather)
          if (random.Next(0, 100) < 25)
          {
            weather = Variables.WeatherTypes.Find(x => x.weatherType == LevelWeatherType.DustClouds);
          }
        }

        currentWeather[level.PlanetName] = weather;
        Variables.CurrentWeathers[level] = weather;

        Plugin.logger.LogDebug($"Selected weather: {weather.Name}");
        try
        {
          Plugin.logger.LogDebug(
            $"Chance for that was {weatherWeights.Where(x => x == weather).Count()} / {weatherWeights.Count} ({(float)weatherWeights.Where(x => x == weather).Count() / weatherWeights.Count * 100}%)"
          );
        }
        catch { }

        // currentWeather[level.PlanetName] = currentWeather[level.PlanetName];
      }

      SelectableLevel companyMoon = LethalLevelLoader.PatchedContent.SeletectableLevels.Find(level => level.PlanetName == "71 Gordion");
      if (companyMoon != null)
      {
        Variables.CurrentWeathers[companyMoon] = Variables.NoneWeather;
        currentWeather[companyMoon.PlanetName] = Variables.NoneWeather;
      }

      Plugin.logger.LogDebug("-------------");

      return currentWeather;
    }

    private static Dictionary<string, WeatherType> FirstDayWeathers(
      List<SelectableLevel> levels,
      List<string> planetsWithoutWeather,
      System.Random random
    )
    {
      Plugin.logger.LogInfo("First day, setting predefined weather conditions");

      var possibleWeathersTable = new ConsoleTables.ConsoleTable("planet", "randomWeathers");

      // from all levels, 2 cannot have a weather condition (41 Experimentation and 56 Vow)
      // if there are more than 9 levels (vanilla amount), make it 3 without weather

      Dictionary<string, WeatherType> selectedWeathers = new Dictionary<string, WeatherType>();

      foreach (SelectableLevel level in levels)
      {
        string planetName = level.PlanetName;
        Plugin.logger.LogDebug($"planet: {planetName}");

        if (ConfigManager.AlwaysClear.Value)
        {
          Plugin.logger.LogDebug("AlwaysClear is true, setting weather to None");
          selectedWeathers[level.PlanetName] = Variables.NoneWeather;
          continue;
        }

        var randomWeathers = Variables
          .GetPlanetWeatherTypes(level)
          .Where(randomWeather =>
            randomWeather.weatherType != LevelWeatherType.None
            && randomWeather.weatherType != LevelWeatherType.DustClouds
            && randomWeather.Type == CustomWeatherType.Vanilla
          )
          .ToList();

        // var randomWeathers = level.randomWeathers.ToList();
        Plugin.logger.LogDebug($"randomWeathers count: {randomWeathers.Count}");
        randomWeathers.Do(x => Plugin.logger.LogDebug($"randomWeathers: {x.Name}"));

        var stringifiedRandomWeathers = JsonConvert.SerializeObject(randomWeathers.Select(x => x.weatherType.ToString()).ToList());
        possibleWeathersTable.AddRow(level.PlanetName, stringifiedRandomWeathers);

        randomWeathers.RemoveAll(x => x.weatherType == LevelWeatherType.Eclipsed);

        if (randomWeathers.Count == 0 || randomWeathers == null)
        {
          Plugin.logger.LogDebug($"No random weathers for {planetName}, skipping");
          continue;
        }

        if (planetsWithoutWeather.Contains(planetName))
        {
          selectedWeathers[planetName] = Variables.NoneWeather;
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
            Plugin.logger.LogDebug($"Eclipsed not possible for {planetName}, setting random weather");
          }
          else
          {
            selectedRandom = randomWeathers.First(x => x.weatherType == LevelWeatherType.Eclipsed);
          }
        }

        WeatherType selectedWeather = Variables.WeatherTypes.Find(x =>
          x.weatherType == selectedRandom.weatherType && x.Type == CustomWeatherType.Vanilla
        );
        selectedWeathers[planetName] = selectedWeather;
        Variables.CurrentWeathers[level] = selectedWeather;
        Plugin.logger.LogDebug($"Set weather for {planetName}: {selectedWeather.Name}");
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
