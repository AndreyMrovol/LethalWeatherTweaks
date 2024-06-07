using HarmonyLib;
using WeatherRegistry;

namespace WeatherTweaks.Patches
{
  [HarmonyPatch(typeof(WeatherRegistry.WeatherManager))]
  internal class WeatherRegistryPatches
  {
    [HarmonyPatch(typeof(WeatherRegistry.Patches.SetMapScreenInfoToCurrentLevelPatch))]
    [HarmonyPatch("GetDisplayWeatherString")]
    [HarmonyPostfix]
    internal static void GetDisplayWeatherStringPatch(ref string __result, SelectableLevel level, Weather weather)
    {
      __result = Variables.GetPlanetCurrentWeather(level);
    }

    [HarmonyPatch(typeof(WeatherRegistry.WeatherManager))]
    [HarmonyPatch("GetCurrentWeatherName")]
    [HarmonyPostfix]
    internal static void GetCurrentWeatherNamePatch(ref string __result, SelectableLevel level)
    {
      __result = Variables.GetPlanetCurrentWeather(level, false);
    }
  }
}
