using System.Collections.Generic;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using LethalLevelLoader;
using UnityEngine;

namespace WeatherTweaks
{
  [HarmonyPatch(typeof(EntranceTeleport))]
  internal class EntranceTeleportPatch
  {
    internal static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("WeatherTweaks EntranceTeleport");
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

        List<WeatherEffect> weatherEffects = Variables.GetCurrentWeather().Effects;

        weatherEffects.Do(effect =>
        {
          logger.LogDebug($"Effect: {effect.name}");
          logger.LogDebug($"Effect Enabled: {effect.effectEnabled}");
        });

        foreach (WeatherEffect timeOfDayEffect in TimeOfDay.Instance.effects)
        {
          logger.LogDebug($"Effect: {timeOfDayEffect.name}");
          logger.LogDebug($"Effect Enabled: {timeOfDayEffect.effectEnabled}");
          // logger.LogDebug($"Effect Object Enabled: {timeOfDayEffect.effectObject.activeSelf}");
          logger.LogInfo($"Contains: {weatherEffects.Contains(timeOfDayEffect)}");

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
          }
          // else
          // {
          //   timeOfDayEffect.effectEnabled = false;

          //   if (timeOfDayEffect.effectObject != null)
          //   {
          //     timeOfDayEffect.effectObject.SetActive(false);
          //   }

          //   if (timeOfDayEffect.effectPermanentObject != null)
          //   {
          //     timeOfDayEffect.effectPermanentObject.SetActive(false);
          //   }
          // }
        }
      }
    }
  }
}
