using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using WeatherRegistry;
using WeatherTweaks.Definitions;
using static WeatherTweaks.Definitions.Types;

namespace WeatherTweaks.Patches
{
  public class TerminalStartPatch
  {
    public static void Start()
    {
      // Variables.CombinedWeathers.Clear();
      // Variables.ProgressingWeathers.Clear();
      // Variables.WeatherTypes.Clear();
      Variables.CurrentEffects.Clear();
      // Variables.CurrentWeathers.Clear();

      Plugin.logger.LogWarning("Terminal start start");

      // Variables.PopulateWeathers();

      // foreach (Definitions.Types.CombinedWeatherType combined in Variables.CombinedWeathers)
      // {
      //   Variables.WeatherTypes.Add(combined);
      // }

      // foreach (Definitions.Types.ProgressingWeatherType progressing in Variables.ProgressingWeathers)
      // {
      //   Variables.WeatherTypes.Add(progressing);
      // }

      WeatherRegistry.Settings.ScreenMapColors.Add("+", Color.white);
      WeatherRegistry.Settings.ScreenMapColors.Add("/", Color.white);
      WeatherRegistry.Settings.ScreenMapColors.Add(">", Color.white);
      WeatherRegistry.Settings.ScreenMapColors.Add("?", Color.white);
      WeatherRegistry.Settings.ScreenMapColors.Add("[UNKNOWN]", new Color(0.29f, 0.29f, 0.29f));

      Variables.WeatherTweaksTypes.ForEach(weatherType => { });

      Variables.IsSetupFinished = true;
      StartOfRound.Instance.SetPlanetsWeather();
      StartOfRound.Instance.SetMapScreenInfoToCurrentLevel();
    }
  }
}
