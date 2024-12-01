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
      __result = Variables.GetPlanetCurrentWeather(level);
    }

    [HarmonyPatch(typeof(WeatherRegistry.WeatherManager))]
    [HarmonyPatch("GetCurrentWeatherName")]
    [HarmonyPostfix]
    internal static void GetCurrentWeatherNamePatch(ref string __result, SelectableLevel level)
    {
      __result = Variables.GetPlanetCurrentWeather(level, false);
    }

    [HarmonyPatch(typeof(WeatherRegistry.Patches.SpawnScrapInLevelPatches))]
    [HarmonyPatch("ChangeMultipliers")]
    [HarmonyPostfix]
    internal static void ChangeMultipliersPatch(RoundManager __0)
    {
      if (!StartOfRound.Instance.IsHost)
      {
        return;
      }

      Definitions.WeatherType currentWeather = Variables.GetPlanetCurrentWeatherType(StartOfRound.Instance.currentLevel);
      Plugin.DebugLogger.LogDebug($"Changing multipliers for weather {currentWeather.Name}");
      (float valueMultiplier, float amountMultiplier) = currentWeather.GetMultiplierData();


      switch (currentWeather.Type)
      {
        case CustomWeatherType.Normal:
          break;
        case CustomWeatherType.Combined:
        case CustomWeatherType.Progressing:
          __0.scrapValueMultiplier = valueMultiplier * 0.4f;
          __0.scrapAmountMultiplier = amountMultiplier;
          break;
        default:
          Plugin.logger.LogError($"Unknown weather type {currentWeather.Type}");
          break;
      }
    }
  }
}
