// using System;
// using System.Numerics;
// using DigitalRuby.ThunderAndLightning;
// using HarmonyLib;
// using UnityEngine;
// using Object = UnityEngine.Object;
// using Vector3 = UnityEngine.Vector3;

// namespace WeatherTweaks
// {
//   [HarmonyPatch(typeof(TimeOfDay))]
//   public class SpawnLightning
//   {
//     static LightningBoltPrefabScript lightningBoltPrefabScript;

//     public static void SpawnLightningBolt(Vector3 strikePosition, int howMany = 1)
//     {
//       // This will ignore if the strikePosition is inside something, oh well

//       LightningBoltPrefabScript localLightningBoltPrefabScript;
//       UnityEngine.Vector3 vector = Vector3.zero;
//       System.Random random;

//       random = new System.Random(StartOfRound.Instance.randomMapSeed);
//       float num = (float)random.Next(-32, 32);
//       float num2 = (float)random.Next(-32, 32);
//       vector = strikePosition + Vector3.up * 160f + new Vector3((float)random.Next(-32, 32), 0f, (float)random.Next(-32, 32));

//       StormyWeather stormy = UnityEngine.Object.FindObjectOfType<StormyWeather>(true);

//       if (stormy == null)
//       {
//         Plugin.logger.LogError("StormyWeather not found");
//         return;
//       }

//       Plugin.logger.LogDebug($"{vector} -> {strikePosition}");

//       // clone the prefab
//       localLightningBoltPrefabScript = Object.Instantiate(stormy.targetedThunder);
//       localLightningBoltPrefabScript.enabled = true;

//       if (localLightningBoltPrefabScript == null)
//       {
//         Plugin.logger.LogError("localLightningBoltPrefabScript not found");
//         return;
//       }

//       localLightningBoltPrefabScript.Camera = GameNetworkManager.Instance.localPlayerController.gameplayCamera;
//       localLightningBoltPrefabScript.AutomaticModeSeconds = 0.2f;
//       // localLightningBoltPrefabScript.Trigger();

//       localLightningBoltPrefabScript.Source.transform.position = vector;
//       localLightningBoltPrefabScript.Destination.transform.position = strikePosition;
//       localLightningBoltPrefabScript.CreateLightningBoltsNow();

//       // localLightningBoltPrefabScript.CallLightning();

//       AudioSource audioSource = Object.Instantiate(stormy.targetedStrikeAudio);
//       audioSource.transform.position = strikePosition + Vector3.up * 0.5f;
//       audioSource.enabled = true;

//       // localLightningBoltPrefabScript.Trigger();
//       stormy.PlayThunderEffects(strikePosition, audioSource);
//     }

//     // [HarmonyPostfix]
//     // [HarmonyPatch("MoveTimeOfDay")]
//     // public static void MoveTimeOfDayPatch(TimeOfDay __instance)
//     // {
//     //   if (!StartOfRound.Instance.IsHost)
//     //   {
//     //     return;
//     //   }

//     //   float normalizedTimeOfDay = __instance.normalizedTimeOfDay;
//     //   Plugin.logger.LogDebug($"Normalized time of day: {normalizedTimeOfDay}");
//     //   Plugin.logger.LogDebug($"{GameNetworkManager.Instance.localPlayerController.oldPlayerPosition}");

//     //   if (normalizedTimeOfDay >= 0.16f && normalizedTimeOfDay <= 0.25f)
//     //   {
//     //     Vector3 strikePosition = StartOfRound.Instance.localPlayerController.transform.position;
//     //     SpawnLightningBolt(strikePosition);

//     //     // Object
//     //     //   .FindObjectOfType<StormyWeather>(true)
//     //     //   .LightningStrike(GameNetworkManager.Instance.localPlayerController.oldPlayerPosition, true);

//     //     // RoundManager.Instance.LightningStrikeServerRpc(GameNetworkManager.Instance.localPlayerController.oldPlayerPosition);
//     //   }
//     // }
//   }
// }
