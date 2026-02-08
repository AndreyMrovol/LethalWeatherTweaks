using UnityEngine;

namespace WeatherTweaks.Patches
{
  public class TerminalStartPatch
  {
    public static void Start()
    {
      Variables.CurrentEffects.Clear();

      WeatherRegistry.Settings.ScreenMapColors.Add("+", WeatherRegistry.Utils.ColorConverter.ToTMPColorGradient(Color.white));
      WeatherRegistry.Settings.ScreenMapColors.Add("/", WeatherRegistry.Utils.ColorConverter.ToTMPColorGradient(Color.white));
      WeatherRegistry.Settings.ScreenMapColors.Add(">", WeatherRegistry.Utils.ColorConverter.ToTMPColorGradient(Color.white));
      WeatherRegistry.Settings.ScreenMapColors.Add("?", WeatherRegistry.Utils.ColorConverter.ToTMPColorGradient(Color.white));
      WeatherRegistry.Settings.ScreenMapColors.Add(
        "[UNKNOWN]",
        WeatherRegistry.Utils.ColorConverter.ToTMPColorGradient(new Color(0.29f, 0.29f, 0.29f))
      );

      Variables.IsSetupFinished = true;
      StartOfRound.Instance.SetPlanetsWeather();
      StartOfRound.Instance.SetMapScreenInfoToCurrentLevel();
    }
  }
}
