using System;
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
          WeatherType weatherType = Variables.GetFullWeatherType(weatherData[levelName]);

          level.currentWeather = weatherType.weatherType;
          Variables.CurrentWeathers[level] = weatherType;

          logger.LogDebug($"Setting weather for {levelName} to {weatherType.Name}");
        }
        else
        {
          Plugin.logger.LogWarning($"Weather data for {levelName} somehow not found, skipping");
        }
      }

      StartOfRound.Instance.SetMapScreenInfoToCurrentLevel();
    }

    internal static void SetWeather(WeatherType weatherType)
    {
      SelectableLevel level = StartOfRound.Instance.currentLevel;

      level.currentWeather = weatherType.weatherType;
      Variables.CurrentWeathers[level] = weatherType;

      logger.LogDebug($"Setting weather for {level.PlanetName} to {weatherType.Name}");
    }

    internal static void SetWeatherEffects(TimeOfDay timeOfDay, List<WeatherEffect> weatherEffects)
    {
      // timeOfDay.globalTimeSpeedMultiplier = 0.001f;

      logger.LogDebug($"Setting weather effects for {timeOfDay.currentLevel.PlanetName}: {weatherEffects.Count} effects");
      if (weatherEffects == null)
      {
        logger.LogDebug("No weather effects to set");
        return;
      }

      Variables.CurrentEffects = weatherEffects;
      List<LevelWeatherType> sunBools = new List<LevelWeatherType>();

      foreach (WeatherEffect timeOfDayEffect in timeOfDay.effects)
      {
        int index = timeOfDay.effects.ToList().IndexOf(timeOfDayEffect);
        RandomWeatherWithVariables weatherVariables = StartOfRound
          .Instance.currentLevel.randomWeathers.ToList()
          .Find(x => x.weatherType == (LevelWeatherType)index);

        // logger.LogDebug($"Effect: {timeOfDayEffect.name}");
        // logger.LogWarning($"Is player inside: {EntranceTeleportPatch.isPlayerInside}");

        if (weatherEffects.Contains(timeOfDayEffect))
        {
          logger.LogDebug($"Enabling effect: {timeOfDayEffect.name}");

          if (!EntranceTeleportPatch.isPlayerInside)
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
          }
          else
          {
            logger.LogWarning($"Player is inside, skipping effect object activation");
            if (timeOfDayEffect.effectObject != null)
            {
              timeOfDayEffect.effectObject.SetActive(false);
            }

            if (timeOfDayEffect.effectPermanentObject != null)
            {
              timeOfDayEffect.effectPermanentObject.SetActive(false);
            }
          }

          if (timeOfDayEffect.sunAnimatorBool != "" && timeOfDayEffect.sunAnimatorBool != null)
          {
            sunBools.Add(weatherVariables.weatherType);
          }
        }
        else
        {
          logger.LogDebug($"Disabling effect: {timeOfDayEffect.name}");
          timeOfDayEffect.effectEnabled = false;

          if (timeOfDayEffect.effectObject != null)
          {
            timeOfDayEffect.effectObject.SetActive(false);
          }

          if (timeOfDayEffect.effectPermanentObject != null)
          {
            timeOfDayEffect.effectPermanentObject.SetActive(false);
          }

          try
          {
            if (!String.IsNullOrEmpty(timeOfDayEffect.sunAnimatorBool))
            {
              logger.LogDebug($"Removing sun animator bool, weather: {weatherVariables.weatherType}, bool: {timeOfDayEffect.sunAnimatorBool}");
              sunBools.Remove(weatherVariables.weatherType);
            }
          }
          catch (Exception e)
          {
            logger.LogWarning($"Error removing sun animator bool: {e}");
          }
        }
      }

      if (sunBools.Count == 0)
      {
        SunAnimator.OverrideSunAnimator(LevelWeatherType.None);
      }
      else
      {
        sunBools.Distinct().ToList().ForEach(loopWeatherType => SunAnimator.OverrideSunAnimator(loopWeatherType));
      }
    }
  }
}
