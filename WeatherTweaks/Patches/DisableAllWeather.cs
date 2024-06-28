using System;
using BepInEx.Logging;
using HarmonyLib;

namespace WeatherTweaks
{
  [HarmonyPatch(typeof(TimeOfDay))]
  public static partial class TimeOfDayPatch
  {
    internal static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("WeatherTweaks TimeOfDay");

    [HarmonyPostfix]
    [HarmonyPatch(typeof(TimeOfDay), "DisableAllWeather")]
    private static void DisableAllWeatherPatch(TimeOfDay __instance, bool deactivateObjects)
    {
      logger.LogDebug("Disabling all weather");

      if (!deactivateObjects)
      {
        return;
      }

      logger.LogDebug("DecativateObjects is true");

      foreach (WeatherEffect effect in TimeOfDay.Instance.effects)
      {
        effect.effectEnabled = false;

        if (effect.effectObject != null)
        {
          effect.effectObject.SetActive(false);
        }

        if (effect.effectPermanentObject != null)
        {
          effect.effectPermanentObject.SetActive(false);
        }
      }

      ChangeMidDay.lastCheckedEntry = 0;
      // LLLDungeonExitPatch.RemoveListener();

      ChangeMidDay.currentEntry = null;
      ChangeMidDay.nextEntry = null;

      if (StartOfRound.Instance.IsHost)
      {
        NetworkedConfig.SetWeatherEffects([]);
        NetworkedConfig.SetWeatherType(null);
        NetworkedConfig.SetProgressingWeatherEntry(null);

        ChangeMidDay.random = null;
      }
    }
  }
}
