using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;

namespace WeatherTweaks
{
  internal class UncertainWeather
  {
    public static Dictionary<string, string> uncertainWeathers = new();

    public static Dictionary<string, string> GenerateUncertainty()
    {
      uncertainWeathers.Clear();

      Plugin.logger.LogInfo("GenerateUncertainty called.");
      StartOfRound startOfRound = StartOfRound.Instance;
      System.Random random = new(startOfRound.randomMapSeed + 31);

      // the plan is as follows:
      // we have 4 degrees of weather certainty:

      // 1. certain weather - we know exactly what the weather is
      // 2. uncertain weather - we know the weather is uncertain, but we don't know what it is - there's gonna be a (?) in the UI
      // 3. uncertain 50/50 - we know the weather is uncertain, but we know it's one of two options - there's gonna be a selection of two in the UI
      // 4. unknown weather - we don't know if the weather is uncertain or not - there's gonna be a [no data] in the UI

      // we're gonna use a dictionary to store the data

      int howManyPlanetsUncertain = Mathf.Clamp(
        (int)(
          (double)Mathf.Clamp(startOfRound.planetsWeatherRandomCurve.Evaluate((float)random.NextDouble()) * 0.4f, 0.0f, 1f)
          * (double)Variables.GameLevels.Count
        ),
        0,
        Variables.GameLevels.Count - 2
      );

      Plugin.logger.LogDebug($"howManyPlanetsUncertain: {howManyPlanetsUncertain}");

      List<SelectableLevel> whereWeatherUncertain = new();

      for (int i = 0; i < howManyPlanetsUncertain; i++)
      {
        SelectableLevel randomLevel = Variables.GameLevels[random.Next(Variables.GameLevels.Count)];

        if (!whereWeatherUncertain.Contains(randomLevel))
        {
          whereWeatherUncertain.Add(randomLevel);
        }
        else
        {
          i--;
        }
      }

      // create uncertainty strings
      Dictionary<string, string> uncertainWeather = new();

      foreach (SelectableLevel uncertainLevel in whereWeatherUncertain)
      {
        int uncertainType = random.Next(0, 5);

        switch (uncertainType)
        {
          case 1:
            Plugin.logger.LogDebug($"Weather on {uncertainLevel.PlanetName} is uncertain.");
            uncertainWeather.Add(uncertainLevel.PlanetName, GetUncertainString(uncertainLevel, random, false));
            break;
          case 2:
            Plugin.logger.LogDebug($"Weather on {uncertainLevel.PlanetName} is probable.");
            uncertainWeather.Add(uncertainLevel.PlanetName, GetUncertainString(uncertainLevel, random, true));
            break;
          case 3:
            Plugin.logger.LogDebug($"Weather on {uncertainLevel.PlanetName} is unknown.");
            uncertainWeather.Add(uncertainLevel.PlanetName, "[UNKNOWN]");
            break;
          default:
            Plugin.logger.LogDebug($"Weather on {uncertainLevel.PlanetName} is certain.");
            uncertainWeather.Add(uncertainLevel.PlanetName, uncertainLevel.currentWeather.ToString());
            break;
        }
      }

      uncertainWeathers = uncertainWeather;
      return uncertainWeather;
    }

    private static string GetUncertainString(SelectableLevel level, System.Random random, bool pickTwo = false)
    {
      var weather = level.currentWeather;

      // get random weather from list of random weathers without the current weather
      var randomWeathers = level.randomWeathers.Where(w => w.weatherType != weather).ToList();
      var randomWeather = randomWeathers[random.Next(randomWeathers.Count)];

      if (!pickTwo)
      {
        // 33% chance of getting wrong weather
        if (random.Next(0, 3) == 0)
        {
          Plugin.logger.LogDebug($"Weather on {level.PlanetName}: {randomWeather.weatherType}?");
          return $"{randomWeather.weatherType}?";
        }
        else
        {
          Plugin.logger.LogDebug($"Weather on {level.PlanetName}: {weather}?");
          return $"{weather}?";
        }
      }
      else
      {
        Plugin.logger.LogDebug($"Weather on {level.PlanetName}: {weather}/{randomWeather.weatherType}");

        if (random.Next(0, 2) == 0)
        {
          return $"{weather}/{randomWeather.weatherType}";
        }
        else
        {
          return $"{randomWeather.weatherType}/{weather}";
        }
      }
    }
  }
}
