using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using WeatherAPI;
using WeatherTweaks.Definitions;
using static WeatherTweaks.Definitions.Types;

namespace WeatherTweaks.Patches
{
  [HarmonyPatch(typeof(Terminal))]
  public class TerminalStartPatch
  {
    [HarmonyPostfix]
    [HarmonyPatch("Start")]
    [HarmonyPriority(Priority.Last)]
    [HarmonyAfter("WeatherAPI")]
    public static void Postfix(Terminal __instance)
    {
      Variables.PopulateWeathers(StartOfRound.Instance);

      foreach (Modules.Types.CombinedWeatherType combined in CustomWeatherHandler.RegisteredCombinedWeathers)
      {
        List<Weather> weathers = [];
        foreach (LevelWeatherType levelWeather in combined.Weathers)
        {
          weathers.Add(WeatherAPI.WeatherManager.GetWeather(levelWeather));
        }

        CombinedWeatherType combinedWeather = new CombinedWeatherType(combined.Name, weathers) { weightModify = combined.WeightModify, };

        Variables.CombinedWeatherTypes.Add(combinedWeather);
        Variables.WeatherTypes.Add(combinedWeather);
      }

      foreach (Modules.Types.ProgressingWeatherType progressing in CustomWeatherHandler.RegisteredProgressingWeathers)
      {
        List<ProgressingWeatherEntry> weatherEntries = progressing.WeatherEntries;
        List<Weather> weathers = [];
        foreach (ProgressingWeatherEntry entry in weatherEntries)
        {
          weathers.Add(WeatherAPI.WeatherManager.GetWeather(entry.Weather));
        }

        ProgressingWeatherType progressingWeather = new ProgressingWeatherType(progressing.Name, progressing.StartingWeather, weatherEntries)
        {
          weightModify = progressing.WeightModify,
        };

        Variables.WeatherTypes.Add(progressingWeather);
      }

      Variables.IsSetupFinished = true;
    }
  }
}
