using HarmonyLib;

namespace WeatherTweaks
{
  [HarmonyPatch(typeof(TimeOfDay))]
  public static partial class TimeOfDayPatch
  {
    [HarmonyPostfix]
    [HarmonyPatch(typeof(TimeOfDay), "SetWeatherEffects")]
    private static void PostfixSetWeatherEffects(TimeOfDay __instance)
    {
      WeatherType currentlySelectedWeather = Variables.CurrentWeathers[__instance.currentLevel];

      if (currentlySelectedWeather == null)
      {
        return;
      }

      if (currentlySelectedWeather.Type == CustomWeatherType.Vanilla)
      {
        return;
      }

      if (!StartOfRound.Instance.IsHost)
      {
        return;
      }
      else
      {
        NetworkedConfig.SetWeatherEffects(currentlySelectedWeather.Weathers);
      }

      GameInteraction.SetWeatherEffects(__instance, currentlySelectedWeather.Effects);
    }
  }
}
