using BepInEx.Configuration;
using WeatherRegistry;
using WeatherRegistry.Modules;

namespace WeatherTweaks.Definitions
{
  public class WeatherTweaksConfig : RegistryWeatherConfig
  {
    public BooleanConfigHandler EnableWeather = new(true);

    public override void Init(Weather weather)
    {
      Plugin.DebugLogger.LogInfo($"Creating WeatherConfig: {weather}");

      EnableWeather.SetConfigEntry(weather, "Enable Weather", new("Enable this weather type"));

      base.Init(weather);
    }
  }
}
