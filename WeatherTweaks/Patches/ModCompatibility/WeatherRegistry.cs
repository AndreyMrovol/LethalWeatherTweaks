using System.Linq;
using HarmonyLib;
using WeatherRegistry;
using WeatherTweaks.Definitions;

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
      Plugin.logger.LogDebug($"Getting display weather string for {level.PlanetName}");
      __result = Variables.GetPlanetCurrentWeather(level);
    }

    [HarmonyPatch(typeof(WeatherRegistry.WeatherManager))]
    [HarmonyPatch("GetCurrentWeatherName")]
    [HarmonyPostfix]
    internal static void GetCurrentWeatherNamePatch(ref string __result, SelectableLevel level)
    {
      __result = Variables.GetPlanetCurrentWeather(level, false);
    }

    [HarmonyPatch(typeof(WeatherRegistry.Patches.SetPlanetsWeatherPatch))]
    [HarmonyPatch("GameMethodPatch")]
    [HarmonyPrefix]
    internal static bool GameMethodPatch()
    {
      return Variables.IsSetupFinished;
    }
  }
}
