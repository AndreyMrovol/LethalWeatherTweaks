// using System.Collections.Generic;
// using BepInEx.Logging;
// using GameNetcodeStuff;
// using HarmonyLib;
// using LethalLevelLoader;
// using UnityEngine;

// namespace WeatherTweaks
// {
//   internal class LLLDungeonExitPatch
//   {
//     internal static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("WeatherTweaks LLL Exit");

//     internal static bool isPlayerInside = false;

//     internal static void StartListener()
//     {
//       ExtendedLevel currentExtendedLevel = LethalLevelLoader.LevelManager.CurrentExtendedLevel;

//       currentExtendedLevel.levelEvents.onPlayerExitDungeon.AddListener(OnPlayerLeftDungeon);
//       currentExtendedLevel.levelEvents.onPlayerEnterDungeon.AddListener(OnPlayerEnteredDungeon);
//     }

//     internal static void RemoveListener()
//     {
//       ExtendedLevel currentExtendedLevel = LethalLevelLoader.LevelManager.CurrentExtendedLevel;

//       currentExtendedLevel.levelEvents.onPlayerExitDungeon.RemoveListener(OnPlayerLeftDungeon);
//       currentExtendedLevel.levelEvents.onPlayerEnterDungeon.RemoveListener(OnPlayerEnteredDungeon);

//       isPlayerInside = false;
//     }

//     private static void OnPlayerEnteredDungeon((EntranceTeleport, GameNetcodeStuff.PlayerControllerB) eventData)
//     {
//       // Handle player entered dungeon event
//       logger.LogDebug($"Player {eventData.Item2.playerUsername} entered dungeon");

//       if (eventData.Item2 != StartOfRound.Instance.localPlayerController)
//       {
//         return;
//       }

//       isPlayerInside = true;
//     }

//     private static void OnPlayerLeftDungeon((EntranceTeleport, GameNetcodeStuff.PlayerControllerB) eventData)
//     {
//       // Handle player left dungeon event
//       logger.LogDebug($"Player {eventData.Item2.playerUsername} left dungeon");

//       if (eventData.Item2 != StartOfRound.Instance.localPlayerController)
//       {
//         return;
//       }

//       isPlayerInside = false;

//       List<WeatherEffect> weatherEffects = Variables.CurrentLevelWeather.Effects;

//       weatherEffects.Do(effect =>
//       {
//         logger.LogDebug($"Effect: {effect.name}");
//         logger.LogDebug($"Effect Enabled: {effect.effectEnabled}");
//       });

//       foreach (WeatherEffect timeOfDayEffect in TimeOfDay.Instance.effects)
//       {
//         logger.LogDebug($"Effect: {timeOfDayEffect.name}");
//         logger.LogDebug($"Effect Enabled: {timeOfDayEffect.effectEnabled}");
//         // logger.LogDebug($"Effect Object Enabled: {timeOfDayEffect.effectObject.activeSelf}");
//         logger.LogInfo($"Contains: {weatherEffects.Contains(timeOfDayEffect)}");

//         if (weatherEffects.Contains(timeOfDayEffect))
//         {
//           timeOfDayEffect.effectEnabled = true;

//           if (timeOfDayEffect.effectObject != null)
//           {
//             timeOfDayEffect.effectObject.SetActive(true);
//           }

//           if (timeOfDayEffect.effectPermanentObject != null)
//           {
//             timeOfDayEffect.effectPermanentObject.SetActive(true);
//           }
//         }
//       }
//     }
//   }
// }
