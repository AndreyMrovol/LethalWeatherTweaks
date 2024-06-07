using System;
using System.Collections;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Newtonsoft.Json;
using WeatherType = WeatherTweaks.Definitions.WeatherType;

namespace WeatherTweaks
{
  [HarmonyPatch(typeof(WeatherRegistry.OpeningDoorsSequencePatch))]
  internal class OpeningDoorsSequencePatch
  {
    [HarmonyPatch(nameof(WeatherRegistry.OpeningDoorsSequencePatch.SetWeatherEffects))]
    [HarmonyPrefix]
    internal static bool SetWeatherEffects()
    {
      WeatherType currentWeather = Variables.GetFullWeatherType(Variables.CurrentWeathers[TimeOfDay.Instance.currentLevel]);

      if (StartOfRound.Instance.IsHost)
      {
        if (currentWeather.GetType() == typeof(WeatherTweaks.Definitions.Types.CombinedWeatherType))
        {
          Plugin.logger.LogWarning($"WeatherType is CombinedWeatherType");

          Definitions.Types.CombinedWeatherType combinedWeather = (Definitions.Types.CombinedWeatherType)currentWeather;

          NetworkedConfig.SetWeatherEffects(combinedWeather.Weathers);
        }
        else
        {
          // Plugin.logger.LogWarning($"WeatherType is default");

          NetworkedConfig.SetWeatherEffects([currentWeather.Weather]);
        }
      }

      Variables.CurrentLevelWeather = currentWeather;

      Plugin.logger.LogError(
        $"Landing at {TimeOfDay.Instance.currentLevel.PlanetName} with weather {JsonConvert.SerializeObject(
        currentWeather,
        Formatting.None,
        new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }
      )}"
      );

      return false;
    }

    [HarmonyPatch(nameof(WeatherRegistry.OpeningDoorsSequencePatch.SetWeatherEffects))]
    [HarmonyPostfix]
    internal static void LogEffectsEnabled()
    {
      foreach (WeatherEffect effect in TimeOfDay.Instance.effects)
      {
        Plugin.logger.LogError($"Effect {effect} enabled: {effect.effectEnabled}");
      }
    }

    [HarmonyPatch(nameof(WeatherRegistry.OpeningDoorsSequencePatch.RunWeatherPatches))]
    [HarmonyPostfix]
    internal static void RunWeatherPatches()
    {
      BasegameWeatherPatch.ChangeFog();
    }
  }
}
