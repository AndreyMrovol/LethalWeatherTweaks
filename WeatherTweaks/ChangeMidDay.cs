using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using WeatherTweaks.Definitions;
using static WeatherTweaks.Definitions.Types;
using static WeatherTweaks.Modules.Types;

namespace WeatherTweaks
{
  [HarmonyPatch(typeof(TimeOfDay))]
  public static class ChangeMidDay
  {
    // internal static List<ProgressingWeatherEntry> weatherEntries = [];
    internal static float lastCheckedEntry = 0.0f;
    internal static System.Random random;
    internal static ProgressingWeatherEntry currentEntry;

    internal static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("WeatherTweaks ChangeMidDay");

    [HarmonyPostfix]
    [HarmonyPatch("MoveTimeOfDay")]
    internal static void MoveTimeOfDayPatch(TimeOfDay __instance)
    {
      WeatherType currentWeather = Variables.CurrentLevelWeather;

      if (currentWeather.Type != CustomWeatherType.Progressing)
      {
        return;
      }

      if (!StartOfRound.Instance.IsHost)
      {
        return;
      }

      float normalizedTimeOfDay = __instance.normalizedTimeOfDay;
      if (random == null)
      {
        random = new System.Random(StartOfRound.Instance.randomMapSeed);
      }

      // when this runs we know that weather is progressing *and* nothing else has been set as a current weather
      // this allows us to not set the weather type at 0.0f time, instead uses the "base" weather as a 0.0 entry
      if (currentEntry == null)
      {
        currentEntry = new ProgressingWeatherEntry()
        {
          DayTime = 0.0f,
          Chance = 1.0f,
          Weather = currentWeather.weatherType
        };
      }

      Definitions.Types.ProgressingWeatherType progressingWeather = Variables.ProgressingWeatherTypes.First(weather =>
        weather.Name == currentWeather.Name
      );
      List<ProgressingWeatherEntry> weatherEntries = progressingWeather.WeatherEntries.ToList();
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

          float randomRoll = (float)random.NextDouble();
          // entry.Chance = 1;

          if (randomRoll > entry.Chance)
          {
            logger.LogWarning($"Random roll failed - got {randomRoll}, needed {entry.Chance} or lower");
            lastCheckedEntry = entry.DayTime;
            break;
          }

          // TODO if currentEntry weather type is the same as the new entry, don't change it

          NetworkedConfig.SetProgressingWeatherEntry(entry);
          NetworkedConfig.SetWeatherEffects([entry.GetWeatherType().Weather]);

          TimeOfDay.Instance.StartCoroutine(DoMidDayChange(entry));

          lastCheckedEntry = entry.DayTime;
          currentEntry = entry;
          break;
        }
      }
    }

    internal static IEnumerator DoMidDayChange(ProgressingWeatherEntry entry)
    {
      if (entry == null)
      {
        logger.LogError("ProgressingWeatherEntry is null");
        yield return null;
      }

      logger.LogWarning(
        $"Changing weather to {entry.GetWeatherType().Name} at {entry.DayTime}, chance {entry.Chance} - is player inside? {EntranceTeleportPatch.isPlayerInside}"
      );

      HUDManager.Instance.ReadDialogue(entry.GetDialogueSegment().ToArray());

      yield return new WaitForSeconds(3);

      WeatherType fullWeatherType = Variables.GetFullWeatherType(entry.GetWeatherType());

      logger.LogWarning($"{fullWeatherType.Name} {fullWeatherType.Type} {fullWeatherType.weatherType}");

      StartOfRound.Instance.currentLevel.currentWeather = fullWeatherType.weatherType;
      TimeOfDay.Instance.currentLevelWeather = fullWeatherType.weatherType;
      GameNetworkManager.Instance.localPlayerController.currentAudioTrigger.weatherEffect = (int)fullWeatherType.weatherType;

      currentEntry = entry;

      GameInteraction.SetWeatherEffects(TimeOfDay.Instance, [fullWeatherType.Weather.Effect]);
      // TODO account for player being dead
    }
  }
}
