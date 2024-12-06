using HarmonyLib;
using UnityEngine;
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

      // destroy all objects of class WeatherTweaksWeather that are vanilla weathers
      foreach (WeatherTweaksWeather weather in Object.FindObjectsOfType<WeatherTweaksWeather>())
      {
        if (weather.Type == WeatherRegistry.WeatherType.Vanilla || weather.Type == WeatherRegistry.WeatherType.Clear)
        {
          Object.Destroy(weather);
        }
      }
    }
  }
}
