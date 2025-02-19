using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using UnityEngine.AI;
using WeatherRegistry;
using WeatherTweaks.Definitions;

namespace WeatherTweaks
{
  [HarmonyPatch(typeof(TimeOfDay))]
  public static class ChangeMidDay
  {
    internal static System.Random random;

    internal static ProgressingWeatherType currentWeather = null;
    internal static List<ProgressingWeatherEntry> weatherEntries = [];

    internal static float LastCheckedEntry = 0;

    internal static ProgressingWeatherEntry CurrentEntry = null;
    internal static ProgressingWeatherEntry NextEntry => weatherEntries.FirstOrDefault();

    internal static float NextEntryTime => NextEntry != null ? NextEntry.DayTime : 0f;

    internal static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("WeatherTweaks ChangeMidDay");

    [HarmonyPostfix]
    [HarmonyPatch("MoveTimeOfDay")]
    internal static void MoveTimeOfDayPatch(TimeOfDay __instance)
    {
      if (!StartOfRound.Instance.IsHost)
      {
        return;
      }

      if (currentWeather == null)
      {
        return;
      }

      float normalizedTimeOfDay = __instance.normalizedTimeOfDay;
      // logger.LogDebug($"Normalized time of day: {normalizedTimeOfDay}");
      if (normalizedTimeOfDay >= NextEntryTime)
      {
        RunProgressingEntryActions(normalizedTimeOfDay);
      }
    }

    internal static void RunProgressingEntryActions(float normalizedTimeOfDay)
    {
      weatherEntries.RemoveAll(entry => entry.DayTime < LastCheckedEntry);

      // the plan:
      // all entries are sorted by the Time float
      // we save the last time we've checked the list
      // if the new time is greater than an entry, we change the weather to that entry's weather
      // we then update the last time we've checked the list to the new time
      // this way we can change the weather at specific times of the day
      // without having to check every frame (although we do lol)

      foreach (ProgressingWeatherEntry entry in weatherEntries)
      {
        if (normalizedTimeOfDay > entry.DayTime && entry.DayTime > LastCheckedEntry)
        // this means we've passed the time of day for this entry and we haven't checked it yet
        {
          logger.LogInfo($"Changing weather to {entry.GetWeather().Name} at {entry.DayTime}");

          float randomRoll = (float)random.NextDouble();
          // entry.Chance = 1;

          if (randomRoll > entry.Chance)
          {
            logger.LogWarning($"Random roll failed - got {randomRoll}, needed {entry.Chance} or lower");
            LastCheckedEntry = entry.DayTime;
            break;
          }

          NetworkedConfig.SetProgressingWeatherEntry(entry);
          TimeOfDay.Instance.StartCoroutine(DoMidDayChange(entry));

          LastCheckedEntry = entry.DayTime;
          CurrentEntry = entry;
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

      logger.LogMessage(
        $"Changing weather to {entry.GetWeather().Name} at {entry.DayTime}, chance {entry.Chance} - is player inside? {WeatherRegistry.Patches.EntranceTeleportPatch.isPlayerInside}"
      );

      HUDManager.Instance.ReadDialogue(entry.GetDialogueSegment().ToArray());

      yield return new WaitForSeconds(3);

      WeatherTweaksWeather fullWeatherType = Variables.WeatherTweaksTypes.FirstOrDefault(type => type.Name == entry.GetWeather().Name);

      logger.LogWarning($"{fullWeatherType.Name} {fullWeatherType.Type} {fullWeatherType.VanillaWeatherType}");

      StartOfRound.Instance.currentLevel.currentWeather = fullWeatherType.VanillaWeatherType;
      TimeOfDay.Instance.currentLevelWeather = fullWeatherType.VanillaWeatherType;
      GameNetworkManager.Instance.localPlayerController.currentAudioTrigger.weatherEffect = (int)fullWeatherType.VanillaWeatherType;

      CurrentEntry = entry;
    
      if (entry.Weather.WeatherType == LevelWeatherType.Rainy)
      {
        TimeOfDay.Instance.StartCoroutine(SpawnMudPatches());
      }

      // GameInteraction.SetWeatherEffects(TimeOfDay.Instance, [fullWeatherType.Effect]);
      WeatherController.SetWeatherEffects(fullWeatherType.VanillaWeatherType);

      // TODO account for player being dead
    }

    internal static void ShipLandingPatch(SelectableLevel currentLevel, Weather currentWeather)
    {
      if (currentWeather is ProgressingWeatherType progressingWeather)
      {
        ChangeMidDay.SetCurrentWeather(progressingWeather);
      }
    }

    internal static void SetCurrentWeather(ProgressingWeatherType weather)
    {
      logger.LogInfo($"Setting current weather to {weather.Name}");

      Reset();
      random ??= new System.Random(StartOfRound.Instance.randomMapSeed);

      currentWeather = weather;
      weatherEntries = weather.WeatherEntries.ToList();

      // when this runs we know that weather is progressing *and* nothing else has been set as a current weather
      // this allows us to not set the weather type at 0.0f time, instead uses the "base" weather as a 0.0 entry
      CurrentEntry = new ProgressingWeatherEntry()
      {
        DayTime = 0.0f,
        Chance = 1.0f,
        Weather = new WeatherTypeResolvable(weather.StartingWeather.WeatherType)
      };

      weatherEntries.Sort((a, b) => a.DayTime.CompareTo(b.DayTime));

      NetworkedConfig.SetProgressingWeatherEntry(CurrentEntry);
      TimeOfDay.Instance.StartCoroutine(DoMidDayChange(CurrentEntry));
    }

    internal static void Reset()
    {
      currentWeather = null;
      LastCheckedEntry = 0;
      weatherEntries = [];
    }

    internal static bool IsMudPickValid(Vector3 position)
    {
        float mudSqrDistance = 100f; //Squared distance between possible mud location and a player
        bool isValidPick = true;
        foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
        {
            if (Vector3.SqrMagnitude(position - player.transform.position) < mudSqrDistance)
            {
                isValidPick = false;
                break;
            }
        }
        return isValidPick;
    }

    internal static Vector3 TryGetValidMudPick(Vector3 position)
    {
        System.Random random = new(StartOfRound.Instance.randomMapSeed + 2);
        NavMeshHit navHit = new();
        int attemptNum = 0;
        int maxMudPlacementAttempts = 10;
        Vector3 adjustedPosition = position;
        while (attemptNum < maxMudPlacementAttempts && !IsMudPickValid(position))
        {
            adjustedPosition = RoundManager.Instance.outsideAINodes[random.Next(0, RoundManager.Instance.outsideAINodes.Length)].transform.position;
            adjustedPosition = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(adjustedPosition, 30f, navHit, random, -1) + Vector3.up;
            attemptNum++;
        }

        return adjustedPosition;
    }

    internal static IEnumerator SpawnMudPatches()
    {
        logger.LogDebug("Spawning mud patches!");
        System.Random random = new(StartOfRound.Instance.randomMapSeed + 2);
        NavMeshHit navMeshHit = new();
        int numberOfPuddles = random.Next(5, 15);
        if (random.Next(0, 100) < 7)
        {
            numberOfPuddles = random.Next(5, 30);
        }
        for (int i = 0; i < numberOfPuddles; i++)
        {
            Vector3 mudPosition = RoundManager.Instance.outsideAINodes[random.Next(0, RoundManager.Instance.outsideAINodes.Length)].transform.position;
            mudPosition = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(mudPosition, 30f, navMeshHit, random, -1);
            mudPosition = TryGetValidMudPick(mudPosition) + Vector3.up;
            GameObject.Instantiate(RoundManager.Instance.quicksandPrefab, mudPosition, Quaternion.identity, RoundManager.Instance.mapPropsContainer.transform);
            yield return null;
        }
    }
  }
}
