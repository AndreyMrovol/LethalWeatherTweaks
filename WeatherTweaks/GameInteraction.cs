using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using HarmonyLib;
using LethalNetworkAPI;
using Newtonsoft.Json;

namespace WeatherTweaks
{
  internal class GameInteraction
  {
    internal static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("WeatherTweaks GameInteraction");

    internal static void SetWeather(Dictionary<string, WeatherType> weatherData)
    {
      Plugin.logger.LogMessage("Setting weather");

      List<SelectableLevel> levels = Variables.GetGameLevels(StartOfRound.Instance);
      foreach (SelectableLevel level in levels)
      {
        string levelName = level.PlanetName;

        if (weatherData.ContainsKey(levelName))
        {
          level.currentWeather = weatherData[levelName].weatherType;
          logger.LogDebug($"Setting weather for {levelName} to {level.currentWeather}");
        }
        else
        {
          Plugin.logger.LogWarning($"Weather data for {levelName} somehow not found, skipping");
        }
      }

      StartOfRound.Instance.SetMapScreenInfoToCurrentLevel();
    }

    internal static void SetWeatherEffects(TimeOfDay timeOfDay, List<WeatherEffect> weatherEffects)
    {
      // timeOfDay.globalTimeSpeedMultiplier = 0.001f;

      logger.LogDebug($"Setting weather effects for {timeOfDay.currentLevel.PlanetName}");

      // timeOfDay.Instance.DisableAllWeather();
      foreach (WeatherEffect timeOfDayEffect in timeOfDay.effects)
      {
        int index = timeOfDay.effects.ToList().IndexOf(timeOfDayEffect);
        RandomWeatherWithVariables weatherVariables = StartOfRound
          .Instance.currentLevel.randomWeathers.ToList()
          .Find(x => x.weatherType == (LevelWeatherType)index);

        logger.LogDebug($"Effect: {timeOfDayEffect.name}");

        if (weatherEffects.Contains(timeOfDayEffect))
        {
          timeOfDayEffect.effectEnabled = true;

          if (timeOfDayEffect.effectObject != null)
          {
            timeOfDayEffect.effectObject.SetActive(true);
          }

          if (timeOfDayEffect.effectPermanentObject != null)
          {
            timeOfDayEffect.effectPermanentObject.SetActive(true);
          }

          if (timeOfDayEffect.sunAnimatorBool == "eclipse")
          {
            timeOfDay.sunAnimator.SetBool(timeOfDay.effects[index].sunAnimatorBool, true);
          }

          if (timeOfDayEffect.name == "flooded")
          {
            timeOfDay.currentWeatherVariable = weatherVariables.weatherVariable;
            timeOfDay.currentWeatherVariable2 = weatherVariables.weatherVariable2;
          }
        }
      }
    }
  }
}
