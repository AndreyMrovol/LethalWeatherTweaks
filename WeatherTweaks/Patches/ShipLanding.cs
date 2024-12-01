using WeatherRegistry;
using static WeatherTweaks.Definitions.Types;

namespace WeatherTweaks.Patches
{
  internal class ShipLandingPatches
  {
    internal static void ShipLandingPatch(SelectableLevel currentLevel, Weather currentWeather)
    {
      if (currentWeather is ProgressingWeatherType progressingWeather)
      {
        ChangeMidDay.SetCurrentWeather(progressingWeather);
      }
    }
  }
}
