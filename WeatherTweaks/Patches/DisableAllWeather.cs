using System;
using HarmonyLib;

namespace WeatherTweaks
{
  [HarmonyPatch(typeof(TimeOfDay))]
  public static partial class TimeOfDayPatch
  {
    [HarmonyPostfix]
    [HarmonyPatch(typeof(TimeOfDay), "DisableAllWeather")]
    private static void PostfixDisableAllWeather(TimeOfDay __instance, ref bool deactivateObjects)
    {
      deactivateObjects = true;
      logger.LogDebug("Disabling all weather");

      foreach (WeatherEffect effect in __instance.effects)
      {
        effect.effectEnabled = false;
        if (effect.effectPermanentObject != null)
        {
          effect.effectPermanentObject.SetActive(false);
        }

        if (effect.effectObject != null)
        {
          effect.effectObject.SetActive(false);
        }
      }

      if (StartOfRound.Instance.IsHost)
      {
        NetworkedConfig.weatherEffectsSynced.Value = [];
      }
    }
  }
}
