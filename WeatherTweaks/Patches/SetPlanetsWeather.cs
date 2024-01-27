using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UIElements.Collections;

namespace WeatherTweaks
{
  [HarmonyPatch(typeof(StartOfRound))]
  public static class SetPlanetsWeatherPatch
  {
    [HarmonyPatch("SetPlanetsWeather")]
    [HarmonyPrefix]
    private static bool SetWeathers(int connectedPlayersOnServer, StartOfRound __instance)
    {
      if (!StartOfRound.Instance.IsHost)
        return true;

      Plugin.logger.LogMessage("SetPlanetsWeather called.");

      var table = new ConsoleTables.ConsoleTable("Planet", "Weather", "Previous", "Vanilla");

      System.Random random = new System.Random(__instance.randomMapSeed + 31);

      Dictionary<string, LevelWeatherType> previousDayWeather = new Dictionary<string, LevelWeatherType>();
      Dictionary<string, LevelWeatherType> vanillaSelectedWeather = VanillaWeathers(
        connectedPlayersOnServer,
        __instance
      );

      SelectableLevel[] levels = __instance.levels;
      int day = __instance.gameStats.daysSpent;
      int dayInQuota = day % 3;

      if (day == 0)
      {
        List<string> noWeatherOnStartPlanets = new List<string> { "41 Experimentation", "56 Vow" };

        if (levels.Length > 9)
        {
          // pick another random planet
          noWeatherOnStartPlanets.Add(levels[random.Next(0, levels.Length)].PlanetName);
        }

        FirstDayWeathers(__instance, noWeatherOnStartPlanets, random);
        return false;
      }

      foreach (SelectableLevel level in levels)
      {
        if (level.PlanetName == "71 Gordion")
        {
          continue;
        }

        previousDayWeather[level.PlanetName] = level.currentWeather;

        LevelWeatherType vanillaWeather = vanillaSelectedWeather.ContainsKey(level.PlanetName)
          ? vanillaSelectedWeather[level.PlanetName]
          : LevelWeatherType.None;

        // the weather should be more random by making it less random:

        // if weather was clear, 50% chance for weather next day
        // if weather was not clear, weather cannot repeat
        // if eclipsed, 85% chance for no weather next day (15% chance for weather - not eclipsed)

        // possible weathers taken from level.randomWeathers
        // use random for seeded randomness

        Plugin.logger.LogDebug("-------------");
        Plugin.logger.LogDebug($"{level.PlanetName}");

        level.currentWeather = LevelWeatherType.None;

        if (level.overrideWeather)
        {
          Plugin.logger.LogDebug($"Override weather present, changing weather to {level.overrideWeatherType}");
          level.currentWeather = level.overrideWeatherType;
          table.AddRow(level.PlanetName, level.currentWeather, previousDayWeather[level.PlanetName], vanillaWeather);
          continue;
        }

        // and now the fun part

        // if weather was clear, 50% chance for weather next day
        if (previousDayWeather[level.PlanetName] == LevelWeatherType.None)
        {
          Plugin.logger.LogDebug("Weather was clear, 50% chance for weather");
          if (random.Next(0, 2) == 0)
          {
            level.currentWeather = LevelWeatherType.None;
          }
          else
          {
            if (level.randomWeathers.Length == 0 || level.randomWeathers == null)
            {
              Plugin.logger.LogDebug("No random weathers, setting to None");
              level.currentWeather = LevelWeatherType.None;
              table.AddRow(
                level.PlanetName,
                level.currentWeather,
                previousDayWeather[level.PlanetName],
                vanillaWeather
              );
              continue;
            }

            level.currentWeather = level.randomWeathers[random.Next(0, level.randomWeathers.Length)].weatherType;
          }
        }
        else
        // if weather was not clear, weather cannot repeat
        // if eclipsed, 85% chance for no weather next day (15% chance for weather - not eclipsed)
        {
          Plugin.logger.LogDebug("Weather was not clear, weather cannot repeat");

          var possibleWeathers = level
            .randomWeathers.ToList()
            .Where(x => x.weatherType != previousDayWeather[level.PlanetName])
            .ToList();

          if (possibleWeathers.Count == 0)
          {
            Plugin.logger.LogDebug("No possible weathers, setting to None");
            table.AddRow(level.PlanetName, level.currentWeather, previousDayWeather[level.PlanetName], vanillaWeather);
            continue;
          }

          if (previousDayWeather[level.PlanetName] == LevelWeatherType.Eclipsed)
          {
            if (random.Next(0, 100) < 85)
            {
              Plugin.logger.LogDebug("Eclipsed, 85% chance for no weather");
              level.currentWeather = LevelWeatherType.None;
            }
            else
            {
              Plugin.logger.LogDebug("Eclipsed, 15% chance for weather");
              level.currentWeather = possibleWeathers[random.Next(0, possibleWeathers.Count)].weatherType;
            }
          }
          else
          {
            level.currentWeather = possibleWeathers[random.Next(0, possibleWeathers.Count)].weatherType;
          }
        }

        Plugin.logger.LogDebug($"currentWeather: {level.currentWeather}");
        table.AddRow(level.PlanetName, level.currentWeather, previousDayWeather[level.PlanetName], vanillaWeather);

        // SavedWeatherData.SaveCurrentWeatherData(day, previousDayWeather);

        //
        //
      }

      // VanillaWeather(connectedPlayersOnServer, __instance);

      var tableToPrint = table.ToMinimalString();
      Plugin.logger.LogInfo("\n" + tableToPrint);

      return false;
    }

    private static void FirstDayWeathers(
      StartOfRound __instance,
      List<string> planetsWithoutWeather,
      System.Random random
    )
    {
      Plugin.logger.LogInfo("First day, setting predefined weather conditions");

      var table = new ConsoleTables.ConsoleTable("planet", "currentWeather");

      // from all levels, 2 cannot have a weather condition (41 Experimentation and 56 Vow)
      // if there are more than 9 levels (vanilla amount), make it 3 without weather

      SelectableLevel[] levels = __instance.levels;
      foreach (SelectableLevel level in levels)
      {
        string planetName = level.PlanetName;

        if (planetsWithoutWeather.Contains(planetName))
        {
          level.currentWeather = LevelWeatherType.None;
        }
        else
        {
          var randomWeathers = level.randomWeathers.ToList();
          randomWeathers.Do(x => Plugin.logger.LogDebug($"randomWeathers: {x.weatherType}"));

          if (randomWeathers.Count == 0 || randomWeathers == null)
          {
            Plugin.logger.LogDebug($"No random weathers for {planetName}, skipping");
            table.AddRow(level.PlanetName, level.currentWeather);
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
              table.AddRow(level.PlanetName, level.currentWeather);
              continue;
            }
            else
            {
              selectedRandom = randomWeathers.First(x => x.weatherType == LevelWeatherType.Eclipsed);
            }
          }

          Plugin.logger.LogDebug($"planet: {planetName}");
          Plugin.logger.LogDebug($"randomWeather: {selectedRandom.weatherType}");

          level.currentWeather = randomWeathers[random.Next(0, randomWeathers.Count)].weatherType;
        }

        table.AddRow(level.PlanetName, level.currentWeather);
      }

      var tableToPrint = table.ToMinimalString();
      Plugin.logger.LogInfo("\n" + tableToPrint);
    }

    private static Dictionary<string, LevelWeatherType> VanillaWeathers(
      int connectedPlayersOnServer,
      StartOfRound __instance
    )
    {
      Dictionary<string, LevelWeatherType> vanillaSelectedWeather = new Dictionary<string, LevelWeatherType>();

      System.Random random = new System.Random(__instance.randomMapSeed + 31);
      List<SelectableLevel> list = ((IEnumerable<SelectableLevel>)__instance.levels).ToList<SelectableLevel>();
      float num1 = 1f;
      if (
        connectedPlayersOnServer + 1 > 1
        && __instance.daysPlayersSurvivedInARow > 2
        && __instance.daysPlayersSurvivedInARow % 3 == 0
      )
        num1 = (float)random.Next(15, 25) / 10f;
      int num2 = Mathf.Clamp(
        (int)(
          (double)
            Mathf.Clamp(__instance.planetsWeatherRandomCurve.Evaluate((float)random.NextDouble()) * num1, 0.0f, 1f)
          * (double)__instance.levels.Length
        ),
        0,
        __instance.levels.Length
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
