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

    public enum UncertainWeatherType
    {
      Certain = 0,
      Uncertain = 1,
      Uncertain5050 = 2,
      Unknown = 3
    }

    public static Dictionary<string, string> GenerateUncertainty()
    {
      uncertainWeathers.Clear();

      if (!ConfigManager.UncertainWeatherEnabled.Value || !ConfigManager.MapScreenPatch.Value)
      {
        Plugin.logger.LogInfo("Uncertain weathers are disabled.");
        return uncertainWeathers;
      }

      if (StartOfRound.Instance.gameStats.daysSpent == 0 && !(ConfigManager.AlwaysUncertain.Value || ConfigManager.AlwaysUnknown.Value))
      {
        Plugin.logger.LogInfo("It's the first day, no uncertainty will be generated.");
        return uncertainWeathers;
      }

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

      if (ConfigManager.AlwaysUncertain.Value || ConfigManager.AlwaysUnknown.Value)
      {
        howManyPlanetsUncertain = Variables.GameLevels.Count;
      }

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
      Dictionary<string, string> uncertainWeathersRolled = [];
      List<UncertainWeatherType> uncertainTypes = [];

      if (!ConfigManager.AlwaysUncertain.Value && !ConfigManager.AlwaysUnknown.Value)
      {
        Plugin.logger.LogDebug("Adding certain type.");
        uncertainTypes.Add(UncertainWeatherType.Uncertain);
      }

      if (ConfigManager.UncertainUncertain.Value)
      {
        Plugin.logger.LogDebug("Adding uncertain type.");
        uncertainTypes.Add(UncertainWeatherType.Uncertain);
      }

      if (ConfigManager.Uncertain5050.Value)
      {
        Plugin.logger.LogDebug("Adding uncertain 50/50 type.");
        uncertainTypes.Add(UncertainWeatherType.Uncertain5050);
      }

      if (ConfigManager.UncertainUnknown.Value)
      {
        Plugin.logger.LogDebug("Adding unknown type.");
        uncertainTypes.Add(UncertainWeatherType.Unknown);
      }

      if (ConfigManager.AlwaysUnknown.Value)
      {
        Plugin.logger.LogDebug("Setting possible types to only unknown.");
        uncertainTypes.Clear();
        uncertainTypes.Add(UncertainWeatherType.Unknown);
      }

      Plugin.logger.LogDebug($"uncertainTypes: {uncertainTypes.Count}");

      foreach (SelectableLevel uncertainLevel in whereWeatherUncertain)
      {
        int rolledType = random.Next(uncertainTypes.Count);
        // roll for uncertain type from uncertainTypes list, resolve it, log and add to dictionary

        // convert rolledType to UncertainWeatherType

        switch (uncertainTypes[rolledType])
        {
          case UncertainWeatherType.Uncertain:
            Plugin.logger.LogDebug($"Weather on {uncertainLevel.PlanetName} is uncertain.");
            uncertainWeathersRolled.Add(uncertainLevel.PlanetName, GetUncertainString(uncertainLevel, random));
            break;
          case UncertainWeatherType.Uncertain5050:
            Plugin.logger.LogDebug($"Weather on {uncertainLevel.PlanetName} is probable.");
            uncertainWeathersRolled.Add(uncertainLevel.PlanetName, GetUncertainString(uncertainLevel, random, true));
            break;
          case UncertainWeatherType.Unknown:
            Plugin.logger.LogDebug($"Weather on {uncertainLevel.PlanetName} is unknown.");
            uncertainWeathersRolled.Add(uncertainLevel.PlanetName, "[UNKNOWN]");
            break;
          case UncertainWeatherType.Certain:
            Plugin.logger.LogDebug($"Weather on {uncertainLevel.PlanetName} is certain.");
            break;
        }
      }

      uncertainWeathers = uncertainWeathersRolled;
      return uncertainWeathersRolled;
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
