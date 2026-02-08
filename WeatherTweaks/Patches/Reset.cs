using UnityEngine;
using WeatherRegistry.Enums;
using WeatherTweaks.Definitions;

namespace WeatherTweaks.Patches
{
  public class Reset
  {
    public static void ResetThings()
    {
      Variables.CurrentEffects.Clear();
      Variables.IsSetupFinished = false;
      Variables.WeatherTweaksTypes.Clear();
    }
  }
}
