using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MrovLib;
using Newtonsoft.Json;
using UnityEngine;

namespace WeatherTweaks
{
  internal class UncertainWeather
  {
    public static Dictionary<string, string> uncertainWeathers = [];
    public static List<Modules.Types.UncertainWeatherType> uncertainWeatherTypes = [];

    public static void Init()
    {
      Plugin.logger.LogInfo("UncertainWeather initialized.");

      uncertainWeatherTypes = [new UncertainTypes.Uncertain(), new UncertainTypes.Uncertain5050(), new UncertainTypes.Unknown()];
    }

    public static Dictionary<string, string> GenerateUncertainty()
    {
      uncertainWeathers.Clear();

      if (!ConfigManager.UncertainWeatherEnabled.Value)
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

      int howManyPlanetsUncertain = Mathf.Clamp(
        (int)(
          (double)Mathf.Clamp(startOfRound.planetsWeatherRandomCurve.Evaluate((float)random.NextDouble()) * 0.4f, 0.0f, 1f)
          * (double)Variables.GameLevels.Count
        ),
        1,
        Variables.GameLevels.Count - 2
      );

      if (ConfigManager.AlwaysUncertain.Value || ConfigManager.AlwaysUnknown.Value)
      {
        howManyPlanetsUncertain = Variables.GameLevels.Count;
      }

      Plugin.logger.LogDebug($"howManyPlanetsUncertain: {howManyPlanetsUncertain}");

      List<SelectableLevel> whereWeatherUncertain = [];

      for (int i = 0; i < howManyPlanetsUncertain; i++)
      {
        SelectableLevel randomLevel = Variables.GameLevels[random.Next(Variables.GameLevels.Count)];

        if (randomLevel == LevelHelper.CompanyMoon)
        {
          i--;
          continue;
        }

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
      List<Modules.Types.UncertainWeatherType> uncertainTypes = [];

      foreach (Modules.Types.UncertainWeatherType type in uncertainWeatherTypes)
      {
        if (type.Enabled.Value)
        {
          uncertainTypes.Add(type);
        }
      }

      if (ConfigManager.AlwaysUnknown.Value)
      {
        Plugin.logger.LogDebug("Setting possible types to only unknown.");
        uncertainTypes = [new UncertainTypes.Unknown()];
      }

      Plugin.logger.LogDebug($"uncertainTypes: {uncertainTypes.Count}");

      if (uncertainTypes.Count == 0)
      {
        Plugin.logger.LogInfo("No uncertain types are enabled, skipping uncertainty generation.");
        return uncertainWeathers;
      }

      foreach (SelectableLevel uncertainLevel in whereWeatherUncertain)
      {
        int rolledType = random.Next(uncertainTypes.Count);

        string uncertainString = uncertainTypes[rolledType].CreateUncertaintyString(uncertainLevel, random);
        Plugin.logger.LogDebug($"Rolled type: {uncertainTypes[rolledType].Name}, setting its uncertainty to {uncertainString}.");

        uncertainWeathersRolled.Add(uncertainLevel.PlanetName, uncertainString);
      }

      uncertainWeathers = uncertainWeathersRolled;
      return uncertainWeathersRolled;
    }
  }
}
