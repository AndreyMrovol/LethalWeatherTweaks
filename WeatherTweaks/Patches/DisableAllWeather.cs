using System;
using BepInEx.Logging;
using HarmonyLib;
using WeatherTweaks.Definitions;

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

      foreach (Weather weather in Variables.Weathers)
      {
        weather.Effect.EffectEnabled = false;
      }

      ChangeMidDay.lastCheckedEntry = 0;
      ChangeMidDay.currentEntry = null;
      // LLLDungeonExitPatch.RemoveListener();

      SunAnimator.Clear();

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
