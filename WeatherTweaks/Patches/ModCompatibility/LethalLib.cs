using System.Collections.Generic;
using System.Linq;
using LethalLib;
using WeatherTweaks.Definitions;
using static LethalLib.Modules.Weathers;

namespace WeatherTweaks.Patches
{
  public class LethalLibPatch(string guid, string version = null) : MrovLib.Compatibility.CompatibilityBase(guid, version)
  {
    public static List<CustomWeather> GetLethalLibWeathers()
    {
      // Get all the weathers from LethalLib

      return LethalLib.Modules.Weathers.customWeathers.Values.ToList();
    }

    public static List<Weather> ConvertLLWeathers()
    {
      List<CustomWeather> llWeathers = GetLethalLibWeathers();
      List<Weather> weathers = new List<Weather>();

      foreach (CustomWeather llWeather in llWeathers)
      {
        WeatherTweaks.Definitions.WeatherEffect effect =
          new(llWeather.weatherEffect.effectObject, llWeather.weatherEffect.effectPermanentObject)
          {
            SunAnimatorBool = llWeather.weatherEffect.sunAnimatorBool,
            DefaultVariable1 = llWeather.weatherVariable1,
            DefaultVariable2 = llWeather.weatherVariable2,
          };

        Weather weather = new Weather(llWeather.name, effect);
        weathers.Add(weather);
      }

      return weathers;
    }
  }
}
