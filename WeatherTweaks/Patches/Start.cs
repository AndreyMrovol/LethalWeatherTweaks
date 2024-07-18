using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using WeatherRegistry;
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
    [HarmonyAfter("mrov.WeatherRegistry")]
    public static void Postfix(Terminal __instance)
    {
      // Variables.CombinedWeatherTypes.Clear();
      // Variables.ProgressingWeatherTypes.Clear();
      Variables.WeatherTypes.Clear();
      Variables.CurrentEffects.Clear();
      Variables.CurrentWeathers.Clear();

      Variables.PopulateWeathers(StartOfRound.Instance);

      foreach (Definitions.Types.CombinedWeatherType combined in Variables.CombinedWeatherTypes)
      {
        Variables.WeatherTypes.Add(combined);
      }

      foreach (Definitions.Types.ProgressingWeatherType progressing in Variables.ProgressingWeatherTypes)
      {
        Variables.WeatherTypes.Add(progressing);
      }

      WeatherRegistry.Settings.ScreenMapColors.Add("+", Color.white);
      WeatherRegistry.Settings.ScreenMapColors.Add("/", Color.white);
      WeatherRegistry.Settings.ScreenMapColors.Add(">", Color.white);
      WeatherRegistry.Settings.ScreenMapColors.Add("?", Color.white);
      WeatherRegistry.Settings.ScreenMapColors.Add("[UNKNOWN]", new Color(0.29f, 0.29f, 0.29f));

      Variables.IsSetupFinished = true;
      StartOfRound.Instance.SetPlanetsWeather();
      StartOfRound.Instance.SetMapScreenInfoToCurrentLevel();
    }
  }
}
