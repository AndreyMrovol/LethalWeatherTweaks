using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using HarmonyLib;
using LethalNetworkAPI;
using Newtonsoft.Json;
using WeatherRegistry;
using WeatherTweaks.Definitions;
using WeatherType = WeatherTweaks.Definitions.WeatherType;

namespace WeatherTweaks
{
  internal class GameInteraction
  {
    internal static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("WeatherTweaks GameInteraction");

    internal static void SetWeather(Dictionary<string, WeatherType> weatherData)
    {
      Plugin.logger.LogMessage("Setting weather");

      List<SelectableLevel> levels = Variables.GetGameLevels();
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

    internal static void SetWeatherEffects(TimeOfDay timeOfDay, List<ImprovedWeatherEffect> weatherEffects)
    {
      // timeOfDay.globalTimeSpeedMultiplier = 0.001f;

      logger.LogDebug($"Setting weather effects for {timeOfDay.currentLevel.PlanetName}: {weatherEffects.Count} effects");
      if (weatherEffects == null)
      {
        logger.LogDebug("No weather effects to set");
        return;
      }

      Variables.CurrentEffects = weatherEffects;
      List<LevelWeatherType> sunBools = [];

      foreach (Weather weather in WeatherRegistry.WeatherManager.Weathers)
      {
        if (weather.Effect == null)
        {
          continue;
        }

        ImprovedWeatherEffect Effect = weather.Effect;

        if (weatherEffects.Contains(Effect))
        {
          logger.LogDebug($"Enabling effect from weather: {weather.Name}");

          if (!EntranceTeleportPatch.isPlayerInside)
          {
            weather.Effect.EffectEnabled = true;
          }
          else
          {
            logger.LogWarning($"Player is inside, skipping effect object activation");
            weather.Effect.DisableEffect(true);
          }

          if (Effect.SunAnimatorBool != "" && Effect.SunAnimatorBool != null)
          {
            sunBools.Add(weather.VanillaWeatherType);
          }
        }
        else
        {
          logger.LogDebug($"Disabling effect: {weather.Name}");

          weather.Effect.DisableEffect(true);

          // try
          // {
          //   if (!String.IsNullOrEmpty(Effect.SunAnimatorBool))
          //   {
          //     logger.LogDebug($"Removing sun animator bool, weather: {weather.Name}, bool: {Effect.SunAnimatorBool}");
          //     sunBools.Remove(weather.VanillaWeatherType);
          //   }
          // }
          // catch (Exception e)
          // {
          //   logger.LogInfo($"Cannot remove sun animator bool: {e.Message}");
          // }
        }
      }

      if (sunBools.Count == 0)
      {
        WeatherRegistry.Patches.SunAnimator.OverrideSunAnimator(LevelWeatherType.None);
      }
      else
      {
        sunBools.Distinct().ToList().ForEach(loopWeatherType => WeatherRegistry.Patches.SunAnimator.OverrideSunAnimator(loopWeatherType));
      }
    }
  }
}
