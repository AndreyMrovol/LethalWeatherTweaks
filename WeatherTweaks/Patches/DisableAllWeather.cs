using System;
using BepInEx.Logging;
using HarmonyLib;
using WeatherRegistry;

namespace WeatherTweaks
{
  public static class DisableAllWeathers
  {
    internal static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("WeatherTweaks TimeOfDay");

    internal static void DisableAllWeather()
    {
      ChangeMidDay.lastCheckedEntry = 0;

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
