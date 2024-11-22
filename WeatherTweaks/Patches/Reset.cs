using HarmonyLib;

namespace WeatherTweaks.Patches
{
  public class Reset
  {
    public static void ResetThings()
    {
      // Variables.WeatherTypes.Clear();
      Variables.CurrentEffects.Clear();
      // Variables.CurrentWeathers.Clear();

      Variables.IsSetupFinished = false;
    }
  }
}
