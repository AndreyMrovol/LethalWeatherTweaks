using System;
using System.Collections;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Newtonsoft.Json;
using WeatherRegistry;
using WeatherType = WeatherTweaks.Definitions.WeatherType;

namespace WeatherTweaks
{
  internal class OpeningDoorsSequencePatch
  {
    internal static void SetWeatherEffects(SelectableLevel level, Weather weather)
    {
      WeatherType currentWeather = Variables.GetFullWeatherType(Variables.CurrentWeathers[level]);

      if (StartOfRound.Instance.IsHost)
      {
        if (currentWeather.Type == Definitions.CustomWeatherType.Combined)
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

      Plugin.logger.LogWarning(
        $"Landing at {MrovLib.SharedMethods.GetNumberlessPlanetName(TimeOfDay.Instance.currentLevel)} with weather {JsonConvert.SerializeObject(
        currentWeather,
        Formatting.None,
        new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }
      )}"
      );
    }

    [HarmonyPatch(nameof(WeatherRegistry.OpeningDoorsSequencePatch.RunWeatherPatches))]
    [HarmonyPostfix]
    internal static void RunWeatherPatches()
    {
      // BasegameWeatherPatch.ChangeFog();
    }
  }
}
