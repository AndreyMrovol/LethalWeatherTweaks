using System;
using System.Collections;
using BepInEx.Logging;
using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.AI;

namespace WeatherTweaks
{
  public class MudPatches
  {
    internal static System.Random random;
    internal static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("WeatherTweaks MudPatch");

    internal static IEnumerator SpawnMudPatches()
    {
      logger.LogDebug("Spawning mud patches!");

      random ??= new System.Random(StartOfRound.Instance.randomMapSeed);
      System.Random seededRandom = new(StartOfRound.Instance.randomMapSeed + 2);
      NavMeshHit navHit = new();

      int numberOfPuddles = random.Next(5, 15);
      if (random.Next(0, 100) < 7)
      {
        numberOfPuddles = random.Next(5, 30);
      }

      for (int i = 0; i < numberOfPuddles; i++)
      {
        Vector3 mudPosition = TryGetValidMudPick(seededRandom, navHit) + Vector3.up;
        GameObject.Instantiate(
          RoundManager.Instance.quicksandPrefab,
          mudPosition,
          Quaternion.identity,
          RoundManager.Instance.mapPropsContainer.transform
        );
        yield return null;
      }
    }

    internal static bool IsMudPickValid(Vector3 position)
    {
      if (position == default)
      {
        return false;
      }

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

    internal static Vector3 TryGetValidMudPick(System.Random seededRandom, NavMeshHit navHit)
    {
      Vector3 mudPosition = default;
      int attemptNum = 0;
      int maxMudPlacementAttempts = 10;
      while (attemptNum < maxMudPlacementAttempts && !IsMudPickValid(mudPosition))
      {
        mudPosition = RoundManager.Instance.outsideAINodes[seededRandom.Next(0, RoundManager.Instance.outsideAINodes.Length)].transform.position;
        mudPosition = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(mudPosition, 30f, navHit, seededRandom, -1) + Vector3.up;
        attemptNum++;
      }

      return mudPosition;
    }
  }
}
