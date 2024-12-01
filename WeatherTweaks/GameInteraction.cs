using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using HarmonyLib;
using LethalNetworkAPI;
using Newtonsoft.Json;
using WeatherRegistry;
using WeatherTweaks.Definitions;

namespace WeatherTweaks
{
  internal class GameInteraction
  {
    internal static MrovLib.Logger logger = new("WeatherTweaks GameInteraction", ConfigManager.LogLogs);

    internal static void SetWeather(Dictionary<string, WeatherTweaksWeather> weatherData)
    {
      Plugin.logger.LogMessage("Setting weather");

      List<SelectableLevel> levels = Variables.GetGameLevels();
      foreach (SelectableLevel level in levels)
      {
        string levelName = level.PlanetName;
        logger.LogDebug($"Setting weather for {levelName}");

        if (weatherData.ContainsKey(levelName))
        {
          WeatherTweaksWeather weatherType = Variables.GetFullWeatherType(weatherData[levelName]);

          level.currentWeather = weatherType.VanillaWeatherType;
          // Variables.CurrentWeathers[level] = weatherType;

          logger.LogDebug($"Setting weather for {levelName} to {weatherType.Name}");
        }
        else
        {
          Plugin.logger.LogWarning($"Weather data for {levelName} somehow not found, skipping");
        }
      }

      StartOfRound.Instance.SetMapScreenInfoToCurrentLevel();
    }

    internal static void SetWeather(WeatherTweaksWeather weatherType)
    {
      SelectableLevel level = StartOfRound.Instance.currentLevel;

      level.currentWeather = weatherType.VanillaWeatherType;
      // Variables.CurrentWeathers[level] = weatherType;

      logger.LogDebug($"Setting weather for {level.PlanetName} to {weatherType.Name}");
    }

    internal static void SetWeatherEffects(TimeOfDay timeOfDay, List<ImprovedWeatherEffect> weatherEffects)
    {
      // timeOfDay.globalTimeSpeedMultiplier = 0.001f;

      if (!Variables.IsSetupFinished)
      {
        logger.LogDebug("Setup not finished, skipping setting weather effects");
        return;
      }

      logger.LogDebug($"Setting weather effects for {timeOfDay.currentLevel.PlanetName}: {weatherEffects.Count} effects");
      if (weatherEffects == null)
      {
        logger.LogDebug("No weather effects to set");
        return;
      }

      Variables.CurrentEffects = weatherEffects;
      List<LevelWeatherType> sunBools = [];

      List<ImprovedWeatherEffect> effectsToEnable = [];

      foreach (Weather weather in WeatherRegistry.WeatherManager.Weathers)
      {
        if (weather.Effect == null)
        {
          continue;
        }

        ImprovedWeatherEffect Effect = weather.Effect;

        if (weatherEffects.Contains(Effect))
        {
          effectsToEnable.Add(Effect);
        }

        Plugin.logger.LogDebug($"Disabling effect from weather: {weather.Name}");
        Effect.DisableEffect(true);
      }

      foreach (Weather weather in WeatherRegistry.WeatherManager.Weathers)
      {
        ImprovedWeatherEffect Effect = weather.Effect;

        if (weatherEffects.Contains(Effect))
        {
          logger.LogDebug($"Enabling effect from weather: {weather.Name}");

          if (!WeatherRegistry.Patches.EntranceTeleportPatch.isPlayerInside)
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
      }
    }
  }
}
