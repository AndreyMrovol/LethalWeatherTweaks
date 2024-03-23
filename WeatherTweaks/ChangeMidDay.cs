using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using static WeatherTweaks.Modules.Types;

namespace WeatherTweaks
{
  [HarmonyPatch(typeof(TimeOfDay))]
  public static class ChangeMidDay
  {
    // internal static List<ProgressingWeatherEntry> weatherEntries = [];
    internal static float lastCheckedEntry = 0.0f;

    internal static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("WeatherTweaks ChangeMidDay");

    [HarmonyPostfix]
    [HarmonyPatch("MoveTimeOfDay")]
    internal static void MoveTimeOfDayPatch(TimeOfDay __instance)
    {
      WeatherType currentWeather = Variables.CurrentLevelWeather;

      if (!StartOfRound.Instance.IsHost)
      {
        return;
      }

      if (currentWeather.Type != CustomWeatherType.Progressing)
      {
        return;
      }

      float normalizedTimeOfDay = __instance.normalizedTimeOfDay;

      ProgressingWeatherType progressingWeather = Variables.ProgressingWeatherTypes.First(weather => weather.Name == currentWeather.Name);
      List<ProgressingWeatherEntry> weatherEntries = progressingWeather.WeatherEntries;
      weatherEntries.RemoveAll(entry => entry.DayTime < lastCheckedEntry);

      // the plan:
      // all entries should be sorted by the Time float
      // we save the last time we've checked the list
      // if the new time is greater than an entry, we change the weather to that entry's weather
      // we then update the last time we've checked the list to the new time
      // this way we can change the weather at specific times of the day
      // without having to check every frame (although we do lol)

      foreach (ProgressingWeatherEntry entry in weatherEntries)
      {
        if (normalizedTimeOfDay > entry.DayTime && entry.DayTime > lastCheckedEntry)
        // this means we've passed the time of day for this entry and we haven't checked it yet
        {
          logger.LogInfo($"Changing weather to {entry.GetWeatherType().Name} at {entry.DayTime}");

          NetworkedConfig.SetWeatherEffects(entry.GetWeatherType().Weathers.ToList());
          NetworkedConfig.SetWeatherType(entry.GetWeatherType());

          Variables.CurrentLevelWeather = entry.GetWeatherType();

          logger.LogWarning($"Player inside: {EntranceTeleportPatch.isPlayerInside}");

          GameInteraction.SetWeatherEffects(__instance, entry.GetWeatherType().Effects.ToList());

          HUDManager.Instance.ReadDialogue(entry.GetDialogueSegment().ToArray());

          lastCheckedEntry = entry.DayTime;
        }
      }
    }
  }
}
