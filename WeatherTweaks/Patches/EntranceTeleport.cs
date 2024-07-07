using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using WeatherRegistry;
using WeatherTweaks.Definitions;
using WeatherType = WeatherTweaks.Definitions.WeatherType;

namespace WeatherTweaks
{
  [HarmonyPatch(typeof(EntranceTeleport))]
  internal class EntranceTeleportPatch
  {
    internal static MrovLib.Logger logger = new("WeatherTweaks EntranceTeleport", ConfigManager.LogLogs);
    internal static bool isPlayerInside = false;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(EntranceTeleport), "TeleportPlayer")]
    private static void TeleportPlayerPatch(EntranceTeleport __instance)
    {
      logger.LogDebug($"TeleportPlayerPatch called with {__instance.name}");

      isPlayerInside = __instance.isEntranceToBuilding;

      if (isPlayerInside)
      {
        logger.LogDebug("Player is inside");
      }
      else
      {
        logger.LogDebug("Player is outside");

        List<ImprovedWeatherEffect> weatherEffects = [];
        WeatherType currentWeather = Variables.GetCurrentWeather();

        if (currentWeather.Type == CustomWeatherType.Combined)
        {
          Definitions.Types.CombinedWeatherType currentWeatherCombined = (Definitions.Types.CombinedWeatherType)currentWeather;
          weatherEffects = currentWeatherCombined.Weathers.Select(weather => weather.Effect).ToList();
        }
        else
        {
          weatherEffects = [currentWeather.Weather.Effect];
        }

        // weatherEffects.Do(effect =>
        // {
        //   logger.LogDebug($"Effect Enabled: {effect.EffectEnabled}");
        // });

        foreach (Weather weather in WeatherRegistry.WeatherManager.Weathers)
        {
          logger.LogDebug($"Weather: {weather.Name}");

          if (weather.Type == WeatherRegistry.WeatherType.Clear)
          {
            continue;
          }

          if (weatherEffects.Contains(weather.Effect))
          {
            weather.Effect.EffectEnabled = true;
          }
          else
          {
            weather.Effect.DisableEffect();
          }
        }

        // foreach (WeatherEffect timeOfDayEffect in TimeOfDay.Instance.effects)
        // {
        //   logger.LogDebug($"Effect: {timeOfDayEffect.name}");
        //   logger.LogDebug($"Effect Enabled: {timeOfDayEffect.effectEnabled}");
        //   // logger.LogDebug($"Effect Object Enabled: {timeOfDayEffect.effectObject.activeSelf}");
        //   logger.LogInfo($"Contains: {weatherEffects.Contains(timeOfDayEffect)}");

        //   if (weatherEffects.Contains(timeOfDayEffect))
        //   {
        //     timeOfDayEffect.effectEnabled = true;

        //     if (timeOfDayEffect.effectObject != null)
        //     {
        //       timeOfDayEffect.effectObject.SetActive(true);
        //     }

        //     if (timeOfDayEffect.effectPermanentObject != null)
        //     {
        //       timeOfDayEffect.effectPermanentObject.SetActive(true);
        //     }
        //   }
        //   // else
        //   // {
        //   //   timeOfDayEffect.effectEnabled = false;

        //   //   if (timeOfDayEffect.effectObject != null)
        //   //   {
        //   //     timeOfDayEffect.effectObject.SetActive(false);
        //   //   }

        //   //   if (timeOfDayEffect.effectPermanentObject != null)
        //   //   {
        //   //     timeOfDayEffect.effectPermanentObject.SetActive(false);
        //   //   }
        //   // }
        // }
      }
    }
  }
}
