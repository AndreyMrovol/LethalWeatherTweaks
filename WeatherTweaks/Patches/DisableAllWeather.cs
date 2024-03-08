using System;
using HarmonyLib;

namespace WeatherTweaks
{
  [HarmonyPatch(typeof(TimeOfDay))]
  public static partial class TimeOfDayPatch
  {
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

      if (StartOfRound.Instance.IsHost)
      {
        logger.LogDebug("IsHost is true");
        NetworkedConfig.SetWeatherEffects([]);
      }
    }
  }
}
