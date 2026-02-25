using TMPro;
using UnityEngine;

namespace WeatherTweaks.Patches
{
  public class TerminalStartPatch
  {
    public static void Start()
    {
      Variables.CurrentEffects.Clear();

      TMP_ColorGradient specialSymbolGradient = WeatherRegistry.Utils.ColorConverter.ToTMPColorGradient(Color.white);

      WeatherRegistry.Settings.ScreenMapColors.Add("+", specialSymbolGradient);
      WeatherRegistry.Settings.ScreenMapColors.Add("plus", specialSymbolGradient);

      WeatherRegistry.Settings.ScreenMapColors.Add("/", specialSymbolGradient);
      WeatherRegistry.Settings.ScreenMapColors.Add("slash", specialSymbolGradient);

      WeatherRegistry.Settings.ScreenMapColors.Add(">", specialSymbolGradient);
      WeatherRegistry.Settings.ScreenMapColors.Add("arrow", specialSymbolGradient);

      WeatherRegistry.Settings.ScreenMapColors.Add("?", specialSymbolGradient);
      WeatherRegistry.Settings.ScreenMapColors.Add("question", specialSymbolGradient);

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
