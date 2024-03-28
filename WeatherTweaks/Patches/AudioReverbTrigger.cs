// using System.Collections.Generic;
// using System.Linq;
// using BepInEx.Logging;
// using GameNetcodeStuff;
// using HarmonyLib;

// namespace WeatherTweaks
// {
//   [HarmonyPatch(typeof(AudioReverbTrigger))]
//   class AudioReverbTriggerPatch
//   {
//     internal static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("WeatherTweaks ART");

//     [HarmonyPostfix]
//     [HarmonyPatch(typeof(AudioReverbTrigger), "ChangeAudioReverbForPlayer")]
//     private static void ChangeAudioReverbForPlayerPatch(AudioReverbTrigger __instance, PlayerControllerB pScript)
//     {
//       if (!__instance.isShipRoom)
//       {
//         // logger.LogDebug(
//         //   $"ChangeAudioReverbForPlayerPatch called with {__instance.weatherEffect}/{__instance.enableCurrentLevelWeather}/{__instance.disableAllWeather}"
//         // );
//         logger.LogDebug($"Current weather: {TimeOfDay.Instance.currentLevelWeather}/{StartOfRound.Instance.currentLevel.currentWeather}");
//       }

//       if (__instance.disableAllWeather)
//       {
//         TimeOfDay.Instance.DisableAllWeather();
//       }
//       else
//       {
//         // logger.LogDebug($"Weather Effect: {__instance.weatherEffect}");
//         // logger.LogDebug($"Effect Enabled: {__instance.effectEnabled}");
//         // logger.LogDebug($"Enable Current Level Weather: {__instance.enableCurrentLevelWeather}");
//         // logger.LogDebug($"Current Level Weather: {TimeOfDay.Instance.currentLevelWeather}");

//         if (__instance.weatherEffect != -1)
//         {
//           TimeOfDay.Instance.effects[__instance.weatherEffect].effectEnabled = __instance.effectEnabled;
//         }

//         // logger.LogWarning($"{TimeOfDay.Instance.currentLevelWeather}");

//         logger.LogDebug(
//           $"Is player inside: {pScript.isInsideFactory}/{pScript.isInHangarShipRoom}/{pScript.isInElevator}/{__instance.isShipRoom}"
//         );
//         // TimeOfDay.Instance.effects.Do(effect =>
//         // {
//         //   logger.LogDebug($"Effect: {effect.name} is {effect.effectEnabled}");
//         // });

//         if (__instance.enableCurrentLevelWeather)
//         {
//           List<WeatherEffect> weatherEffects = Variables.CurrentLevelWeather.Effects;

//           logger.LogWarning($"Setting weather effects for {TimeOfDay.Instance.currentLevel.PlanetName}");

//           foreach (WeatherEffect timeOfDayEffect in TimeOfDay.Instance.effects)
//           {
//             logger.LogDebug($"Effect: {timeOfDayEffect.name}");
//             logger.LogDebug($"Effect Enabled: {timeOfDayEffect.effectEnabled}");
//             // logger.LogDebug($"Effect Object Enabled: {timeOfDayEffect.effectObject.activeSelf}");
//             logger.LogInfo($"Contains: {weatherEffects.Contains(timeOfDayEffect)}");

//             if (weatherEffects.Contains(timeOfDayEffect))
//             {
//               timeOfDayEffect.effectEnabled = true;
//             }
//           }

//           // TimeOfDay.Instance.effects[(int)TimeOfDay.Instance.currentLevelWeather].effectEnabled = true;
//         }
//       }
//     }
//   }
// }
