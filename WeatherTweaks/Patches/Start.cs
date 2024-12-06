using UnityEngine;

namespace WeatherTweaks.Patches
{
  public class TerminalStartPatch
  {
    public static void Start()
    {
      Variables.CurrentEffects.Clear();

      WeatherRegistry.Settings.ScreenMapColors.Add("+", Color.white);
      WeatherRegistry.Settings.ScreenMapColors.Add("/", Color.white);
      WeatherRegistry.Settings.ScreenMapColors.Add(">", Color.white);
      WeatherRegistry.Settings.ScreenMapColors.Add("?", Color.white);
      WeatherRegistry.Settings.ScreenMapColors.Add("[UNKNOWN]", new Color(0.29f, 0.29f, 0.29f));

      // Variables.WeatherTweaksTypes.ForEach(weatherType => { });

      FoggyPatch.CreateEffectOverrides();

      Variables.IsSetupFinished = true;
      StartOfRound.Instance.SetPlanetsWeather();
      StartOfRound.Instance.SetMapScreenInfoToCurrentLevel();
    }
  }
}
