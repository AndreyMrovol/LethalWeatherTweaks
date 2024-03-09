using System.Collections.Generic;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;

namespace WeatherTweaks
{
  [HarmonyPatch(typeof(AudioReverbTrigger))]
  class AudioReverbTriggerPatch
  {
    internal static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("WeatherTweaks ART");

    [HarmonyPostfix]
    [HarmonyPatch(typeof(AudioReverbTrigger), "ChangeAudioReverbForPlayer")]
    private static void ChangeAudioReverbForPlayerPatch(AudioReverbTrigger __instance, PlayerControllerB pScript)
    {
      if (__instance.disableAllWeather)
      {
        TimeOfDay.Instance.DisableAllWeather();
      }
      else
      {
        // logger.LogDebug($"Weather Effect: {__instance.weatherEffect}");
        // logger.LogDebug($"Effect Enabled: {__instance.effectEnabled}");
        // logger.LogDebug($"Enable Current Level Weather: {__instance.enableCurrentLevelWeather}");
        // logger.LogDebug($"Current Level Weather: {TimeOfDay.Instance.currentLevelWeather}");

        if (__instance.weatherEffect != -1)
        {
          TimeOfDay.Instance.effects[__instance.weatherEffect].effectEnabled = __instance.effectEnabled;
        }

        if (__instance.enableCurrentLevelWeather && TimeOfDay.Instance.currentLevelWeather != LevelWeatherType.None)
        {
          List<WeatherEffect> weatherEffects = Variables.GetFullWeatherType(Variables.CurrentWeathers[TimeOfDay.Instance.currentLevel]).Effects;

          foreach (WeatherEffect timeOfDayEffect in TimeOfDay.Instance.effects)
          {
            if (weatherEffects.Contains(timeOfDayEffect))
            {
              timeOfDayEffect.effectEnabled = true;
            }
          }

          // TimeOfDay.Instance.effects[(int)TimeOfDay.Instance.currentLevelWeather].effectEnabled = true;
        }
      }
    }
  }
}
