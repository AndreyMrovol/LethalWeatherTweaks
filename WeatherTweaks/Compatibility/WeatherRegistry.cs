using HarmonyLib;
using WeatherRegistry;

namespace WeatherTweaks.Compatibility
{
  [HarmonyPatch(typeof(WeatherRegistry.WeatherManager))]
  internal class WeatherRegistryPatches
  {
    [HarmonyPatch(typeof(WeatherRegistry.Patches.SetMapScreenInfoToCurrentLevelPatch))]
    [HarmonyPatch("GetDisplayWeatherString")]
    [HarmonyPostfix]
    internal static void GetDisplayWeatherStringPatch(ref string __result, SelectableLevel level, Weather weather)
    {
      Plugin.logger.LogDebug($"Getting display weather string for {level.PlanetName}");
      __result = Variables.GetPlanetCurrentWeather(level);
    }

    [HarmonyPatch(typeof(WeatherRegistry.Patches.SetPlanetsWeatherPatch))]
    [HarmonyPatch("GameMethodPatch")]
    [HarmonyPrefix]
    internal static bool GameMethodPatch()
    {
      return Variables.IsSetupFinished;
    }

    [HarmonyPatch(typeof(WeatherRegistry.WeatherManager))]
    [HarmonyPatch("WeatherDisplayOverride")]
    [HarmonyPostfix]
    internal static void WeatherDisplayOverridePatch(ref string __result, SelectableLevel level)
    {
      if (Variables.IsSetupFinished)
      {
        __result = Variables.GetPlanetCurrentWeather(level, true);
      }
    }

    [HarmonyPatch(typeof(WeatherRegistry.Patches.TerminalPostprocessPatch))]
    [HarmonyPatch("GetPlanetWeatherDisplayString")]
    [HarmonyPostfix]
    internal static void GetPlanetWeatherDisplayStringPatch(ref string __result, bool parentheses)
    {
      if (__result.Contains("[UNKNOWN]"))
      {
        __result = "[UNKNOWN]";
      }
    }
  }
}
